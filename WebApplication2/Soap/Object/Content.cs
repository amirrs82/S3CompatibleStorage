using System.Xml.Serialization;

namespace WebApplication2.Soap.Object;

public class Content
{
    [XmlElement("Key")] public string Key { get; set; }

    [XmlElement("LastModified")] public DateTime LastModified { get; set; }

    [XmlElement("ETag")] public string ETag { get; set; } = "asdlk;jafl";

    [XmlElement("Size")] public long Size { get; set; }

    [XmlElement("StorageClass")] public string StorageClass { get; set; } = "STANDARD";
}