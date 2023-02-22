using Amazon.CDK.AWS.CloudFront.Origins;
using Amazon.CDK.AWS.CloudFront;
using Amazon.CDK.AWS.S3;
using Constructs;
using System.Net.Sockets;

namespace AwsImageGallery.Constructs
{
    internal class CDNConstruct : Construct
    {
        public CDNConstruct(Construct scope, string id, CdnConstructProps props) : base(scope, id)
        {
            // First need to allow the OAI to read from the bucket (what CloudFront will use)
            var originAccessIdentity = new OriginAccessIdentity(this, "cdn-to-bucket");
            props.WebBucket.GrantRead(originAccessIdentity);

            // Create the CloudFront distribution
            _ = new Distribution(this, "gallery-distribution", new DistributionProps
            {
                DefaultRootObject = "index.html",
                DefaultBehavior = new BehaviorOptions
                {
                    Origin = new S3Origin(props.WebBucket, new S3OriginProps
                    {
                        OriginAccessIdentity = originAccessIdentity
                    }),
                },
            });
        }
    }
}
