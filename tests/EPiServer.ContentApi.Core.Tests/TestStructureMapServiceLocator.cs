using EPiServer.ServiceLocation;
using StructureMap;
using System;
using System.Collections.Generic;

namespace EPiServer.ContentApi.Core.Tests
{
    //This is a rip-off from EPiServer.StructureMap which cant be referenced since it is not signed

    /// <summary>
    /// <see cref="IServiceLocator"/> implementation for StructureMap
    /// </summary>
    public class TestStructureMapServiceLocator : ServiceLocatorImplBase
    {
       private IContainer _container;

        /// <summary>
        /// Creates a new instance of <see cref="TestStructureMapServiceLocator"/>
        /// </summary>
        /// <param name="container"></param>
        public TestStructureMapServiceLocator(IContainer container)
        {
            this._container = container;
        }

        public override void Buildup(object service)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// When implemented by inheriting classes, this method will do the actual work of
        /// resolving all the requested service instances.
        /// </summary>
        /// <param name="serviceType">Type of service requested.</param>
        /// <returns>
        /// Sequence of service instance objects.
        /// </returns>
        protected override IEnumerable<object> DoGetAllInstances(Type serviceType)
        {
            foreach (object obj in _container.GetAllInstances(serviceType))
            {
                yield return obj;
            }
        }

        /// <summary>
        /// When implemented by inheriting classes, this method will do the actual work of resolving
        /// the requested service instance.
        /// </summary>
        /// <param name="serviceType">Type of instance requested.</param>
        /// <param name="key">Name of registered service you want. May be null.</param>
        /// <returns>
        /// The requested service instance.
        /// </returns>
        protected override object DoGetInstance(Type serviceType, string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return _container.GetInstance(serviceType);
            }
            else
            {
                return _container.GetInstance(serviceType, key);
            }
        }

        /// <summary>
        /// When implemented by inheriting classes, this method will look if the instance has been
        /// created and that it exists.
        /// </summary>
        /// <param name="serviceType">Type of instance requested.</param>
        /// <param name="key">Name of registered service you want. May be null.</param>
        /// <param name="instance">The requested service instance or null if it do not exist</param>
        /// <returns>
        /// True if the instance was found
        /// </returns>
        protected override bool DoTryGetExistingInstance(Type serviceType, string key, out object instance)
        {
            if (string.IsNullOrEmpty(key))
            {
                instance = _container.TryGetInstance(serviceType);
            }
            else
            {
                instance = _container.TryGetInstance(serviceType, key);
            }

            return instance != null;
        }
    }
}
