using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.S3.Model;
using Amazon.S3;
using System.Text.Json;

namespace AwsImageGallery.Lambda.ListImages;

public class ListImagesFunction
{
    private readonly string _bucket;
    private readonly AmazonS3Client _client;

    public ListImagesFunction()
    {
        var bucket = Environment.GetEnvironmentVariable("Bucket");
        if (bucket is null)
        {
            throw new Exception("Missing Bucket Variable");
        }

        _bucket = bucket;
        _client = new AmazonS3Client();
    }

    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest input)
    {
        Console.WriteLine("Reading list of objects from S3: {0}", JsonSerializer.Serialize(input));

        var prefix = $"{input.PathParameters["category_name"]}/";

        var request = new ListObjectsV2Request
        {
            BucketName = _bucket,
            Prefix = prefix
        };

        var files = new List<string>();
        ListObjectsV2Response response;
        do
        {
            response = await _client.ListObjectsV2Async(request);
            Console.WriteLine("Response: {0}", JsonSerializer.Serialize(response));

            response.S3Objects.ForEach(o =>
            {
                if (!o.Key.Equals(prefix))
                {
                    files.Add(o.Key[prefix.Length..]);
                }
            });

            request.ContinuationToken = response.NextContinuationToken;

        } while (response.IsTruncated);

        Console.WriteLine("Found {0} images", files.Count);

        return new APIGatewayProxyResponse
        {
            Body = JsonSerializer.Serialize(files),
            StatusCode = 200
        };
    }
}
