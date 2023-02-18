using Amazon.CDK.AWS.S3;

namespace AwsImageGallery.Constructs
{
    internal struct ApiConstructProps
    {
        internal IBucket UploadBucket { get; set; }
    }
}
