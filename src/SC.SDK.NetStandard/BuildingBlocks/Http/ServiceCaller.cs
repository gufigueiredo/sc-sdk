﻿using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Net.Cache;
using Microsoft.Extensions.Logging;
using Polly;

namespace SC.SDK.NetStandard.BuildingBlocks.Http
{
    public class ServiceCaller : IServiceCaller
    {
        public string AuthorizationHeader { get; private set; }
        public bool IgnoreAuthorization { get; set; }
        public bool UseFluentResponse { get; set; }

        public IReadOnlyCollection<RequestHeader> Headers => _headers;
        private List<RequestHeader> _headers;

        private ILogger _logger;

        public ServiceCaller()
        {
            _headers = new List<RequestHeader>();
        }

        public IServiceCaller Create()
        {
            return new ServiceCaller() as IServiceCaller;
        }

        public IServiceCaller Create(ILogger logger)
        {
            return new ServiceCaller()
            {
                _logger = logger
            } as IServiceCaller;
        }

        public IServiceCaller Create(ILogger logger, bool useFluentResponse)
        {
            return new ServiceCaller()
            {
                _logger = logger, 
                UseFluentResponse = useFluentResponse
            } as IServiceCaller;
        }

        public void AddAuthorization(AuthenticationSchema schema, string hash)
        {
            if (string.IsNullOrWhiteSpace(hash))
                throw new ArgumentNullException(nameof(hash));

            switch (schema)
            {
                case AuthenticationSchema.Basic:
                    AuthorizationHeader = $"Basic {hash}";
                    break;
                case AuthenticationSchema.Bearer:
                    AuthorizationHeader = $"Bearer {hash}";
                    break;
            }
        }

        public void AddHeader(string key, string value)
        {
            _headers.Add(RequestHeader.Create(key, value));
        }

        public void RemoveHeader(string key)
        {
            var header = _headers.FirstOrDefault(h => h.Key == key);
            if (header != null)
                _headers.Remove(header);
        }

        public void ClearHeaders() => _headers.Clear();

        public Task<ServiceResponse> DoGet(string baseUrl, string path)
        {
            return ExecuteRequest(baseUrl, path, Method.GET);
        }

        public Task<ServiceResponse<T>> DoGet<T>(string baseUrl, string path) 
            where T : new()
        {
            return ExecuteRequest<T>(baseUrl, path, Method.GET);
        }

        public Task<ServiceResponse> DoGet(string baseUrl, string path, params RequestParameter[] parameters)
        {
            return ExecuteRequest(baseUrl, path, Method.GET, null, parameters.ToList());
        }

        public Task<ServiceResponse<T>> DoGet<T>(string baseUrl, string path, params RequestParameter[] parameters)
            where T : new()
        {
            return ExecuteRequest<T>(baseUrl, path, Method.GET, null, parameters.ToList());
        }

        public Task<ServiceResponse> DoPost(string baseUrl, string path)
        {
            return ExecuteRequest(baseUrl, path, Method.POST);
        }

        public Task<ServiceResponse> DoPost(string baseUrl, string path, object body)
        {
            return ExecuteRequest(baseUrl, path, Method.POST, body);
        }

        public Task<ServiceResponse> DoPost(string baseUrl, string path, List<RequestParameter> parameters = null)
        {
            return ExecuteRequest(baseUrl, path, Method.POST, null, parameters);
        }

        public Task<ServiceResponse<T>> DoPost<T>(string baseUrl, string path)
            where T : new()
        {
            return ExecuteRequest<T>(baseUrl, path, Method.POST);
        }

        public Task<ServiceResponse<T>> DoPost<T>(string baseUrl, string path, object body)
            where T : new()
        {
            return ExecuteRequest<T>(baseUrl, path, Method.POST, body);
        }

        public Task<ServiceResponse<T>> DoPost<T>(string baseUrl, string path, List<RequestParameter> parameters = null)
            where T : new()
        {
            return ExecuteRequest<T>(baseUrl, path, Method.POST, null, parameters);
        }

        public async Task<ServiceResponse<T>> ExecuteRequest<T>(string baseUrl,
            string path, Method method, object body = null, List<RequestParameter> parameters = null,
            List<RequestHeader> headers = null, int retries = 3, TimeSpan? timeBetweenRetries = null)
            where T : new()
        {
            var client = new RestClient(baseUrl);
            var request = ConfigureRequest(path, method, body, parameters, headers);
            var fullUrl = client.BuildUri(request);

            var policy = Policy.
                HandleResult<IRestResponse<T>>(r => r.StatusCode != HttpStatusCode.NotFound && !r.IsSuccessful)
                .Or<Exception>()
                .WaitAndRetryAsync(retries, retryAttemp => timeBetweenRetries ?? TimeSpan.FromSeconds(Math.Pow(2, retryAttemp)), (exception, timeSpan, context) =>
                {
                    _logger.LogError(exception.Exception, $"[ExecuteRequest][{exception.Result?.StatusCode.ToString()}] Request failed to {fullUrl.ToString()}");
                });

            IRestResponse<T> response = await policy.ExecuteAsync(() => client.ExecuteTaskAsync<T>(request));

            string responseErr = null;
            ServiceResponseException responseEx = null;
            if (!response.IsSuccessful)
            {
                responseErr = string.IsNullOrWhiteSpace(response.ErrorMessage) ? response.ErrorMessage : response.Content;
                LogFailedRequest(response.ErrorException, response.StatusCode, responseErr);
                if (!string.IsNullOrWhiteSpace(responseErr) || response.ErrorException != null)
                    responseEx =  new ServiceResponseException(response.StatusCode, responseErr, response.ErrorException);
            }

            return new ServiceResponse<T>(response.Data, response, response.StatusCode, response.IsSuccessful, responseErr, responseEx);
        }

        public async Task<ServiceResponse> ExecuteRequest(string baseUrl, string path, Method method, object body = null, 
            List<RequestParameter> parameters = null, List<RequestHeader> headers = null, int retries = 3, TimeSpan? timeBetweenRetries = null)
        {
            var client = new RestClient(baseUrl);
            var request = ConfigureRequest(path, method, body, parameters, headers);
            var fullUrl = client.BuildUri(request);

            var policy = Policy.
                HandleResult<IRestResponse>(r => r.StatusCode != HttpStatusCode.NotFound && !r.IsSuccessful)
                .Or<Exception>()
                .WaitAndRetryAsync(retries, retryAttemp => timeBetweenRetries ?? TimeSpan.FromSeconds(Math.Pow(2, retryAttemp)), (exception, timeSpan, context) =>
                {
                    _logger.LogError(exception.Exception, $"[ExecuteRequest][{exception.Result?.StatusCode.ToString()}] Request failed to {fullUrl.ToString()}");
                });

            IRestResponse response = await policy.ExecuteAsync(() => client.ExecuteTaskAsync(request));

            string responseErr = null;
            ServiceResponseException responseEx = null;
            if (!response.IsSuccessful)
            {
                responseErr = string.IsNullOrWhiteSpace(response.ErrorMessage) ? response.ErrorMessage : response.Content;
                LogFailedRequest(response.ErrorException, response.StatusCode, responseErr);
                responseEx = new ServiceResponseException(response.StatusCode, responseErr, response.ErrorException);
            }

            return new ServiceResponse(response.Content, response, response.StatusCode, response.IsSuccessful, responseErr, responseEx);
        }

        private RestRequest ConfigureRequest(string path, Method method, object body = null, List<RequestParameter> parameters = null, List<RequestHeader> headers = null)
        {
            var request = new RestRequest(path, method);
            if (headers != null && headers.Count > 0)
                headers.ForEach(h => request.AddHeader(h.Key, h.Value));

            if (_headers.Count > 0)
            {
                foreach (var header in _headers)
                {
                    var headerAlreadyExists = headers != null && headers.Any(h => h.Key == header.Key);
                    if (!headerAlreadyExists)
                        request.AddHeader(header.Key, header.Value);
                }
            }

            if (!string.IsNullOrWhiteSpace(AuthorizationHeader) && !IgnoreAuthorization)
            {
                var overwriteAuthorization = headers != null && headers.Any(h => h.Key == "Authorization");
                if (!overwriteAuthorization)
                    request.AddHeader("Authorization", AuthorizationHeader);
            }

            if (body != null)
                request.AddJsonBody(body);

            if (parameters != null)
                parameters.ForEach(p => request.AddParameter(p.Name, p.Value));

            return request;
        }

        private void LogFailedRequest(Exception exception, HttpStatusCode statusCode, string message)
        {
            if (_logger != null)
                _logger.LogError(exception, $"[ErroRequisicao][{statusCode}] {message}");
        }
    }

    public enum AuthenticationSchema
    {
        Basic,
        Bearer
    }

    public interface IServiceCaller
    {
        IServiceCaller Create();
        IServiceCaller Create(ILogger logger);
        IServiceCaller Create(ILogger logger, bool useFluentResponse);
        void AddAuthorization(AuthenticationSchema schema, string hash);
        void AddHeader(string key, string value);
        void RemoveHeader(string key);
        void ClearHeaders();
        Task<ServiceResponse> DoGet(string baseUrl, string path);
        Task<ServiceResponse> DoGet(string baseUrl, string path, params RequestParameter[] parameters);
        Task<ServiceResponse<T>> DoGet<T>(string baseUrl, string path) where T : new();
        Task<ServiceResponse<T>> DoGet<T>(string baseUrl, string path, params RequestParameter[] parameters) where T : new();
        Task<ServiceResponse> DoPost(string baseUrl, string path);
        Task<ServiceResponse> DoPost(string baseUrl, string path, object body);
        Task<ServiceResponse> DoPost(string baseUrl, string path, List<RequestParameter> parameters = null);
        Task<ServiceResponse<T>> DoPost<T>(string baseUrl, string path) where T : new();
        Task<ServiceResponse<T>> DoPost<T>(string baseUrl, string path, object body) where T : new();
        Task<ServiceResponse<T>> DoPost<T>(string baseUrl, string path, List<RequestParameter> parameters = null) where T : new();
    }
}
