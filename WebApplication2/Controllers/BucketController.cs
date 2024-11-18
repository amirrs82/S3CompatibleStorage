using System.Net;
using Amazon.S3;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.Configuration;
using WebApplication2.Models;
using WebApplication2.Soap.Bucket;

namespace WebApplication2.Controllers;

[Produces("application/xml")]
[Route("")]
[ApiController]
public class BucketController : ControllerBase
{
    private readonly string storagePath = "YourStoragePath"; // Replace with your desired path

    [HttpPut("{bucketName}")]
    public async Task<IActionResult> CreateBucket(string bucketName)
    {
        var bucket = new Bucket
        {
            CreationDate = DateTime.UtcNow,
            Name = bucketName,
        };
        try
        {
            App.BucketsResponse.Buckets.Add(bucket);
            App.BucketsResponse.Owner = App.DefaultOwner;
            return Ok();
        }
        catch (AmazonS3Exception e)
        {
            return StatusCode(500, new { error = e.Message });
        }
    }


    [HttpDelete("{bucketName}")]
    public async Task<IActionResult> DeleteBucket(string bucketName)
    {
        var s3Client = S3Configuration.GetS3Client();

        try
        {
            await s3Client.DeleteBucketAsync(bucketName);
            return Ok(new { message = $"Bucket '{bucketName}' deleted successfully." });
        }
        catch (AmazonS3Exception e)
        {
            return StatusCode(500, new { error = e.Message });
        }
    }

    [HttpGet]
    public IActionResult ListBuckets()
    {
        try
        {
            var buckets = App.BucketsResponse;
            return Ok(buckets);
        }
        catch (AmazonS3Exception e)
        {
            return StatusCode(500, new { error = e.Message });
        }
    }
}