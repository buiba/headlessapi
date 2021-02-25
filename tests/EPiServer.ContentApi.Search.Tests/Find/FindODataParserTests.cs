using System;
using System.Linq;
using System.Web.OData.Query;
using EPiServer.ContentApi.Search.Internal;
using EPiServer.Core;
using EPiServer.Find;
using EPiServer.Find.Api;
using EPiServer.Find.Api.Querying;
using EPiServer.Find.Api.Querying.Filters;
using EPiServer.Find.Api.Querying.Queries;
using EPiServer.Find.ClientConventions;
using Moq;
using Xunit;

#pragma warning disable CS0612 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
namespace EPiServer.ContentApi.Search.Tests.Find
{
    public class FindODataParserTests
    {

        [Fact]
        public void ParseFilter_ShouldReturnTermFilter_WhenMatchingString()
        {
            string filterString = "Name eq 'Test'";

            var oDataParser = CreateODataParser();

            var filter = oDataParser.ParseFilter(filterString) as TermFilter;

            var filterValue = (FieldFilterValue<string>)filter.Value;
            var expectedValue = FieldFilterValue.Create("Test");

            Assert.Equal(expectedValue.Value, filterValue.Value);
        }

        [Fact]
        public void ParseFilter_ShouldReturnNotFilter_WhenMatchingNotEquals()
        {
            string filterString = "Name ne 'Test'";

            var oDataParser = CreateODataParser();

            var filter = oDataParser.ParseFilter(filterString) as NotFilter;

            var termFilter = filter.Filter as TermFilter;
            var filterValue = (FieldFilterValue<string>)termFilter.Value;
            var expectedValue = FieldFilterValue.Create("Test");

            Assert.Equal(expectedValue.Value, filterValue.Value);
        }

        [Fact]
        public void ParseFilter_ShouldReturnTermFilter_WhenMatchingExtendedPropertyString()
        {
            string filterString = "UnknownProperty eq 'Test'";

            var oDataParser = CreateODataParser();

            var filter = oDataParser.ParseFilter(filterString) as TermFilter;

            var filterValue = (FieldFilterValue<string>)filter.Value;
            var expectedValue = FieldFilterValue.Create("Test");

            Assert.Equal(expectedValue.Value, filterValue.Value);
        }


        [Fact]
        public void ParseFilter_ShouldReturnTermFilter_WhenMatchingContentTypeViaAny()
        {
            string filterString = "ContentType/any(t:t eq 'Page')";

            var oDataParser = CreateODataParser();

            var filter = oDataParser.ParseFilter(filterString) as TermFilter;
            var value = filter.Value as FieldFilterValue<String>;
            Assert.Equal("ContentApiModel.ContentType", filter.Field);
            Assert.Equal("Page", value.Value);
        }

        [Fact]
        public void ParseFilter_ShouldReturnNotFilter_WhenMatchingContentTypeViaAny()
        {
            string filterString = "ContentType/any(t:t ne 'Page')";

            var oDataParser = CreateODataParser();

            var filter = oDataParser.ParseFilter(filterString) as NotFilter;
            var termFilter = filter.Filter as TermFilter;
            var value = termFilter.Value as FieldFilterValue<String>;
            Assert.Equal("ContentApiModel.ContentType", termFilter.Field);
            Assert.Equal("Page", value);
        }

        [Fact]
        public void ParseFilter_ShouldThrowException_WhenMatchingOpenPropertyCollectionViaAny()
        {
            string filterString = "OpenProperty/any(t:t eq 'MyString')";

            var oDataParser = CreateODataParser();

            Assert.Throws<FilterParseException>(() => oDataParser.ParseFilter(filterString));
        }

        [Fact]
        public void ParseFilter_ShouldSupportToLower_WhenMatchingString()
        {
            string filterString = "tolower(Name) eq 'test'";

            var oDataParser = CreateODataParser();

            var filter = oDataParser.ParseFilter(filterString) as TermFilter;

            Assert.Equal("ContentApiModel.Name$$string.lowercase", filter.Field);
        }

        [Fact]
        public void ParseFilter_ShouldSupportContains_WhenMatchingString()
        {
            string filterString = "contains(Name, 'test')";

            var oDataParser = CreateODataParser();

            var filter = oDataParser.ParseFilter(filterString) as QueryFilter;
            var query = filter.Query as WildcardQuery;
            Assert.Equal("ContentApiModel.Name$$string", query.Field);
            Assert.Equal("*test*", query.Value);
        }

        [Fact]
        public void ParseFilter_ShouldSupportContainsToLower_WhenMatchingString()
        {
            string filterString = "contains(tolower(Name), 'test')";

            var oDataParser = CreateODataParser();

            var filter = oDataParser.ParseFilter(filterString) as QueryFilter;
            var query = filter.Query as WildcardQuery;
            Assert.Equal("ContentApiModel.Name$$string.lowercase", query.Field);
            Assert.Equal("*test*", query.Value);
        }

        [Fact]
        public void ParseFilter_ShouldSupportContainsToLower_WhenMatchingOpenString()
        {
            string filterString = "contains(tolower(ArtistName/Value), 'test')";

            var oDataParser = CreateODataParser();

            var filter = oDataParser.ParseFilter(filterString) as QueryFilter;
            var query = filter.Query as WildcardQuery;
            Assert.Equal("ContentApiModel.ArtistName.Value.lowercase", query.Field);
            Assert.Equal("*test*", query.Value);
        }

        [Fact]
        public void ParseFilter_ShouldReturnNotExistsFilter_WhenMatchingNull()
        {
            string filterString = "TestProperty eq null";

            var oDataParser = CreateODataParser();

            var filter = oDataParser.ParseFilter(filterString) as NotFilter;

            Assert.Equal("ContentApiModel.TestProperty", (filter.Filter as ExistsFilter).Field);
        }

        [Fact]
        public void ParseFilter_ShouldReturnExistsFilter_WhenMatchingNotNull()
        {
            string filterString = "TestProperty ne null";

            var oDataParser = CreateODataParser();

            var filter = oDataParser.ParseFilter(filterString) as ExistsFilter;

            Assert.Equal("ContentApiModel.TestProperty", filter.Field);
        }

        [Fact]
        public void ParseFilter_ShouldReturnIntegerFilterValue_WhenMatchingInteger()
        {
            string filterString = "ContentLink/Id eq 123";

            var oDataParser = CreateODataParser();

            var filter = oDataParser.ParseFilter(filterString) as TermFilter;

            var filterValue = (FieldFilterValue<int>)filter.Value;
            var expectedValue = (FieldFilterValue<int>)FieldFilterValue.Create(123);

            Assert.Equal(expectedValue, filterValue.Value);
        }


        [Fact]
        public void ParseFilter_ShouldReturnFloatFilterValue_WhenMatchingSingle()
        {
            string filterString = "ExtendedProperty eq 123.00";

            var oDataParser = CreateODataParser();

            var filter = oDataParser.ParseFilter(filterString) as TermFilter;

            var filterValue = (FieldFilterValue<float>)filter.Value;
            var expectedValue = FieldFilterValue.Create((float)123.00);

            Assert.Equal(expectedValue, filterValue.Value);
        }


        [Fact]
        public void ParseFilter_ShouldReturnDoubleFilterValue_WhenMatchingDouble()
        {
            string filterString = "ExtendedProperty eq 123.00d";

            var oDataParser = CreateODataParser();

            var filter = oDataParser.ParseFilter(filterString) as TermFilter;

            var filterValue = (FieldFilterValue<double>)filter.Value;
            var expectedValue = FieldFilterValue.Create(123.00);

            Assert.Equal(expectedValue, filterValue.Value);
        }

        [Fact]
        public void ParseFilter_ShouldReturnBoolFilterValue_WhenMatchingBoolean()
        {
            string filterString = "ExtendedProperty eq true";

            var oDataParser = CreateODataParser();

            var filter = oDataParser.ParseFilter(filterString) as TermFilter;

            var filterValue = (FieldFilterValue<bool>)filter.Value;
            var expectedValue = FieldFilterValue.Create(true);

            Assert.Equal(expectedValue, filterValue.Value);
        }

        [Fact]
        public void ParseFilter_ShouldReturnDecimalFilterValue_WhenMatchingDecimal()
        {
            string filterString = "ExtendedProperty eq 123.00M";

            var oDataParser = CreateODataParser();

            var filter = oDataParser.ParseFilter(filterString) as TermFilter;

            var filterValue = (FieldFilterValue<decimal>)filter.Value;
            var expectedValue = (FieldFilterValue<decimal>)FieldFilterValue.Create(123.00M);

            Assert.Equal(expectedValue.Value, filterValue.Value);
        }

        [Fact]
        public void ParseFilter_ShouldReturnLongFilterValue_WhenMatchingInt64()
        {
            string filterString = "ExtendedProperty eq 123L";

            var oDataParser = CreateODataParser();

            var filter = oDataParser.ParseFilter(filterString) as TermFilter;

            var filterValue = (FieldFilterValue<long>)filter.Value;
            var expectedValue = FieldFilterValue.Create((long)123);

            Assert.Equal(expectedValue.Value, filterValue.Value);
        }

        [Fact]
        public void ParseFilter_ShouldReturnDateTimeFilterValue_WhenMatchingDateTimeOffsetUtc()
        {
            string filterString = "Created eq 2002-10-10T17:00:00Z";

            var oDataParser = CreateODataParser();

            var filter = oDataParser.ParseFilter(filterString) as TermFilter;

            var filterValue = (FieldFilterValue<DateTime>)filter.Value;
            var expectedValue = FieldFilterValue.Create(new DateTime(2002, 10, 10, 17, 0, 0));

            Assert.Equal(expectedValue.Value, filterValue.Value);
        }

        [Fact]
        public void ParseFilter_ShouldReturnDateTimeFilterValue_WhenMatchingDateTimeOffsetLocal()
        {
            string filterString = "Created eq 2002-10-10T17:00:00-05:00";

            var oDataParser = CreateODataParser();

            var filter = oDataParser.ParseFilter(filterString) as TermFilter;

            var filterValue = (FieldFilterValue<DateTime>)filter.Value;
            var expectedValue = FieldFilterValue.Create(new DateTime(2002, 10, 10, 22, 0, 0));

            Assert.Equal(expectedValue.Value, filterValue.Value);
        }

        [Fact]
        public void ParseFilter_ShouldReturnAndFilter_WhenMatchingMultipleCriteria()
        {
            string filterString = "ContentLink/Id eq 123 and Name eq 'Start'";

            var oDataParser = CreateODataParser();

            var filter = oDataParser.ParseFilter(filterString) as AndFilter;

            var firstExpectedFilter =
                new TermFilter("ContentApiModel.ContentLink.Id$$number", FieldFilterValue.Create(123));
            var secondExpectedFilter = new TermFilter("ContentApiModel.Name$$string", FieldFilterValue.Create("Start"));

            var firstActualFilter = filter.Filters[0] as TermFilter;
            var secondActualFilter = filter.Filters[1] as TermFilter;
            Assert.Equal(2, filter.Filters.Count);
            Assert.Equal(firstExpectedFilter.Field, firstActualFilter.Field);
            Assert.Equal(secondExpectedFilter.Field, secondActualFilter.Field);
        }

        [Fact]
        public void ParseFilter_ShouldReturnOrFilter_WhenMatchingMultipleCriteria()
        {
            string filterString = "ContentLink/Id eq 123 or Name eq 'Start'";

            var oDataParser = CreateODataParser();

            var filter = oDataParser.ParseFilter(filterString) as OrFilter;

            var firstExpectedFilter =
                new TermFilter("ContentApiModel.ContentLink.Id$$number", FieldFilterValue.Create(123));
            var secondExpectedFilter = new TermFilter("ContentApiModel.Name$$string", FieldFilterValue.Create("Start"));

            var firstActualFilter = filter.Filters[0] as TermFilter;
            var secondActualFilter = filter.Filters[1] as TermFilter;
            Assert.Equal(2, filter.Filters.Count);
            Assert.Equal(firstExpectedFilter.Field, firstActualFilter.Field);
            Assert.Equal(secondExpectedFilter.Field, secondActualFilter.Field);
        }

        [Fact]
        public void ParseFilter_ShouldReturnRangeFilter_WhenFilteringLocalDateGreaterEqual()
        {
            string filterString = "Created ge 2017-12-01T12:00:00-05:00";

            var oDataParser = CreateODataParser();

            var filter = oDataParser.ParseFilter(filterString) as RangeFilter<DateTime>;

            Assert.Equal("ContentApiModel.Created$$date", filter.Field);
            Assert.Equal(filter.From, new DateTime(2017, 12, 01, 17, 0, 0, DateTimeKind.Utc));
            Assert.Equal(filter.To, DateTime.MaxValue);
            Assert.Equal(true, filter.IncludeUpper);
            Assert.Equal(true, filter.IncludeLower);
        }

        [Fact]
        public void ParseFilter_ShouldReturnRangeFilter_WhenFilteringLocalUtcDateGreaterEqual()
        {
            string filterString = "Created ge 2017-12-01T12:00:00Z";

            var oDataParser = CreateODataParser();

            var filter = oDataParser.ParseFilter(filterString) as RangeFilter<DateTime>;

            Assert.Equal("ContentApiModel.Created$$date", filter.Field);
            Assert.Equal(filter.From, new DateTime(2017, 12, 01, 12, 0, 0, DateTimeKind.Utc));
            Assert.Equal(filter.To, DateTime.MaxValue);
            Assert.Equal(true, filter.IncludeUpper);
            Assert.Equal(true, filter.IncludeLower);
        }


        [Fact]
        public void ParseFilter_ShouldReturnRangeFilter_WhenFilteringLocalDateGreaterThan()
        {
            string filterString = "Created gt 2017-12-01T12:00:00-05:00";

            var oDataParser = CreateODataParser();

            var filter = oDataParser.ParseFilter(filterString) as RangeFilter<DateTime>;

            Assert.Equal("ContentApiModel.Created$$date", filter.Field);
            Assert.Equal(filter.From, new DateTime(2017, 12, 01, 17, 0, 0, DateTimeKind.Utc));
            Assert.Equal(filter.To, DateTime.MaxValue);
            Assert.Equal(true, filter.IncludeUpper);
            Assert.Equal(false, filter.IncludeLower);
        }

        [Fact]
        public void ParseFilter_ShouldReturnRangeFilter_WhenFilteringUtcateGreaterThan()
        {
            string filterString = "Created gt 2017-12-01T12:00:00Z";

            var oDataParser = CreateODataParser();

            var filter = oDataParser.ParseFilter(filterString) as RangeFilter<DateTime>;

            Assert.Equal("ContentApiModel.Created$$date", filter.Field);
            Assert.Equal(filter.From, new DateTime(2017, 12, 01, 12, 0, 0, DateTimeKind.Utc));
            Assert.Equal(filter.To, DateTime.MaxValue);
            Assert.Equal(true, filter.IncludeUpper);
            Assert.Equal(false, filter.IncludeLower);
        }

        [Fact]
        public void ParseFilter_ShouldReturnRangeFilter_WhenFilteringLocalDateLessThanEqual()
        {
            string filterString = "Created le 2017-12-01T12:00:00-05:00";

            var oDataParser = CreateODataParser();

            var filter = oDataParser.ParseFilter(filterString) as RangeFilter<DateTime>;

            Assert.Equal("ContentApiModel.Created$$date", filter.Field);
            Assert.Equal(filter.From, DateTime.MinValue);
            Assert.Equal(filter.To, new DateTime(2017, 12, 01, 17, 0, 0, DateTimeKind.Utc));
            Assert.Equal(true, filter.IncludeUpper);
            Assert.Equal(true, filter.IncludeLower);
        }

        [Fact]
        public void ParseFilter_ShouldReturnRangeFilter_WhenFilteringUtcDateLessThanEqual()
        {
            string filterString = "Created le 2017-12-01T12:00:00Z";

            var oDataParser = CreateODataParser();

            var filter = oDataParser.ParseFilter(filterString) as RangeFilter<DateTime>;

            Assert.Equal("ContentApiModel.Created$$date", filter.Field);
            Assert.Equal(filter.From, DateTime.MinValue);
            Assert.Equal(filter.To, new DateTime(2017, 12, 01, 12, 0, 0, DateTimeKind.Utc));
            Assert.Equal(true, filter.IncludeUpper);
            Assert.Equal(true, filter.IncludeLower);
        }

        [Fact]
        public void ParseFilter_ShouldReturnRangeFilter_WhenFilteringLocalDateLessThan()
        {
            string filterString = "Created lt 2017-12-01T12:00:00-05:00";

            var oDataParser = CreateODataParser();

            var filter = oDataParser.ParseFilter(filterString) as RangeFilter<DateTime>;

            Assert.Equal("ContentApiModel.Created$$date", filter.Field);
            Assert.Equal(filter.From, DateTime.MinValue);
            Assert.Equal(filter.To, new DateTime(2017, 12, 01, 17, 0, 0, DateTimeKind.Utc));
            Assert.Equal(false, filter.IncludeUpper);
            Assert.Equal(true, filter.IncludeLower);
        }

        [Fact]
        public void ParseFilter_ShouldReturnRangeFilter_WhenFilteringUtcDateLessThan()
        {
            string filterString = "Created lt 2017-12-01T12:00:00Z";

            var oDataParser = CreateODataParser();

            var filter = oDataParser.ParseFilter(filterString) as RangeFilter<DateTime>;

            Assert.Equal("ContentApiModel.Created$$date", filter.Field);
            Assert.Equal(filter.From, DateTime.MinValue);
            Assert.Equal(filter.To, new DateTime(2017, 12, 01, 12, 0, 0, DateTimeKind.Utc));
            Assert.Equal(false, filter.IncludeUpper);
            Assert.Equal(true, filter.IncludeLower);
        }

        [Fact]
        public void ParseFilter_ShouldReturnRangeFilter_WhenFilteringIntegerLessThan()
        {
            string filterString = "MyIntegerValue lt 232";

            var oDataParser = CreateODataParser();

            var filter = oDataParser.ParseFilter(filterString) as RangeFilter<int>;

            Assert.Equal("ContentApiModel.MyIntegerValue", filter.Field);
            Assert.Equal(filter.From, Int32.MinValue);
            Assert.Equal(232, filter.To);
            Assert.Equal(false, filter.IncludeUpper);
            Assert.Equal(true, filter.IncludeLower);
        }

        [Fact]
        public void ParseFilter_ShouldReturnRangeFilter_WhenFilteringIntegerLessThanEqual()
        {
            string filterString = "MyIntegerValue le 232";

            var oDataParser = CreateODataParser();

            var filter = oDataParser.ParseFilter(filterString) as RangeFilter<int>;

            Assert.Equal("ContentApiModel.MyIntegerValue", filter.Field);
            Assert.Equal(filter.From, Int32.MinValue);
            Assert.Equal(232, filter.To);
            Assert.Equal(true, filter.IncludeUpper);
            Assert.Equal(true, filter.IncludeLower);
        }

        [Fact]
        public void ParseFilter_ShouldReturnRangeFilter_WhenFilteringIntegerGreaterThan()
        {
            string filterString = "MyIntegerValue gt 232";

            var oDataParser = CreateODataParser();

            var filter = oDataParser.ParseFilter(filterString) as RangeFilter<int>;

            Assert.Equal("ContentApiModel.MyIntegerValue", filter.Field);
            Assert.Equal(232, filter.From);
            Assert.Equal(filter.To, Int32.MaxValue);
            Assert.Equal(true, filter.IncludeUpper);
            Assert.Equal(false, filter.IncludeLower);
        }

        [Fact]
        public void ParseFilter_ShouldReturnRangeFilter_WhenFilteringIntegerGreaterThanEqual()
        {
            string filterString = "MyIntegerValue ge 232";

            var oDataParser = CreateODataParser();

            var filter = oDataParser.ParseFilter(filterString) as RangeFilter<int>;

            Assert.Equal("ContentApiModel.MyIntegerValue", filter.Field);
            Assert.Equal(232, filter.From);
            Assert.Equal(filter.To, Int32.MaxValue);
            Assert.Equal(true, filter.IncludeUpper);
            Assert.Equal(true, filter.IncludeLower);
        }

        [Fact]
        public void ParseFilter_ShouldReturnRangeFilter_WhenFilteringDoubleLessThan()
        {
            string filterString = "MyDoubleValue lt 232.00d";

            var oDataParser = CreateODataParser();

            var filter = oDataParser.ParseFilter(filterString) as RangeFilter<Double>;

            Assert.Equal("ContentApiModel.MyDoubleValue", filter.Field);
            Assert.Equal(filter.From, Double.MinValue);
            Assert.Equal(232.00, filter.To);
            Assert.Equal(false, filter.IncludeUpper);
            Assert.Equal(true, filter.IncludeLower);
        }

        [Fact]
        public void ParseFilter_ShouldReturnRangeFilter_WhenFilteringDoubleLessThanEqual()
        {
            string filterString = "MyDoubleValue le 232.00d";

            var oDataParser = CreateODataParser();

            var filter = oDataParser.ParseFilter(filterString) as RangeFilter<Double>;

            Assert.Equal("ContentApiModel.MyDoubleValue", filter.Field);
            Assert.Equal(filter.From, Double.MinValue);
            Assert.Equal(232.00, filter.To);
            Assert.Equal(true, filter.IncludeUpper);
            Assert.Equal(true, filter.IncludeLower);
        }

        [Fact]
        public void ParseFilter_ShouldReturnRangeFilter_WhenFilteringDoubleGreaterThan()
        {
            string filterString = "MyDoubleValue gt 232.00d";

            var oDataParser = CreateODataParser();

            var filter = oDataParser.ParseFilter(filterString) as RangeFilter<Double>;

            Assert.Equal("ContentApiModel.MyDoubleValue", filter.Field);
            Assert.Equal(232.00, filter.From);
            Assert.Equal(filter.To, Double.MaxValue);
            Assert.Equal(true, filter.IncludeUpper);
            Assert.Equal(false, filter.IncludeLower);
        }

        [Fact]
        public void ParseFilter_ShouldReturnRangeFilter_WhenFilteringDoubleGreaterThanEqual()
        {
            string filterString = "MyDoubleValue ge 232.00d";

            var oDataParser = CreateODataParser();

            var filter = oDataParser.ParseFilter(filterString) as RangeFilter<Double>;

            Assert.Equal("ContentApiModel.MyDoubleValue", filter.Field);
            Assert.Equal(232.00, filter.From);
            Assert.Equal(filter.To, Double.MaxValue);
            Assert.Equal(true, filter.IncludeUpper);
            Assert.Equal(true, filter.IncludeLower);
        }

        [Fact]
        public void ParseFilter_ShouldReturnRangeFilter_WhenFilteringDecimalLessThan()
        {
            string filterString = "MyDecimalValue lt 232.00M";

            var oDataParser = CreateODataParser();

            var filter = oDataParser.ParseFilter(filterString) as RangeFilter<Decimal>;

            Assert.Equal("ContentApiModel.MyDecimalValue", filter.Field);
            Assert.Equal(filter.From, Decimal.MinValue);
            Assert.Equal(232.00M, filter.To);
            Assert.Equal(false, filter.IncludeUpper);
            Assert.Equal(true, filter.IncludeLower);
        }

        [Fact]
        public void ParseFilter_ShouldReturnRangeFilter_WhenFilteringDecimalLessThanEqual()
        {
            string filterString = "MyDecimalValue le 232.00M";

            var oDataParser = CreateODataParser();

            var filter = oDataParser.ParseFilter(filterString) as RangeFilter<Decimal>;

            Assert.Equal("ContentApiModel.MyDecimalValue", filter.Field);
            Assert.Equal(filter.From, Decimal.MinValue);
            Assert.Equal(232.00M, filter.To);
            Assert.Equal(true, filter.IncludeUpper);
            Assert.Equal(true, filter.IncludeLower);
        }

        [Fact]
        public void ParseFilter_ShouldReturnRangeFilter_WhenFilteringDecimalGreaterThan()
        {
            string filterString = "MyDecimalValue gt 232.00M";

            var oDataParser = CreateODataParser();

            var filter = oDataParser.ParseFilter(filterString) as RangeFilter<Decimal>;

            Assert.Equal("ContentApiModel.MyDecimalValue", filter.Field);
            Assert.Equal(232.00M, filter.From);
            Assert.Equal(filter.To, Decimal.MaxValue);
            Assert.Equal(true, filter.IncludeUpper);
            Assert.Equal(false, filter.IncludeLower);
        }

        [Fact]
        public void ParseFilter_ShouldReturnRangeFilter_WhenFilteringDecimalGreaterThanEqual()
        {
            string filterString = "MyDecimalValue ge 232.00M";

            var oDataParser = CreateODataParser();

            var filter = oDataParser.ParseFilter(filterString) as RangeFilter<Decimal>;

            Assert.Equal("ContentApiModel.MyDecimalValue", filter.Field);
            Assert.Equal(232.00M, filter.From);
            Assert.Equal(filter.To, Decimal.MaxValue);
            Assert.Equal(true, filter.IncludeUpper);
            Assert.Equal(true, filter.IncludeLower);
        }

        [Fact]
        public void ParseFilter_ShouldReturnRangeFilter_WhenFilteringLongLessThan()
        {
            string filterString = "MyLongValue lt 232L";

            var oDataParser = CreateODataParser();

            var filter = oDataParser.ParseFilter(filterString) as RangeFilter<long>;

            Assert.Equal("ContentApiModel.MyLongValue", filter.Field);
            Assert.Equal(filter.From, long.MinValue);
            Assert.Equal(232, filter.To);
            Assert.Equal(false, filter.IncludeUpper);
            Assert.Equal(true, filter.IncludeLower);
        }

        [Fact]
        public void ParseFilter_ShouldReturnRangeFilter_WhenFilteringLongLessThanEqual()
        {
            string filterString = "MyLongValue le 232L";

            var oDataParser = CreateODataParser();

            var filter = oDataParser.ParseFilter(filterString) as RangeFilter<long>;

            Assert.Equal("ContentApiModel.MyLongValue", filter.Field);
            Assert.Equal(filter.From, long.MinValue);
            Assert.Equal(232, filter.To);
            Assert.Equal(true, filter.IncludeUpper);
            Assert.Equal(true, filter.IncludeLower);
        }

        [Fact]
        public void ParseFilter_ShouldReturnRangeFilter_WhenFilteringLongGreaterThan()
        {
            string filterString = "MyLongValue gt 232L";

            var oDataParser = CreateODataParser();

            var filter = oDataParser.ParseFilter(filterString) as RangeFilter<long>;

            Assert.Equal("ContentApiModel.MyLongValue", filter.Field);
            Assert.Equal(232, filter.From);
            Assert.Equal(filter.To, long.MaxValue);
            Assert.Equal(true, filter.IncludeUpper);
            Assert.Equal(false, filter.IncludeLower);
        }

        [Fact]
        public void ParseFilter_ShouldReturnRangeFilter_WhenFilteringLongGreaterThanEqual()
        {
            string filterString = "MyLongValue ge 232L";

            var oDataParser = CreateODataParser();

            var filter = oDataParser.ParseFilter(filterString) as RangeFilter<long>;

            Assert.Equal("ContentApiModel.MyLongValue", filter.Field);
            Assert.Equal(232, filter.From);
            Assert.Equal(filter.To, long.MaxValue);
            Assert.Equal(true, filter.IncludeUpper);
            Assert.Equal(true, filter.IncludeLower);
        }

        [Fact]
        public void ParseFilter_ShouldReturnRangeFilter_WhenFilteringFloatLessThan()
        {
            string filterString = "MyFloatValue lt 232.00";

            var oDataParser = CreateODataParser();

            var filter = oDataParser.ParseFilter(filterString) as RangeFilter<float>;

            Assert.Equal("ContentApiModel.MyFloatValue", filter.Field);
            Assert.Equal(filter.From, float.MinValue);
            Assert.Equal(232, filter.To);
            Assert.Equal(false, filter.IncludeUpper);
            Assert.Equal(true, filter.IncludeLower);
        }

        [Fact]
        public void ParseFilter_ShouldReturnRangeFilter_WhenFilteringFloatLessThanEqual()
        {
            string filterString = "MyFloatValue le 232.00";

            var oDataParser = CreateODataParser();

            var filter = oDataParser.ParseFilter(filterString) as RangeFilter<float>;

            Assert.Equal("ContentApiModel.MyFloatValue", filter.Field);
            Assert.Equal(filter.From, float.MinValue);
            Assert.Equal(232.00, filter.To);
            Assert.Equal(true, filter.IncludeUpper);
            Assert.Equal(true, filter.IncludeLower);
        }

        [Fact]
        public void ParseFilter_ShouldReturnRangeFilter_WhenFilteringFloatGreaterThan()
        {
            string filterString = "MyFloatValue gt 232.00";

            var oDataParser = CreateODataParser();

            var filter = oDataParser.ParseFilter(filterString) as RangeFilter<float>;

            Assert.Equal("ContentApiModel.MyFloatValue", filter.Field);
            Assert.Equal(232.00, filter.From);
            Assert.Equal(filter.To, float.MaxValue);
            Assert.Equal(true, filter.IncludeUpper);
            Assert.Equal(false, filter.IncludeLower);
        }

        [Fact]
        public void ParseFilter_ShouldReturnRangeFilter_WhenFilteringFloatGreaterThanEqual()
        {
            string filterString = "MyFloatValue ge 232.00";

            var oDataParser = CreateODataParser();

            var filter = oDataParser.ParseFilter(filterString) as RangeFilter<float>;

            Assert.Equal("ContentApiModel.MyFloatValue", filter.Field);
            Assert.Equal(232.00, filter.From);
            Assert.Equal(filter.To, float.MaxValue);
            Assert.Equal(true, filter.IncludeUpper);
            Assert.Equal(true, filter.IncludeLower);
        }

        [Fact]
        public void ParseFilter_ShouldThrowException_WhenGreaterThanRangeFilterOnString()
        {
            string filterString = "Name ge 'A'";

            var oDataParser = CreateODataParser();

            Assert.Throws<FilterParseException>(() => oDataParser.ParseFilter(filterString));
        }

        [Fact]
        public void ParseFilter_ShouldThrowException_WhenLessThanRangeFilterOnString()
        {
            string filterString = "Name le 'A'";

            var oDataParser = CreateODataParser();

            Assert.Throws<FilterParseException>(() => oDataParser.ParseFilter(filterString));
        }


        [Fact]
        public void ParseFilter_ShouldReturnCorrectFilters_WhenFilterContainsParenthesis()
        {
            string filterString = "Name eq 'Root' or (ContentLink/Id eq 123 and Name eq 'Start')";

            var oDataParser = CreateODataParser();

            var filter = oDataParser.ParseFilter(filterString) as OrFilter;

            var firstFilter = new TermFilter("ContentApiModel.Name$$string", FieldFilterValue.Create("Root"));

            var firstActualFilter = filter.Filters[0] as TermFilter;
            var secondActualFilter = filter.Filters[1] as AndFilter;

            Assert.Equal(2, filter.Filters.Count);
            Assert.Equal(firstFilter.Field, firstActualFilter.Field);
            Assert.Equal(2, secondActualFilter.Filters.Count);
        }

        [Fact]
        public void ParseFilter_ShouldThrowException_WhenFilterFormatIsIncorrect()
        {
            string filterString = "Name eq Test";

            var oDataParser = CreateODataParser();

            Assert.Throws<FilterParseException>(() => oDataParser.ParseFilter(filterString));
        }

        [Fact]
        public void ParseFilter_ShouldThrowException_WhenUnsupportedFunctionIsUsed()
        {
            string filterString = "toupper(Name) eq 'Test'";

            var oDataParser = CreateODataParser();

            Assert.Throws<FilterParseException>(() => oDataParser.ParseFilter(filterString));
        }

        [Fact]
        public void ParseFilter_ShouldReturnCorrectFilters_WhenFilterContainsSpecialCharacters()
        {
            string filterString = "contains(Name, 'te#st')";

            var oDataParser = CreateODataParser();

            var filter = oDataParser.ParseFilter(filterString) as QueryFilter;
            var query = filter.Query as WildcardQuery;
            Assert.Equal("ContentApiModel.Name$$string", query.Field);
            Assert.Equal("*te#st*", query.Value);
        }

        [Fact]
        public void ParseOrderBy_ShouldReturnSingleSort_WhenOrderByProperty()
        {
            string orderByString = "Name";

            var oDataParser = CreateODataParser();

            var sorting = oDataParser.ParseOrderBy(orderByString).ToList();

            Assert.Single(sorting);
        }

        [Fact]
        public void ParseOrderBy_ShouldReturnSingleSort_WhenOrderByOpenProperty()
        {
            string orderByString = "MyProperty";

            var oDataParser = CreateODataParser();

            var sorting = oDataParser.ParseOrderBy(orderByString).ToList();

            Assert.Single(sorting);
        }

        [Fact]
        public void ParseOrderBy_ShouldReturnAscendingSort_WhenOrderByHasNoDirection()
        {
            string orderByString = "Name";

            var oDataParser = CreateODataParser();

            var sorting = oDataParser.ParseOrderBy(orderByString).ToList();

            var firstClause = sorting[0];

            Assert.Equal(SortOrder.Ascending, firstClause.Order);
            Assert.Equal(SortMissing.First, firstClause.Missing);
        }

        [Fact]
        public void ParseOrderBy_ShouldReturnAscendingSort_WhenOrderByIsAscending()
        {
            string orderByString = "Name asc";

            var oDataParser = CreateODataParser();

            var sorting = oDataParser.ParseOrderBy(orderByString).ToList();

            var firstClause = sorting[0];

            Assert.Equal(SortOrder.Ascending, firstClause.Order);
            Assert.Equal(SortMissing.First, firstClause.Missing);
        }

        [Fact]
        public void ParseOrderBy_ShouldReturnDescendingSort_WhenOrderByIsAscending()
        {
            string orderByString = "Name desc";

            var oDataParser = CreateODataParser();

            var sorting = oDataParser.ParseOrderBy(orderByString).ToList();

            var firstClause = sorting[0];

            Assert.Equal(SortOrder.Descending, firstClause.Order);
            Assert.Equal(SortMissing.Last, firstClause.Missing);
        }

        [Fact]
        public void ParseOrderBy_ShouldAttachSortToPropertyName_WhenOrderByString()
        {
            string orderByString = "Name";

            var oDataParser = CreateODataParser();

            var sorting = oDataParser.ParseOrderBy(orderByString).ToList();

            var firstClause = sorting[0];

            Assert.Equal("ContentApiModel.Name$$string.sort", firstClause.FieldName);
        }


        [Fact]
        public void ParseOrderBy_ShouldNotAttachSortToPropertyName_WhenOrderByInteger()
        {
            string orderByString = "ContentLink/Id";

            var oDataParser = CreateODataParser();

            var sorting = oDataParser.ParseOrderBy(orderByString).ToList();

            var firstClause = sorting[0];

            Assert.Equal("ContentApiModel.ContentLink.Id$$number", firstClause.FieldName);
        }

        [Fact]
        public void ParseOrderBy_ShouldNotAttachSortToPropertyName_WhenOrderByOpenProperty()
        {
            string orderByString = "CustomProperty";

            var oDataParser = CreateODataParser();

            var sorting = oDataParser.ParseOrderBy(orderByString).ToList();

            var firstClause = sorting[0];

            Assert.Equal("ContentApiModel.CustomProperty", firstClause.FieldName);
        }


        [Fact]
        public void ParseOrderBy_ShouldReturnMultipleSort_WhenOrderByMultipleCriteria()
        {
            string orderByString = "Name, MyProperty desc";

            var oDataParser = CreateODataParser();

            var sorting = oDataParser.ParseOrderBy(orderByString).ToList();

            var firstClause = sorting[0];
            var secondClause = sorting[1];

            Assert.Equal(2, sorting.Count);
            Assert.Equal("ContentApiModel.Name$$string.sort", firstClause.FieldName);
            Assert.Equal(SortOrder.Ascending, firstClause.Order);
            Assert.Equal(SortMissing.First, firstClause.Missing);

            Assert.Equal(true, firstClause.IgnoreUnmapped);

            Assert.Equal("ContentApiModel.MyProperty", secondClause.FieldName);
            Assert.Equal(SortOrder.Descending, secondClause.Order);
            Assert.Equal(SortMissing.Last, secondClause.Missing);
            Assert.Equal(true, secondClause.IgnoreUnmapped);
        }

        [Fact]
        public void ParseOrderBy_ShouldThrowException_WhenOrderByFormatIsIncorrect()
        {
            string orderByString = "Name fasdfadsfadsf";

            var oDataParser = CreateODataParser();

            Assert.Throws<OrderByParseException>(() => oDataParser.ParseOrderBy(orderByString));
        }

        private FindODataParser CreateODataParser()
        {
            var client = new Mock<IClient>();
            var mockSearch = new Mock<ITypeSearch<IContent>>();
            mockSearch.Setup(x => x.Client).Returns(client.Object);

            client.Setup(x => x.Search<IContent>()).Returns(mockSearch.Object);
            client.Setup(x => x.Conventions).Returns(new DefaultConventions(client.Object));


            return new FindODataParser(client.Object, new ODataValidationSettings
            {
                AllowedFunctions = AllowedFunctions.ToLower | AllowedFunctions.Contains | AllowedFunctions.Any,
                AllowedLogicalOperators = AllowedLogicalOperators.LessThanOrEqual | AllowedLogicalOperators.LessThan |
                    AllowedLogicalOperators.GreaterThan | AllowedLogicalOperators.GreaterThanOrEqual |
                    AllowedLogicalOperators.Equal | AllowedLogicalOperators.NotEqual |
                    AllowedLogicalOperators.And | AllowedLogicalOperators.Or,
                AllowedArithmeticOperators = AllowedArithmeticOperators.None,
                AllowedQueryOptions = AllowedQueryOptions.Filter,
            });
        }
    }
}
#pragma warning restore CS0612 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete
