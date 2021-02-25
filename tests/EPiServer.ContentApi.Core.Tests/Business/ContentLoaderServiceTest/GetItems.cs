using EPiServer.Core;
using System.Globalization;
using System.Linq;
using EPiServer.Security;
using Xunit;
using Moq;

namespace EPiServer.ContentApi.Core.Tests.Business
{
	public class GetItems : ContentLoaderServiceTest
	{
		private readonly ContentReference _pageID_1 = new ContentReference(1);
		private readonly ContentReference _pageID_2 = new ContentReference(2);

		public GetItems()
		{
			_defaultContentLoader.Setup(svc => svc.Get<IContent>(It.Is<ContentReference>(x => x == _pageID_1), It.IsAny<CultureInfo>())).Returns(new PageData(new AccessControlList(), _publishedProperties));
			_defaultContentLoader.Setup(svc => svc.Get<IContent>(It.Is<ContentReference>(x => x == _pageID_2), It.IsAny<CultureInfo>())).Returns(new PageData(new AccessControlList(), _publishedProperties));
		}

		[Fact]
		public void ShouldReturnEmptyList_WhenReferenceList_IsNull()
		{
			Assert.Empty(Subject.GetItems(null, new CultureInfo("en")));
		}

		[Fact]
		public void ShouldReturnEmptyList_WhenReferenceList_IsEmpty()
		{
			Assert.Empty(Subject.GetItems(Enumerable.Empty<ContentReference>(), new CultureInfo("en")));
		}
		
		[Fact]
		public void ShouldReturnContentList_WhenReferenceList_HasNonDuplicatedValues()
		{
			var expectedResult = Subject.GetItems(new ContentReference[] { _pageID_1 , _pageID_2 }, new CultureInfo("en"));
			Assert.Equal(2, expectedResult.Count());
		}

		[Fact]
		public void ShouldReturnDuplicatedContentList_WhenReferenceList_HasDuplicatedValues()
		{
			var expectedResult = Subject.GetItems(new ContentReference[] { _pageID_1, _pageID_2, _pageID_2 }, new CultureInfo("en"));
			Assert.Equal(3, expectedResult.Count());
		}
	}
}
