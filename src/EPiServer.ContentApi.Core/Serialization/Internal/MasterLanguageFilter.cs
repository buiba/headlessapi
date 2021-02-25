using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPiServer.ContentApi.Core.Serialization.Internal
{
    [ServiceConfiguration(typeof(IContentApiModelFilter), Lifecycle = ServiceInstanceScope.Singleton)]
    internal class MasterLanguageFilter : ContentApiModelFilter<ContentApiModel>
    {
        public override void Filter(ContentApiModel contentApiModel, ConverterContext converterContext)
        {
            if (!converterContext.Options.IncludeMasterLanguage)
            {
                contentApiModel.MasterLanguage = null;
            }
        }
    }
}
