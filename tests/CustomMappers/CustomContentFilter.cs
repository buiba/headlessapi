using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Text;

namespace CustomMappers
{
    [ServiceConfiguration(typeof(IContentFilter), Lifecycle = ServiceInstanceScope.Singleton)]
    internal class CustomContentFilter : ContentFilter<PageWithCustomHandledProperty>
    {
        public override void Filter(PageWithCustomHandledProperty content, ConverterContext converterContext)
        {
            content.PropertyToBeRemovedByFilter = null;
        }
    }
}
