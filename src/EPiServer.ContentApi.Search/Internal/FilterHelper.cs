using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using EPiServer.Find.Api.Querying;
using EPiServer.Logging;
using Microsoft.OData.Edm;

namespace EPiServer.ContentApi.Search.Internal
{
    /// <summary>
    ///     Helper class for working with OData Filter strings
    /// </summary>
    public static class FilterHelper
    {
        private static readonly ILogger Log = LogManager.GetLogger(typeof(FilterHelper));
        public static LambdaExpression GetPropertyNameAsLambdaExpression(Type type, string dotNotation)
        {
            try
            {
                IEnumerable<string> properties = dotNotation.Split('.');

                Type t = type;
                ParameterExpression parameter = Expression.Parameter(t);
                Expression expression = parameter;

                for (int i = 0; i < properties.Count(); i++)
                {
                    expression = Expression.Property(expression, t, properties.ElementAt(i));
                    t = expression.Type;
                }

                var lambdaExpression = Expression.Lambda(expression, parameter);

                return lambdaExpression;
            }
            catch (Exception ex)
            {
                Log.Debug("Fail to build lamda expression", ex);
                // ignored
            }

            return null;
        }

        public static FieldFilterValue GetTypedFieldFilterValue(IEdmTypeReference type, object value)
        {
            if (type.IsString())
            {
                return FieldFilterValue.Create(value.ToString());
            }
            else if (type.IsInt32())
            {
                return FieldFilterValue.Create((int)value);
            }
            else if (type.IsInt64())
            {
                return FieldFilterValue.Create((long)value);
            }
            else if (type.IsBoolean())
            {
                return FieldFilterValue.Create((bool)value);
            }
            else if (type.IsDateTimeOffset())
            {
                return FieldFilterValue.Create(((DateTimeOffset)value).UtcDateTime);
            }
            else if (type.IsDecimal())
            {
                return FieldFilterValue.Create((decimal)value);
            }
            else if (type.IsSingle())
            {
                return FieldFilterValue.Create((float)value);
            }
            else if (type.IsDouble())
            {
                return FieldFilterValue.Create((double)value);
            }


            return FieldFilterValue.Create(value.ToString());

        }
    }
}
