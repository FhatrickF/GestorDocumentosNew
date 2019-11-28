using GestorDocumentosUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestorDocumentosUtilities
{
    public class ErrorHandler
    {
        public static Exception Error(Exception ex)
        {
            LogManager.Instance.Error(ex);
            return ex;
        }
    }
}
