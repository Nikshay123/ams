using Microsoft.Extensions.Logging;
using System;

namespace WebApp.Common.Exceptions
{
    public class ApiException : Exception
    {
        public ErrorResponse.ErrorEnum ErrCode { get; set; }

        public ApiException(string details, Exception ex = null, ILogger logger = null) : base(details, ex)
        {
            if (logger != null)
            {
                logger.LogError(Message);
            }
            ErrCode = ErrorResponse.ErrorEnum.BadRequest;
        }

        public ApiException(ErrorResponse.ErrorEnum errorCode, string details = null, Exception ex = null, ILogger logger = null, bool rawMsg = false) : base(rawMsg ? details : (ErrorResponse.GetErrorMessage(errorCode) + ": " + details), ex)
        {
            if (logger != null)
            {
                logger.LogError(Message);
            }
            ErrCode = errorCode;
        }
    }
}