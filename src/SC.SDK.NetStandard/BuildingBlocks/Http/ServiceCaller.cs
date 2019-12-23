using SC.SDK.NetStandard.BuildingBlocks.Logging;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace SC.SDK.NetStandard.BuildingBlocks.Http
{
    public class ServiceCaller
    {
        public string AuthorizationHeader { get; private set; }
        public bool IgnoreAuthorization { get; set; }
        public bool UseFluentResponse { get; set; }

        public IReadOnlyCollection<RequestHeader> Headers => _headers;
        private List<RequestHeader> _headers;

        private readonly ILogger _logger;

        public ServiceCaller(ILogger logger)
        {
            _headers = new List<RequestHeader>();
            _logger = logger;
        }

        public ServiceCaller(ILogger logger, bool useFluentResponse)
        {
            _headers = new List<RequestHeader>();
            _logger = logger;
            UseFluentResponse = useFluentResponse;
        }

        public void AddAuthorization(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentNullException(nameof(token));
            AuthorizationHeader = token;
        }

        public void AddAuthorization(AuthenticationSchema schema, string hash)
        {
            if (string.IsNullOrWhiteSpace(hash))
                throw new ArgumentNullException(nameof(hash));

            switch (schema)
            {
                case AuthenticationSchema.Basic:
                    AddAuthorization($"Basic {hash}");
                    break;
                case AuthenticationSchema.Bearer:
                    AddAuthorization($"Bearer {hash}");
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

        public async Task<ServiceResponse<T>> ExecuteRequest<T>(string baseUrl, string path, Method method, object body = null, List<RequestParameter> parameters = null, List<RequestHeader> headers = null)
            where T : new()
        {
            var client = new RestClient(baseUrl);
            var request = ConfigureRequest(path, method, body, parameters, headers);

            IRestResponse<T> response = await client.ExecuteTaskAsync<T>(request);
            if (!response.IsSuccessful)
            {
                LogFailedRequest(response.ErrorException, response.StatusCode, response.ErrorMessage);
                if (!UseFluentResponse)
                    throw response.ErrorException;
            }

            return new ServiceResponse<T>(response.Data, response.StatusCode, response.IsSuccessful, response.ErrorMessage, response.ErrorException);
        }

        public async Task<ServiceResponse> ExecuteRequest(string baseUrl, string path, Method method, object body = null, List<RequestParameter> parameters = null, List<RequestHeader> headers = null)
        {
            var client = new RestClient(baseUrl);
            var request = ConfigureRequest(path, method, body, parameters, headers);

            IRestResponse response = await client.ExecuteTaskAsync(request);
            if (!response.IsSuccessful)
            {
                LogFailedRequest(response.ErrorException, response.StatusCode, response.ErrorMessage);
                if (!UseFluentResponse)
                    throw response.ErrorException;
            }

            return new ServiceResponse(response.Content, response.StatusCode, response.IsSuccessful, response.ErrorMessage, response.ErrorException);
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
            _logger.LogError(exception, $"[ErroRequisicao][{statusCode}] {message}");
        }
    }

    public enum AuthenticationSchema
    {
        Basic,
        Bearer
    }
}
