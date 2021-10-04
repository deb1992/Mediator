using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Web.Infrastructure.DynamicValidationHelper;
using System.Collections.Specialized;
using GenericMediator.Globals;
using clsPinGeneratorComponent;
using clsPinGeneratorComponent_Ecal;

namespace GenericMediator.Controllers
{
    public class MediatorController : ApiController
    {
        #region Global Variables
        private const HttpStatusCode oK = HttpStatusCode.OK;
        private const HttpStatusCode iSE = HttpStatusCode.InternalServerError;
        #endregion

        #region Web Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="MediatorCode"></param>
        /// <param name="throwException"></param>
        /// <returns></returns>
        [System.Web.Http.AllowAnonymous]
        [System.Web.Http.HttpPost]
        [System.Web.Http.HttpGet]
        [System.Web.Http.ActionName("Mediate")]
        [ValidateInput(false)]
        public HttpResponseMessage Mediate(string MediatorCode)
        {
            #region Action
            try
            {
                XmlIntegration xmlbll = new XmlIntegration();
                List<Integration> integrations = xmlbll.ReadXMLIntegrationFile().Where(o => Convert.ToBoolean(o.IsActive) && o.Code == MediatorCode).ToList();
                var context = HttpContext.Current;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

                foreach (var item in integrations)
                {
                    try
                    {
                        HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(item.ApiAddress.ToString());
                        httpRequest.Method = HttpContext.Current.Request.HttpMethod;
                        httpRequest.ContentType = item.ContentType;
                        httpRequest.ContentLength = 0;
                        httpRequest.Timeout = item.Timeout;
                        if (item.SOAPAction != "")
                            httpRequest.Headers.Add("SOAPAction", item.SOAPAction);
                        if (item.Auth == "True")
                        {
                            if (item.AuthType == "Basic")
                            {
                                var username = item.Username;
                                var password = item.Password;
                                var bytes = Encoding.UTF8.GetBytes($"{username}:{password}");
                                httpRequest.Headers.Add("Authorization", $"Basic {Convert.ToBase64String(bytes)}");
                            }
                            if (item.AuthType == "Bearer")
                            {
                                httpRequest.Headers.Add("Authorization", $"Bearer {item.AuthToken}");
                            }
                        }
                        if ((item.Param != "") && (HttpContext.Current.Request.HttpMethod == "POST"))
                        {
                            Func<NameValueCollection> formGetter;
                            Func<NameValueCollection> queryStringGetter;
                            ValidationUtility.GetUnvalidatedCollections(HttpContext.Current, out formGetter, out queryStringGetter);
                            var form = formGetter();
                            var xmlBytes = Encoding.ASCII.GetBytes(form[item.Param]);

                            httpRequest.ContentLength = xmlBytes.Length;
                            using (Stream reqStream = httpRequest.GetRequestStream())
                            {
                                reqStream.Write(xmlBytes, 0, xmlBytes.Count());
                            }
                        }
                        HttpResponseMessage response = new HttpResponseMessage(oK);
                        HttpWebResponse httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                        response.Content = new StreamContent(httpResponse.GetResponseStream());
                        response.StatusCode = httpResponse.StatusCode;
                        if (httpResponse.ContentType.IndexOf(";").Equals(0))
                        {
                            response.Content.Headers.ContentType = new MediaTypeHeaderValue(httpResponse.ContentType);
                        }
                        else
                        {
                            response.Content.Headers.ContentType = new MediaTypeHeaderValue(httpResponse.ContentType.Substring(0, httpResponse.ContentType.IndexOf(";")));
                        }
                        return response;
                    }
                    catch (Exception e)
                    {
                        byte[] byteArray = Encoding.ASCII.GetBytes(e.Message);
                        MemoryStream stream = new MemoryStream(byteArray);
                        HttpResponseMessage response = new HttpResponseMessage(oK);
                        response.Content = new StreamContent(stream);
                        response.StatusCode = iSE;
                        response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
                        return response;
                    }
                }
            }
            catch (Exception e)
            {
                byte[] byteArray = Encoding.ASCII.GetBytes(e.Message);
                MemoryStream stream = new MemoryStream(byteArray);
                HttpResponseMessage response = new HttpResponseMessage(oK);
                response.Content = new StreamContent(stream);
                response.StatusCode = iSE;
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
                return response;
            }
            return null;
            #endregion
        }

        /// <summary>
        /// This GET Resource is specifically for Pin Generator
        /// </summary>
        /// <param name="TokenNo"></param>
        /// <param name="WorkOrderNo"></param>
        /// <param name="PadSerialNo"></param>
        /// <param name="FSEPwd"></param>
        /// <returns></returns>
        [System.Web.Http.AllowAnonymous]
        [System.Web.Http.HttpGet]
        [System.Web.Http.ActionName("PinGenerator")]
        [ValidateInput(false)]
        public HttpResponseMessage PinGenerator(string TokenNo = null, string WorkOrderNo = null, string PadSerialNo = null, string FSEPwd = null)
        {
            #region Local Variables
            string ret = "";
            string returnMessage = "";
            string sToken = TokenNo;
            string sWorkOrderNo = WorkOrderNo;
            string sPadSerialNo = PadSerialNo;
            string sFSEPwd = FSEPwd;
            string sDynamicPin = "";
            byte bRetStatus = new byte();
            byte bSchemaNo = new byte();
            #endregion

            #region Action
            try
            {
                ret = PinGenerator(sToken, sWorkOrderNo, sPadSerialNo, sFSEPwd, ref sDynamicPin, ref bRetStatus, ref bSchemaNo);
                returnMessage = "STATUS_CODE" + Convert.ToChar(31) + bRetStatus.ToString() + Convert.ToChar(30) + "STATUS_MESSAGE" + Convert.ToChar(31) + ret + Convert.ToChar(30) + "PIN" + Convert.ToChar(31) + sDynamicPin + Convert.ToChar(30);
                byte[] byteArray = Encoding.ASCII.GetBytes(returnMessage);
                MemoryStream stream = new MemoryStream(byteArray);
                HttpResponseMessage response = new HttpResponseMessage(oK);
                response.Content = new StreamContent(stream);
                response.StatusCode = oK;
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
                return response;
            }
            catch (Exception e)
            {
                byte[] byteArray = Encoding.ASCII.GetBytes(e.Message);
                MemoryStream stream = new MemoryStream(byteArray);
                HttpResponseMessage response = new HttpResponseMessage(oK);
                response.Content = new StreamContent(stream);
                response.StatusCode = iSE;
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
                return response;
            }
            #endregion
        }


        #endregion

        #region Methods
        public string PinGenerator(string sToken, string sWorkOrderNo, string sPadSerialNo, string sFSEPwd, ref string sDynamicPin, ref byte bRetStatus, ref byte bSchemaNo)
        {
            #region Local Variables
            string ret = "";
            #endregion

            #region Action
            Class1 PinGeneratorObject = new Class1();
            Ecal PinGeneratorObject_Ecal = new Ecal();
            if (sToken.Length == 18)
            {
                ret = PinGeneratorObject.fnGeneratePinforToken(sToken, sWorkOrderNo, sPadSerialNo, sFSEPwd, ref sDynamicPin, ref bRetStatus, ref bSchemaNo);
            }
            else if (sToken.Length == 20)
            {
                ret = PinGeneratorObject_Ecal.fnGeneratePinForDynamicOTP(sToken, sWorkOrderNo, sPadSerialNo, sFSEPwd, ref sDynamicPin, ref bRetStatus, ref bSchemaNo);
            }
            else
            {
                ret = "Token Length Mismatch";
            }
            return ret;
            #endregion
        }
        #endregion

    }
}
