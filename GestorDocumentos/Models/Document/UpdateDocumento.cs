using GestorDocumentosEntities;
using GestorDocumentosExceptions;
using mvc4.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace GestorDocumentos.Models.Document
{
    public class UpdateDocumento
    {
        public static Documento updateDoc(sgd_documentoEntity _DocumentoEntity)
        {
            Documento d = new Documento();
            try
            {
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
                return d;
            }
            catch (Exception ex)
            {
                new TechnicalException("Error al tranformar docuemnto", ex);
                throw new BusinessException("No es posible cambiar de tipo de entidad el docuemnto");
            }
        }
    }
}