using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ServiceLocation;
using Microsoft.Extensions.Internal;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;

namespace EPiServer.ContentApi.Core.ContentResult
{
    /// <summary>
    /// Responsible for building response content with a given serializer
    /// </summary>
    [ServiceConfiguration(typeof(ContentResultService), Lifecycle = ServiceInstanceScope.Singleton)]
    public class ContentResultService
    {
        protected readonly IContentApiSerializer _contentApiSerializer;

        /// <exclude/>
        protected ContentResultService()
        { }

        /// <summary>
        /// Create a new instance of <see cref="ContentResultService" />
        /// </summary>
        public ContentResultService(IContentApiSerializer contentApiSerializer)
        {
            _contentApiSerializer = contentApiSerializer;
        }


        /// <summary>
        /// Build string content from object use given serializer
        /// </summary>
        public virtual StringContent BuildContent(object value)
        {
            var content = _contentApiSerializer.Serialize(value);
            return new StringContent(content, _contentApiSerializer.Encoding, _contentApiSerializer.MediaType);
        }

      
    }
}
