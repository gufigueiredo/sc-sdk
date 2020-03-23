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
        public bool RequestSuccess { get; }
        public bool HasError { get; }
        public string ErrorMessage { get; }
        public ServiceResponseException Exception { get; }
        public bool HasException => Exception != null;
        public bool ResourceNotFound => StatusCode == HttpStatusCode.NotFound;
        public bool ResourceNullOrNotFound => ResourceNotFound || Content == null;

        protected ServiceResponse(HttpStatusCode statusCode, bool success, string errorMessage, Exception ex)
        {
            StatusCode = statusCode;
            RequestSuccess = success;
            ErrorMessage = errorMessage;
            Exception = (ServiceResponseException)ex;
            if (!success || (int)statusCode >= 500 || statusCode == HttpStatusCode.BadRequest || (int)statusCode == 0 || HasException)
            {
                HasError = true;
            }
        }

        public ServiceResponse(string content, IRestResponse source, HttpStatusCode statusCode, bool success, string errorMessage, Exception ex)
            : this(statusCode, success, errorMessage, ex)
        {
            Content = content;
            Source = source;
        }
    }

    public class ServiceResponse<T> : ServiceResponse 
        where T : new()
    {
        public new T Content { get; }
        public bool HasContent { get => Content != null; }
        public new bool ResourceNullOrNotFound => ResourceNotFound || Content == null;
        public new IRestResponse<T> Source { get; }
        public ServiceResponse(T content, IRestResponse<T> source, HttpStatusCode statusCode, bool success, string errorMessage, Exception ex)
            : base(statusCode, success, errorMessage, ex)
        {
            Content = content;
            Source = source;
        }

        public ServiceResponse<T> Then(Action<ServiceResponse<T>> callback)
        {
            if (RequestSuccess && callback != null)
            {
                callback.Invoke(this);
            }
            return this;
        }

        public ServiceResponse<T> Catch(Action<ServiceResponse> callback)
        {
            if (!RequestSuccess && callback != null)
            {
                callback.Invoke(this);
            }
            return this;
        }
    }
}
