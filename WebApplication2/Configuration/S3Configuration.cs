using Amazon.S3;

namespace WebApplication2.Configuration
{
    public static class S3Configuration
    {
        public static string EndpointUrl = "http://localhost:5209/api";  // e.g., http://localhost:9000 for MinIO
        public static string AccessKey = "admin";
        public static string SecretKey = "!@#123qwe";

        public static AmazonS3Client GetS3Client()
        {
            AmazonS3Config config = new()
            {
                ServiceURL = EndpointUrl
            };
            return new AmazonS3Client(AccessKey, SecretKey, config);
        }
    }
}
