using System.Xml.Serialization;

namespace WebApplication2.Soap.Object;

[XmlRoot("ListBucketResult", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
public class ListObjectsResponse
{
    [XmlElement("Name")] public string Name { get; set; }

    [XmlElement("Prefix")] public string Prefix { get; set; }

    [XmlElement("KeyCount")]
    public int KeyCount
    {
        get => Contents.Count;
        set { }
    }

    [XmlElement("MaxKeys")] public int MaxKeys { get; set; } = 1000;

    [XmlElement("Delimiter")] public string Delimiter { get; set; } = "/";

    [XmlElement("IsTruncated")] public bool IsTruncated { get; set; }
    [XmlElement("Contents")] public List<Content> Contents { get; set; } = [];
}