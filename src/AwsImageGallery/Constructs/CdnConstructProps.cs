using Amazon.CDK.AWS.S3;

namespace AwsImageGallery.Constructs
{
    internal struct CdnConstructProps
    {
        internal IBucket WebBucket { get; private set; }

        internal CdnConstructProps(IBucket webBucket)
        {
            WebBucket = webBucket;
        }
    }
}
