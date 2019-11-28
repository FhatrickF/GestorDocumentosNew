using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using GestorDocumentosEntities;
using GestorDocumentosExceptions;
using mvc4.Business;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace mvc4.Business
{
    public class SolrBO
    {
        private static string URL_SOLR = WebConfigurationManager.AppSettings["webSolr"] + "/solr/test-2/update?commitWithin=1000&overwrite=true&wt=json"; 
        private static string directorio_ma = WebConfigurationManager.AppSettings["MVC-DATA-MA"];
        private static string SOLR_URL = WebConfigurationManager.AppSettings["SOLR-URL"];
        private static string SOLR_CORE = WebConfigurationManager.AppSettings["SOLR-CORE"];
        public static string SolrSelect(string texto, int pagina)
        {
            String urlAddress = "";
            string urlSolr = WebConfigurationManager.AppSettings["webSolr"];
            if (texto == "")
                urlAddress = urlSolr + "/solr/test-1/select?hl.fl=ma_texto&hl=on&q=*%3A*&rows=5&start=0";
            else
                urlAddress = urlSolr + "/solr/test-1/select?fl=id%2Cma_iddocumento%2Cma_categoria%2Cma_norma%2Cma_numero%2C%20ma_organismo&hl.fl=ma_texto&hl.simple.post=%3C%2Flabel%3E&hl.simple.pre=%3Clabel%20style%3D%22background-color%3A%20yellow%22%3E&hl=on&q=ma_texto%3A\"" + texto + "\"&rows=5&start=" + pagina + "&wt=json";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
            HttpWebResponse response;
            response = (HttpWebResponse)request.GetResponse();

            string responseStr = "";
            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream responseStream = response.GetResponseStream();
                responseStr = new StreamReader(responseStream).ReadToEnd();
            }
            return responseStr;
        }

        public static bool SolrAdd(string xmlData)
        {
            xmlData = xmlData.Replace("sgd_documento", "SolrXml");
            SolrXml solrXml = new SolrXml();
            solrXml = (SolrXml)FileBo.DeserializeXML(solrXml.GetType(), xmlData);

            string tagPattern = @"<[!--\W*?]*?[/]*?\w+.*?>";
            MatchCollection matches = Regex.Matches(solrXml.Texto, tagPattern);
            foreach (Match match in matches)
            {
                solrXml.Texto = solrXml.Texto.Replace(match.Value, string.Empty).Replace("\n", "");
            }

            string texto = HttpUtility.HtmlDecode(solrXml.Texto);
            string xml = "<add><doc>";
            xml += "<field name=\"id\">" + solrXml.IdDocumento + "</field>";
            xml += "<field name=\"IdDocumento\">" + solrXml.IdDocumento + "</field>";
            xml += "<field name=\"Titulo\">" + solrXml.Titulo + "</field>";
            xml += "<field name=\"Descripcion\">" + solrXml.Descripcion + "</field>";
            xml += "<field name=\"FechaCreacion\">" + solrXml.FechaCreacion + "</field>";
            xml += "<field name=\"Version\">" + solrXml.Version + "</field>";
            xml += "<field name=\"Texto\">" + texto + "</field>";
            xml += "</doc></add>";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL_SOLR);
            byte[] bytes;
            bytes = System.Text.Encoding.UTF8.GetBytes(xml);
            request.ContentType = "text/xml; encoding='utf-8'";
            request.ContentLength = bytes.Length;
            request.Method = "POST";
            Stream requestStream = request.GetRequestStream();
            requestStream.Write(bytes, 0, bytes.Length);
            requestStream.Close();
            HttpWebResponse response;
            response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream responseStream = response.GetResponseStream();
                string responseStr = new StreamReader(responseStream).ReadToEnd();
            }

            return true;
        }

        public static string SolrQueryById(string id)
        {
            string urlSolr = WebConfigurationManager.AppSettings["webSolr"];
            string url = urlSolr + "/ solr/test-1/select?q=id%3A" + id;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response;
            response = (HttpWebResponse)request.GetResponse();

            string responseStr = "";
            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream responseStream = response.GetResponseStream();
                responseStr = new StreamReader(responseStream).ReadToEnd();
            }
            return responseStr;
        }

        public static sgd_documentoEntity getDocumentoById(string id)
        {
            try
            {
                string urlSolr = WebConfigurationManager.AppSettings["SOLR-URL"];
                string query = "select?q=id%3A" + id + "%20OR%20IdDocumento%3A" + id;
                sgd_documentoEntity doc = new sgd_documentoEntity();
                string response = getResponseSolr(query);

                if (response != "")
                {
                    var expConverter = new ExpandoObjectConverter();
                    dynamic obj = JsonConvert.DeserializeObject<ExpandoObject>(response, expConverter);
                    string idDocumento = "";
                    string Norma = "";
                    string xml = "";
                    foreach (var doc_ in obj.response.docs)
                    {
                        idDocumento = doc_.IdDocumento;
                        Norma = (doc_.Norma).Replace(" ", "_") + "\\";
                    }
                    string ruta = directorio_ma + Norma + idDocumento + ".xml";
                    if (System.IO.File.Exists(ruta))
                    {
                        ruta = directorio_ma + Norma + idDocumento + ".xml";
                        xml = System.IO.File.ReadAllText(ruta);
                        doc = (sgd_documentoEntity)FileBo.DeserializeXML(doc.GetType(), xml);
                    }
                    else
                    {
                        doc = null;
                    }
                }
                else
                {
                    doc = null;
                }

                return doc;
            }
            catch (BusinessException bx)
            {
                throw bx;
            }
            catch (Exception ex)
            {
                new TechnicalException("Error al buscar docuemnto por id en el sorl Method getDocumentoById", ex);
                throw new BusinessException("No es posible buscar el documento por id, volver a intentar más tarde.");
            }
        }

        public static string SolrQuery(string query)
        {
            try
            {
                return getResponseSolr(query);
            }
            catch (BusinessException bx)
            {
                throw bx;
            }
            catch (Exception ex)
            {
                new TechnicalException("Error al buscar docuemnto por id en el sorl Method SolrQuery", ex);
                throw new BusinessException("No es posible buscar el documento por query, volver a intentar más tarde.");
            }
        }

        private static string getResponseSolr(string query)
        {
            try
            {
                query = query.Replace(",", "%2C").Replace(" ", "%20").Replace(":", "%3A").Replace("'", "%22");
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(SOLR_URL + SOLR_CORE + query);
                HttpWebResponse response;
                response = (HttpWebResponse)request.GetResponse();

                string responseStr = "";
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Stream responseStream = response.GetResponseStream();
                    responseStr = new StreamReader(responseStream).ReadToEnd();
                }
                return responseStr;
            }
            catch (Exception ex)
            {
                new TechnicalException("Error al buscar docuemnto en el sorl Method getResponseSolr", ex);
                throw new BusinessException("Error, por favor comunicarse con el administrador.");
            }
        }

        public static bool sendXmlDocumento(sgd_documentoEntity documento, bool nuevo)
        {
            try
            {
                documento.Texto = Regex.Replace(documento.Texto, "[<].*?>", " ");
                documento.Texto = Regex.Replace(documento.Texto, @"\s+", " ");

                documento.Texto = DecodeHtmlText(documento.Texto);

                string xml = "";

                xml = "<add><doc>";
                if (!nuevo)
                {
                    xml += "<field name=\"id\">" + documento.id + "</field>";
                }
                xml += "<field name=\"IdDocumento\">" + documento.IdDocumento + "</field>";
                xml += "<field name=\"Orden\">" + documento.Orden + "</field>";
                xml += "<field name=\"Coleccion\">" + documento.Coleccion + "</field>";
                xml += "<field name=\"Anio\">" + documento.Anio + "</field>";
                xml += "<field name=\"Apendice\">" + documento.Apendice + "</field>";
                xml += "<field name=\"AplicaNorma\">" + documento.AplicaNorma + "</field>";
                xml += "<field name=\"Articulo\">" + documento.Articulo + "</field>";
                xml += "<field name=\"AplicaArticulo\">" + documento.AplicaArticulo + "</field>";
                xml += "<field name=\"Categoria\">" + documento.Categoria + "</field>";
                xml += "<field name=\"Comentario\">" + documento.Comentario + "</field>";
                xml += "<field name=\"Cve\">" + documento.Cve + "</field>";
                xml += "<field name=\"Fecha\">" + documento.Fecha + "</field>";
                xml += "<field name=\"Iddo\">" + documento.Iddo + "</field>";
                xml += "<field name=\"IdRep\">" + documento.IdRep + "</field>";
                xml += "<field name=\"Inciso\">" + documento.Inciso + "</field>";
                xml += "<field name=\"Minred\">" + documento.Minred + "</field>";
                xml += "<field name=\"Nompop\">" + documento.Nompop + "</field>";
                xml += "<field name=\"Norma\">" + documento.Norma + "</field>";
                xml += "<field name=\"Numero\">" + documento.Numero + "</field>";
                xml += "<field name=\"Organismo\">" + documento.Organismo + "</field>";
                xml += "<field name=\"OrgansimoUno\">" + documento.OrganismoUno + "</field>";
                xml += "<field name=\"Regco\">" + documento.Regco + "</field>";
                xml += "<field name=\"Resuel\">" + documento.Resuel + "</field>";
                xml += "<field name=\"Rol\">" + documento.Rol + "</field>";
                xml += "<field name=\"Seccion\">" + documento.Seccion + "</field>";
                xml += "<field name=\"Suborganismo\">" + documento.Suborganismo + "</field>";
                xml += "<field name=\"Tema\">" + documento.Tema + "</field>";
                xml += "<field name=\"Temas\">" + documento.Temas + "</field>";
                xml += "<field name=\"Titulo\">" + documento.Titulo + "</field>";
                xml += "<field name=\"Texto\">" + documento.Texto + "</field>";
                xml += "</doc></add>";

                if (sendDocument(xml))
                    return true;
                else
                    return false;
            }
            catch (BusinessException bx)
            {
                throw bx;
            }
            catch (Exception ex)
            {
                new TechnicalException("Error al indexar docuemnto en el sorl Method sendXmlDocumento", ex);
                throw new BusinessException("No es posible indexar el documento, volver a intentar más tarde.");
            }
        }

        private static bool sendDocument(string xml)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(SOLR_URL + SOLR_CORE + "update?commitWithin=1000&overwrite=true&wt=json");
                byte[] bytes;
                bytes = System.Text.Encoding.UTF8.GetBytes(xml);
                request.ContentType = "text/xml; encoding='utf-8'";
                request.ContentLength = bytes.Length;
                request.Method = "POST";
                Stream requestStream = request.GetRequestStream();
                requestStream.Write(bytes, 0, bytes.Length);
                requestStream.Close();
                HttpWebResponse response;
                response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Stream responseStream = response.GetResponseStream();
                    string responseStr = new StreamReader(responseStream).ReadToEnd();
                    return true;
                }
            }
            catch (Exception ex)
            {
                new TechnicalException("Error al indexar, por favor verificar servicio solr.",ex);
                return false;
            }
            return false;
        }
        private static string DecodeHtmlText(string texto)
        {
            StringWriter myWriter = new StringWriter();
            HttpUtility.HtmlDecode(texto, myWriter);
            return myWriter.ToString();
        }

        public static string getUrlDocumentById(string id)
        {
            try
            {
                string urlSolr = WebConfigurationManager.AppSettings["webSolr"];
                string url = SOLR_URL + SOLR_CORE + "select?q=id%3A" + id + "%20OR%20IdDocumento%3A" + id;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                HttpWebResponse response;
                response = (HttpWebResponse)request.GetResponse();

                string responseStr = "";
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Stream responseStream = response.GetResponseStream();
                    responseStr = new StreamReader(responseStream).ReadToEnd();

                    var expConverter = new ExpandoObjectConverter();
                    dynamic obj = JsonConvert.DeserializeObject<ExpandoObject>(responseStr, expConverter);

                    string idDocumento = "";
                    string Norma = "";
                    foreach (var doc in obj.response.docs)
                    {
                        idDocumento = doc.IdDocumento;
                        Norma = (doc.Norma).Replace(" ", "_") + "\\";
                    }
                    if (System.IO.File.Exists(directorio_ma + Norma + idDocumento + ".xml"))
                        responseStr = directorio_ma + Norma + idDocumento + ".xml";
                    else
                        responseStr = "";
                }
                return responseStr;
            }
            catch (Exception ex)
            {
                new TechnicalException("Error al buscar por url, por favor verificar servicio solr.", ex);
                throw new BusinessException("Error al buscar documento, por favor volver a intentar");
            }
        }
    }
}
