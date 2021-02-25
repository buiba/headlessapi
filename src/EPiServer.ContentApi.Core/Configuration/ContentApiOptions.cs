using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace EPiServer.ContentApi.Core.Configuration
{
    /// <summary>
    ///     Options to control behavior of the Content Api
    /// </summary>
    public class ContentApiOptions : ICloneable
    {
        /// <summary>
        /// Creates a new instance of <see cref="ContentApiOptions"/>
        /// </summary>
        public ContentApiOptions()
        { }

        /// <summary>
        /// Creates a new instance of <see cref="ContentApiOptions"/>
        /// </summary>
        public ContentApiOptions(string requiredRole, bool multiSiteFilteringEnabled, bool siteDefinitionApiEnabled, string minimumRole, IEnumerable<ContentApiClient> clients)
        {
            RequiredRole = requiredRole;
            MultiSiteFilteringEnabled = multiSiteFilteringEnabled;
            SiteDefinitionApiEnabled = siteDefinitionApiEnabled;
            MinimumRoles = minimumRole;
            Clients = clients;
        }

        /// <summary>
        ///     Comma separated list of Minimum Roles required to hit any method on the Content API.
        ///     Default is 'contentapiread' and it should be mapped to WebEditors, Administrators, and WebAdmins
        /// </summary>
        public virtual string MinimumRoles
        {
            get; protected set;
        }

        /// <summary>
        ///     Sets the required role that must be assigned to content in order for it to be returned in API calls
        ///     Default is 'contentapiread' and it should be mapped to WebEditors, Administrators, and WebAdmins
        /// </summary>
        public virtual string RequiredRole
        {
            get; protected set;
        }

        /// <summary>
        /// Controls which content is exposed in the Content Api.
        /// When enabled, Content and Site Definition calls are filtered to only return content associated with the current site, based on the context of the request.
        /// When disabled, Content and Site Definition calls return all content, regardless of associated site.
        /// </summary>
        public virtual bool MultiSiteFilteringEnabled
        {
            get; protected set;
        }

        /// <summary>
        /// Controls whether or not the Site Definition Api is enabled. When disabled, the API will return a 403 Forbidden response.
        /// </summary>
        public virtual bool SiteDefinitionApiEnabled
        {
            get; protected set;
        }


        /// <summary>
        /// Controls wheter the existence of a template should be validated when constructing the url for a content item. If template is validated
        /// then content item without a template will have url set to null.
        /// </summary>
        /// <remarks>Default value is true</remarks>
        public virtual bool ValidateTemplateForContentUrl { get; protected set; } = true;

        /// <summary>
        /// Controls wheter the serialized representation of custom properties should only include the value of all properties rather than both value and property type.
        /// </summary>
        public virtual bool FlattenPropertyModel { get; protected set; }

        /// <summary>
        /// Controls if all content should be resolved using Contextmode Default or if Edit and Preview is allowed
        /// </summary>
        public virtual bool EnablePreviewMode { get; protected set; } = false;

        /// <summary>
        /// Controls wheter values with null values should be included (for example content property values)-
        /// </summary>
        /// <remarks>
        /// Default value is true.
        /// </remarks>
        public virtual bool IncludeNullValues { get; protected set; } = true;

        /// <summary>
        /// Controls wheter property for master language should be included
        /// </summary>
        /// <remarks>
        /// Default value is true.
        /// </remarks>
        public virtual bool IncludeMasterLanguage { get; protected set; } = true;

        /// <summary>
        /// Controls if the site model should include host information when returned.
        /// </summary>
        /// <remarks>
        /// Default value is true.
        /// </remarks>
        public virtual bool IncludeSiteHosts { get; protected set; } = true;

        /// <summary>
        /// Controls if the site model should include internal content roots or if they should be left out.
        /// </summary>
        /// <remarks>
        /// Default value is true.
        /// </remarks>
        public virtual bool IncludeInternalContentRoots { get; protected set; } = true;

        /// <summary>
        /// Controls wheter the serialized representation of the content types should only include a string value for each content type or if extended information should be included.
        /// </summary>
        /// <remarks>
        /// Preview API: This setting is current in preview state meaning it might change between minor versions
        /// </remarks>
        [Obsolete("This preview setting will be removed in an upcoming version")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual bool ExtendedContentTypeModel { get; protected set; }

        /// <summary>
        /// Controls valid Clients for usage with OAuth. Use separate clients to segment refresh token expiration and CORs considerations per client application
        /// </summary>
        public virtual IEnumerable<ContentApiClient> Clients
        {
            get; protected set;
        }

        /// <summary>
        /// If set to true the returned value will always be an absolute URL; otherwise it will depend on the current context. 
        /// The default value is false.
        /// </summary>
        /// <remarks>
        /// Preview API: This setting is current in preview state meaning it might change between minor versions
        /// </remarks>
        public virtual bool ForceAbsolute { get; protected set; } = false;

        /// <summary>
        /// The language behavior for expanded properties.<see cref="ExpandedLanguageBehavior"/>
        /// The default value is ContentLanguage.
        /// </summary>
        /// <remarks>
        /// Preview API: This setting is current in preview state meaning it might change between minor versions
        /// </remarks>
        public virtual ExpandedLanguageBehavior ExpandedBehavior { get; protected set; } = ExpandedLanguageBehavior.ContentLanguage;

        /// <summary>
        ///  Control s-maxage on Cache-Control header for cachable responses. 
        /// </summary>
        public virtual TimeSpan HttpResponseExpireTime
        {
            get; protected set;
        }

        /// <summary>
        /// If set to false the Id and Work Id properties in Content Reference model will be removed from the JSON.
        /// The default value is true.
        /// </summary>
        /// <remarks>
        /// Preview API: This setting is current in preview state meaning it might change between minor versions
        /// </remarks>
        public virtual bool IncludeNumericContentIdentifier { get; protected set; } = true;

        /// <summary>
        /// Set minimum roles
        /// </summary>
        public virtual ContentApiOptions SetMinimumRoles(string minimumRoles)
        {
            MinimumRoles = minimumRoles;
            return this;
        }

        /// <summary>
        /// Set required roles
        /// </summary>
        public virtual ContentApiOptions SetRequiredRole(string requiredRole)
        {
            RequiredRole = requiredRole;
            return this;
        }

        /// <summary>
        /// Set multiSiteFilteringEnabled
        /// </summary>
        public virtual ContentApiOptions SetMultiSiteFilteringEnabled(bool multiSiteFilteringEnabled)
        {
            MultiSiteFilteringEnabled = multiSiteFilteringEnabled;
            return this;
        }

        /// <summary>
        /// Set SiteDefinitionApiEnabled
        /// </summary>
        public virtual ContentApiOptions SetSiteDefinitionApiEnabled(bool siteDefinitionApiEnabled)
        {
            SiteDefinitionApiEnabled = siteDefinitionApiEnabled;
            return this;
        }

        /// <summary>
        /// Set Clients
        /// </summary>
        public virtual ContentApiOptions SetClients(IEnumerable<ContentApiClient> clients)
        {
            Clients = clients;
            return this;
        }

        /// <summary>
        /// Sets <see cref="ValidateTemplateForContentUrl"/>
        /// </summary>
        public virtual ContentApiOptions SetValidateTemplateForContentUrl(bool validateTemplate)
        {
            ValidateTemplateForContentUrl = validateTemplate;
            return this;
        }

        /// <summary>
        /// Sets <see cref="FlattenPropertyModel"/>
        /// </summary>
        public virtual ContentApiOptions SetFlattenPropertyModel(bool flatten)
        {
            FlattenPropertyModel = flatten;
            return this;
        }

        /// <summary>
        /// Sets <see cref="EnablePreviewMode"/>
        /// </summary>
        public virtual ContentApiOptions SetEnablePreviewMode(bool enablePreviewMode)
        {
            EnablePreviewMode = enablePreviewMode;
            return this;
        }

        /// <summary>
        /// Sets <see cref="IncludeNullValues"/>
        /// </summary>
        public virtual ContentApiOptions SetIncludeNullValues(bool includeNullValues)
        {
            IncludeNullValues = includeNullValues;
            return this;
        }

        /// <summary>
        /// Sets <see cref="IncludeMasterLanguage"/>
        /// </summary>
        public virtual ContentApiOptions SetIncludeMasterLanguage(bool includeMasterLanguage)
        {
            IncludeMasterLanguage = includeMasterLanguage;
            return this;
        }

        /// <summary>
        /// Sets <see cref="ExpandedBehavior"/>
        /// </summary>
        public virtual ContentApiOptions SetExpandedBehavior(ExpandedLanguageBehavior expandedBehavior)
        {
            ExpandedBehavior = expandedBehavior;
            return this;
        }

        /// <summary>
        /// Sets <see cref="IncludeSiteHosts"/>
        /// </summary>
        public virtual ContentApiOptions SetIncludeSiteHosts(bool includeSiteHosts)
        {
            IncludeSiteHosts = includeSiteHosts;
            return this;
        }

        /// <summary>
        /// Sets <see cref="IncludeInternalContentRoots"/>
        /// </summary>
        public virtual ContentApiOptions SetIncludeInternalContentRoots(bool includeInternalContentRoots)
        {
            IncludeInternalContentRoots = includeInternalContentRoots;
            return this;
        }

        /// <summary>
        /// Sets <see cref="ExtendedContentTypeModel"/>
        /// </summary>
        /// <remarks>
        /// Preview API: This setting is current in preview state meaning it might change between minor versions
        /// </remarks>
        [Obsolete("This preview setting will be removed in an upcoming version")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual ContentApiOptions SetExtendedContentTypeModel(bool extend)
        {
            ExtendedContentTypeModel = extend;
            return this;
        }

        /// <summary>
        /// Sets <see cref="ForceAbsolute"/>
        /// </summary>
        public virtual ContentApiOptions SetForceAbsolute(bool forceAbsolute)
        {
            ForceAbsolute = forceAbsolute;
            return this;
        }

        /// <summary>
        /// Sets <see cref="HttpResponseExpireTime"/>
        /// </summary>
        public virtual ContentApiOptions SetHttpResponseExpireTime(TimeSpan expireTime)
        {
            HttpResponseExpireTime = expireTime;
            return this;
        }

        /// <summary>
        /// Sets <see cref="IncludeNumericContentIdentifier "/>
        /// </summary>
        public virtual ContentApiOptions SetIncludeNumericContentIdentifier(bool includeNumericContentIdentifier)
        {
            IncludeNumericContentIdentifier = includeNumericContentIdentifier;
            return this;
        }

        /// <summary>
        /// Clone object
        /// </summary>
        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
