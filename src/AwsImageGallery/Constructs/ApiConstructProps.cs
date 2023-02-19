using Amazon.CDK.AWS.S3;

namespace AwsImageGallery.Constructs
{
    internal struct ApiConstructProps
    {
        internal IBucket UploadBucket { get; private set; }

        internal IBucket WebBucket { get; private set; }

        internal ApiConstructProps(IBucket uploadBucket, IBucket webBucket)
        {
            UploadBucket = uploadBucket;
            WebBucket = webBucket;
        }
    }
}
