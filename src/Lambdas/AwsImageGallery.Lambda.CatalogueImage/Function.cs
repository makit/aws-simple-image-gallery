using Amazon.Lambda.S3Events;
using Amazon.S3.Model;
using Amazon.S3;
using System.Text.Json;

namespace AwsImageGallery.Lambda.CatalogueImage;

public class Function
{
    private readonly string _webBucket;
    private readonly AmazonS3Client _client;

    public Function()
    {
        var webBucket = Environment.GetEnvironmentVariable("WebBucket");
        if (webBucket is null)
        {
            throw new Exception("Missing WebBucket Variable");
        }

        _webBucket = webBucket;
        _client = new AmazonS3Client();
    }

    public async Task<bool> FunctionHandler(S3Event input)
    {
        Console.WriteLine("Objects added to bucket {0}", JsonSerializer.Serialize(input));

        var catalogueFolder = DateTime.UtcNow.ToString("yyyy-MM-dd");

        foreach (var record in input.Records)
        {
            var sourceBucket = record.S3.Bucket.Name;
            var objectKey = record.S3.Object.Key;

            var response = await CopyingObjectAsync(sourceBucket, objectKey, catalogueFolder);
            if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                Console.WriteLine("Copy complete for {0}", objectKey);

                var deleteResult = await _client.DeleteObjectAsync(new DeleteObjectRequest
                {
                    BucketName = sourceBucket,
                    Key = objectKey
                });

                Console.WriteLine("Delete Result: {0}", JsonSerializer.Serialize(deleteResult));
            }
        }

        return true;
    }

    private async Task<CopyObjectResponse> CopyingObjectAsync(string sourceBucketName, string objectKey, string prefix)
    {
        var response = new CopyObjectResponse();
        try
        {
            var request = new CopyObjectRequest
            {
                SourceBucket = sourceBucketName,
                SourceKey = objectKey,
                DestinationBucket = _webBucket,
                DestinationKey = $"{prefix}/{objectKey}",
            };
            response = await _client.CopyObjectAsync(request);
        }
        catch (AmazonS3Exception ex)
        {
            Console.WriteLine("Error copying object: {0}", ex.Message);
        }

        return response;
    }
}
