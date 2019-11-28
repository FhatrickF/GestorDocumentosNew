using System;

namespace GestorDocumentosUtil
{
    public class FunctionalException : Exception
    {
        public string ErrorCode { get; set; }
        public FunctionalException()
        {
        }

        public FunctionalException(string message) : base(message)
        {
        }

        public ErrorResponseInfo GetErrorMessageJson()
        {
            ErrorResponseInfo errorResp = new ErrorResponseInfo();
            errorResp.ErrorMessage = this.Message;
            errorResp.ErrorDetails = this.StackTrace;
            return errorResp;
        }
    }
}
