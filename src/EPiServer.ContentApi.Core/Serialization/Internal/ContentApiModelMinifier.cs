using EPiServer.ContentApi.Core.Serialization.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPiServer.ContentApi.Core.Serialization.Internal
{
    internal class ContentApiModelMinifier
    {
        public virtual MinifiedContentApiModel Minify(ContentApiModel contentApiModel, IEnumerable<string> selectedProperties)
        {
            if (contentApiModel == null) throw new ArgumentNullException(nameof(contentApiModel));

            var minifiedModel = new MinifiedContentApiModel(contentApiModel.ContentLink, contentApiModel.Name, contentApiModel.Language);
            //we always want to include ContentType
            selectedProperties = new HashSet<string>(selectedProperties.Concat(new[] { nameof(contentApiModel.ContentType) }), StringComparer.OrdinalIgnoreCase);
            
            foreach (var entry in CombineAllProperties(contentApiModel))
            {
                if (selectedProperties.Contains(entry.Key))
                {
                    minifiedModel.Properties.Add(entry);
                }
            }

            return minifiedModel;
        }

        private IDictionary<string, object> CombineAllProperties(ContentApiModel contentApiModel)
        {
            //Reason for this construction compared to switch over metadata properties and contentModel.Properties.TryGet is that for
            //output of custom properties we want the key in dictionary rather than the inpassed select parameter (that can differ in casing)
            var dictionary = new Dictionary<string, object>(contentApiModel.Properties);
            dictionary.Add(nameof(contentApiModel.Changed), contentApiModel.Changed);
            if (!contentApiModel.Properties.ContainsKey(nameof(contentApiModel.ContentType)))
            {
                dictionary.Add(nameof(contentApiModel.ContentType), contentApiModel.ContentType);
            }
            dictionary.Add(nameof(contentApiModel.Created), contentApiModel.Created);
            dictionary.Add(nameof(contentApiModel.ExistingLanguages), contentApiModel.ExistingLanguages);
            dictionary.Add(nameof(contentApiModel.Language), contentApiModel.Language);
            dictionary.Add(nameof(contentApiModel.MasterLanguage), contentApiModel.MasterLanguage);
            dictionary.Add(nameof(contentApiModel.ParentLink), contentApiModel.ParentLink);
            dictionary.Add(nameof(contentApiModel.RouteSegment), contentApiModel.RouteSegment);
            dictionary.Add(nameof(contentApiModel.Saved), contentApiModel.Saved);
            dictionary.Add(nameof(contentApiModel.StartPublish), contentApiModel.StartPublish);
            dictionary.Add(nameof(contentApiModel.Status), contentApiModel.Status);
            dictionary.Add(nameof(contentApiModel.StopPublish), contentApiModel.StopPublish);
            dictionary.Add(nameof(contentApiModel.Url), contentApiModel.Url);
            return dictionary;
        }
    }
}
