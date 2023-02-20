using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.S3;
using Constructs;
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
            AddListCategoriesEndpoint(categories, props.WebBucket);
            AddListImagesEndpoint(categories, props.WebBucket);
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

        private void AddListCategoriesEndpoint(Resource categories, IBucket webBucket)
        {
            var lambda = CreateLambda("list-folders-lambda", "ListFoldersFunction", webBucket);

            var integration = new LambdaIntegration(lambda);

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

        private void AddListImagesEndpoint(Resource categories, IBucket webBucket)
        {
            var lambda = CreateLambda("list-files-lambda", "ListImagesFunction", webBucket);

            var category = categories.AddResource("{category_name}");

            var integration = new LambdaIntegration(lambda);

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

        private Function CreateLambda(string id, string functionClass, IBucket webBucket)
        {
            var lambda = new Function(this, id, new FunctionProps
            {
                Runtime = Runtime.DOTNET_6,
                Architecture = Architecture.ARM_64,
                Code = Code.FromAsset("./src/Lambdas/AwsImageGallery.Lambda.ListImages/bin/Release/net6.0/linux-arm64/publish"),
                Handler = $"AwsImageGallery.Lambda.ListImages::AwsImageGallery.Lambda.ListImages.{functionClass}::FunctionHandler",
                Environment = new Dictionary<string, string>
                {
                    { "Bucket", webBucket.BucketName }
                },
                Timeout = Amazon.CDK.Duration.Seconds(10)
            });
            lambda.AddToRolePolicy(new PolicyStatement(new PolicyStatementProps
            {
                Resources = new[] { webBucket.BucketArn, $"{webBucket.BucketArn}/*" },
                Actions = new[] { "s3:ListBucket" }
            }));
            return lambda;
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
            return role;
        }
    }
}
