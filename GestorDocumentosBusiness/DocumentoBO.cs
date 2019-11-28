using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GestorDocumentosDataAccess;
using GestorDocumentosEntities;
using GestorDocumentosExceptions;

namespace GestorDocumentosBusiness
{
    public class DocumentoBO
    {
        public static bool setDocumento(sgd_documentoEntity nuevo)
        {
            try
            {

                return DocumentoDAL.setDocumento(nuevo);
            }
            catch (BusinessException ex)
            {
                return false;
            }
        }

        public static sgd_documentoEntity getDocumentoById(int id)
        {
            try
            {
                return DocumentoDAL.getDocumentoById(id);
            }
            catch (BusinessException ex)
            {
                throw new Exception("Error al Buscar id del documento");
            }
        }

        public static void setSaveDocumento(sgd_documentoEntity documento)
        {
            try
            {
                DocumentoDAL.setSaveDocumento(documento);
            }
            catch (BusinessException ex)
            {
                throw new BusinessException("Error al guardar documentos.");
            }
        }

        public static void deleteById(int id)
        {
            try
            {
                DocumentoDAL.deleteById(id);
            }
            catch (BusinessException ex)
            {
                throw new BusinessException("Error al eliminar documentos.");
            }
        }

        public static int setDocumentoMA(sgd_documentoEntity nuevo)
        {
            try
            {
                return DocumentoDAL.setDocumentoMA(nuevo);
            }
            catch (BusinessException ex)
            {

                throw new Exception("No se puede crear el archivo");
            }
        }
    }
}
