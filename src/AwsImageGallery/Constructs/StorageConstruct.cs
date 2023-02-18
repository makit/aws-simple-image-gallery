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
            UploadBucket = new Bucket(this, "gallery-upload");
            WebBucket = new Bucket(this, "gallery-web");
        }
    }
}
