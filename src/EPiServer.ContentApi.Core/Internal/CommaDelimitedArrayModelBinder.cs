using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;

namespace EPiServer.ContentApi.Core.Internal
{
    /// <summary>
    /// Model binder to convert comma-seperated parameter to Array.
    /// </summary>
    public class CommaDelimitedArrayModelBinder : IModelBinder
    {
        public bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
        {
            var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (valueProviderResult == null)
            {
                bindingContext.ModelState.AddModelError(bindingContext.ModelName, "Value Provider Error.");
                return false;
            }

            bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);

            if (string.IsNullOrWhiteSpace(valueProviderResult.AttemptedValue))
            {
                bindingContext.Model = Array.CreateInstance(bindingContext.ModelType.GetElementType(), 0);
                return true;
            }

            var collectionType = bindingContext.ModelType;
            if (collectionType.IsArray && (collectionType.GetElementType() == typeof(string) || collectionType.GetElementType().GetTypeInfo().IsValueType))
            {
                var converter = TypeDescriptor.GetConverter(collectionType.GetElementType());

                var values = valueProviderResult.AttemptedValue.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                if (values.Any(x => !converter.IsValid(x)))
                {
                    bindingContext.ModelState.AddModelError(bindingContext.ModelName, $"Invalid values contained in instance of '{collectionType.FullName}'");
                    return false;
                }

                var convertedValues = values.Select(x => converter.ConvertFromString(x.Trim())).ToArray();

                var typedValues = Array.CreateInstance(collectionType.GetElementType(), convertedValues.Length);
                convertedValues.CopyTo(typedValues, 0);

                bindingContext.Model = typedValues;
                return true;
            }

            bindingContext.ModelState.AddModelError(bindingContext.ModelName, $"Model binding using type '{collectionType.FullName}' is not supported.");
            return false;
        }
    }
}
