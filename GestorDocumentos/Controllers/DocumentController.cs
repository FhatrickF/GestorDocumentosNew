using GestorDocumentos.Models.Document;
using GestorDocumentosBusiness;
using GestorDocumentosEntities;
using GestorDocumentosExceptions;
using GestorDocumentosUtilities;
using mvc4.Business;
using mvc4.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;

namespace GestorDocumentos.Controllers
{
    public class DocumentController : Controller
    {
        private string directorio = WebConfigurationManager.AppSettings["MVC-DATA"];
        private string directorio_ma = WebConfigurationManager.AppSettings["MVC-DATA-MA"];
        private static string URL_SOLR = WebConfigurationManager.AppSettings["webSolr"] + @"/solr/test-1/update?commitWithin=1000&overwrite=true&wt=json";

        // GET: Document
        public ActionResult Index()
        {
            var loggin = User.Identity.IsAuthenticated;
            if (!loggin)
                return RedirectToAction("Login", "Account");

            return View();
        }

        public ActionResult Nuevo(string id)
        {
            if (id != null && id != "")
            {
                System.Web.HttpContext.Current.Session["id-doc-referenciaNueva"] = id;
            }

            log_documento log_ = new log_documento();
            log_.idUser = User.Identity.Name;
            log_.idDocumento = id;
            log_.descripcion = "Se crea nuevo documento";

            LogBO.setLogCreateDoc(log_);

            NuevoDocumentViewModel modelo = new NuevoDocumentViewModel();
            return View(modelo);
        }

        [HttpPost]
        public ActionResult Nuevo(NuevoDocumentViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    sgd_documentoEntity nuevo = new sgd_documentoEntity();

                    nuevo.Descripcion = model.Descripcion;
                    nuevo.Titulo = model.Titulo;
                    nuevo.Texto = model.Texto;
                    nuevo.FechaCreacion = DateTime.Now;
                    nuevo.VersionFinal = true;
                    if (!DocumentoBO.setDocumento(nuevo))
                        throw new BusinessException("No se puede guerdar el documento");

                    string xml = FileBo.SerializeXML(nuevo);
                    FileBo.setXmlStringToFile(directorio + nuevo.IdDocumento + ".xml", xml);
                    SolrBO.SolrAdd(xml);

                    return Redirect("~/Document");
                }
                return View(model);
            }
            catch (BusinessException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        public ActionResult Editar(int id)
        {
            NuevoDocumentViewModel model = new NuevoDocumentViewModel();
            DetalleDocumento detalleDocumento = new DetalleDocumento();
            try
            {
                sgd_documentoEntity documento = DocumentoBO.getDocumentoById(id);

                string xml = "";
                if (documento.EsBorrador)
                {
                    xml = System.IO.File.ReadAllText(directorio + documento.IdDocumento + "_borrador.xml");
                }
                else
                {
                    xml = System.IO.File.ReadAllText(directorio + documento.IdDocumento + ".xml");
                }

                documento = (sgd_documentoEntity)FileBo.DeserializeXML(documento.GetType(), xml);
                model.Texto = documento.Texto;
                model.Titulo = documento.Titulo;
                model.Descripcion = documento.Descripcion;
                model.IdDocumento = documento.IdDocumento;
                model.VersionFinal = documento.VersionFinal;
                model.Version = documento.Version;
                model.EsBorrador = documento.EsBorrador;
                model.FechaCreacion = documento.FechaCreacion;
                model.Texto = documento.Texto;

                List<VersionesDocumento> versiones = new List<VersionesDocumento>();
                if (documento.Version > 0)
                {
                    DirectoryInfo di = new DirectoryInfo(directorio);
                    foreach (var fi in di.GetFiles(documento.IdDocumento + "_version*.xml"))
                    {
                        VersionesDocumento version = new VersionesDocumento();
                        version.nombre = fi.Name;
                        versiones.Add(version);
                    }
                }

                detalleDocumento.Document = model;
                detalleDocumento.ListaVersiones = versiones;

                log_documento log_ = new log_documento();
                log_.idUser = User.Identity.Name;
                log_.idDocumento = model.IdDocumento;
                log_.descripcion = "Se crea nueva version del documento " + model.IdDocumento;

                LogBO.setLogCreateDoc(log_);

                return View(detalleDocumento);
            }
            catch (BusinessException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        [HttpPost]
        public ActionResult Editar(DetalleDocumento model)
        {
            string ruta = "";
            bool esBorrador = false;
            bool esVersion = false;
            bool esDocumento = false;
            int version = 0;
            try
            {
                if (ModelState.IsValid)
                {
                    sgd_documentoEntity documento = DocumentoBO.getDocumentoById(Int32.Parse(model.Document.IdDocumento.ToString()));
                    sgd_documentoEntity doc_ = documento;

                    documento.Descripcion = model.Document.Descripcion;
                    documento.EsBorrador = model.Document.EsBorrador;
                    documento.Texto = model.Document.Texto;
                    documento.Titulo = model.Document.Titulo;
                    documento.VersionFinal = model.Document.VersionFinal;

                    if (model.Document.EsBorrador && !model.Document.VersionFinal)
                    {
                        documento.EsBorrador = model.Document.EsBorrador;
                        documento.VersionFinal = model.Document.VersionFinal;
                        esBorrador = true;
                    }
                    else if (model.Document.VersionFinal && model.Document.EsBorrador)
                    {
                        version = documento.Version + 1;
                        documento.Version = version;
                        documento.EsBorrador = false;
                        esVersion = true;
                        esDocumento = true;
                    }
                    else
                    {
                        esDocumento = true;
                    }

                    DocumentoBO.setSaveDocumento(documento);

                    if (esBorrador)
                    {
                        ruta = directorio + documento.IdDocumento + "_borrador.xml";
                        guardaArchivo(ruta, documento);
                    }
                    if (esVersion)
                    {
                        ruta = directorio + documento.IdDocumento + "_version-" + version + ".xml";
                        guardaArchivo(ruta, doc_);
                    }
                    if (esDocumento)
                    {
                        ruta = directorio + documento.IdDocumento + ".xml";
                        guardaArchivo(ruta, documento);
                    }

                    log_documento log_ = new log_documento();
                    log_.idUser = User.Identity.Name;
                    log_.idDocumento = model.Document.IdDocumento;
                    log_.descripcion = "Se edito el documento " + model.Document.IdDocumento;

                    LogBO.setLogCreateDoc(log_);

                    return Redirect("~/Document");
                }
                return View(model);
            }
            catch (BusinessException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        [HttpGet]
        public ActionResult Eliminar(int id)
        {
            try
            {
                DocumentoBO.deleteById(id);

                log_documento log_ = new log_documento();
                log_.idUser = User.Identity.Name;
                log_.idDocumento = id.ToString();
                log_.descripcion = "Se elimino el documento";

                LogBO.setLogCreateDoc(log_);

                return Redirect("~/Document");
            }
            catch (BusinessException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }
        }

        public ActionResult Ma_NuevaReferencia(string id)
        {
            sgd_documentoEntity doc = new sgd_documentoEntity();
            doc = SolrBO.getDocumentoById(id);
            ViewBag.TextoOriginal = doc.Texto;
            sgd_documentoEntity referencia = new sgd_documentoEntity();
            referencia.IdDocumento = id;

            log_documento log_ = new log_documento();
            log_.idUser = User.Identity.Name;
            log_.idDocumento = id;
            log_.descripcion = "Se crea nueva referencia del documento Medio Ambiental";

            LogBO.setLogCreateDoc(log_);

            return View(referencia);
        }

        [HttpPost]
        public ActionResult Ma_NuevaReferencia(sgd_documentoEntity doc)
        {
            ViewBag.Error = null;
            NuevoDocumentViewModel model = new NuevoDocumentViewModel();
            sgd_documentoEntity docOriginal = SolrBO.getDocumentoById(doc.IdDocumento);
            try
            {
                if (doc.Norma == "0" || doc.Titulo == null || doc.Texto == null)
                {
                    ViewBag.Error = "Estimado usuario, todos los campos son obligatorios,<br />por favor complete el formulario para continuar.";
                    ViewBag.TextoOriginal = docOriginal.Texto;
                    return View(doc);
                }
                else
                {
                    doc.Fecha = DateTime.Now;
                    doc.Coleccion = docOriginal.Coleccion;

                    #region links documento nuevo
                    doc.Links = new List<LinkEntity>();
                    LinkEntity l = new LinkEntity();
                    l.Tipo = docOriginal.Norma;
                    l.Url = docOriginal.IdDocumento;
                    string textoReferenciaDestino = "";
                    if (docOriginal.Numero != null && docOriginal.Numero != "")
                    {
                        textoReferenciaDestino = "Número " + docOriginal.Numero;
                    }
                    else
                    {
                        if (docOriginal.Articulo != null && docOriginal.Articulo != "")
                            textoReferenciaDestino += "Artículo N° " + docOriginal.Articulo;
                        if (docOriginal.Inciso != null && docOriginal.Inciso != "")
                            textoReferenciaDestino += ", Inciso " + docOriginal.Inciso;
                    }
                    textoReferenciaDestino += ".- " + docOriginal.Titulo;
                    l.Texto = textoReferenciaDestino;
                    doc.Links.Add(l);
                    #endregion

                    string versionFinal = FileBo.SerializeXML(doc);
                    doc.id = null;
                    doc.IdDocumento = Utiles.getMd5(versionFinal);

                    #region links documento original

                    if (docOriginal.Links == null)
                        docOriginal.Links = new List<LinkEntity>();

                    List<LinkEntity> links = new List<LinkEntity>();
                    l = new LinkEntity();
                    l.Texto = doc.Titulo;
                    l.Tipo = doc.Norma;
                    l.Url = doc.IdDocumento;
                    links.Add(l);

                    foreach (LinkEntity link in docOriginal.Links)
                    {
                        links.Add(link);
                    }
                    docOriginal.Links = links;
                    #endregion

                    versionFinal = FileBo.SerializeXML(doc);
                    FileBo.setXmlStringToFile(directorio_ma + doc.Norma + "\\" + doc.IdDocumento + ".xml", versionFinal);

                    versionFinal = FileBo.SerializeXML(docOriginal);
                    FileBo.setXmlStringToFile(directorio_ma + docOriginal.Norma.Replace(" ", "_") + "\\" + docOriginal.IdDocumento + ".xml", versionFinal);

                    log_documento log_ = new log_documento();
                    log_.idUser = User.Identity.Name;
                    log_.idDocumento = doc.IdDocumento;
                    log_.descripcion = "Se crea nueva referencia del documento Medio Ambiental";

                    LogBO.setLogCreateDoc(log_);

                    SolrBO.sendXmlDocumento(doc, true);
                }
                return Redirect("~/Document/Ma_VerDocumento/" + docOriginal.IdDocumento);
            }
            catch (BusinessException bx)
            {
                ViewBag.Error = bx.Message;
                ModelState.AddModelError("", bx.Message);
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        public ActionResult Ma_Nuevo()
        {
            MedioAmbiental ma = new MedioAmbiental();
            return View(ma);
        }

        [HttpPost]
        public ActionResult Ma_Nuevo(sgd_documentoEntity ma)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string norma = ma.Norma.Replace(" ", "_") + "\\";
                    string xml = FileBo.SerializeXML(ma);
                    string idDoc = Utiles.getMd5(xml);
                    FileBo.setXmlStringToFile(directorio_ma + norma + idDoc + ".xml", xml);

                    Ma_SendSorl(ma, true);

                    log_documento log_ = new log_documento();
                    log_.idUser = User.Identity.Name;
                    log_.idDocumento = ma.IdDocumento;
                    log_.descripcion = "Se crea nuevo documento Medio Ambiental";

                    LogBO.setLogCreateDoc(log_);

                    return Redirect("~/Document/Ma_NuevoSuccess/");
                }
                return View(ma);
            }
            catch (BusinessException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(ma);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(ma);
            }

        }

        public ActionResult Ma_NuevoSuccess()
        {
            return View();
        }

        public ActionResult Ma_VerVersion(string id)
        {
            sgd_documentoEntity ma = new sgd_documentoEntity();
            
            try
            {
                string response = SolrBO.SolrQueryById(id.Substring(0, id.IndexOf("_")));

                var expConverter = new ExpandoObjectConverter();
                dynamic obj = JsonConvert.DeserializeObject<ExpandoObject>(response, expConverter);

                string idDocumento = "";
                string norma = "";
                foreach (var doc in obj.response.docs)
                {
                    idDocumento = doc.IdDocumento;
                    norma = (doc.Norma).Replace(" ", "_") + "\\";
                }

                if(id.IndexOf("_") > -1)
                {
                    string[] archivo = id.Split('_');
                    ViewBag.Version = archivo[1].TrimStart('0');
                }
                else
                {
                    ViewBag.Version = "original";
                }

                string xml = System.IO.File.ReadAllText(directorio_ma + norma + id + ".xml");
                ma = (sgd_documentoEntity)FileBo.DeserializeXML(ma.GetType(), xml);
                return View(ma);
            }
            catch (BusinessException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(ma);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(ma);
            }
        }

        public ActionResult SetReferencia(string textoReferencia, string idDocumento)
        {
            string idDocOriginal = "";
            try
            {
                idDocOriginal = (string)System.Web.HttpContext.Current.Session["id-doc-referencia"];

                string norma = "";

                sgd_documentoEntity docOriginal = SolrBO.getDocumentoById(idDocOriginal);
                sgd_documentoEntity docDestino = SolrBO.getDocumentoById(idDocumento);

                string texto_original = docOriginal.Links.FirstOrDefault().Texto;

                if (docOriginal != null)
                {
                    #region documento original
                    string textoReferenciaDestino = "";
                    if (docOriginal.Numero != null && docOriginal.Numero != "")
                    {
                        textoReferenciaDestino = "Número " + docOriginal.Numero + ".- " + docOriginal.Titulo;
                    }
                    else
                    {
                        if (docOriginal.Articulo != null && docOriginal.Articulo != "")
                            textoReferenciaDestino += "Artículo N° " + docOriginal.Articulo;
                        if (docOriginal.Inciso != null && docOriginal.Inciso != "")
                            textoReferenciaDestino += ", Inciso " + docOriginal.Inciso;
                    }

                    if (docOriginal.Links == null)
                        docOriginal.Links = new List<LinkEntity>();

                    List<LinkEntity> links = new List<LinkEntity>();
                    LinkEntity l = new LinkEntity();
                    l.Texto = textoReferencia;
                    l.Tipo = docDestino.Norma;
                    l.Url = idDocumento;
                    links.Add(l);

                    foreach (LinkEntity link in docOriginal.Links)
                    {
                        links.Add(link);
                    }

                    docOriginal.Links = links;

                    norma = docOriginal.Norma.Replace(" ", "_") + "\\";
                    string versionFinal = FileBo.SerializeXML(docOriginal);
                    FileBo.setXmlStringToFile(directorio_ma + norma + idDocOriginal + ".xml", versionFinal);
                    #endregion
                    #region documento destino                   
                    if (docDestino.Links == null)
                        docDestino.Links = new List<LinkEntity>();

                    links = new List<LinkEntity>();
                    l = new LinkEntity();
                    l.Texto = textoReferenciaDestino;
                    l.Tipo = docOriginal.Norma;
                    l.Url = idDocOriginal;
                    links.Add(l);

                    foreach (LinkEntity link in docDestino.Links)
                    {
                        links.Add(link);
                    }
                    docDestino.Links = links;
                    norma = docDestino.Norma.Replace(" ", "_") + "\\";
                    versionFinal = FileBo.SerializeXML(docDestino);
                    FileBo.setXmlStringToFile(directorio_ma + norma + idDocumento + ".xml", versionFinal);

                    log_documento log_ = new log_documento();
                    log_.idUser = User.Identity.Name;
                    log_.idDocumento = idDocumento;
                    log_.descripcion = "Se edito texto de referencia del documento, nuevo texto: " + textoReferencia + " texto original : " + texto_original;

                    LogBO.setLogCreateDoc(log_);
                    #endregion
                }
                System.Web.HttpContext.Current.Session["id-doc-referencia"] = null;
            }
            catch (BusinessException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }

            return Redirect("~/Document/Ma_VerDocumento/" + idDocOriginal);
        }

        public ActionResult Ma_VerDocumento(string id)
        {
            Documento d = new Documento();
            sgd_documentoEntity ma = new sgd_documentoEntity();
            sgd_documentoEntity docR = new sgd_documentoEntity();
            string rutaDoc = "";
            ViewBag.Referencia = false;
            try
            {
                try
                {
                    if (System.Web.HttpContext.Current.Session["id-doc-referencia"] != null)
                    {
                        string idR = (string)System.Web.HttpContext.Current.Session["id-doc-referencia"];
                        rutaDoc = SolrBO.getUrlDocumentById(idR); //SolrBO.SolrGetUrlDocumentById(idR);
                        string xml = System.IO.File.ReadAllText(rutaDoc);
                        docR = (sgd_documentoEntity)FileBo.DeserializeXML(ma.GetType(), xml);
                        ViewBag.Referencia = true;
                        ViewBag.DocumentoR = docR;
                    }
                }
                catch (Exception ex)
                { }
                sgd_documentoEntity _DocumentoEntity = SolrBO.getDocumentoById(id); // SolrBO.SolrQueryById(id);

                d = UpdateDocumento.updateDoc(_DocumentoEntity);


                string Norma = (d.Norma).Replace(" ", "_") + "\\";

                if (System.IO.File.Exists(directorio_ma + Norma + d.IdDocumento + ".xml"))
                {
                    string xml = System.IO.File.ReadAllText(directorio_ma + Norma + d.IdDocumento + ".xml");

                    ma = (sgd_documentoEntity)FileBo.DeserializeXML(ma.GetType(), xml);

                    d = UpdateDocumento.updateDoc(ma);

                    return View(d);
                }
                else
                {
                    throw new BusinessException("No se pudo encontrar el xml del documento. Contáctese con el administrador");
                }
            }
            catch (BusinessException ex)
            {
                ModelState.AddModelError("Error", ex.Message);
                return View(d);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Error", "No se encontro el xml del documento por favor verificar o comunicarse con el administrador.");
                new TechnicalException("Error al ver documento, id :" + id, ex);
                return View(d);
            }
        }

        public ActionResult Ma_EditarDocumento(string id)
        {
            sgd_documentoEntity ma = new sgd_documentoEntity();

            try
            {
                sgd_documentoEntity d = SolrBO.getDocumentoById(id);

                string Norma = (d.Norma).Replace(" ", "_") + "\\";

                bool esBorrador = false;
                if (System.IO.File.Exists(directorio_ma + Norma + d.IdDocumento + "_borrador.xml"))
                    esBorrador = true;

                string xml = "";
                if (!esBorrador)
                    xml = System.IO.File.ReadAllText(directorio_ma + Norma + d.IdDocumento + ".xml");
                else
                    xml = System.IO.File.ReadAllText(directorio_ma + Norma + d.IdDocumento + "_borrador.xml");

                ViewBag.EsBorrador = esBorrador;
                ma = (sgd_documentoEntity)FileBo.DeserializeXML(ma.GetType(), xml);
                #region versiones
                List<VersionesDocumento> versiones = new List<VersionesDocumento>();
                if (ma.Versiones != null && ma.Versiones.Count > 0)
                {
                    foreach (VersionesDocumentoEntity v in ma.Versiones)
                    {
                        VersionesDocumento version = new VersionesDocumento();
                        version.nombre = v.nombre;
                        version.id = v.id;
                        versiones.Add(version);
                    }
                    ma.Versiones = null;
                }
                else
                {
                    VersionesDocumento version = new VersionesDocumento();
                    version.nombre = d.IdDocumento;
                    version.id = "original";
                    versiones.Add(version);
                }
                ViewBag.Versiones = versiones;
                #endregion
                #region links
                List<LinkEntity> links = new List<LinkEntity>();
                if (ma.Links != null && ma.Links.Count > 0)
                {

                    foreach (LinkEntity l in ma.Links)
                    {
                        LinkEntity link = new LinkEntity();
                        link.Texto = l.Texto;
                        ViewBag.Aplica = "";
                        link.Tipo = l.Tipo;
                        link.Url = l.Url;
                        links.Add(link);
                    }
                    ma.Links = null;
                }
                else
                {
                    LinkEntity link = new LinkEntity();
                    link.Tipo = "El documento no contiene links";
                    links.Add(link);
                }
                ViewBag.Links = links;
                #endregion
                return View(ma);
            }
            catch (BusinessException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(ma);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(ma);
            }
        }

        [HttpPost]
        public ActionResult Ma_EditarDocumento(sgd_documentoEntity ma)
        {
            try
            {
                string norma = ma.Norma.Replace(" ", "_") + "\\";
                bool existeBorrador = System.IO.File.Exists(directorio_ma + norma + ma.IdDocumento + "_borrador.xml");
                if (!ma.EsBorrador && !existeBorrador)
                    ma.EsBorrador = true;

                string xml = FileBo.SerializeXML(ma);
                if (ma.EsBorrador)
                {
                    ma.Versiones = null;
                    FileBo.setXmlStringToFile(directorio_ma + norma + ma.IdDocumento + "_borrador.xml", xml);

                }
                else
                {
                    if (System.IO.File.Exists(directorio_ma + norma + ma.IdDocumento + "_borrador.xml"))
                        System.IO.File.Delete(directorio_ma + norma + ma.IdDocumento + "_borrador.xml");

                    sgd_documentoEntity d = SolrBO.getDocumentoById(ma.id);

                    string versionXmlOriginal = System.IO.File.ReadAllText(directorio_ma + norma + d.IdDocumento + ".xml");
                    sgd_documentoEntity VersionOriginalMa = (sgd_documentoEntity)FileBo.DeserializeXML(ma.GetType(), versionXmlOriginal);

                    int totalVersiones = 0;
                    VersionesDocumentoEntity v = new VersionesDocumentoEntity();
                    List<VersionesDocumentoEntity> versiones = new List<VersionesDocumentoEntity>();
                    if (VersionOriginalMa.Versiones == null)
                    {
                        totalVersiones = 1;
                        v.nombre = d.IdDocumento + "_" + string.Format("{0:0000000000}", totalVersiones);
                        v.id = "1";
                    }
                    else
                    {
                        totalVersiones = VersionOriginalMa.Versiones.Count + 1;
                        v.nombre = d.IdDocumento + "_" + string.Format("{0:0000000000}", totalVersiones);
                        v.id = Convert.ToString(totalVersiones);
                    }
                    VersionOriginalMa.Versiones.Add(v);

                    versionXmlOriginal = FileBo.SerializeXML(VersionOriginalMa);
                    FileBo.setXmlStringToFile(directorio_ma + norma + ma.IdDocumento + "_" + string.Format("{0:0000000000}", totalVersiones) + ".xml", versionXmlOriginal);

                    //ma.Versiones = VersionOriginalMa.Versiones;
                    //ma.Links = VersionOriginalMa.Links;

                    VersionOriginalMa.Norma = ma.Norma;
                    VersionOriginalMa.Organismo = ma.Organismo;
                    VersionOriginalMa.Suborganismo = ma.Suborganismo;
                    VersionOriginalMa.Seccion = ma.Seccion;
                    VersionOriginalMa.Articulo = ma.Articulo;
                    VersionOriginalMa.Inciso = ma.Inciso;
                    VersionOriginalMa.Numero = ma.Numero;
                    VersionOriginalMa.Categoria = ma.Categoria;
                    VersionOriginalMa.Tema = ma.Tema;
                    VersionOriginalMa.Titulo = ma.Titulo;
                    VersionOriginalMa.Texto = ma.Texto;

                    string versionFinal = FileBo.SerializeXML(VersionOriginalMa);
                    FileBo.setXmlStringToFile(directorio_ma + norma + d.IdDocumento + ".xml", versionFinal);

                    SolrBO.sendXmlDocumento(VersionOriginalMa, false);

                    log_documento log_ = new log_documento();
                    log_.idUser = User.Identity.Name;
                    log_.idDocumento = ma.IdDocumento;
                    log_.descripcion = "Se edita documento Medio Ambiental";

                    LogBO.setLogCreateDoc(log_);
                }
            }
            catch (BusinessException bx)
            {
                ModelState.AddModelError("", bx.Message);
                return View(ma);
            }
            catch (Exception ex)
            {
                //ModelState.AddModelError("", ex.Message);
                //return View(ma);
            }
            return Redirect("~/Document/Ma_EditarDocumento/" + ma.id);
        }

        [HttpPost]
        public ActionResult ma_buscar(string q, int p)
        {
            try
            {
                if (p == 0)
                {
                    System.Web.HttpContext.Current.Session["sessBusqueda"] = q;
                    p = 1;
                }
                else
                {
                    q = System.Web.HttpContext.Current.Session["sessBusqueda"] as String;
                }

                System.Web.HttpContext.Current.Session["sessPagina"] = p.ToString();

                string Query = SolrBO.SolrSelect(q, (p - 1) * 5);
                var lista = JsonConvert.DeserializeObject<ExpandoObject>(Query);

                ViewBag.Query = lista;
                ViewBag.Pagina = p;
                ViewBag.Busqueda = DecodeHtmlText(q);
                return View();
            }
            catch (BusinessException bx)
            {
                ModelState.AddModelError("", bx.Message);
                return View();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }
        }

        [HttpPost]
        public ActionResult Buscar(string q, int p)
        {
            if (p == 0)
            {
                System.Web.HttpContext.Current.Session["sessBusqueda"] = q;
                p = 1;
            }
            else
            {
                q = System.Web.HttpContext.Current.Session["sessBusqueda"] as String;
            }

            System.Web.HttpContext.Current.Session["sessPagina"] = p.ToString();

            string Query = SolrBO.SolrSelect(q, (p - 1) * 5);
            var lista = JsonConvert.DeserializeObject<ExpandoObject>(Query);

            ViewBag.Query = lista;
            ViewBag.Pagina = p;
            ViewBag.Busqueda = DecodeHtmlText(q);
            return View();

        }

        [HttpGet]
        public ActionResult Buscar()
        {
            try
            {
                string q = System.Web.HttpContext.Current.Session["sessBusqueda"] as String;
                string p = System.Web.HttpContext.Current.Session["sessPagina"] as String;

                if (q == null)
                {
                    return Redirect("~/Account/LogOff");
                }

                string Query = SolrBO.SolrSelect(q, (Convert.ToInt32(p) - 1) * 5);
                var lista = JsonConvert.DeserializeObject<ExpandoObject>(Query);

                ViewBag.Query = lista;
                ViewBag.Pagina = Convert.ToInt32(p);
                ViewBag.Busqueda = DecodeHtmlText(q);
                return View();
            }
            catch (BusinessException bx)
            {
                ModelState.AddModelError("", bx.Message);
                return View();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }

        }

        private bool guardaArchivo(string ruta, sgd_documentoEntity doc)
        {
            string xml = FileBo.SerializeXML(doc);
            FileBo.setXmlStringToFile(ruta, xml);
            return true;
        }

        private bool Ma_SendSorl(sgd_documentoEntity ma, bool nuevo)
        {
            ma.Texto = Regex.Replace(ma.Texto, "[<].*?>", " ");
            ma.Texto = Regex.Replace(ma.Texto, @"\s+", " ");

            ma.Texto = DecodeHtmlText(ma.Texto);

            string xml = "";

            xml = "<add><doc>";
            if (!nuevo)
            {
                xml += "<field name=\"id\">" + ma.id + "</field>";
            }
            xml += "<field name=\"IdDocumento\">" + ma.IdDocumento + "</field>";
            xml += "<field name=\"Orden\">" + ma.Orden + "</field>";
            xml += "<field name=\"Coleccion\">" + ma.Coleccion + "</field>";
            xml += "<field name=\"Anio\">" + ma.Anio + "</field>";
            xml += "<field name=\"Apendice\">" + ma.Apendice + "</field>";
            xml += "<field name=\"AplicaNorma\">" + ma.AplicaNorma + "</field>";
            xml += "<field name=\"Articulo\">" + ma.Articulo + "</field>";
            xml += "<field name=\"AplicaArticulo\">" + ma.AplicaArticulo + "</field>";
            xml += "<field name=\"Categoria\">" + ma.Categoria + "</field>";
            xml += "<field name=\"Comentario\">" + ma.Comentario + "</field>";
            xml += "<field name=\"Cve\">" + ma.Cve + "</field>";
            xml += "<field name=\"Fecha\">" + ma.Fecha + "</field>";
            xml += "<field name=\"Iddo\">" + ma.Iddo + "</field>";
            xml += "<field name=\"IdRep\">" + ma.IdRep + "</field>";
            xml += "<field name=\"Inciso\">" + ma.Inciso + "</field>";
            xml += "<field name=\"Minred\">" + ma.Minred + "</field>";
            xml += "<field name=\"Nompop\">" + ma.Nompop + "</field>";
            xml += "<field name=\"Norma\">" + ma.Norma + "</field>";
            xml += "<field name=\"Numero\">" + ma.Numero + "</field>";
            xml += "<field name=\"Organismo\">" + ma.Organismo + "</field>";
            xml += "<field name=\"OrgansimoUno\">" + ma.OrganismoUno + "</field>";
            xml += "<field name=\"Regco\">" + ma.Regco + "</field>";
            xml += "<field name=\"Resuel\">" + ma.Resuel + "</field>";
            xml += "<field name=\"Rol\">" + ma.Rol + "</field>";
            xml += "<field name=\"Seccion\">" + ma.Seccion + "</field>";
            xml += "<field name=\"Suborganismo\">" + ma.Suborganismo + "</field>";
            xml += "<field name=\"Tema\">" + ma.Tema + "</field>";
            xml += "<field name=\"Temas\">" + ma.Temas + "</field>";
            xml += "<field name=\"Titulo\">" + ma.Titulo + "</field>";
            xml += "<field name=\"Texto\">" + ma.Texto + "</field>";
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

        private string DecodeHtmlText(string texto)
        {
            StringWriter myWriter = new StringWriter();
            HttpUtility.HtmlDecode(texto, myWriter);
            return myWriter.ToString();
        }
    }
}