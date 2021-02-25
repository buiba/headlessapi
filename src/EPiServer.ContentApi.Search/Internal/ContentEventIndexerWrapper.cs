using EPiServer.Core;
using EPiServer.Find.Cms;
using EPiServer.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPiServer.ContentApi.Search.Internal
{
    /// <summary>
    /// Wrapper for ContentEventIndexer. We need this wrapper to use mock in writing unit test because ContentEventIndexer.SavingContent() is NOT virtual
    /// </summary>
    [ServiceConfiguration(typeof(ContentEventIndexerWrapper))]
    public class ContentEventIndexerWrapper
    {
        protected readonly ContentEventIndexer _contentEventIndexer;

        /// <summary>
        /// Initialize a instance of <see cref="ContentEventIndexerWrapper"/> with given parameters
        /// </summary>
        /// <param name="contentEventIndexer"></param>
        public ContentEventIndexerWrapper(ContentEventIndexer contentEventIndexer)
        {
            _contentEventIndexer = contentEventIndexer;
        }

        /// <summary>
        /// Wrap method for ContentEventIndexer.SavingContent()
        /// </summary>
        /// <param name="contentLink"></param>
        public virtual void SavingContent(ContentReference contentLink)
        {
            _contentEventIndexer.SavingContent(contentLink);
        }
    }
}
