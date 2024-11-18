using System.Xml.Serialization;

namespace WebApplication2.Soap.Bucket;

public class Bucket
{
    [XmlElement("Name")] public string Name { get; set; }

    [XmlElement("CreationDate")] public DateTime CreationDate { get; set; }

    // public DateTime CreationDateDateTime => DateTime.Parse(CreationDate, CultureInfo.InvariantCulture);
}
