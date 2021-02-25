using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using EPiServer.ContentApi.Error.Internal;
using EPiServer.DataAbstraction;
using EPiServer.DataAbstraction.RuntimeModel;
using EPiServer.DataAbstraction.RuntimeModel.Internal;

namespace EPiServer.DefinitionsApi.ContentTypes.Internal
{
    internal class DefaultExternalContentTypeServiceExtension : IExternalContentTypeServiceExtension
    {
        private readonly ContentTypeRepository _internalRepository;
        private readonly IContentTypeBaseProvider _contentTypeBaseProvider;

        public DefaultExternalContentTypeServiceExtension(
            ContentTypeRepository contentTypeRepository,
            IEnumerable<IContentTypeBaseProvider> contentTypeBaseProviders)
        {
            _internalRepository = contentTypeRepository ?? throw new ArgumentNullException(nameof(contentTypeRepository));
            _contentTypeBaseProvider = contentTypeBaseProviders.FirstOrDefault(x => x is DefaultContentTypeBaseProvider) ?? new DefaultContentTypeBaseProvider();
        }

        public void Save(IEnumerable<ExternalContentType> externalContentTypes, IEnumerable<ContentType> internalContentTypes, ContentTypeSaveOptions contentTypeSaveOptions)
        {
            var cmsContentTypes = internalContentTypes.Where(x => CanHandle(x)).ToList();

            try
            {
                _internalRepository.Save(cmsContentTypes, contentTypeSaveOptions);
            }
            catch (InvalidContentTypeBaseException ex)
            {
                throw new ErrorException(HttpStatusCode.Conflict, ex.Message, ProblemCode.InvalidBase);
            }
            catch (ConflictingResourceException ex)
            {
                throw new ErrorException(HttpStatusCode.Conflict, ex.Message);
            }
            catch (VersionValidationException ex)
            {
                throw new ErrorException(HttpStatusCode.Conflict, ex.Message, ProblemCode.Version);
            }
        }

        public bool TryDelete(Guid id)
        {
            var contentType = _internalRepository.Load(id);

            if (contentType is object && CanHandle(contentType))
            {
                _internalRepository.Delete(contentType);
                return true;
            }

            return false;
        }

        private bool CanHandle(ContentType contentType) => _contentTypeBaseProvider.Resolve(contentType.Base) is null ? false : true;
    }
}
