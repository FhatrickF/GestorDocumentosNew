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
    public class LogDAL
    {
        public static void setLogCreateDoc(log_documento log_)
        {
            try
            {
                using (infoEntities db = new infoEntities())
                {
                    Log_Documento _Documento = new Log_Documento();
                    _Documento.idDocumento = log_.idDocumento;
                    _Documento.idUser = log_.idUser;
                    _Documento.logDescripcion = log_.descripcion;
                    db.Log_Documento.Add(_Documento);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                new TechnicalException("Error.- No se pudo guardar la acción en el log. ", ex);
                throw new BusinessException("error");
            }
        }
    }
}
