using System;
using System.Net.Http;

namespace Service.Client
{
    public class ApiServiceClient: IApiServiceClient
    {
        private readonly HttpClient _httpClient;
        public ApiServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
    }
}
