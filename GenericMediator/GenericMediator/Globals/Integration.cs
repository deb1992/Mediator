using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace GenericMediator.Globals
{
    public class Integration
    {
        #region Attributes
        [XmlAttribute]
        public string IsActive { get; set; }
        [XmlAttribute]
        public string Code { get; set; }
        [XmlAttribute]
        public string ContentType { get; set; }
        [XmlAttribute]
        public string ApiAddress { get; set; }
        [XmlAttribute]
        public string Param { get; set; }
        [XmlAttribute]
        public string Method { get; set; }
        [XmlAttribute]
        public int Timeout { get; set; }
        [XmlAttribute]
        public string Auth { get; set; }
        [XmlAttribute]
        public string AuthType { get; set; }
        [XmlAttribute]
        public string AuthToken { get; set; }
        [XmlAttribute]
        public string Username { get; set; }
        [XmlAttribute]
        public string Password { get; set; }
        [XmlAttribute]
        public string SOAPAction { get; set; }
        [XmlAttribute]
        public string Apikey { get; set; }
        [XmlAttribute]
        public string XApiAccesskey { get; set; }
        #endregion
    }
}