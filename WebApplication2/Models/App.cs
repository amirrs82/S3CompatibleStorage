using WebApplication2.Soap.Bucket;
using WebApplication2.Soap.Object;

namespace WebApplication2.Models;

public class App
{
    public static Owner DefaultOwner { get; set; } = new()
    {
        DisplayName = "admin",
        ID = "123"
    };
    public static ListBucketsResponse BucketsResponse { get; set; } = new();
    public static ListObjectsResponse ObjectsResponse { get; set; } = new();
}