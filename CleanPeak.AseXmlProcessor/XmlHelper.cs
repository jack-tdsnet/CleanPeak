using System.Reflection;
using System.Xml;

namespace CleanPeak.AseXmlProcessor
{
    public static class XmlHelper
    {
        private static XmlDocument _aseTemplate;

        public static XmlDocument AseTemplate
        {
            get
            {
                if (_aseTemplate != null)
                    return (XmlDocument)_aseTemplate.Clone();

                var template = GetTemplate("AseTemplate.xml");

                if (template == null)
                    throw new Exception("Unable to locate internal resource 'AseTemplate.xml'");

                _aseTemplate = new XmlDocument();
                _aseTemplate.LoadXml(template);

                return (XmlDocument)_aseTemplate.Clone();
            }
        }

        private static string GetTemplate(string name)
        {
            var ass = Assembly.GetExecutingAssembly();

            Stream stream = null;
            StreamReader rdr = null;
            string templateContents;

            try
            {
                stream = ass.GetManifestResourceStream("CleanPeak.AseXmlProcessor." + name);
                if (stream == null)
                    throw new Exception($"Unable to locate internal resource '{name}'");

                rdr = new StreamReader(stream);
                templateContents = rdr.ReadToEnd();
            }
            finally
            {
                rdr?.Close();
                stream?.Close();
            }

            return templateContents;
        }

        public static string SelectSchemaVersion(this XmlDocument xdoc)
        {
            try
            {
                return xdoc.DocumentElement.Attributes["xmlns:ase"].Value.Split(':')[2];
            }
            catch
            {
                return $"r44";
            }
        }

        public static string GetElement(XmlElement parentElement, string tag, bool mandatory = true)
        {
            var node = parentElement.SelectSingleNode(tag);

            if (node != null)
                return node.InnerText;

            if (!mandatory)
                return "";

            throw new Exception($"Element {parentElement.LocalName} is missing child element {tag} ");
        }

        public static DateTime FromXmlDateTime(string timeString)
        {
            string parseFormat;
            var result = new DateTime(1900, 1, 1);
            if (timeString == "9999-12-31T00:00:00+10:00" || timeString == "9999-12-31")
                return new DateTime(9999, 12, 31);

            if (timeString.Length > 10)
            {
                timeString = timeString.Substring(0, 19);
                parseFormat = "yyyy-MM-ddTHH:mm:ss";
            }
            else
            {
                parseFormat = "yyyy-MM-dd";
            }

            if (timeString.Length > 0)
                result = DateTime.ParseExact(timeString, parseFormat, System.Globalization.DateTimeFormatInfo.CurrentInfo);

            return result;
        }

        public static string ToXmlDateTime(DateTime dateTime)
        {
            return XmlConvert.ToString(dateTime, "yyyy-MM-ddTHH:mm:ss") + "+10:00";
        }

        public static void SetNode(ref XmlDocument document, string node, string value)
        {
            var foundNode = document.SelectSingleNode("//" + node);

            if (foundNode == null)
                throw new Exception("could not find node " + node + " in document ");

            foundNode.InnerText = value;
        }
    }
}
