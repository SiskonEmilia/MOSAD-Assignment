using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace InternetApplication
{
    class ResponsXML
    {
        private static XmlDocument _source = new XmlDocument();
        private static XmlNode node;

        private ResponsXML() { }

        public static void SetSource(string source) {
            _source.LoadXml(source);
            node = _source.SelectSingleNode("/query");
        }

        public static string GetField (string field)
        {
            return node.SelectSingleNode(field).InnerText;
        }
    }
}
