using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.IAM;
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
                BinaryMediaTypes = new[] { "*/*" }, // TODO: Check - application/octet-stream", "image/jpeg
                MinimumCompressionSize = 0, // TODO: Check
            });
            AddUploadEndpoint(api, props.UploadBucket, role);

            var categories = api.Root.AddResource("categories");
            categories.AddMethod("GET"); // Will return a list of image categories

            var category = categories.AddResource("{category_name}");
            category.AddMethod("GET"); // Will return a list of images in the given category
        }

        private static void AddUploadEndpoint(RestApi api, IBucket uploadBucket, Role role)
        {
            var imageResource = api.Root.AddResource("image");
            var fileNameResource = imageResource.AddResource("{filename}");

            var uploadImageIntegration = new AwsIntegration(new AwsIntegrationProps
            {
                Service = "s3",
                IntegrationHttpMethod = "PUT",
                Path = $"{uploadBucket.BucketName}/{{filename}}",
                Options = new IntegrationOptions
                {
                    CredentialsRole = role,
                    PassthroughBehavior = PassthroughBehavior.WHEN_NO_TEMPLATES, // TODO: Check
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
                            ResponseParameters = new Dictionary<string, string>
                            {
                                { "method.response.header.Content-Type", "integration.response.header.Content-Type" }
                            }
                        }
                    }
                }
            });

            fileNameResource.AddMethod("POST", uploadImageIntegration, new MethodOptions
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
