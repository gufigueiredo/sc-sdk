using RestSharp;
using System;
using System.Net;

namespace SC.SDK.NetStandard.BuildingBlocks.Http
{
    public class ServiceResponse
    {
        public string Content { get; }
        public IRestResponse Source { get; }
        public HttpStatusCode StatusCode { get; }
        public bool Success { get; }
        public string ErrorMessage { get; }
        public ServiceResponseException Exception { get; }
        public bool HasException { get => Exception != null; }

        public ServiceResponse(string content, IRestResponse source, HttpStatusCode statusCode, bool success, string errorMessage, Exception ex)
        {
            Content = content;
            StatusCode = statusCode;
            Success = success;
            ErrorMessage = errorMessage;
            Exception = (ServiceResponseException)ex;
            Source = source;
        }

        protected ServiceResponse(HttpStatusCode statusCode, bool success, string errorMessage, Exception ex)
        {
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
        public new IRestResponse<T> Source { get; }
        public ServiceResponse(T content, IRestResponse<T> source, HttpStatusCode statusCode, bool success, string errorMessage, Exception ex)
            : base(statusCode, success, errorMessage, ex)
        {
            Content = content;
            Source = source;
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
}
