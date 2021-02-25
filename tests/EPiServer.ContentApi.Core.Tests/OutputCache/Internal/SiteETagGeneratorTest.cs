using EPiServer.ContentApi.Core.OutputCache.Internal;
using EPiServer.ContentApi.Core.Tracking;
using EPiServer.Core;
using EPiServer.Web;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace EPiServer.ContentApi.Core.Tests.OutputCache.Internal
{
    public class SiteETagGeneratorTest
    {
        [Fact]
        public void Generate_WhenSameInput_ShouldBeSameEtag()
        {
            var id = Guid.NewGuid();
            var now = DateTime.Now;
            var sites = new[] { Create(id, now), Create(id, now.AddDays(1)) };
            var subject = new SiteETagGenerator();
            Assert.Equal(subject.Generate(sites), subject.Generate(sites));
        }

        [Fact]
        public void Generate_WhenSiteOrderDiffer_ShouldBeSameEtag()
        {
            var sites = new[] { Create(Guid.NewGuid(), DateTime.Now), Create(Guid.NewGuid(), DateTime.Now) };
            var subject = new SiteETagGenerator();
            Assert.Equal(subject.Generate(sites), subject.Generate(sites.Reverse()));
        }

        [Fact]
        public void Generate_WhenIdDiffer_ShouldBeDifferentEtag()
        {
            var now = DateTime.Now;
            var site = Create(Guid.NewGuid(), now);
            var site2 = Create(Guid.NewGuid(), now);
            var subject = new SiteETagGenerator();
            Assert.NotEqual(subject.Generate(new[] { site }), subject.Generate(new[] { site2 }));
        }

        [Fact]
        public void Generate_WhenSavedDiffer_ShouldBeDifferentEtag()
        {
            var id = Guid.NewGuid();
            var site = Create(id, DateTime.Now.Add(TimeSpan.FromDays(1)));
            var site2 = Create(id, DateTime.Now);
            var subject = new SiteETagGenerator();
            Assert.NotEqual(subject.Generate(new[] { site }), subject.Generate(new[] { site2 }));
        }

        [Fact]
        public void Generate_WhenSavedAndIdDiffer_ShouldBeDifferentEtag()
        {
            var site = Create(Guid.NewGuid(), DateTime.Now.Add(TimeSpan.FromDays(1)));
            var site2 = Create(Guid.NewGuid(), DateTime.Now);
            var subject = new SiteETagGenerator();
            Assert.NotEqual(subject.Generate(new[] { site }), subject.Generate(new[] { site2 }));
        }

        private ReferencedSiteMetadata  Create(Guid id, DateTime? saved)
        {
            return new ReferencedSiteMetadata (id, saved);
        }
    }
}
