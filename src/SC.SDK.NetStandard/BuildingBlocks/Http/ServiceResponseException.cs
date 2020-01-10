using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace SC.SDK.NetStandard.BuildingBlocks.Http
{
    public class ServiceResponseException : Exception
    {
        public HttpStatusCode StatusCode { get; }
        public Exception HttpException { get; }
        public ServiceResponseException(HttpStatusCode statusCode, string message, Exception httpException)
            : base(message)
        {
            StatusCode = statusCode;
            HttpException = HttpException;
        }
    }
}
