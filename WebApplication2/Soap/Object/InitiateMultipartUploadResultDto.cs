using System.Xml.Serialization;

[XmlRoot("InitiateMultipartUploadResult", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
public class InitiateMultipartUploadResultDto
{
    [XmlElement("Bucket")]
    public string Bucket { get; set; }

    [XmlElement("Key")]
    public string Key { get; set; }

    [XmlElement("UploadId")]
    public string UploadId { get; set; }
}