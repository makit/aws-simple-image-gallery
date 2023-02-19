using Amazon.CDK.AWS.S3;
using Constructs;

namespace AwsImageGallery.Constructs
{
    internal class StorageConstruct : Construct
    {
        public IBucket UploadBucket { get; private set; }

        public IBucket WebBucket { get; private set; }

        public StorageConstruct(Construct scope, string id) : base(scope, id)
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
        }
    }
}
