using EPiServer.ContentApi.Forms.Model;
using EPiServer.Core;
using EPiServer.Framework.Configuration;
using System;
using System.Web;
using Xunit;

namespace EPiServer.ContentApi.Forms.Tests
{
    [Obsolete]
    public class AssetTests : TestBase
    {
        public AssetTests()
        {
            HttpContext.Current = CreateHttpContext("http://newhttpcontext.com", string.Empty);
        }

        public class When_http_context_is_null : AssetTests
        {
            public When_http_context_is_null()
            {
                HttpContext.Current = null;
            }

            [Fact]
            public void It_should_return_default_cms_content_model()
            {
                var model = _mapper.TransformContent(_formContainerBlock as IContent, false, string.Empty) as FormContentApiModel;
                Assert.Null(model.FormModel);                
            }
        }

        public class When_all_assets_should_be_acquired : AssetTests
        {
            public class When_is_in_debug_mode: When_all_assets_should_be_acquired
            {
                public When_is_in_debug_mode()
                {
                    EPiServerFrameworkSection.Instance.ClientResources.Debug = true;
                }

                [Fact]
                public void It_should_contain_all_required_assets()
                {
                    var model = _mapper.TransformContent(_formContainerBlock as IContent, false, string.Empty) as FormContentApiModel;
                    Assert.Equal(_numberOfAssets, model.FormModel.Assets.Count);

                    //replace "call service to get js value" by AssetConstants.ViewmodeJs later on
                    Assert.Contains(model.FormModel.Assets, asset => string.Equals(asset.Value, "scriptContent", StringComparison.OrdinalIgnoreCase));
                }
            }

            public class When_is_not_in_debug_mode : When_all_assets_should_be_acquired
            {
                public When_is_not_in_debug_mode()
                {
                    EPiServerFrameworkSection.Instance.ClientResources.Debug = false;
                }

                [Fact]
                public void It_should_contain_all_required_assets()
                {
                    var model = _mapper.TransformContent(_formContainerBlock as IContent, false, string.Empty) as FormContentApiModel;
                    Assert.Equal(_numberOfAssets, model.FormModel.Assets.Count);

                    //replace "call service to get js value" by AssetConstants.ViewmodeJsMin later on
                    Assert.Contains(model.FormModel.Assets, asset => string.Equals(asset.Value, _formRenderingService.Object.GetScriptFromAssemblyByName(asset.Value), StringComparison.OrdinalIgnoreCase));
                }
            }
            
        }

        public class When_jquery_should_not_be_injected : AssetTests
        {
            public When_jquery_should_not_be_injected()
            {
                _formConfig.SetupGet(config => config.InjectFormOwnJQuery).Returns(false);
            }

            [Fact]
            public void It_should_not_content_jquery_in_assets()
            {
                var model = _mapper.TransformContent(_formContainerBlock as IContent, false, string.Empty) as FormContentApiModel;

                Assert.Equal(_numberOfAssets - 1, model.FormModel.Assets.Count);
                Assert.False(model.FormModel.Assets.ContainsKey("Jquery"));
            }
        }

        public class When_css_should_not_be_injected : AssetTests
        {
            public When_css_should_not_be_injected()
            {
                _formConfig.SetupGet(config => config.InjectFormOwnStylesheet).Returns(false);
            }

            [Fact]
            public void It_should_not_content_jquery_in_assets()
            {
                var model = _mapper.TransformContent(_formContainerBlock as IContent, false, string.Empty) as FormContentApiModel;

                Assert.Equal(_numberOfAssets - 1, model.FormModel.Assets.Count);
                Assert.False(model.FormModel.Assets.ContainsKey("Css"));
            }
        }

        public class When_css_and_jquery_should_not_be_injected : AssetTests
        {
            public When_css_and_jquery_should_not_be_injected()
            {
                _formConfig.SetupGet(config => config.InjectFormOwnStylesheet).Returns(false);
                _formConfig.SetupGet(config => config.InjectFormOwnJQuery).Returns(false);
            }

            [Fact]
            public void It_should_not_content_jquery_in_assets()
            {
                var model = _mapper.TransformContent(_formContainerBlock as IContent, false, string.Empty) as FormContentApiModel;

                Assert.Equal(_numberOfAssets - 2, model.FormModel.Assets.Count);
                Assert.False(model.FormModel.Assets.ContainsKey("Css"));
                Assert.False(model.FormModel.Assets.ContainsKey("Jquery"));
            }
        }
    }
}
