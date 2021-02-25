using EPiServer.ContentApi.Core.Serialization.Internal;
using EPiServer.Core;
using EPiServer.SpecializedProperties;
using Moq;
using System.Security.Principal;
using System.Web;
using Xunit;

namespace EPiServer.ContentApi.Core.Tests.Serialization
{
	public class Render : DefaultXHtmlStringPropertyRendererTest
	{
		[Fact]
		public void It_should_return_empty_when_property_is_null()
		{
			Assert.Empty(Subject.Render(null, false));
		}

		[Fact]
		public void It_should_return_empty_when_fragment_list_is_empty()
		{
			HeadlessXhtmlString xhtmlString = new HeadlessXhtmlString(string.Empty)
			{
				FragmentParser = new MockableConstFragmentParser()
			};

			Assert.Empty(Subject.Render(new PropertyXhtmlString { XhtmlString = xhtmlString }, false));
		}

		public class when_httpContext_is_null : Render
		{
			public when_httpContext_is_null()
			{
				HttpContext.Current = null;
			}

			[Fact]
			public void It_should_create_new_httpContext_and_return_value_when_fragment_list_is_not_empty()
			{
				_htmlHelperWrapper = CreateMockableHelpHelperWrapper();
				Subject = new DefaultXHtmlStringPropertyRenderer(_htmlHelperWrapper.Object);

				HeadlessXhtmlString xhtmlString = new HeadlessXhtmlString("some thing")
				{
					FragmentParser = new MockableConstFragmentParser()
				};

				var propertyXhtmlString = new PropertyXhtmlString { XhtmlString = xhtmlString };
				var actualResult = Subject.Render(propertyXhtmlString, false);

				// Assert HttpContext is created
				_htmlHelperWrapper.Verify(x => x.RenderXHTMLString(
					It.Is<HttpContextBase>(ctx => ctx.Request.Url.ToString().Contains("episerver.com")),
					It.IsAny<XhtmlString>()), Times.Once);

				Assert.Equal("some thing", actualResult);
			}
		}

		public class when_httpContext_is_not_null : Render
		{
			public when_httpContext_is_not_null()
			{
				HttpContext.Current = CreateHttpContext();
				HttpContext.Current.User = new GenericPrincipal(new GenericIdentity("quanchua"), new[] { "Administrators" });
			}

			[Fact]
			public void It_should_not_create_HttpContext()
			{
				_htmlHelperWrapper = CreateMockableHelpHelperWrapper();
				Subject = new DefaultXHtmlStringPropertyRenderer(_htmlHelperWrapper.Object);

				HeadlessXhtmlString xhtmlString = new HeadlessXhtmlString("some thing")
				{
					FragmentParser = new MockableConstFragmentParser()
				};

				var propertyXhtmlString = new PropertyXhtmlString { XhtmlString = xhtmlString };

				Subject.Render(propertyXhtmlString, false);

				// Assert HttpContext is not created
				_htmlHelperWrapper.Verify(x => x.RenderXHTMLString(
					It.Is<HttpContextBase>(ctx => ctx.Request.Url.ToString().Contains("episerver.com")),
					It.IsAny<XhtmlString>()), Times.Never);
			}

			public class when_excludePersonalizedContent_is_false : when_httpContext_is_not_null
			{
				[Fact]
				public void It_should_not_exclude_ContentFragment()
				{
					HeadlessXhtmlString xhtmlString = new HeadlessXhtmlString("some thing")
					{
						FragmentParser = new MockableConstFragmentParser()
					};

					var contentFragment = CreateContentFragment(new ContentReference(1), System.Guid.NewGuid());
					xhtmlString.Fragments.Add(contentFragment);  // Fragments contains: one ConstFragment and one ContentFragment

					var propertyXhtmlString = new PropertyXhtmlString { XhtmlString = xhtmlString };

					var actualResult = Subject.Render(propertyXhtmlString, false);
					Assert.NotEqual("some thing", actualResult);
				}
			}

			public class when_excludePersonalizedContent_is_true : when_httpContext_is_not_null
			{
				[Fact]
				public void It_should_exclude_ContentFragment()
				{
					HeadlessXhtmlString xhtmlString = new HeadlessXhtmlString("fragment_1")
					{
						FragmentParser = new MockableConstFragmentParser()
					};

					var contentFragment = CreateContentFragment(new ContentReference(1), System.Guid.NewGuid());
					xhtmlString.Fragments.Add(contentFragment);
					xhtmlString.Fragments.Add(new ConstFragment("fragment_2"));  // Fragments contains: two ConstFragment(s) and one ContentFragment

					var propertyXhtmlString = new PropertyXhtmlString { XhtmlString = xhtmlString };

					var actualResult = Subject.Render(propertyXhtmlString, true);
					Assert.Equal("fragment_1fragment_2", actualResult);
				}
			}
		}
	}
}
