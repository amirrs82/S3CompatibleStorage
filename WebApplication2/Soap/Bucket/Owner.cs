using System.Xml.Serialization;

namespace WebApplication2.Soap.Bucket;

public class Owner
{
    [XmlElement("ID")] public string ID { get; set; }

    [XmlElement("DisplayName")] public string DisplayName { get; set; }
}

