using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.S3.Model;
using Amazon.S3;
using System.Text.Json;

namespace AwsImageGallery.Lambda.ListImages;

public class ListFoldersFunction
{
    private readonly string _bucket;
    private readonly AmazonS3Client _client;

    public ListFoldersFunction()
    {
        var bucket = Environment.GetEnvironmentVariable("Bucket");
        if (bucket is null)
        {
            throw new Exception("Missing Bucket Variable");
        }

        _bucket = bucket;
        _client = new AmazonS3Client();
    }

    public async Task<APIGatewayProxyResponse> FunctionHandler()
    {
        Console.WriteLine("Reading list of objects from S3");

        var request = new ListObjectsV2Request
        {
            BucketName = _bucket,
            Delimiter = "/" // Causes it to only return "folders"
        };

        var response = await _client.ListObjectsV2Async(request);
        Console.WriteLine("Response: {0}", JsonSerializer.Serialize(response));

        var folders = response.CommonPrefixes.Select(r => r.Replace("/", ""));

        return new APIGatewayProxyResponse
        {
            Body = JsonSerializer.Serialize(folders),
            StatusCode = 200
        };
    }
}
