using GestorDocumentosDataAccess.Modelo;
using GestorDocumentosEntities;
using GestorDocumentosExceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestorDocumentosDataAccess
{
    public class DocumentoDAL
    {
        public static bool setDocumento(sgd_documentoEntity nuevo)
        {
            try
            {
                using (infoEntities db = new infoEntities())
                {
                    sgd_documento doc = new sgd_documento();
                    doc.Descripcion = nuevo.Descripcion;
                    doc.Titulo = nuevo.Titulo;
                    doc.Texto = nuevo.Texto;
                    doc.FechaCreacion = DateTime.Now;
                    doc.VersionFinal = true;
                    db.sgd_documento.Add(doc);
                    db.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                new TechnicalException("Error al guardar docuemnto", ex);
                throw new BusinessException("No es posible guradar el documento");
            }
        }

        public static sgd_documentoEntity getDocumentoById(int id)
        {
            try
            {
                using (infoEntities db = new infoEntities())
                {
                    var doc = db.sgd_documento.Find(id);
                    sgd_documentoEntity sgd = new sgd_documentoEntity();
                    sgd.EsBorrador = doc.EsBorrador;
                    sgd.IdDocumento = doc.IdDocumento.ToString();
                    sgd.Descripcion = doc.Descripcion;
                    sgd.FechaCreacion = doc.FechaCreacion;
                    sgd.Texto = doc.Texto;
                    sgd.Titulo = doc.Titulo;
                    sgd.Version = doc.Version;
                    sgd.VersionFinal = doc.VersionFinal;
                    return sgd;
                }
            }
            catch (Exception ex)
            {
                new TechnicalException("Error al buscar id del documento, id = "+ id , ex);
                throw new BusinessException("Error al buscar id del documento");
            }
        }

        public static int setDocumentoMA(sgd_documentoEntity nuevo)
        {
            try
            {
                using (infoEntities db = new infoEntities())
                {
                    sgd_documento doc = new sgd_documento();
                    doc.Descripcion = nuevo.Descripcion;
                    doc.Titulo = nuevo.Titulo;
                    doc.Texto = nuevo.Texto;
                    doc.FechaCreacion = DateTime.Now;
                    doc.VersionFinal = true;
                    db.sgd_documento.Add(doc);
                    db.SaveChanges();

                    return doc.IdDocumento;
                }
            }
            catch (Exception ex)
            {
                new TechnicalException("Error al guradar documento Medio Ambiental", ex);
                throw new BusinessException("Error al guradar documento Medio Ambiental");
            }
        }

        public static void deleteById(int id)
        {
            try
            {
                using (infoEntities db = new infoEntities())
                {
                    var doc = db.sgd_documento.Find(id);
                    db.sgd_documento.Remove(doc);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                new TechnicalException("Error al eliminar documento", ex);
                throw new BusinessException("No se puede eliminar el documento");
            }
        }

        public static void setSaveDocumento(sgd_documentoEntity documento)
        {
            try
            {
                using (infoEntities db = new infoEntities())
                {
                    var doc = db.sgd_documento.Find(documento.IdDocumento);
                    doc.Descripcion = documento.Descripcion;
                    doc.EsBorrador = documento.EsBorrador;
                    doc.FechaCreacion = documento.FechaCreacion;
                    doc.Texto = documento.Texto;
                    doc.Titulo = documento.Titulo;
                    doc.Version = documento.Version;
                    doc.VersionFinal = documento.VersionFinal;

                    db.Entry(doc).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                new TechnicalException("Error al guardar documento", ex);
                throw new BusinessException("Error al actualizar documento");
            }
        }
    }
}
