using System.Xml.Serialization;

namespace WebApplication2.Soap.Bucket;

[XmlRoot("ListAllMyBucketsResult", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
public class ListBucketsResponse
{
    [XmlElement("Owner")] public Owner Owner { get; set; }

    [XmlArray("Buckets")]
    [XmlArrayItem("Bucket")]
    public List<Bucket> Buckets { get; set; } = [];
}


