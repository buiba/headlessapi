using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomMappers
{
    [ServiceConfiguration(typeof(IContentApiModelFilter), Lifecycle = ServiceInstanceScope.Singleton)]
    public class CountingContentModelFilter : ContentApiModelFilter<ContentApiModel>
    {
        public IList<ContentModelReference> CalledItems { get; } = new List<ContentModelReference>();

        public override void Filter(ContentApiModel contentApiModel, ConverterContext converterContext)
        {
            CalledItems.Add(contentApiModel.ContentLink);
        }

    }
}
