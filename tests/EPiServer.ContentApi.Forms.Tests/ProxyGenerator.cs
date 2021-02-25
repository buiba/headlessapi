using EPiServer.Construction;
using EPiServer.Construction.Internal;
using EPiServer.Core;
using EPiServer.DataAbstraction.RuntimeModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPiServer.ContentApi.Forms.Tests
{
    public class ProxyGenerator
    {
        /// <summary>
        /// Creates EPi shared block, that is, a block which is castable to <see cref="IContent"/>, <see cref="IVersionable"/> and other property interfaces for shared blocks.
        /// </summary>
        /// <typeparam name="T">Block type.</typeparam>
        /// <returns>Shared block of the specified type. Cannot be null.</returns>
        public static T OfType<T>() where T : IContentData
        {
            var sharedBlockFactory = new SharedBlockFactory(null, new ConstructorParameterResolver(), () => new ContentDataInterceptor(new ContentDataInterceptorHandler(new ConstructorParameterResolver())));
            var block = sharedBlockFactory.CreateSharedBlock(typeof(T));
            return (T)block;

        }
    }
}
