using System.Xml.Serialization;

public class FileMetadataDto
{
    [XmlElement("ETag")] public string ETag { get; set; } // The entity tag (ETag) of the file
    [XmlElement("LastModified")] public DateTime LastModified { get; set; } // The last modified date of the file
}