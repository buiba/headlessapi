using EPiServer.Data.Dynamic;
using EPiServer.ServiceLocation;
using System;

namespace EPiServer.ContentApi.OAuth.Internal
{
	/// <summary>
	/// Factory to get store. This helps us to write UT more easily with Store
	/// </summary>
	[ServiceConfiguration(Lifecycle = ServiceInstanceScope.Hybrid)]
	public class ContentApiDynamicDataStoreFactory
	{
		/// <summary>
		/// Get <see cref="DynamicDataStore"/> by type
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public virtual DynamicDataStore GetStore(Type type)
		{
			var store = DynamicDataStoreFactory.Instance.GetStore(type);
			if (store == null)
			{
				store = DynamicDataStoreFactory.Instance.CreateStore(type);
			}
			return store;
		}
	}
}
