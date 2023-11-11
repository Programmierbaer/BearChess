using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace www.SoLaNoSoft.com.BearChessTools
{
    [XmlRoot("BearChess")]
    public class ConfigurationSettings<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable
    {
        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)

        {
            if (reader.IsEmptyElement)
            {
                return;
            }

            reader.Read();

            while (reader.NodeType != XmlNodeType.EndElement)
            {
                object key = reader.GetAttribute("Title");
                object value = reader.GetAttribute("Value");
                Add((TKey) key, (TValue) value);
                reader.Read();
            }
        }

        public void WriteXml(XmlWriter writer)

        {
            foreach (var key in Keys)
            {
                writer.WriteStartElement("Config");
                writer.WriteAttributeString("Title", key.ToString());
                writer.WriteAttributeString("Value", this[key].ToString());
                writer.WriteEndElement();
            }
        }
    }
}