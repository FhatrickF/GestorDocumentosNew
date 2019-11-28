using System;

namespace GestorDocumentosExceptions
{
    [Serializable]
    public class BusinessException : ApplicationException
    {
        public BusinessException(string mensaje, Exception original) : base(mensaje, original) { }

        public BusinessException(string mensaje) : base(mensaje) { }
    }
}
