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
    internal class ContentReferenceModelFilter : ContentApiModelFilter<ContentApiModel>
    {
        public override void Filter(ContentApiModel contentApiModel, ConverterContext converterContext)
        {
            if (!converterContext.Options.IncludeNumericContentIdentifier)
            {
                if (contentApiModel.ContentLink is object)
                {
                    contentApiModel.ContentLink.Id = contentApiModel.ContentLink.WorkId = null;
                    contentApiModel.ContentLink.Language = contentApiModel.Language;
                }
                if (contentApiModel.ParentLink is object)
                {
                    contentApiModel.ParentLink.Id = contentApiModel.ParentLink.WorkId = null;
                }
                

                //iterate over all properties and remove id and workid
                foreach (var property in contentApiModel.Properties)
                {
                    switch (property.Value)
                    {
                        case ContentModelReference contentModelReference:
                            ExcludeIdInfo(contentModelReference);
                            break;
                        case IEnumerable<ContentModelReference> contentModelReferenceList:
                            {
                                foreach (var contentModelReference in contentModelReferenceList)
                                {
                                    ExcludeIdInfo(contentModelReference);
                                }
                            }
                            break;
                        case IEnumerable<IContentItem> contentItemList:
                            {
                                foreach (var contentItem in contentItemList)
                                {
                                    ExcludeIdInfo(contentItem.ContentLink);
                                }
                            }
                            break;
                    }
                }
            }
            
            void ExcludeIdInfo(ContentModelReference modelReference)
            {
                modelReference.Id = modelReference.WorkId = null;
                if (modelReference.Expanded != null)
                {
                    modelReference.Language = modelReference.Expanded.Language;
                    Filter(modelReference.Expanded, converterContext);
                }
            }
        }
    }
}
