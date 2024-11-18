using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.Configuration;
using WebApplication2.Models;
using WebApplication2.Soap.Object;

namespace WebApplication2.Controllers;

[Produces("application/xml")]
[ApiController]
[Route("{bucketName}")]
public class ObjectController : ControllerBase
{
    [HttpGet("")]
    public IActionResult ListObjects(string bucketName)
    {
        try
        {
            var objects = App.ObjectsResponse;
            objects.Name = bucketName;
            return Ok(objects);
        }
        catch (AmazonS3Exception e)
        {
            return StatusCode(500, new { error = e.Message });
        }
    }

    [HttpPut("{fileKey}")]
    public async Task<IActionResult> UploadObject(string bucketName, string fileKey, [FromQuery] string? uploadId,
        [FromQuery] int? partNumber)
    {
        IActionResult result;
        if (uploadId == null && partNumber == null)
        {
            result = await UploadSmallObject(bucketName, fileKey);
        }
        else
        {
            result = await UploadLargeObject(uploadId, partNumber);
        }

        return Ok(result);
    }


    [HttpPost("{fileKey}")]
    public async Task<IActionResult> MultipartUpload(string bucketName, string fileKey, [FromQuery] string? uploads,
        [FromQuery] string? uploadId)
    {
        if (uploadId == null)
        {
            return await InitiateMultipartUpload(bucketName, fileKey);
        }

        return await CompleteMultipartUpload(bucketName, fileKey, uploadId);
    }


    [HttpGet("{fileKey}")] // Download an object
    public async Task<IActionResult> DownloadObject(string bucketName, string fileKey)
    {
        var s3Client = S3Configuration.GetS3Client();

        try
        {
            var response = await s3Client.GetObjectAsync(bucketName, fileKey);
            return File(response.ResponseStream, response.Headers.ContentType, fileKey);
        }
        catch (AmazonS3Exception e)
        {
            return StatusCode(500, new { error = e.Message });
        }
    }

    [HttpDelete("{fileKey}")] // Delete an object
    public async Task<IActionResult> DeleteObject(string bucketName, string fileKey)
    {
        try
        {
            var a = App.ObjectsResponse.Contents;
            return Ok();
        }
        catch (AmazonS3Exception e)
        {
            return StatusCode(500, new { error = e.Message });
        }
    }

    private async Task<IActionResult> UploadSmallObject(string bucketName, string fileKey)
    {
        byte[] binaryData = [];
        using (var memoryStream = new MemoryStream())
        {
            await Request.Body.CopyToAsync(memoryStream);
            binaryData = memoryStream.ToArray();
        }

        AddToS3(bucketName, fileKey, binaryData.Length);
        return Ok();
    }

    private static void AddToS3(string bucketName, string fileKey, long fileSize)
    {
        var newObject = new Content
        {
            Key = fileKey.Split("[\\/]").Last(),
            LastModified = DateTime.UtcNow,
            Size = fileSize
        };
        App.ObjectsResponse.Contents.Add(newObject);
        App.ObjectsResponse.Name = bucketName;
    }

    private async Task<IActionResult> UploadLargeObject(string? uploadId, int? partNumber)
    {
        var filePath = Path.Combine("TempUploads", $"{uploadId}_part_{partNumber}");
        await using (var stream = new FileStream(filePath, FileMode.OpenOrCreate))
        {
            await Request.Body.CopyToAsync(stream);
        }

        return Ok();
    }

    private async Task<IActionResult> InitiateMultipartUpload(string bucketName, string fileKey)
    {
        var uploadId = Guid.NewGuid().ToString();

        // Respond with the upload ID (mimicking S3's response)
        var result = new InitiateMultipartUploadResultDto
        {
            Bucket = bucketName,
            Key = fileKey,
            UploadId = uploadId
        };
        return Ok(result);
    }

    private async Task<IActionResult> CompleteMultipartUpload(string bucketName, string fileKey, string? uploadId)
    {
        var finalFilePath = Path.Combine("Uploads", fileKey);
        await using (var finalFile = new FileStream(finalFilePath, FileMode.Create))
        {
            var partNumber = 1;
            while (true)
            {
                var partPath = Path.Combine("TempUploads", $"{uploadId}_part_{partNumber}");
                if (!System.IO.File.Exists(partPath))
                {
                    break;
                }

                await using (var partStream = new FileStream(partPath, FileMode.Open))
                {
                    await partStream.CopyToAsync(finalFile);
                }

                System.IO.File.Delete(partPath);
                partNumber++;
            }
        }

        var result = new CompleteMultipartUploadResultDto
        {
            Location = $"/{bucketName}/{fileKey}",
            Bucket = bucketName,
            Key = fileKey,
            ETag = "fab8b49e4b17a0172c6ff5f52cd815b-2" // Example ETag
        };

        AddToS3(bucketName, fileKey, new FileInfo(finalFilePath).Length);
        return Ok(result);
    }
}