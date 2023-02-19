using Amazon.CDK;
using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.S3;
using Amazon.CDK.AWS.IAM;
using Constructs;
using AwsImageGallery.Constructs;

namespace AwsImageGallery
{
    public class AwsImageGalleryStack : Stack
    {
        internal AwsImageGalleryStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            var storage = new StorageConstruct(this, "storage");

            _ = new ApiConstruct(this, "api", new ApiConstructProps(storage.UploadBucket, storage.WebBucket));
        }
    }
}
