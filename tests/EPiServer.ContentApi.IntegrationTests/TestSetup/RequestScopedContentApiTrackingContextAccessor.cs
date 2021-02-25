using EPiServer.ContentApi.Core.Tracking;
using System;
using System.Collections.Generic;
using System.Text;

namespace EPiServer.ContentApi.IntegrationTests.TestSetup
{
    internal class RequestScopedContentApiTrackingContextAccessor : IContentApiTrackingContextAccessor, IRestRequestInitializer
    {
        public ContentApiTrackingContext Current { get; private set; } = new ContentApiTrackingContext();

        public void InitiateRequest() => Current = new ContentApiTrackingContext();
    }
}
