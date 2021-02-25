using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.Core;
using EPiServer.Web;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace EPiServer.ContentApi.Core.Serialization
{
    /// <summary>
    /// Context used when converting <see cref="IContent"/> instances to <see cref="ContentApiModel"/>
    /// <para>
    /// Preview API: This API is current in preview state meaning it might change between minor versions
    /// </para>
    /// </summary>
    public class ConverterContext
    {
        private bool _expandAll;
        private HashSet<string> _expandedProperties;
        private HashSet<string> _selectedProperties;

        internal ConverterContext Parent { get; set; }

        /// <summary>
        /// Creates a new instance of <see cref="ConverterContext"/>
        /// </summary>
        public ConverterContext(
            ContentApiOptions contentApiOptions,
            string select,
            string expand,
            bool excludePersonalizedContent,
            CultureInfo language)
            : this(
                  contentApiOptions,
                  select,
                  expand,
                  excludePersonalizedContent,
                  language,
                  ContextMode.Default,
                  false)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="ConverterContext"/>
        /// </summary>
        public ConverterContext(
            ContentApiOptions contentApiOptions,
            string select,
            string expand,
            bool excludePersonalizedContent,
            CultureInfo language,
            ContextMode contextMode) : this(
                contentApiOptions,
                select,
                expand,
                excludePersonalizedContent,
                language,
                contextMode,
                false)
        {
        }

        public ConverterContext(
            ContentApiOptions contentApiOptions,
            string select,
            string expand,
            bool excludePersonalizedContent,
            CultureInfo language,
            ContextMode contextMode,
            bool isContentManagementRequest)
        {
            Options = contentApiOptions;

            if ("*".Equals(expand))
            {
                _expandAll = true;
            }

            _expandedProperties = string.IsNullOrWhiteSpace(expand) ? new HashSet<string>() : new HashSet<string>(expand.Split(',').Select(x => x.Trim()), StringComparer.OrdinalIgnoreCase);
            _selectedProperties = string.IsNullOrWhiteSpace(select) ? new HashSet<string>() : new HashSet<string>(select.Split(',').Select(x => x.Trim()), StringComparer.OrdinalIgnoreCase);

            ExcludePersonalizedContent = excludePersonalizedContent;
            Language = language ?? CultureInfo.InvariantCulture;
            ContextMode = contextMode;
            IsContentManagementRequest = isContentManagementRequest;
        }

        internal ConverterContext(ConverterContext parentContext)
            : this(
                  parentContext.Options,
                  null,
                  null,
                  parentContext.ExcludePersonalizedContent,
                  parentContext.Language,
                  parentContext.ContextMode)
        {
            Parent = parentContext;
        }

        /// <summary>
        /// The options to use during mapping
        /// </summary>
        public ContentApiOptions Options { get; }

        /// <summary>
        /// Contains properties that should be expanded, separated by comma. Eg: expand=MainContentArea,productPageLinks. Expand='*' means all properties should be expanded
        /// </summary>
        public IEnumerable<string> ExpandedProperties => _expandedProperties;

        /// <summary>
        /// Contains properties that should be selected, separated by comma. Eg: select=MainContentArea,productPageLinks. If no specific properties are selected is all returned
        /// </summary>
        public IEnumerable<string> SelectedProperties => _selectedProperties;

        /// <summary>
        /// Indicates whether or not to return personalization data in the instance of the ContentApiModel
        /// </summary>
        public bool ExcludePersonalizedContent { get; }

        /// <summary>
        /// The content language for this context
        /// </summary>
        public CultureInfo Language { get; }

        /// <summary>
        /// The context mode this converter context should operate under.
        /// </summary>
        public ContextMode ContextMode { get; }

        /// <summary>
        /// Determines if property with specified <paramref name="propertyName"/> should be expanded
        /// </summary>
        /// <param name="propertyName">The property name that expand should be determined for</param>
        /// <returns>true if property should be expanded else false</returns>
        public bool ShouldExpand(string propertyName) => _expandAll || _expandedProperties.Contains(propertyName);

        /// <summary>
        /// The request comes from ContentManagementApi if true
        /// </summary>
        public bool IsContentManagementRequest { get; }

        /// <summary>
        /// Indicates whether or not to return personalization data in the property model
        /// </summary>
        /// <returns>true if request is coming from ContentManagementApi, otherwise false</returns>
        internal bool ShouldIncludeAllPersonalizedContent => IsContentManagementRequest;

        /// <summary>
        /// Indicates whether or not to return expand data in the property model
        /// </summary>
        /// <returns>true if request is coming from ContentManagementApi, otherwise false</returns>
        internal bool ShouldIgnoreExpandValue => IsContentManagementRequest;
    }
}
