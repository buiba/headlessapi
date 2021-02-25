using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.DataAbstraction;

namespace EPiServer.DefinitionsApi.ContentTypes.Internal
{
    internal class ContentTypeAnalyzer
    {
        private readonly IContentTypeAnalyzer _contentTypeAnalyzer;
        private readonly ContentTypeRepository _contentTypeRepository;
        private readonly ContentTypeMapper _contentTypeMapper;

        protected ContentTypeAnalyzer() { }

        public ContentTypeAnalyzer(IContentTypeAnalyzer contentTypeAnalyzer, ContentTypeRepository contentTypeRepository, ContentTypeMapper contentTypeMapper)
        {
            _contentTypeAnalyzer = contentTypeAnalyzer ?? throw new ArgumentNullException(nameof(contentTypeAnalyzer));
            _contentTypeRepository = contentTypeRepository ?? throw new ArgumentNullException(nameof(contentTypeRepository));
            _contentTypeMapper = contentTypeMapper ?? throw new ArgumentNullException(nameof(contentTypeMapper));
        }

        public IEnumerable<ExternalContentTypeDifference> Analyze(ExternalContentType externalContentType)
        {
            ContentType internalContentType;
            if (externalContentType.Id == Guid.Empty)
            {
                internalContentType = _contentTypeRepository.Load(externalContentType.Name)?.CreateWritableClone() as ContentType;
            }
            else
            {
                internalContentType = _contentTypeRepository.Load(externalContentType.Id)?.CreateWritableClone() as ContentType;
            }

            if (internalContentType is object)
            {
                _contentTypeMapper.MapToInternal(externalContentType, internalContentType);

                var original = _contentTypeRepository.Load(internalContentType.GUID);

                return _contentTypeAnalyzer.Analyze(original, internalContentType).Select(Map);
            }

            return Enumerable.Empty<ExternalContentTypeDifference>();
        }

        private ExternalContentTypeDifference Map(ContentTypeDifference source)
        {
            if (source is null)
            {
                return ExternalContentTypeDifference.None;
            }

            if (!source.IsValid)
            {
                return ExternalContentTypeDifference.Invalid(source.Reason);
            }

            return new ExternalContentTypeDifference(source.VersionComponent, UpdateReasonForExternalContentType(source.Reason));
        }

        private string UpdateReasonForExternalContentType(string internalReason)
        {
            if (string.IsNullOrEmpty(internalReason))
            {
                return string.Empty;
            }

            foreach (var item in _propertiesReplacement)
            {
                if (!internalReason.Contains(item.Key))
                {
                    continue;
                }

                foreach (var replacementItem in item.Value)
                {
                    internalReason = internalReason.Replace(replacementItem.Key, replacementItem.Value);
                }

                break;
            }

            return internalReason;
        }

        private readonly Dictionary<string, Dictionary<string,string>> _propertiesReplacement = new Dictionary<string, Dictionary<string, string>>
        {
            {
                $"'{nameof(ContentType.SortOrder)}'",
                new Dictionary<string, string>
                {
                    { $"'{nameof(ContentType.SortOrder)}'", $"'{nameof(ExternalContentTypeEditSettings.Order)}'" }
                }
            },
            {
                "'Availability'",
                new Dictionary<string, string>
                {
                    { "'Availability'", $"'{nameof(ExternalContentTypeEditSettings.Available)}'" }
                }
            },
            {
                $"'{nameof(PropertyDefinition.LanguageSpecific)}'",
                new Dictionary<string, string>
                {
                    { $"'{nameof(PropertyDefinition.LanguageSpecific)}'", $"'{nameof(ExternalProperty.BranchSpecific)}'" }
                }
            },
            {
                $"'{nameof(PropertyDefinition.EditCaption)}'",
                new Dictionary<string, string>
                {
                    { $"'{nameof(PropertyDefinition.EditCaption)}'", $"'{nameof(ExternalPropertyEditSettings.DisplayName)}'" }
                }
            },
            {
                $"'{nameof(PropertyDefinition.FieldOrder)}'",
                new Dictionary<string, string>
                {
                    { $"'{nameof(PropertyDefinition.FieldOrder)}'", $"'{nameof(ExternalPropertyEditSettings.Order)}'" }
                }
            },
            {
                $"'{nameof(PropertyDefinition.DisplayEditUI)}'",
                new Dictionary<string, string>
                {
                    { $"'{nameof(PropertyDefinition.DisplayEditUI)}'", $"'{nameof(ExternalPropertyEditSettings.Visibility)}'" },
                    { $"'{true}'", $"'{nameof(VisibilityStatus.Default)}'" },
                    { $"'{false}'", $"'{nameof(VisibilityStatus.Hidden)}'" }
                }
            },
            {
                $"'{nameof(PropertyDefinition.EditorHint)}'",
                new Dictionary<string, string>
                {
                    { $"'{nameof(PropertyDefinition.EditorHint)}'", $"'{nameof(ExternalPropertyEditSettings.Hint)}'" }
                }
            },
            {
                $"'{nameof(PropertyDefinition.Tab)}'",
                new Dictionary<string, string>
                {
                    { $"'{nameof(PropertyDefinition.Tab)}'", $"'{nameof(ExternalPropertyEditSettings.GroupName)}'" }
                }
            }
        };
    }
}
