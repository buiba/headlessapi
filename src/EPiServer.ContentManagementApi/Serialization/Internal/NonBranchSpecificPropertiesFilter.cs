using System.Linq;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.DataAbstraction;
using EPiServer.ServiceLocation;

namespace EPiServer.ContentManagementApi.Serialization.Internal
{
    /// <summary>
    ///  Do not return non-branch specific properties when editing the non-master language content.
    /// </summary>
    [ServiceConfiguration(typeof(IContentApiModelFilter), Lifecycle = ServiceInstanceScope.Singleton)]
    public class NonBranchSpecificPropertiesFilter : ContentApiModelFilter<ContentApiModel>
    {
        private readonly IContentTypeRepository _contentTypeRepository;

        public NonBranchSpecificPropertiesFilter(IContentTypeRepository contentTypeRepository)
        {
            _contentTypeRepository = contentTypeRepository;
        }

        public override void Filter(ContentApiModel contentApiModel, ConverterContext converterContext)
        {
            if (!converterContext.IsContentManagementRequest || contentApiModel is null || contentApiModel.Properties.Count == 0)
            {
                return;
            }

            if (contentApiModel.MasterLanguage is null || contentApiModel.Language is null)
            {
                return;
            }

            if (contentApiModel.MasterLanguage.Name.Equals(contentApiModel.Language.Name, System.StringComparison.OrdinalIgnoreCase))
            {
                // master language content, do nothing
                return;
            }

            var contentType = _contentTypeRepository.Load(contentApiModel.ContentType.LastOrDefault());
            if (contentType is null || !contentType.PropertyDefinitions.Any())
            {
                return;
            }

            foreach (var property in contentType.PropertyDefinitions)
            {
                if (!property.LanguageSpecific)
                {
                    contentApiModel.Properties.Remove(property.Name);
                }
            }
        }
    }
}
