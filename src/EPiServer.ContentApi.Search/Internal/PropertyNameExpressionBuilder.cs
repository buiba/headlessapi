using System.Collections.Generic;
using Microsoft.OData.UriParser;

namespace EPiServer.ContentApi.Search.Internal
{
    /// <summary>
    ///     QueryNodeVisitor implementation which traverses the syntax tree, mapping each relevant property as a string for use with Episerver Find
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    public class PropertyNameExpressionBuilder<TSource> : QueryNodeVisitor<TSource>
        where TSource : class
    {
        public List<string> PropertyList = new List<string>();

        public override TSource Visit(SingleValuePropertyAccessNode nodeIn)
        {
            PropertyList.Insert(0, nodeIn.Property.Name);
            nodeIn.Source.Accept(this);
            return null;
        }

        public override TSource Visit(BinaryOperatorNode nodeIn)
        {
            nodeIn.Right.Accept(this);
            nodeIn.Left.Accept(this);
            return null;
        }

        public override TSource Visit(ConvertNode nodeIn)
        {
            nodeIn.Source.Accept(this);
            return null;
        }

        public override TSource Visit(SingleValueOpenPropertyAccessNode nodeIn)
        {
            PropertyList.Insert(0, nodeIn.Name);
            nodeIn.Source.Accept(this);
            return null;
        }

        public override TSource Visit(SingleValueFunctionCallNode nodeIn)
        {
            foreach (var local in nodeIn.Parameters)
            {
                local.Accept(this);
            }
            return null;
        }

        public override TSource Visit(SingleNavigationNode nodeIn)
        {
            PropertyList.Insert(0, nodeIn.NavigationProperty.Name);
            return null;
        }

        public override TSource Visit(CollectionPropertyAccessNode nodeIn)
        {
            PropertyList.Insert(0, nodeIn.Property.Name);
            return null;
        }

        public override TSource Visit(CollectionOpenPropertyAccessNode nodeIn)
        {
            PropertyList.Insert(0, nodeIn.Name);
            return null;
        }

        public override TSource Visit(UnaryOperatorNode nodeIn)
        {
            return null;
        }

        public override TSource Visit(NamedFunctionParameterNode nodeIn)
        {
            return null;
        }

        public override TSource Visit(ParameterAliasNode nodeIn)
        {
            return null;
        }

        public override TSource Visit(SearchTermNode nodeIn)
        {
            return null;
        }

        public override TSource Visit(SingleComplexNode nodeIn)
        {
            return null;
        }

        public override TSource Visit(CollectionComplexNode nodeIn)
        {
            return null;
        }

        public override TSource Visit(AllNode nodeIn)
        {
            return null;
        }

        public override TSource Visit(AnyNode nodeIn)
        {
            return null;
        }

        public override TSource Visit(CountNode nodeIn)
        {
            return null;
        }

        public override TSource Visit(CollectionNavigationNode nodeIn)
        {
            return null;
        }
        
        public override TSource Visit(ConstantNode nodeIn)
        {
            return null;
        }

        public override TSource Visit(CollectionResourceCastNode nodeIn)
        {
            return null;
        }

        public override TSource Visit(ResourceRangeVariableReferenceNode nodeIn)
        {
            return null;
        }

        public override TSource Visit(NonResourceRangeVariableReferenceNode nodeIn)
        {
            return null;
        }

        public override TSource Visit(SingleResourceCastNode nodeIn)
        {
            return null;
        }

        public override TSource Visit(SingleResourceFunctionCallNode nodeIn)
        {
            return null;
        }

        public override TSource Visit(CollectionResourceFunctionCallNode nodeIn)
        {
            return null;
        }

        public override TSource Visit(CollectionFunctionCallNode nodeIn)
        {
            return null;
        }

    }
}