using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.OData.Builder;
using System.Web.OData.Query;
using System.Web.OData.Query.Validators;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.Find;
using EPiServer.Find.Api;
using EPiServer.Find.Api.Querying;
using EPiServer.Find.Api.Querying.Filters;
using EPiServer.Find.Api.Querying.Queries;
using EPiServer.Logging;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace EPiServer.ContentApi.Search.Internal
{

    /// <summary>
    ///     <see cref="IFindODataParser"/> implementation which leverages <see cref="UriParser"/> in order to parse filter and orderby strings, 
    ///     and subsequently converts them to <see cref="Filter"/> and <see cref="Sorting"/> instances for use with Episerver Find"/>
    /// </summary>
    public class FindODataParser : IFindODataParser
    {
        protected const string ODataToLowerFunction = "tolower";
        protected const string ODataContainsFunction = "contains";

        private readonly IClient _searchClient;
        private readonly FilterQueryValidator _filterQueryValidator;
        private readonly IEdmModel _model;
        private readonly ILogger _log = LogManager.GetLogger(typeof(FindODataParser));
        
        private readonly ODataValidationSettings _oDataFilterValidationSettings;        

        public FindODataParser(IClient searchClient, ODataValidationSettings oDataValidationSettings)
        {
            _searchClient = searchClient;
            _filterQueryValidator = new FilterQueryValidator(new DefaultQuerySettings { EnableFilter = true });
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<ContentApiModel>("Search").EntityType.HasKey(x => x.Name);
            _model = builder.GetEdmModel();
            _oDataFilterValidationSettings = oDataValidationSettings;
        }

        /// <summary>
        ///     Given a OData orderby string, parse the orderby into a List of Find <see cref="Sorting"/> objects
        /// </summary>
        /// <param name="orderby">Orderby string to process. Example: Changed desc, Name asc</param>
        /// <returns>List of <see cref="Sorting"/> instances, if successfully parsed from the provided orderby string</returns>
        /// <exception cref="OrderByParseException">Thrown when an orderby string cannot be successfully parsed</exception>
        [Obsolete]
        public IEnumerable<Sorting> ParseOrderBy(string orderby)
        {

            try
            {
                var orderByClause = CreateOrderByClause(orderby);

                var sorting = new List<Sorting>();

                while (orderByClause != null)
                {
                    var propertyName = ParsePropertyName(orderByClause.Expression);
                    var fieldName = CreateFieldNameForSearch(propertyName);

                    var propertyType = orderByClause.Expression.TypeReference;
                    if (propertyType != null && propertyType.IsString())
                    {
                        fieldName = $"{fieldName}.sort";
                    }

                    sorting.Add(new Sorting(fieldName)
                    {
                        Order = orderByClause.Direction == OrderByDirection.Ascending
                            ? SortOrder.Ascending
                            : SortOrder.Descending,
                        IgnoreUnmapped = true,
                        Missing = orderByClause.Direction == OrderByDirection.Ascending
                            ? SortMissing.First
                            : SortMissing.Last
                    });

                    orderByClause = orderByClause.ThenBy;
                }
                return sorting;
            }
            catch (OrderByParseException)
            {
                throw;
            }
            catch (ODataException e)
            {
                throw new OrderByParseException(e.Message, e);
            }
            catch (Exception e)
            {
                throw new OrderByParseException("Unable to parse provided orderby clause", e);
            }
        }

        /// <summary>
        ///     Given an OData order by clause (like Changed desc, Name asc), construct a <see cref="OrderByClause"/> via <see cref="ODataUriParser"/> to represent the orderby in a tree structure.
        /// </summary>
        /// <param name="orderby">Orderby string to use to generate the FilterClause</param>
        /// <returns><see cref="OrderByClause"/> instance, if successfully parsed from the provided filter string</returns>
        private OrderByClause CreateOrderByClause(string orderby)
        {
            var uriParser = new ODataUriParser(_model, new Uri($"Search?$orderby={orderby}", UriKind.Relative));

            var orderByClause = uriParser.ParseOrderBy();

            return orderByClause;
        }


        /// <summary>
        ///     Given a OData filter string, parse the filter into a Find <see cref="Filter"/>
        /// </summary>
        /// <param name="filter">Filter string to process. Example: ContentLink.Id</param>
        /// <returns><see cref="FilterClause"/> instance, if successfully parsed from the provided filter string</returns>
        /// <exception cref="FilterParseException">Thrown when a filter string cannot be successfully parsed</exception>
        public Filter ParseFilter(string filter)
        {
            try
            {
                var filterClause = CreateFilterClause(filter);

                return ParseFilterInternal(filterClause.Expression);
            }
            catch (FilterParseException filterParseException)
            {
                _log.Error("fail to parse filter", filterParseException);
                throw;
            }
            catch (ODataException oDataException)
            {
                _log.Error("OData related errors happen", oDataException);
                throw new FilterParseException(oDataException.Message, oDataException);
            }
            catch (Exception e)
            {
                _log.Error("Unable to parse provided filter clause", e);
                throw new FilterParseException("Unable to parse provided filter clause", e);
            }
        }

        /// <summary>
        ///     Given an OData string (like ContentLink.Id eq 23), construct a <see cref="FilterClause"/> via <see cref="ODataUriParser"/> to represent the filter in a tree structure.
        /// </summary>
        /// <param name="filter">Filter string to use to generate the FilterClause</param>
        /// <returns><see cref="FilterClause"/> instance, if successfully parsed from the provided filter string</returns>
        private FilterClause CreateFilterClause(string filter)
        {
            var uriParser = new ODataUriParser(_model, new Uri($"Search?$filter={Url.Encode(filter)}", UriKind.Relative));

            var filterClause = uriParser.ParseFilter();

            _filterQueryValidator.Validate(filterClause, _oDataFilterValidationSettings, _model);

            return filterClause;
        }

        /// <summary>
        ///     Recursive function for parsing a provided <see cref="SingleValueNode"/> which may contain multiple And/Or clauses, which contain multiple value-based filters
        /// </summary>
        /// <param name="singleValueNode">The SingleValueNode to parse</param>
        /// <returns>A <see cref="Filter"/> instance which represents the provided <see cref="SingleValueNode"/> in the form of a Find filter</returns>
        private Filter ParseFilterInternal(SingleValueNode singleValueNode)
        {
            var singleFunctionNode = GetSingleValueFunctionCallNode(singleValueNode);
            if (singleFunctionNode != null && singleFunctionNode.Name == ODataContainsFunction)
            {
                return ParseContainsFunction(singleFunctionNode);
            }

            var anyFunctionNode = singleValueNode as AnyNode;
            if (anyFunctionNode != null)
            {
                return ParseAnyFunction(anyFunctionNode);
            }

            var binaryOperator = GetBinaryOperatorNode(singleValueNode);

            if (binaryOperator == null) return null;

            if (binaryOperator.OperatorKind == BinaryOperatorKind.Or)
            {
                return new OrFilter(ParseFilterInternal(binaryOperator.Left), ParseFilterInternal(binaryOperator.Right));
            }

            if (binaryOperator.OperatorKind == BinaryOperatorKind.And)
            {
                return new AndFilter(ParseFilterInternal(binaryOperator.Left), ParseFilterInternal(binaryOperator.Right));
            }

            return ParseValueFilter(binaryOperator);
        }

        /// <summary>
        ///     Parses an any function into a Term Filter. 
        /// </summary>
        /// <param name="anyFunctionNode"><see cref="AnyNode"/> to parse</param>
        /// <remarks>Limited support to single property expression lambda - like this: ContentType/any(t:t eq 'CheckoutPage')</remarks>
        private Filter ParseAnyFunction(AnyNode anyFunctionNode)
        {
            var propertyName = ParsePropertyName(anyFunctionNode.Source);

            var binaryOperatorNode = anyFunctionNode.Body as BinaryOperatorNode;
            var function = ParseFunctionName(binaryOperatorNode.Left);
            var parsedNode = ParseSingleValueNode(binaryOperatorNode.Right);
            var fieldName = CreateFieldNameForSearch(propertyName, function);

            IEdmTypeReference valueType = parsedNode.Item1;
            object value = parsedNode.Item2 as string;

            var termFilter = new TermFilter(fieldName, FilterHelper.GetTypedFieldFilterValue(valueType, value));

            if (binaryOperatorNode.OperatorKind == BinaryOperatorKind.NotEqual)
            {
                return !termFilter;
            }

            return termFilter;
        }

        /// <summary>
        ///     Parses a contains function into a WildcardQuery
        /// </summary>
        /// <param name="single"><see cref="SingleValueFunctionCallNode"/> representing the contains function</param>
        private Filter ParseContainsFunction(SingleValueFunctionCallNode single)
        {
            var parameters = single.Parameters.Cast<SingleValueNode>().ToList();
            var function = ParseFunctionName(parameters[0]);
            var propertyName = ParsePropertyName(parameters[0]);
            var parsedNode = ParseSingleValueNode(parameters[1]);
            var fieldName = CreateFieldNameForSearch(propertyName, function);

            object value = parsedNode.Item2 as string;

            return new QueryFilter(new WildcardQuery(fieldName, $"*{value}*"));
        }

        /// <summary>
        ///     Parses a value filter (one that contains a left and right side comparison) from a provided <see cref="BinaryOperatorNode"/>.
        ///     Returns a <see cref="Filter"/> instance which may be different types depending on the filters contained within the provided BinaryOperatorNode
        /// </summary>
        /// <param name="binaryOperator">The BinaryOperatorNode to parse</param>
        /// <returns><see cref="Filter"/> object which represents the OData filter converted for use with Find</returns>
        private Filter ParseValueFilter(BinaryOperatorNode binaryOperator)
        {
            var propertyName = ParsePropertyName(binaryOperator.Left);
            var function = ParseFunctionName(binaryOperator.Left);
            var parsedNode = ParseSingleValueNode(binaryOperator.Right);
            IEdmTypeReference valueType = parsedNode.Item1;
            object value = parsedNode.Item2;
            var fieldName = CreateFieldNameForSearch(propertyName, function);

            if (binaryOperator.OperatorKind == BinaryOperatorKind.Equal)
            {
                if (value == null)
                {
                    return !new ExistsFilter(fieldName);
                }

                return new TermFilter(fieldName, FilterHelper.GetTypedFieldFilterValue(valueType, value));
            }
            else if (binaryOperator.OperatorKind == BinaryOperatorKind.NotEqual)
            {
                if (value == null)
                {
                    return new ExistsFilter(fieldName);
                }

                return !new TermFilter(fieldName, FilterHelper.GetTypedFieldFilterValue(valueType, value));
            }
            else if (binaryOperator.OperatorKind == BinaryOperatorKind.GreaterThan ||
                     binaryOperator.OperatorKind == BinaryOperatorKind.GreaterThanOrEqual)
            {
                return CreateGreaterThanRangeFilter(fieldName, valueType, value, binaryOperator.OperatorKind);
            }
            else if (binaryOperator.OperatorKind == BinaryOperatorKind.LessThan ||
                     binaryOperator.OperatorKind == BinaryOperatorKind.LessThanOrEqual)
            {
                return CreateLessThanRangeFilter(fieldName, valueType, value, binaryOperator.OperatorKind);
            }

            return null;
        }

        /// <summary>
        ///     Creates a Greater-than Range filter for a given field, in a given type.
        /// </summary>
        /// <param name="fieldName">Parsed Episerver Find field name for use in creating the filter</param>
        /// <param name="typeReference">Type of the value object for the filter</param>
        /// <param name="value">Value of the range filter's lower bound</param>
        /// <param name="operatorKind">Operator to use to create the filter (GreaterThan or GreaterThanOrEqual)</param>
        /// <returns><see cref="RangeFilter{T}"/> instance for usage with Find</returns>
        private Filter CreateGreaterThanRangeFilter(string fieldName, IEdmTypeReference typeReference, object value, BinaryOperatorKind operatorKind)
        {
            if (typeReference.IsDateTimeOffset())
            {
                var dateTimeOffset = (DateTimeOffset)value;
                var rangeFilter = RangeFilter.Create(fieldName, dateTimeOffset.UtcDateTime, DateTime.MaxValue);
                rangeFilter.IncludeLower = operatorKind == BinaryOperatorKind.GreaterThanOrEqual;
                rangeFilter.IncludeUpper = true;

                return rangeFilter;
            }
            else if (typeReference.IsInt32())
            {
                var rangeFilter = RangeFilter.Create(fieldName, (int)value, int.MaxValue);
                rangeFilter.IncludeLower = operatorKind == BinaryOperatorKind.GreaterThanOrEqual;
                rangeFilter.IncludeUpper = true;

                return rangeFilter;
            }
            else if (typeReference.IsDouble())
            {
                var rangeFilter = RangeFilter.Create(fieldName, (double)value, double.MaxValue);
                rangeFilter.IncludeLower = operatorKind == BinaryOperatorKind.GreaterThanOrEqual;
                rangeFilter.IncludeUpper = true;

                return rangeFilter;
            }
            else if (typeReference.IsDecimal())
            {
                var rangeFilter = RangeFilter.Create(fieldName, (decimal)value, decimal.MaxValue);
                rangeFilter.IncludeLower = operatorKind == BinaryOperatorKind.GreaterThanOrEqual;
                rangeFilter.IncludeUpper = true;

                return rangeFilter;
            }
            else if (typeReference.IsInt64())
            {
                var rangeFilter = RangeFilter.Create(fieldName, (long)value, long.MaxValue);
                rangeFilter.IncludeLower = operatorKind == BinaryOperatorKind.GreaterThanOrEqual;
                rangeFilter.IncludeUpper = true;

                return rangeFilter;
            }
            else if (typeReference.IsSingle())
            {
                var rangeFilter = RangeFilter.Create(fieldName, (float)value, float.MaxValue);
                rangeFilter.IncludeLower = operatorKind == BinaryOperatorKind.GreaterThanOrEqual;
                rangeFilter.IncludeUpper = true;

                return rangeFilter;
            }

            _log.Error($"Unable to create range filter due to unsupported type: {typeReference.Definition}");
            throw new FilterParseException($"Unable to create range filter due to unsupported type: {typeReference.Definition}");
        }

        /// <summary>
        ///     Creates a Less-than Range filter for a given field, in a given type.
        /// </summary>
        /// <param name="fieldName">Parsed Episerver Find field name for use in creating the filter</param>
        /// <param name="typeReference">Type of the value object for the filter</param>
        /// <param name="value">Value of the range filter's lower bound</param>
        /// <param name="operatorKind">Operator to use to create the filter (LessThan or LessThanOrEqual)</param>
        /// <returns><see cref="RangeFilter{T}"/> instance for usage with Find</returns>
        /// <exception cref="FilterParseException">Thrown when an</exception>
        private Filter CreateLessThanRangeFilter(string fieldName, IEdmTypeReference typeReference, object value, BinaryOperatorKind operatorKind)
        {
            if (typeReference.IsDateTimeOffset())
            {
                var dateTimeOffset = (DateTimeOffset)value;
                var rangeFilter = RangeFilter.Create(fieldName, DateTime.MinValue, dateTimeOffset.UtcDateTime);
                rangeFilter.IncludeLower = true;
                rangeFilter.IncludeUpper = operatorKind == BinaryOperatorKind.LessThanOrEqual;

                return rangeFilter;
            }
            else if (typeReference.IsInt32())
            {
                var rangeFilter = RangeFilter.Create(fieldName, int.MinValue, (int)value);
                rangeFilter.IncludeLower = true;
                rangeFilter.IncludeUpper = operatorKind == BinaryOperatorKind.LessThanOrEqual;

                return rangeFilter;
            }
            else if (typeReference.IsDouble())
            {
                var rangeFilter = RangeFilter.Create(fieldName, double.MinValue, (double)value);
                rangeFilter.IncludeLower = true;
                rangeFilter.IncludeUpper = operatorKind == BinaryOperatorKind.LessThanOrEqual;

                return rangeFilter;
            }
            else if (typeReference.IsDecimal())
            {
                var rangeFilter = RangeFilter.Create(fieldName, decimal.MinValue, (decimal)value);
                rangeFilter.IncludeLower = true;
                rangeFilter.IncludeUpper = operatorKind == BinaryOperatorKind.LessThanOrEqual;

                return rangeFilter;
            }
            else if (typeReference.IsInt64())
            {
                var rangeFilter = RangeFilter.Create(fieldName, long.MinValue, (long)value);
                rangeFilter.IncludeLower = true;
                rangeFilter.IncludeUpper = operatorKind == BinaryOperatorKind.LessThanOrEqual;

                return rangeFilter;
            }
            else if (typeReference.IsSingle())
            {
                var rangeFilter = RangeFilter.Create(fieldName, float.MinValue, (float)value);
                rangeFilter.IncludeLower = true;
                rangeFilter.IncludeUpper = operatorKind == BinaryOperatorKind.LessThanOrEqual;

                return rangeFilter;
            }

            _log.Error($"Unable to create range filter due to unsupported type: {typeReference.Definition}");
            throw new FilterParseException($"Unable to create range filter due to unsupported type: {typeReference.Definition}");
        }

        /// <summary>
        ///     Extracts an <see cref="IEdmTypeReference"/> and <see cref="object"/> value from a provided <see cref="SingleValueNode"/>.
        /// </summary>
        /// <remarks>Type and value objects are hidden behind ConvertNode instances, and also have a variety of types depending on how Odata filter and sort strings are formatted</remarks>
        /// <param name="valueNode">The SingleValueNode to parse</param>
        /// <returns>Parsed <see cref="IEdmTypeReference"/> and <see cref="object"/> value, which both may be null</returns>
        private Tuple<IEdmTypeReference, object> ParseSingleValueNode(SingleValueNode valueNode)
        {
            //Node with the value is sometimes hidden behind a ConvertNode
            SingleValueNode nodeToProcess = null;
            if (valueNode is ConvertNode)
            {
                var convertNode = valueNode as ConvertNode;
                nodeToProcess = convertNode.Source;

            }
            else
            {
                nodeToProcess = valueNode;
            }

            //Parse Type and Value out of different possible node types (no common interface exists)
            IEdmTypeReference typeReference = null;
            object value = null;

            var constantNode = nodeToProcess as ConstantNode;
            if (constantNode != null)
            {
                typeReference = constantNode.TypeReference;
                value = constantNode.Value;
            }
            else
            {
                var singleValueOpenNode = nodeToProcess as SingleValueOpenPropertyAccessNode;
                if (singleValueOpenNode != null)
                {
                    typeReference = singleValueOpenNode.TypeReference;
                    value = singleValueOpenNode.Name;
                }
            }

            return new Tuple<IEdmTypeReference, object>(typeReference, value);
        }

        /// <summary>
        ///     Returns a function name from a <see cref="SingleValueNode"/> if it exists
        /// </summary>
        /// <param name="singleValueNode">The SingleValueNode to parse</param>
        /// <returns>Function name as string. Example: lowercase</returns>
        private string ParseFunctionName(SingleValueNode singleValueNode)
        {
            string function = string.Empty;

            SingleValueFunctionCallNode functionNode;
            var convertNode = singleValueNode as ConvertNode;
            if (convertNode != null)
            {
                functionNode = convertNode.Source as SingleValueFunctionCallNode;
            }
            else
            {
                functionNode = singleValueNode as SingleValueFunctionCallNode;
            }


            if (functionNode != null)
            {
                function = functionNode.Name;
            }
            return function;
        }

        /// <summary>
        ///     Returns the dot-notation property name from a <see cref="SingleValueNode"/> if it exists
        /// </summary>
        /// <param name="queryNode">The QueryNode to parse</param>
        /// <returns>Dot-notation syntax string for the property - examle: ContentLink.Id</returns>
        private string ParsePropertyName(QueryNode queryNode)
        {
            var visitor = new PropertyNameExpressionBuilder<string>();
            queryNode.Accept(visitor);

            var propertyName = string.Join(".", visitor.PropertyList);
            return propertyName;
        }

        /// <summary>
        ///     Given a <see cref="SingleValueNode"/>, locates the nearest <see cref="BinaryOperatorNode"/> which may be hidden behind an additional node structure
        /// </summary>
        /// <param name="singleValueNode">The BinaryOperatorNode to parse</param>
        /// <returns>Nearest <see cref="BinaryOperatorNode"/> instance</returns>
        private BinaryOperatorNode GetBinaryOperatorNode(SingleValueNode singleValueNode)
        {
            var binaryOperator = singleValueNode as BinaryOperatorNode;
            if (binaryOperator == null)
            {
                var convertNode = singleValueNode as ConvertNode;
                if (convertNode != null)
                {
                    binaryOperator = convertNode.Source as BinaryOperatorNode;
                }
            }
            return binaryOperator;
        }

        private SingleValueFunctionCallNode GetSingleValueFunctionCallNode(SingleValueNode singleValueNode)
        {
            var singleValueFunctionCallNode = singleValueNode as SingleValueFunctionCallNode;

            // By default, 'contains' operator is parsed to SingleValueFunctionCallNode.
            // However, there are some cases it will be parsed to ConvertNode (For example when 'contains' is combined with 'and' operator)
            // In this case, we try to extract SingleValueFunctionCallNode from ConvertNode 
            if (singleValueFunctionCallNode == null)
            {
                var convertNode = singleValueNode as ConvertNode;
                singleValueFunctionCallNode = convertNode?.Source as SingleValueFunctionCallNode;                
            }

            return singleValueFunctionCallNode;
        }

        /// <summary>
        ///     Creates a field name for use with Find <see cref="Filter"/> objects. Find attaches type information to certain properties, and this ensures that is attached properly when generating filters.
        /// </summary>
        /// <param name="property">Property name as string in dot syntax - like 'ContentLink.Id'</param>
        /// <param name="function">Function to apply to the parsed field. Example: lowercase</param>
        /// <returns>Field name suitable for use with <see cref="Filter"/> instances</returns>
        private string CreateFieldNameForSearch(string property, string function = "")
        {
            var typedExpression = FilterHelper.GetPropertyNameAsLambdaExpression(typeof(ContentApiModel), property);

            string parsedField = typedExpression != null ? _searchClient.Conventions.FieldNameConvention.GetFieldName(typedExpression) : property;

            if (function == ODataToLowerFunction)
            {
                return $"ContentApiModel.{parsedField}.lowercase";
            }

            return $"ContentApiModel.{parsedField}";
        }
    }
}
