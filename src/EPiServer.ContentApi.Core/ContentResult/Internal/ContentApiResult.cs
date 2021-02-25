using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ServiceLocation;

namespace EPiServer.ContentApi.Core.ContentResult.Internal
{
    /// <summary>
    ///     Generic IHttpActionResult for all Content Api operations, setting standard serialization and encoding.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ContentApiResult<T> : IHttpActionResult
    {
        public T Value { get; }
        public HttpStatusCode StatusCode { get; }
        public IDictionary<string, string> Headers { get; }

        private readonly ContentApiOptions _contentApiOptions;
        private readonly ContentApiSerializerResolver _contentSerializerResolver;

        /// <summary>
        /// Creates an instance of <see cref="ContentApiResult{T}"/>
        /// </summary>
        public ContentApiResult(T value, HttpStatusCode statusCode) :
            this(value, statusCode, null, null, ServiceLocator.Current.GetInstance<ContentApiSerializerResolver>())
        {}        

        /// <summary>
        /// Creates an instance of <see cref="ContentApiResult{T}"/>
        /// </summary>
        public ContentApiResult(T value, HttpStatusCode statusCode, IDictionary<string, string> headers)
             : this(value, statusCode, headers, null, ServiceLocator.Current.GetInstance<ContentApiSerializerResolver>())
        {}

        /// <summary>
        /// Creates an instance of <see cref="ContentApiResult{T}"/>
        /// </summary>
        public ContentApiResult(T value, HttpStatusCode statusCode, ContentApiOptions contentApiOptions, ContentApiSerializerResolver contentSerializerResolver)
            :this(value, statusCode, null, contentApiOptions, contentSerializerResolver)
        {}

        /// <summary>
        /// Creates an instance of <see cref="ContentApiResult{T}"/>
        /// </summary>
        public ContentApiResult(T value, HttpStatusCode statusCode, IDictionary<string, string> headers, ContentApiOptions contentApiOptions, ContentApiSerializerResolver contentSerializerResolver)
        {
            Value = value;
            Headers = headers;
            StatusCode = statusCode;
            _contentApiOptions = contentApiOptions;
            _contentSerializerResolver = contentSerializerResolver;
        }

        /// <summary>
        /// build content for response.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(StatusCode);
            var serializer = _contentSerializerResolver.Resolve(_contentApiOptions);
            response.Content = new StringContent(serializer.Serialize(Value), serializer.Encoding, serializer.MediaType);
            SetHeaders(response);
            return Task.FromResult(response);
        }

        private void SetHeaders(HttpResponseMessage response)
        {
            if (Headers != null)
            {
                foreach (var keyValuePair in Headers)
                {
                    response.Headers.Add(keyValuePair.Key, keyValuePair.Value);
                }
            }
        }
    }
}
