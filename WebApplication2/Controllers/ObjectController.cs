using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc;
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


    [HttpHead("{fileKey}")]
    public async Task<IActionResult> GetObjectMetaData(string bucketName, string fileKey)
    {
        var a = App.ObjectsResponse.Contents.FindAll(objectsResponseContent =>
            objectsResponseContent.Key.Equals(fileKey)).First();
        Response.Headers.ETag = a.ETag;
        Response.Headers.LastModified = a.LastModified.ToString();
        Response.Headers.ContentLength = a.Size;
        return Ok();
    }

    [HttpGet("{fileKey}")]
    public async Task<IActionResult> DownloadObject(string bucketName, string fileKey)
    {
        var filePath = Path.Combine("Uploads", bucketName, fileKey);
        var range = HttpContext.Request.Headers.Range.ToString();
        var (start, end) = ParseRangeHeader(range);

        await using var sourceStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

        var startByte = start ?? 0;
        var endByte = end ?? sourceStream.Length;
        var bytesToRead = endByte - startByte + 1;
        var buffer = new byte[bytesToRead];

        sourceStream.Seek(startByte, SeekOrigin.Begin);
        sourceStream.Read(buffer, 0, (int)bytesToRead);
        return File(buffer, "application/octet-stream", fileKey);
    }

    [HttpDelete("{fileKey}")] // Delete an object
    public async Task<IActionResult> DeleteObject(string bucketName, string fileKey)
    {
        try
        {
            var filePath = Path.Combine("Uploads", bucketName, fileKey);
            System.IO.File.Delete(filePath);
            RemoveFromS3(fileKey);
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
        Directory.CreateDirectory(Path.Combine("Uploads", bucketName));
        var filePath = Path.Combine("Uploads", bucketName, fileKey);
        using (var memoryStream = new MemoryStream())
        {
            await Request.Body.CopyToAsync(memoryStream);
            binaryData = memoryStream.ToArray();
        }

        await System.IO.File.WriteAllBytesAsync(filePath, binaryData); // Requires System.IO
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

    private static void RemoveFromS3(string fileKey)
    {
        App.ObjectsResponse.Contents.RemoveAll(objectsResponseContent => objectsResponseContent.Key.Equals(fileKey));
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
        Directory.CreateDirectory(Path.Combine("Uploads", bucketName));
        var finalFilePath = Path.Combine("Uploads", bucketName, fileKey);
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

    private static (long? startByte, long? endByte) ParseRangeHeader(string range)
    {
        if (string.IsNullOrEmpty(range))
        {
            return (null, null);
        }

        // Range format: "bytes=startByte-endByte"
        if (!range.StartsWith("bytes=")) return (null, null);
        var rangeValues = range[6..]; // Remove "bytes="
        var rangeParts = rangeValues.Split('-'); // Split into start and end part
        long? startByte = null, endByte = null;

        startByte = string.IsNullOrEmpty(rangeParts[0]) ? null : long.Parse(rangeParts[0]);
        endByte = string.IsNullOrEmpty(rangeParts[1]) ? null : long.Parse(rangeParts[1]);

        return (startByte, endByte);
    }
}