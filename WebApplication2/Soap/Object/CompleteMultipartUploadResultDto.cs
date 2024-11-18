using System.Xml.Serialization;

[XmlRoot("CompleteMultipartUploadResult", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
public class CompleteMultipartUploadResultDto
{
    [XmlElement("Location")]
    public string Location { get; set; }

    [XmlElement("Bucket")]
    public string Bucket { get; set; }

    [XmlElement("Key")]
    public string Key { get; set; }

    [XmlElement("ETag")]
    public string ETag { get; set; }
}