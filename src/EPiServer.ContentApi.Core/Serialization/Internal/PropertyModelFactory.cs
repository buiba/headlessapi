using EPiServer.Core;
using EPiServer.ServiceLocation;
using Microsoft.Extensions.Internal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EPiServer.ContentApi.Core.Serialization.Internal
{
    [ServiceConfiguration(Lifecycle = ServiceInstanceScope.Singleton)]
    internal class PropertyModelFactory
    {
        private ConcurrentDictionary<Type, Func<object[] ,IPropertyModel>> _propertyModelFactoryMethods = new ConcurrentDictionary<Type, Func<object[], IPropertyModel>>();

        public virtual IPropertyModel Create(Type type, PropertyData property, ConverterContext context)
        {
            return _propertyModelFactoryMethods.GetOrAdd(type, t => CreateTypeFactory(t))(new object[] { property, context });
        }

        private delegate object ObjectActivator(params object[] args);

        private Func<object[], IPropertyModel> CreateTypeFactory(Type type)
        {
            var parameterExpression = Expression.Parameter(typeof(object[]), "args");
            var constructorInformation = ResolveConstructor(type);
            var argumentExpression = new Expression[constructorInformation.arguments.Length];
            for (int i = 0; i < constructorInformation.arguments.Length; i++)
            {
                var index = Expression.Constant(i);
                var paramType = constructorInformation.arguments[i];
                var accessor = Expression.ArrayIndex(parameterExpression, index);
                var cast = Expression.Convert(accessor, paramType);
                argumentExpression[i] = cast;
            }

            var newex = Expression.New(constructorInformation.constructor, argumentExpression);
            var lambda = Expression.Lambda(typeof(ObjectActivator), newex, parameterExpression);
            var result = (ObjectActivator)lambda.Compile();

            return args => (IPropertyModel)result(constructorInformation.argumentConverter(args));
        }

        private (ConstructorInfo constructor, Type[] arguments, Func<object[], object[]> argumentConverter) ResolveConstructor(Type type)
        {
            var contextAwareConstructor = GetMatchingConstructor(type, new[] { typeof(PropertyData), typeof(ConverterContext) });
            if (contextAwareConstructor is object)
            {
                return (contextAwareConstructor, new[] { contextAwareConstructor.GetParameters().First().ParameterType, typeof(ConverterContext) }, args => args);
            }
            else if (typeof(IPersonalizableProperty).IsAssignableFrom(type))
            {
                var personalizableConstructor = GetMatchingConstructor(type, new[] { typeof(PropertyData), typeof(bool) });
                if (personalizableConstructor is object)
                {
                    return (personalizableConstructor, new[] { personalizableConstructor.GetParameters().First().ParameterType, typeof(bool) }, args => new object[] { args[0], ((ConverterContext)args[1]).ExcludePersonalizedContent });
                }
            }
            else
            {
                var propertyDataConstructor = GetMatchingConstructor(type, new[] { typeof(PropertyData) });
                if (propertyDataConstructor is object)
                {
                    return (propertyDataConstructor, new[] { propertyDataConstructor.GetParameters().First().ParameterType }, args => new object[] { args[0] });
                }
            }
             
            var defaultConstructor = GetMatchingConstructor(type, new Type[0]);
            if (defaultConstructor is object)
            {
                return (defaultConstructor, new Type[0], args => new object[0]);
            }

            throw new InvalidOperationException($"Could not create instance of {type}. {nameof(IPropertyModel)} implementations should have constructor that takes {nameof(PropertyData)} instance and {nameof(ConverterContext)}");
        }

        private ConstructorInfo GetMatchingConstructor(Type type, Type[] arguments) => type.GetConstructors().FirstOrDefault(c =>
            {
                var parameters = c.GetParameters();
                if (parameters.Length == arguments.Length)
                {
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        if (!arguments[i].IsAssignableFrom(parameters[i].ParameterType))
                        {
                            return false;
                        }
                    }
                    return true;
                }
                return false;
            });
    }
}
