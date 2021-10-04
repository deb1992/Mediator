using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace GenericMediator.Globals
{
    public class XmlIntegration
    {
        #region Action
        public List<Integration> ReadXMLIntegrationFile()
        {
            List<Integration> list = new List<Integration>();
            XmlSerializer ser = new XmlSerializer(typeof(List<Integration>));
            Stream filestream2 = File.OpenRead(HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings["XMLIntegrationFilePath"] + "\\Integrations.xml"));
            list = ser.Deserialize(filestream2) as List<Integration>;
            filestream2.Close();
            return list;
        }
        #endregion
    }
}