using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.ServiceLocation;
using System.Collections.Generic;
using System.Globalization;

namespace EPiServer.ContentApi.Core.OutputCache.Internal
{
    [ServiceConfiguration(Lifecycle = ServiceInstanceScope.Singleton)]
    internal class ContentDependencyPropagator
    {
        private const string ParentLinkKey = "OutputCacheParentLink";
        private const string FirstPublishKey = "OutputCacheFirstPublish";
        private readonly IContentLoader _contentLoader;
        private readonly IContentCacheKeyCreator _contentCacheKeyCreator;

        public ContentDependencyPropagator(IContentLoader contentLoader, IContentCacheKeyCreator contentCacheKeyCreator, IOutputCacheProvider outputCacheProvider)
        {
            _contentLoader = contentLoader;
            _contentCacheKeyCreator = contentCacheKeyCreator;
            OutputCacheProvider = outputCacheProvider;
        }

        // Exposed for test
        internal IOutputCacheProvider OutputCacheProvider { get; set; }

        internal void ContentSecuritySaved(object sender, ContentSecurityEventArg e)
        {
            var keys = new HashSet<string>();
            keys.Add(_contentCacheKeyCreator.CreateCommonCacheKey(e.ContentLink));

            var content = _contentLoader.Get<IContent>(e.ContentLink);

            // Not all content has a parent
            if (!ContentReference.IsNullOrEmpty(content.ParentLink))
            {
                keys.Add(CreateChildrenListingKey(content.ParentLink));
            }

            // Inherited access rights might be changed
            AddDependencyKeysForDescendent(e.ContentLink, ref keys);

            OutputCacheProvider.Remove(keys);
        }

        internal void DeletingContent(object sender, DeleteContentEventArgs e)
        {
            //If targetlink is not set then it is a Delete and not a DeleteChildren, then we should delete child listing for parent as well
            if (ContentReference.IsNullOrEmpty(e.TargetLink))
            {
                e.Items[ParentLinkKey] = _contentLoader.Get<IContent>(e.ContentLink).ParentLink;
            }
        }


        internal void DeletedContent(object sender, DeleteContentEventArgs e)
        {
            var keys = new HashSet<string>();
            keys.Add(CreateChildrenListingKey(e.ContentLink));

            var parentLink = e.Items[ParentLinkKey] as ContentReference;
            if (!ContentReference.IsNullOrEmpty(parentLink))
            {
                keys.Add(CreateChildrenListingKey(parentLink));
                keys.Add(_contentCacheKeyCreator.CreateCommonCacheKey(e.ContentLink));
            }

            foreach (var item in e.DeletedDescendents)
            {
                keys.Add(_contentCacheKeyCreator.CreateCommonCacheKey(item));
            }

            OutputCacheProvider.Remove(keys);
        }

        internal void DeletedContentLanguage(object sender, ContentEventArgs e)
        {
            //Since all items has ExistingLanguages property we need to clear common
            OutputCacheProvider.Remove(_contentCacheKeyCreator.CreateCommonCacheKey(e.ContentLink));
        }

        internal void PublishingContent(object sender, ContentEventArgs e)
        {
            // If this is the first time this language branch is published we should clear parents children listing as well
            if ((e.Content is IVersionable versionable && versionable.IsPendingPublish) || ContentReference.IsNullOrEmpty(e.ContentLink))
            {
                e.Items[FirstPublishKey] = true;
            }
        }


        internal void PublishedContent(object sender, ContentEventArgs e)
        {
            var keys = new HashSet<string>();
            if (e.Items.Contains(FirstPublishKey))
            {
                keys.Add(CreateChildrenListingKey(e.Content.ParentLink));
                // This is the first time this language is published, we need to clear all since ExistingLanguages is changed
                keys.Add(_contentCacheKeyCreator.CreateCommonCacheKey(e.ContentLink));
            }
            else
            {
                if (e.Content is ILocale locale)
                {
                    keys.Add(_contentCacheKeyCreator.CreateLanguageCacheKey(e.ContentLink, locale.Language.Name));
                }
                else
                {
                    keys.Add(_contentCacheKeyCreator.CreateCommonCacheKey(e.ContentLink));
                }
            }

            OutputCacheProvider.Remove(keys);
        }

        internal void MovedContent(object sender, ContentEventArgs e)
        {
            var keys = new HashSet<string>();
            keys.Add(_contentCacheKeyCreator.CreateCommonCacheKey(e.ContentLink));
            keys.Add(CreateChildrenListingKey(e.TargetLink));
            keys.Add(CreateChildrenListingKey((e as MoveContentEventArgs).OriginalParent));

            //Urls are hierarchical constructed and hence changed
            AddDependencyKeysForDescendent(e.ContentLink, ref keys);

            OutputCacheProvider.Remove(keys);
        }

        private void AddDependencyKeysForDescendent(ContentReference contentLink, ref HashSet<string> dependecyKeys)
        {
            foreach (var descendent in _contentLoader.GetDescendents(contentLink))
            {
                dependecyKeys.Add(_contentCacheKeyCreator.CreateCommonCacheKey(descendent));
            }
        }


        internal string CreateChildrenListingKey(ContentReference contentLink)
            => _contentCacheKeyCreator.CreateChildrenCacheKey(contentLink.ToReferenceWithoutVersion(), null);

    }
}
