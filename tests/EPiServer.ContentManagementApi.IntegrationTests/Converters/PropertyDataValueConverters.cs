using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.ContentApi.IntegrationTests.ContentModels.Blocks;
using EPiServer.ContentApi.IntegrationTests.ContentModels.Media;
using EPiServer.ContentApi.IntegrationTests.ContentModels.Pages;
using EPiServer.ContentApi.IntegrationTests.TestSetup;
using EPiServer.ContentManagementApi.Serialization;
using EPiServer.Core;
using EPiServer.Core.Html.StringParsing;
using EPiServer.Framework.Blobs;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using FluentAssertions;
using Xunit;

namespace EPiServer.ContentManagementApi.IntegrationTests.Converters
{
    [Collection(IntegrationTestCollection.Name)]
    public sealed class PropertyDataValueConverters
    {
        private readonly ServiceFixture _fixture;
        private readonly IPropertyDataValueConverterResolver _converterResolver;    

        public PropertyDataValueConverters(ServiceFixture fixture)
        {
            _fixture = fixture;
            _converterResolver = ServiceLocator.Current.GetInstance<IPropertyDataValueConverterResolver>();
        }        

        [Fact]
        public void Converter_WithPropertyModels_ShouldConvertToCorrespondingPropertyData()
        {
            var linkedContent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true);
            var media = _fixture.GetWithDefaultName<DefaultMedia>(ContentReference.StartPage, true);
            var blob = ServiceLocator.Current.GetInstance<IBlobFactory>().CreateBlob(media.BinaryDataContainer, ".png");
            
            var properties = CreatePropertiesDictionary(linkedContent, media, blob);

            var content = _fixture.ContentRepository.GetDefault<AllPropertyPage>(ContentReference.StartPage);
            (content as IContent).Name = "Content";

            foreach (var item in properties)
            {
                var propertyModel = item.Value as IPropertyModel;                
                
                var property = content.Property.Single(p => p.Name.Equals(item.Key, StringComparison.OrdinalIgnoreCase));
                property.Value = _converterResolver.Resolve(propertyModel).Convert(propertyModel, null);                 
            }

            var savedContentReference = _fixture.ContentRepository.Save(content, AccessLevel.NoAccess);
            var savedContent = _fixture.ContentRepository.Get<AllPropertyPage>(savedContentReference);
            var expectedUrl = new Url(GetInternalUrl(media).Replace("~", string.Empty));

            savedContent.String.Should().Be((properties["String"] as StringPropertyModel).Value);
            savedContent.AppSettings.Should().Be((properties["AppSettings"] as AppSettingsPropertyModel).Value);
            savedContent.Blob.Should().BeEquivalentTo(blob);
            savedContent.DocumentUrl.Should().BeEquivalentTo(expectedUrl);
            savedContent.DropDownList.Should().Be((properties["DropDownList"] as DropDownListPropertyModel).Value);
            savedContent.Number.Should().Be(100);
            savedContent.WeekDay.Should().Be((int)Weekday.Monday);
            savedContent.Boolean.Should().BeTrue();           
            savedContent.DateList.Should().BeEquivalentTo((properties["DateList"] as DateListPropertyModel).Value);
            savedContent.XhtmlString.Fragments.Single().Should().BeOfType<ContentFragment>();            
            savedContent.XhtmlString.ToEditString().Should().Be((properties["XhtmlString"] as XHtmlPropertyModel).Value);
            savedContent.ContentArea.Items.Single().ContentLink.Should().BeEquivalentTo(linkedContent.ContentLink.ToReferenceWithoutVersion());
            savedContent.ContentReference.Should().BeEquivalentTo(linkedContent.ContentLink.ToReferenceWithoutVersion());
            savedContent.ContentReferenceList.Single().Should().BeEquivalentTo(linkedContent.ContentLink.ToReferenceWithoutVersion());
            savedContent.Links[0].Href.Should().Be(GetInternalUrl(linkedContent));
            savedContent.Links[1].Href.Should().Be("https://episerver.com");
            savedContent.Url.ToString().Should().Be(GetInternalUrl(linkedContent).Replace("~", string.Empty));
            savedContent.PageType.Should().BeEquivalentTo(_fixture.ContentTypeRepository.Load("PropertyPage"));

            _fixture.ContentRepository.Delete(linkedContent.ContentLink, true, AccessLevel.NoAccess);
            _fixture.ContentRepository.Delete(media.ContentLink, true, AccessLevel.NoAccess);
        }

        [Fact]
        public void Converter_WithNestedBlockPropertyModels_ShouldConvertToBlockData()
        {            
            var content = _fixture.ContentRepository.GetDefault<LocalBlockPage>(ContentReference.StartPage);
            (content as IContent).Name = "Content";

            var blockPropertyModel = new BlockPropertyModel()
            {
                Name = "LocalBlock",
                Properties = new Dictionary<string, object>()
                {
                    {
                        "Heading", new StringPropertyModel() { Value = "Heading" }
                    },
                    {                        
                        "NestedBlock", new BlockPropertyModel()
                        {
                            Name = "NestedBlock",
                            Properties = new Dictionary<string, object>()
                            {
                                { "Title", new StringPropertyModel() { Value = "Some Nested Block Title" } }
                            }
                        }
                    }
                }
            };

            var propertyBlock = content.Property.Single(p => p.Name.Equals("LocalBlock", StringComparison.OrdinalIgnoreCase));
            content.LocalBlock = _converterResolver.Resolve(blockPropertyModel).Convert(blockPropertyModel, propertyBlock) as TextBlock;

            var savedContentReference = _fixture.ContentRepository.Save(content, AccessLevel.NoAccess);
            var savedContent = _fixture.ContentRepository.Get<LocalBlockPage>(savedContentReference);

            savedContent.LocalBlock.Heading.Should().Be("Heading");
            savedContent.LocalBlock.NestedBlock.Title.Should().Be("Some Nested Block Title");
        }

        private string GetInternalUrl(IContent content) => PermanentLinkUtility.GetPermanentLinkVirtualPath(content.ContentGuid, ".aspx");

        private Dictionary<string, object> CreatePropertiesDictionary(StandardPage linkedContent, DefaultMedia media, Blob blob)
        {
            var properties = new Dictionary<string, object>() {
                {
                    "String",
                    new StringPropertyModel(){ Value = "Test"}
                },
                {
                    "AppSettings",
                    new AppSettingsPropertyModel(){ Value = "AppSettings"}
                },
                {
                    "Blob",
                    new BlobPropertyModel(){ Value = blob.ID.ToString()}
                },
                {
                    "DocumentUrl",
                    new DocumentUrlPropertyModel(){ Value = GetInternalUrl(media)}
                },
                {
                    "DropDownList",
                    new DropDownListPropertyModel(){ Value = "Item1,Item2"}
                },
                {
                    "Number",
                    new NumberPropertyModel(){ Value = 100}
                },
                {
                    "WeekDay",
                    new WeekdayPropertyModel(){ Value = "Monday"}
                },
                {
                    "Boolean",
                    new BooleanPropertyModel(){ Value = true}
                },
                {
                    "DateList",
                    new DateListPropertyModel(){ Value = new List<DateTime> { DateTime.Now, DateTime.Now.AddDays(1) } }
                },
                {
                    "XhtmlString",
                    new XHtmlPropertyModel() { Value = CreateXhtmlString(linkedContent).ToEditString()}
                },
                {
                    "ContentArea", new ContentAreaPropertyModel() {
                        Value = new[] { new ContentAreaItemModel() { DisplayOption = "", ContentLink = new ContentModelReference() { Id = linkedContent.ContentLink.ID, WorkId = linkedContent.ContentLink.WorkID }}}
                    }
                },
                {
                    "ContentReference", new ContentReferencePropertyModel() {
                        Value = new ContentModelReference() { Id = linkedContent.ContentLink.ID, WorkId = linkedContent.ContentLink.WorkID }
                    }
                },
                {
                    "ContentReferenceList", new ContentReferenceListPropertyModel() {
                        Value = new List<ContentModelReference> { new ContentModelReference() { Id = linkedContent.ContentLink.ID, WorkId = linkedContent.ContentLink.WorkID } }
                    }
                },
                {
                    "Links", new LinkCollectionPropertyModel() {
                        Value = new List<LinkItemNode> {
                            new LinkItemNode(GetInternalUrl(linkedContent), null, null, linkedContent.Name),
                            new LinkItemNode("https://episerver.com", null, null, "Episerver") }
                    }
                },
                {
                    "Url", new UrlPropertyModel() {
                        Value = GetInternalUrl(linkedContent)
                    }
                },
                {
                    "PageType", new PageTypePropertyModel() {
                        Value = "PropertyPage"
                    }
                }
            };

            return properties;
        }

        private XhtmlString CreateXhtmlString(StandardPage linkedContent)
        {            
            var securityMarkup = ServiceLocator.Current.GetInstance<ISecuredFragmentMarkupGeneratorFactory>().CreateSecuredFragmentMarkupGenerator();
            var publishedStateAssesor = ServiceLocator.Current.GetInstance<IPublishedStateAssessor>();
            var contentAccessEvaluator = ServiceLocator.Current.GetInstance<IContentAccessEvaluator>();
            var contextModeResolver = ServiceLocator.Current.GetInstance<IContextModeResolver>();
            var contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();
            
            var fragment = new ContentFragment(contentRepository, securityMarkup,
                new DisplayOptions(), publishedStateAssesor, contextModeResolver, contentAccessEvaluator, new Dictionary<string, object>())
            { ContentLink = linkedContent.ContentLink.ToReferenceWithoutVersion() };

            var xhtmlString = new XhtmlString();
            xhtmlString.Fragments.Add(fragment);            

            return xhtmlString;    
        }
    }
}
