using System;
using System.Net;

namespace SC.SDK.NetStandard.BuildingBlocks.Http
{
    public class ServiceResponse
    {
        public string Content { get; }
        public HttpStatusCode StatusCode { get; }
        public bool Success { get; }
        public string ErrorMessage { get; }
        public ServiceResponseException Exception { get; }

        public ServiceResponse(string content, HttpStatusCode statusCode, bool success, string errorMessage, Exception ex)
        {
            Content = content;
            StatusCode = statusCode;
            Success = success;
            ErrorMessage = errorMessage;
            Exception = (ServiceResponseException)ex;
        }
    }

    public class ServiceResponse<T> : ServiceResponse 
        where T : new()
    {
        public new T Content { get; }
        public bool HasContent { get => Content != null; }
        public ServiceResponse(T content, HttpStatusCode statusCode, bool success, string errorMessage, Exception ex)
            : base(null, statusCode, success, errorMessage, ex)
        {
            Content = content;
        }

        public ServiceResponse<T> Then(Action<ServiceResponse<T>> callback)
        {
            if (Success && callback != null)
            {
                callback.Invoke(this);
            }
            return this;
        }

        public ServiceResponse<T> Catch(Action<ServiceResponse> callback)
        {
            if (!Success && callback != null)
            {
                callback.Invoke(this);
            }
            return this;
        }
    }

    public class ServiceResponseException : Exception
    {
    }
}
