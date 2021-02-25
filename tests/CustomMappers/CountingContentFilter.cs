using EPiServer.ContentApi.Core.Serialization;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomMappers
{
    [ServiceConfiguration(typeof(IContentFilter), Lifecycle = ServiceInstanceScope.Singleton)]
    public class CountingContentFilter : ContentFilter<IContent>
    {
        public IList<Guid> CalledItems { get; } = new List<Guid>();
        public override void Filter(IContent content, ConverterContext converterContext)
        {
            CalledItems.Add(content.ContentGuid);
        }
    }
}
