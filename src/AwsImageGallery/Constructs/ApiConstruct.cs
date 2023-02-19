using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.S3;
using Constructs;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace AwsImageGallery.Constructs
{
    internal class ApiConstruct : Construct
    {
        public ApiConstruct(Construct scope, string id, ApiConstructProps props) : base(scope, id)
        {
            var role = CreateRole(props);

            var api = new RestApi(this, "image-gallery-api", new RestApiProps
            {
                BinaryMediaTypes = new[] { "*/*" },
                MinimumCompressionSize = 0,
            });

            AddUploadEndpoint(api, props.UploadBucket, role);

            var categories = api.Root.AddResource("categories");
            AddListCategoriesEndpoint(categories, props.WebBucket, role);
            AddListImagesEndpoint(categories, props.WebBucket, role);
        }

        private static void AddUploadEndpoint(RestApi api, IBucket uploadBucket, Role role)
        {
            var imageResource = api.Root.AddResource("image");
            var fileNameResource = imageResource.AddResource("{filename}");

            var integration = new AwsIntegration(new AwsIntegrationProps
            {
                Service = "s3",
                IntegrationHttpMethod = "PUT",
                Path = $"{uploadBucket.BucketName}/{{filename}}",
                Options = new IntegrationOptions
                {
                    CredentialsRole = role,
                    PassthroughBehavior = PassthroughBehavior.WHEN_NO_TEMPLATES,
                    RequestParameters = new Dictionary<string, string>
                    {
                        { "integration.request.path.filename", "method.request.path.filename" },
                        { "integration.request.header.Accept", "method.request.path.Accept" }
                    },
                    IntegrationResponses = new IntegrationResponse[]
                    {
                        new IntegrationResponse
                        {
                            StatusCode = "200",
                            ContentHandling = ContentHandling.CONVERT_TO_TEXT,
                            ResponseParameters = new Dictionary<string, string>
                            {
                                { "method.response.header.Content-Type", "'application/json'" }
                            },
                            ResponseTemplates = new Dictionary<string, string>
                            {
                                { "application/json", "{\"success\":\"true\"}" }
                            }
                        }
                    }
                }
            });

            fileNameResource.AddMethod("POST", integration, new MethodOptions
            {
                RequestParameters = new Dictionary<string, bool>
                {
                    { "method.request.path.filename", true },
                    { "method.request.header.Accept", true },
                    { "method.request.header.Content-Type", true },
                },
                MethodResponses = new MethodResponse[]
                {
                    new MethodResponse
                    {
                        StatusCode = "200",
                        ResponseParameters = new Dictionary<string, bool>
                        {
                            { "method.response.header.Content-Type", true }
                        }
                    }
                }
            });
        }

        private static void AddListCategoriesEndpoint(Resource categories, IBucket webBucket, Role role)
        {
            var integration = new AwsIntegration(new AwsIntegrationProps
            {
                Service = "s3",
                IntegrationHttpMethod = "GET",
                Path = $"{webBucket.BucketName}?delimiter=/",
                Options = new IntegrationOptions
                {
                    CredentialsRole = role,
                    PassthroughBehavior = PassthroughBehavior.WHEN_NO_TEMPLATES,
                    IntegrationResponses = new IntegrationResponse[]
                    {
                        new IntegrationResponse
                        {
                            StatusCode = "200",
                            ResponseTemplates = new Dictionary<string, string>
                            {
                                { "application/json", "{\"success\":\"true\"}" }
                            }
                        }
                    }
                }
            });

            categories.AddMethod("GET", integration, new MethodOptions
            {
                MethodResponses = new MethodResponse[]
                {
                    new MethodResponse
                    {
                        StatusCode = "200",
                        ResponseParameters = new Dictionary<string, bool>
                        {
                            { "method.response.header.Content-Type", true }
                        }
                    }
                }
            });
        }

        private static void AddListImagesEndpoint(Resource categories, IBucket webBucket, Role role)
        {
            var category = categories.AddResource("{category_name}");

            var integration = new AwsIntegration(new AwsIntegrationProps
            {
                Service = "s3",
                IntegrationHttpMethod = "GET",
                Path = $"{webBucket.BucketName}?prefix={{category_name}}",
                Options = new IntegrationOptions
                {
                    CredentialsRole = role,
                    PassthroughBehavior = PassthroughBehavior.WHEN_NO_TEMPLATES,
                    RequestParameters = new Dictionary<string, string>
                    {
                        { "integration.request.path.category_name", "method.request.path.category_name" },
                    },
                    IntegrationResponses = new IntegrationResponse[]
                    {
                        new IntegrationResponse
                        {
                            StatusCode = "200",
                            ResponseTemplates = new Dictionary<string, string>
                            {
                                { "application/json", "{\"success\":\"true\"}" }
                            }
                        }
                    }
                }
            });

            category.AddMethod("GET", integration, new MethodOptions
            {
                RequestParameters = new Dictionary<string, bool>
                {
                    { "method.request.path.category_name", true },
                },
                MethodResponses = new MethodResponse[]
                {
                    new MethodResponse
                    {
                        StatusCode = "200",
                        ResponseParameters = new Dictionary<string, bool>
                        {
                            { "method.response.header.Content-Type", true }
                        }
                    }
                }
            });
        }

        private Role CreateRole(ApiConstructProps props)
        {
            var role = new Role(this, "apigateway-to-s3", new RoleProps
            {
                AssumedBy = new ServicePrincipal("apigateway.amazonaws.com")
            });
            role.AddToPolicy(new PolicyStatement(new PolicyStatementProps
            {
                Resources = new[] { $"{props.UploadBucket.BucketArn}/*" },
                Actions = new[] { "s3:PutObject" }
            }));
            role.AddToPolicy(new PolicyStatement(new PolicyStatementProps
            {
                Resources = new[] { props.WebBucket.BucketArn, $"{props.WebBucket.BucketArn}/*" },
                Actions = new[] { "s3:ListBucket" }
            }));
            return role;
        }
    }
}
