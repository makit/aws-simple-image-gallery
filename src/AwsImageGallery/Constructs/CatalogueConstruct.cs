using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.S3;
using Amazon.CDK.AWS.S3.Deployment;
using Amazon.CDK.AWS.S3.Notifications;
using Constructs;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;

namespace AwsImageGallery.Constructs
{
    internal class CatalogueConstruct : Construct
    {
        public IBucket UploadBucket { get; private set; }

        public IBucket WebBucket { get; private set; }

        public CatalogueConstruct(Construct scope, string id) : base(scope, id)
        {
            // Destroy both buckets as this isn't a production level application
            var bucketProps = new BucketProps
            {
                AutoDeleteObjects = true,
                RemovalPolicy = Amazon.CDK.RemovalPolicy.DESTROY,
                Versioned = false,
            };

            UploadBucket = new Bucket(this, "gallery-upload", bucketProps);
            WebBucket = new Bucket(this, "gallery-web", bucketProps);

            // Deploy the website to the bucket
            _ = new BucketDeployment(this, "bucket-deployment", new BucketDeploymentProps
            {
                DestinationBucket = WebBucket,
                Sources = new []
                {
                    Source.Asset("web/")
                },
                Prune = false,
            });

            var lambda = CreateCatalogueLambda();

            // For every new item then fire the lambda to copy into the catalogue
            UploadBucket.AddEventNotification(EventType.OBJECT_CREATED, new LambdaDestination(lambda));
        }

        private Function CreateCatalogueLambda()
        {
            var lambda = new Function(this, "catalogue-images", new FunctionProps
            {
                Runtime = Runtime.DOTNET_6,
                Architecture = Architecture.ARM_64,
                Code = Code.FromAsset("./src/Lambdas/AwsImageGallery.Lambda.CatalogueImage/bin/Release/net6.0/linux-arm64/publish"),
                Handler = "AwsImageGallery.Lambda.CatalogueImage::AwsImageGallery.Lambda.CatalogueImage.Function::FunctionHandler",
                Environment = new Dictionary<string, string>
                {
                    { "WebBucket", WebBucket.BucketName }
                },
                Timeout = Amazon.CDK.Duration.Seconds(10)
            });
            lambda.AddToRolePolicy(new PolicyStatement(new PolicyStatementProps
            {
                Resources = new[] { $"{WebBucket.BucketArn}/*" },
                Actions = new[] { "s3:PutObject", "s3:PutObjectTagging" }
            }));
            lambda.AddToRolePolicy(new PolicyStatement(new PolicyStatementProps
            {
                Resources = new[] { $"{UploadBucket.BucketArn}/*" },
                Actions = new[] { "s3:DeleteObject", "s3:GetObject", "s3:GetObjectTagging" }
            }));
            return lambda;
        }
    }
}
