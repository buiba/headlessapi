using System;
using System.Net.Http;
using Microsoft.Owin.Testing;

namespace EPiServer.ContentApi.Error.Infrastructure
{
    public class ServiceFixture : IDisposable
    {
        private readonly TestServer _testServer;

        public ServiceFixture()
        {
            _testServer = TestServer.Create<Startup>();
        }

        public HttpClient HttpClient => _testServer.HttpClient;

        public void Dispose()
        {
            _testServer.Dispose();
        }
    }
}
