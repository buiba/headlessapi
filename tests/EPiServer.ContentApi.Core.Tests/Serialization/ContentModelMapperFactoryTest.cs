using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ContentApi.Core.ContentResult;
using EPiServer.ContentApi.Core.ContentResult.Internal;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ContentApi.Core.Serialization.Internal;
using EPiServer.Core;
using EPiServer.Core.Internal;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using EPiServer.Shell.Security;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace EPiServer.ContentApi.Core.Tests.Serialization
{
    [Obsolete]
    public class ContentModelMapperFactoryTest : TestBase
    {
        protected ContentModelMapperFactory _factory;
        protected Mock<IContent> _mockIContent;

        public ContentModelMapperFactoryTest() :base()
        {

            _mockIContent = new Mock<IContent>();
        }

        public class When_mapper_only_inherits_IContentModelMapper: ContentModelMapperFactoryTest
        {
            [Fact]
            public void It_should_not_return_content_model_mapper_base()
            {
                List<IContentModelMapper> mapperList = new List<IContentModelMapper>()
                {
                    new Mock<IContentModelMapper>().Object
                };

                _factory = new ContentModelMapperFactory(mapperList);
                var mapper = _factory.GetMapper(_mockIContent.Object);
                Assert.False(mapper is ContentModelMapperBase);
            }
        }

        public class When_mapper_inherits_ContentModelMapperBase : ContentModelMapperFactoryTest
        {
            [Fact]
            public void It_should_return_content_model_mapper_base()
            {
                var mockContentModelBaseMapper = new Mock<ContentModelMapperBase>();
                mockContentModelBaseMapper.Setup(baseMapper => baseMapper.CanHandle(It.IsAny<IContent>())).Returns(true);
                List<ContentModelMapperBase> contentModelMapperBaseList = new List<ContentModelMapperBase>()
                {
                    mockContentModelBaseMapper.Object
                };
                _factory = new ContentModelMapperFactory(contentModelMapperBaseList);

                var mapper = _factory.GetMapper(_mockIContent.Object);
                Assert.True(mapper is ContentModelMapperBase);
            }
        }
    }
}
