using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Wrap;

namespace SC.SDK.NetStandard.BuildingBlocks.Http
{
    public class ServiceCaller : IServiceCaller
    {
        public string AuthorizationHeader { get; private set; }
        public bool IgnoreAuthorization { get; set; }
        public int RequestTimeout { get; }

        public IReadOnlyCollection<RequestHeader> Headers => _headers;
        private List<RequestHeader> _headers;

        private ILogger _logger;

        public ServiceCaller()
        {
            _headers = new List<RequestHeader>();
        }

        protected ServiceCaller(ILogger logger, int requestTimeout = 5)
            : this()
        {
            _logger = logger;
            RequestTimeout = requestTimeout;
        }

        public IServiceCaller Create<T>(ILogger<T> logger)
        {
            return new ServiceCaller(logger) as IServiceCaller;
        }

        public IServiceCaller Create<T>(ILogger<T> logger, int requestTimeout)
        {
            return new ServiceCaller(logger, requestTimeout) as IServiceCaller;
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

        public Task<ServiceResponse> DoGet(string baseUrl, string path, AsyncPolicyWrap<IRestResponse> policy)
        {
            return ExecuteRequest(baseUrl, path, Method.GET, null, null, null, policy);
        }

        public Task<ServiceResponse> DoGet(string baseUrl, string path, params RequestParameter[] parameters)
        {
            return ExecuteRequest(baseUrl, path, Method.GET, null, parameters.ToList());
        }

        public Task<ServiceResponse> DoGet(string baseUrl, string path, AsyncPolicyWrap<IRestResponse> policy, params RequestParameter[] parameters)
        {
            return ExecuteRequest(baseUrl, path, Method.GET, null, parameters.ToList(), null, policy);
        }

        public Task<ServiceResponse<T>> DoGet<T>(string baseUrl, string path) 
            where T : new()
        {
            return ExecuteRequest<T>(baseUrl, path, Method.GET);
        }

        public Task<ServiceResponse<T>> DoGet<T>(string baseUrl, string path, AsyncPolicyWrap<IRestResponse<T>> policy)
            where T : new()
        {
            return ExecuteRequest<T>(baseUrl, path, Method.GET, null, null, null, policy);
        }

        public Task<ServiceResponse<T>> DoGet<T>(string baseUrl, string path, params RequestParameter[] parameters)
            where T : new()
        {
            return ExecuteRequest<T>(baseUrl, path, Method.GET, null, parameters.ToList());
        }

        public Task<ServiceResponse<T>> DoGet<T>(string baseUrl, string path, AsyncPolicyWrap<IRestResponse<T>> policy, params RequestParameter[] parameters)
            where T : new()
        {
            return ExecuteRequest<T>(baseUrl, path, Method.GET, null, parameters.ToList(), null, policy);
        }

        public Task<ServiceResponse> DoPost(string baseUrl, string path)
        {
            return ExecuteRequest(baseUrl, path, Method.POST);
        }

        public Task<ServiceResponse> DoPost(string baseUrl, string path, object body)
        {
            return ExecuteRequest(baseUrl, path, Method.POST, body);
        }

        public Task<ServiceResponse> DoPost(string baseUrl, string path, object body, AsyncPolicyWrap<IRestResponse> policy)
        {
            return ExecuteRequest(baseUrl, path, Method.POST, body, null, null, policy);
        }

        public Task<ServiceResponse> DoPost(string baseUrl, string path, AsyncPolicyWrap<IRestResponse> policy, List<RequestParameter> parameters = null)
        {
            return ExecuteRequest(baseUrl, path, Method.POST, null, parameters, null, policy);
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

        public Task<ServiceResponse<T>> DoPost<T>(string baseUrl, string path, object body, AsyncPolicyWrap<IRestResponse<T>> policy)
            where T : new()
        {
            return ExecuteRequest<T>(baseUrl, path, Method.POST, body, null, null, policy);
        }

        public Task<ServiceResponse<T>> DoPost<T>(string baseUrl, string path, AsyncPolicyWrap<IRestResponse<T>> policy, List<RequestParameter> parameters = null)
            where T : new()
        {
            return ExecuteRequest<T>(baseUrl, path, Method.POST, null, parameters, null, policy);
        }

        public async Task<ServiceResponse<T>> ExecuteRequest<T>(string baseUrl,
            string path, Method method, object body = null, List<RequestParameter> parameters = null,
            List<RequestHeader> headers = null, AsyncPolicyWrap<IRestResponse<T>> policy = null)
            where T : new()
        {
            var client = new RestClient(baseUrl)
            {
                Timeout = RequestTimeout * 1000
            };

            var request = ConfigureRequest(path, method, body, parameters, headers);
            var fullUrl = client.BuildUri(request);

            //_policyRegistry.TryGet<AsyncCircuitBreakerPolicy>("HttpCircuitBreakerPolicy", out var defaultCircuitBreakerPolicy);
            //if (defaultCircuitBreakerPolicy == null)
            //{
            //    _policyRegistry.TryAdd("HttpCircuitBreakerPolicy", defaultCircuitBreakerPolicy);
            //}
            IRestResponse<T> response = null;

            if (policy == null)
                response = await client.ExecuteTaskAsync<T>(request);
            else
            {
                Context context = new Context().WithLogger(_logger);
                response = await policy.ExecuteAsync(ctx => client.ExecuteTaskAsync<T>(request), context);
            }

            //var cb = policy.GetPolicies();

            string responseErr = null;
            ServiceResponseException responseEx = null;
            HttpStatusCode responseCode = response.StatusCode;
            if (!response.IsSuccessful)
            {
                response.Data = default;
                responseErr = !string.IsNullOrWhiteSpace(response.ErrorMessage) ? response.ErrorMessage : response.Content;
                if (response.ResponseStatus == ResponseStatus.TimedOut)
                    responseCode = HttpStatusCode.GatewayTimeout;
                LogFailedRequest(response.ErrorException, responseCode, responseErr, fullUrl.ToString());
                if (!string.IsNullOrWhiteSpace(responseErr) || response.ErrorException != null)
                    responseEx =  new ServiceResponseException(response.StatusCode, responseErr, response.ErrorException);
            }

            return new ServiceResponse<T>(response.Data, response, responseCode, response.IsSuccessful, responseErr, responseEx);
        }

        public async Task<ServiceResponse> ExecuteRequest(string baseUrl, string path, Method method, object body = null, 
            List<RequestParameter> parameters = null, List<RequestHeader> headers = null, AsyncPolicyWrap<IRestResponse> policy = null)
        {
            var client = new RestClient(baseUrl)
            {
                Timeout = RequestTimeout * 1000
            };

            var request = ConfigureRequest(path, method, body, parameters, headers);
            var fullUrl = client.BuildUri(request);

            IRestResponse response = null;

            if (policy == null)
                response = await client.ExecuteTaskAsync(request);
            else
            {
                Context context = new Context().WithLogger(_logger);
                response = await policy.ExecuteAsync(ctx => client.ExecuteTaskAsync(request), context);
            }

            string responseErr = null;
            ServiceResponseException responseEx = null;
            HttpStatusCode responseCode = response.StatusCode;
            if (!response.IsSuccessful)
            {
                responseErr = !string.IsNullOrWhiteSpace(response.ErrorMessage) ? response.ErrorMessage : response.Content;
                if (response.ResponseStatus == ResponseStatus.TimedOut)
                    responseCode = HttpStatusCode.GatewayTimeout;
                LogFailedRequest(response.ErrorException, responseCode, responseErr, fullUrl.ToString());
                if (!string.IsNullOrWhiteSpace(responseErr) || response.ErrorException != null)
                    responseEx = new ServiceResponseException(response.StatusCode, responseErr, response.ErrorException);
            }

            return new ServiceResponse(response.Content, response, responseCode, response.IsSuccessful, responseErr, responseEx);
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

        private void LogFailedRequest(Exception exception, HttpStatusCode statusCode, string message, string destinationUrl)
        {
            if (_logger != null)
                _logger.LogError($"[ErroRequisicao][{statusCode}]  Request failed to {destinationUrl}", message, exception);
        }        
    }

    public static class ContextExtensions
    {
        private static readonly string LoggerKey = "ILogger";

        public static Context WithLogger(this Context context, ILogger logger)
        {
            context[LoggerKey] = logger;
            return context;
        }

        public static ILogger GetLogger(this Context context)
        {
            if (context.TryGetValue(LoggerKey, out object logger))
            {
                return logger as ILogger;
            }
            return null;
        }
    }

    public enum AuthenticationSchema
    {
        Basic,
        Bearer
    }

    public interface IServiceCaller
    {
        IServiceCaller Create<T>(ILogger<T> logger, int requestTimeout = 5);
        void AddAuthorization(AuthenticationSchema schema, string hash);
        void AddHeader(string key, string value);
        void RemoveHeader(string key);
        void ClearHeaders();
        Task<ServiceResponse> DoGet(string baseUrl, string path);
        Task<ServiceResponse> DoGet(string baseUrl, string path, AsyncPolicyWrap<IRestResponse> policy);
        Task<ServiceResponse> DoGet(string baseUrl, string path, params RequestParameter[] parameters);
        Task<ServiceResponse> DoGet(string baseUrl, string path, AsyncPolicyWrap<IRestResponse> policy, params RequestParameter[] parameters);
        Task<ServiceResponse<T>> DoGet<T>(string baseUrl, string path) where T : new();
        Task<ServiceResponse<T>> DoGet<T>(string baseUrl, string path, AsyncPolicyWrap<IRestResponse<T>> policy) where T : new();
        Task<ServiceResponse<T>> DoGet<T>(string baseUrl, string path, params RequestParameter[] parameters) where T : new();
        Task<ServiceResponse<T>> DoGet<T>(string baseUrl, string path, AsyncPolicyWrap<IRestResponse<T>> policy, params RequestParameter[] parameters) where T : new();
        Task<ServiceResponse> DoPost(string baseUrl, string path);
        Task<ServiceResponse> DoPost(string baseUrl, string path, object body);
        Task<ServiceResponse> DoPost(string baseUrl, string path, object body, AsyncPolicyWrap<IRestResponse> policy);
        Task<ServiceResponse> DoPost(string baseUrl, string path, AsyncPolicyWrap<IRestResponse> policy, List<RequestParameter> parameters = null);
        Task<ServiceResponse<T>> DoPost<T>(string baseUrl, string path) where T : new();
        Task<ServiceResponse<T>> DoPost<T>(string baseUrl, string path, object body) where T : new();
        Task<ServiceResponse<T>> DoPost<T>(string baseUrl, string path, object body, AsyncPolicyWrap<IRestResponse<T>> policy) where T : new();
        Task<ServiceResponse<T>> DoPost<T>(string baseUrl, string path, AsyncPolicyWrap<IRestResponse<T>> policy, List<RequestParameter> parameters = null) where T : new();

        Task<ServiceResponse<T>> ExecuteRequest<T>(string baseUrl,
            string path, Method method, object body = null, List<RequestParameter> parameters = null,
            List<RequestHeader> headers = null, AsyncPolicyWrap<IRestResponse<T>> policy = null)
            where T : new();

        Task<ServiceResponse> ExecuteRequest(string baseUrl, string path, Method method, object body = null,
            List<RequestParameter> parameters = null, List<RequestHeader> headers = null, AsyncPolicyWrap<IRestResponse> policy = null);
    }
}
