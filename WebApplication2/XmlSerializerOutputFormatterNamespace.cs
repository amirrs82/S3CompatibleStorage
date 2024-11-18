using System.Xml;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace WebApplication2;

public class XmlSerializerOutputFormatterNamespace : XmlSerializerOutputFormatter
{
    protected override void Serialize(XmlSerializer xmlSerializer, XmlWriter xmlWriter, object value)
    {
        //applying "empty" namespace will produce no namespaces
        var emptyNamespaces = new XmlSerializerNamespaces();
        emptyNamespaces.Add("", "");
        xmlSerializer.Serialize(xmlWriter, value, emptyNamespaces);
    }
}