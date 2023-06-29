using Microsoft.Extensions.Logging;
using System;
using System.Net;

namespace TenantManagement.Common.Exceptions
{
    public class BaseException : Exception
    {
        public HttpStatusCode ErrCode { get; set; }

        public BaseException(string details, Exception ex = null, ILogger logger = null) : base(details, ex)
        {
            if (logger != null)
            {
                logger.LogError(Message);
            }
            ErrCode = HttpStatusCode.BadRequest;
        }

        public BaseException(HttpStatusCode errorCode, string details = null, Exception ex = null, ILogger logger = null) : base(errorCode.ToString() + ": " + details, ex)
        {
            if (logger != null)
            {
                logger.LogError(Message);
            }
            ErrCode = errorCode;
        }
    }
}