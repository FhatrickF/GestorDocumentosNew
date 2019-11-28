using GestorDocumentosEntities;
using GestorDocumentosExceptions;
using mvc4.Business;
using mvc4.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;


namespace GestorDocumentos.Controllers
{
    public class BusquedaController : Controller
    {
        private string directorio_ma = WebConfigurationManager.AppSettings["MVC-DATA-MA"];

        // GET: Busqueda
        public ActionResult Index(string id)
        {
            System.Web.HttpContext.Current.Session["id-doc-referencia"] = null;
            Documento d = new Documento();
            if (id != null)
            {
                System.Web.HttpContext.Current.Session["id-doc-referencia"] = id;

                sgd_documentoEntity _DocumentoEntity = SolrBO.getDocumentoById(id);

                d.Anio = _DocumentoEntity.Anio;
                d.AnioC = _DocumentoEntity.AnioC;
                d.Apendice = _DocumentoEntity.Apendice;
                d.AplicaArticulo = _DocumentoEntity.AplicaArticulo;
                d.AplicaNorma = _DocumentoEntity.AplicaNorma;
                d.AplicaNumero = _DocumentoEntity.AplicaNumero;
                d.Articulo = _DocumentoEntity.Articulo;
                d.Categoria = _DocumentoEntity.Categoria;
                d.Coleccion = _DocumentoEntity.Coleccion;
                d.Comentario = _DocumentoEntity.Comentario;
                d.Cve = _DocumentoEntity.Cve;
                d.Descripcion = _DocumentoEntity.Descripcion;
                d.EsBorrador = _DocumentoEntity.EsBorrador;
                d.Fecha = _DocumentoEntity.Fecha;
                d.FechaCreacion = _DocumentoEntity.FechaCreacion;
                d.id = _DocumentoEntity.id;
                d.Iddo = _DocumentoEntity.Iddo;
                d.IdDocumento = _DocumentoEntity.IdDocumento;
                d.IdRep = _DocumentoEntity.IdRep;
                d.Inciso = _DocumentoEntity.Inciso;

                List<Link> ls = new List<Link>();
                foreach (var item in _DocumentoEntity.Links)
                {
                    Link l = new Link();
                    l.Texto = item.Texto;
                    l.Tipo = item.Tipo;
                    l.Url = item.Url;
                    ls.Add(l);
                }
                d.Links = ls;
                d.Minred = _DocumentoEntity.Minred;
                d.Nompop = _DocumentoEntity.Nompop;
                d.Norma = _DocumentoEntity.Cve;
                d.Numero = _DocumentoEntity.Cve;
                d.Orden = _DocumentoEntity.Orden;
                d.Organismo = _DocumentoEntity.Organismo;
                d.OrganismoUno = _DocumentoEntity.OrganismoUno;
                d.Regco = _DocumentoEntity.Regco;
                d.Resuel = _DocumentoEntity.Resuel;
                d.Rol = _DocumentoEntity.Rol;
                d.Seccion = _DocumentoEntity.Seccion;
                d.Suborganismo = _DocumentoEntity.Suborganismo;
                d.Tema = _DocumentoEntity.Tema;
                d.Temas = _DocumentoEntity.Temas;
                d.Texto = _DocumentoEntity.Texto;
                d.Titulo = _DocumentoEntity.Titulo;
                d.Version = _DocumentoEntity.Version;

                List<VersionesDocumento> versiones = new List<VersionesDocumento>();
                foreach (var item in _DocumentoEntity.Versiones)
                {
                    VersionesDocumento vr = new VersionesDocumento();
                    vr.id = item.id;
                    vr.nombre = item.nombre;
                    versiones.Add(vr);
                }

                d.Versiones = versiones;
                d.VersionFinal = _DocumentoEntity.VersionFinal;


                ViewBag.Referencia = true;
            }
            else
            {
                ViewBag.Referencia = false;
            }
            return View(d);
        }

        [HttpPost]
        public string Index(FormularioBusqueda form)
        {
            try
            {
                string ct = form.Ct;
                string lr = form.Lr;
                string lt = form.Lt;
                string lzf = form.Lzf;
                string liva = form.Liva;

                string circulares = form.Circular;
                string decretos = form.Decreto;
                string dfl = form.Dfl;
                string dl = form.Dl;
                string ds = form.Ds;
                string ley = form.Ley;
                string resolucion = form.Resolucion;
                string fecha = form.Fecha;
                string numero = form.Numero;
                string articulo = form.Articulo;
                string inciso = form.Inciso;
                string texto = form.Texto;
                string pagina = Convert.ToString(form.Pagina);

                bool ordenBy = false;

                string q = "";
                if (ct != null)
                {
                    q += "Norma:'CODIGO TRIBUTARIO'";
                    ordenBy = true;
                }
                if (lr != null)
                {
                    q += (q != "" && q != null) ? " OR Norma:'LEY DE LA RENTA'" : "Norma:'LEY DE LA RENTA'";
                    ordenBy = true;
                }
                if (lt != null)
                {
                    q += (q != "" && q != null) ? " OR Norma:'LEY DE TIMBRES Y ESTAMPILLAS'" : "Norma:'LEY DE TIMBRES Y ESTAMPILLAS'";
                    ordenBy = true;
                }
                if (lzf != null)
                {
                    q += (q != "" && q != null) ? " OR Norma:'LEY DE ZONA FRANCA'" : "Norma:'LEY DE ZONA FRANCA'";
                    ordenBy = true;
                }
                if (liva != null)
                {
                    q += (q != "" && q != null) ? " OR Norma:'LEY DEL IVA'" : "Norma:'LEY DEL IVA'";
                    ordenBy = true;
                }
                if (circulares != null)
                    q += (q != "" && q != null) ? " OR Norma:'CIRCULARES'" : "Norma:'CIRCULARES'";
                if (decretos != null)
                    q += (q != "" && q != null) ? " OR Norma:'DECRETOS'" : "Norma:'DECRETOS'";
                if (dfl != null)
                    q += (q != "" && q != null) ? " OR Norma:'DECRETOS CON FUERZA DE LEY'" : "Norma:'DECRETOS CON FUERZA DE LEY'";
                if (dl != null)
                    q += (q != "" && q != null) ? " OR Norma:'DECRETOS LEYES'" : "Norma:'DECRETOS LEYES'";
                if (ds != null)
                    q += (q != "" && q != null) ? " OR Norma:'DECRETOS SUPREMOS'" : "Norma:'DECRETOS SUPREMOS'";
                if (ley != null)
                    q += (q != "" && q != null) ? " OR Norma:'LEYES'" : "Norma:'LEYES'";
                if (resolucion != null)
                    q += (q != "" && q != null) ? " OR Norma:'RESOLUCIONES'" : "Norma:'RESOLUCIONES'";

                string bNorma = "";
                if (q != "")
                    bNorma = " AND (" + q + ")";

                q = "";
                if (fecha != null)
                {
                    fecha = fecha.Replace("/", "-");
                    string[] f = fecha.Split('-');
                    fecha = f[2] + "-" + f[1] + "-" + f[0] + @"T00:00:00Z";
                    q += " AND Fecha:'" + fecha + "'";
                }
                if (numero != null)
                    q += " AND Numero:'" + numero + "'";
                if (articulo != null)
                    q += " AND Articulo:'" + articulo + "'";
                if (inciso != null)
                    q += " AND Inciso:'" + inciso + "'";
                if (texto != null)
                    q += " AND Texto:'" + texto + "'";

                string bDatos = "";
                if (q != "")
                    bDatos = q;

                string fl = "Norma,Numero,Articulo,Inciso,Titulo,Fecha,id";

                string orden = "";
                if (ordenBy)
                    orden = "Orden";
                else
                    orden = "Fecha";

                string url = "select?fl=" + fl + "&q=Coleccion:'BITE'" + bNorma + bDatos + "&sort=" + orden + " asc&start=" + pagina;
                url = url.Replace("  ", " ");

                string response = SolrBO.SolrQuery(url);

                var expConverter = new ExpandoObjectConverter();
                dynamic obj = JsonConvert.DeserializeObject<ExpandoObject>(response, expConverter);

                var doc = obj.response;

                //return "{\"Success\":\"true\"}";
                return JsonConvert.SerializeObject(doc);
            }
            catch (BusinessException bx)
            {
                return "{\"MensajeError\":\""+ bx.Message + "\"}";
            }
        }
    }
}