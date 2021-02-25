using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace EPiServer.DefinitionsApi.Manifest.Internal
{
    /// <summary>
    /// Inspired by https://github.com/reustmd/DataAnnotationsValidatorRecursive.
    /// </summary>
    internal static class DataAnnotationsValidator
    {
        public static bool TryValidateObject(object obj, ICollection<ValidationResult> results, IDictionary<object, object> validationContextItems = null)
        {
            return Validator.TryValidateObject(obj, new ValidationContext(obj, null, validationContextItems), results, true);
        }

        public static bool TryValidateObjectRecursive<T>(T obj, ICollection<ValidationResult> results, IDictionary<object, object> validationContextItems = null)
        {
            return TryValidateObjectRecursive(obj, results, new HashSet<object>(), validationContextItems);
        }

        private static bool TryValidateObjectRecursive<T>(T obj, ICollection<ValidationResult> results, ISet<object> validatedObjects, IDictionary<object, object> validationContextItems = null)
        {
            // Short-circuit to avoid infinit loops on cyclical object graphs
            if (validatedObjects.Contains(obj))
            {
                return true;
            }

            validatedObjects.Add(obj);

            var result = TryValidateObject(obj, results, validationContextItems);

            var properties = obj
                .GetType()
                .GetProperties()
                .Where(prop => prop.CanRead && prop.GetIndexParameters().Length == 0)
                .ToList();

            foreach (var property in properties)
            {
                if (property.PropertyType == typeof(string) || property.PropertyType.IsValueType)
                {
                    continue;
                }

                var value = obj.GetPropertyValue(property.Name);

                if (value == null)
                {
                    continue;
                }

                if (value is IEnumerable asEnumerable)
                {
                    foreach (var enumObj in asEnumerable)
                    {
                        if (enumObj != null)
                        {
                            var nestedResults = new Collection<ValidationResult>();
                            if (!TryValidateObjectRecursive(enumObj, nestedResults, validatedObjects, validationContextItems))
                            {
                                result = false;
                                foreach (var validationResult in nestedResults)
                                {
                                    results.Add(new ValidationResult(
                                        validationResult.ErrorMessage,
                                        validationResult.MemberNames.Select(x => property.Name + '.' + x)));
                                }
                            };
                        }
                    }
                }
                else
                {
                    var nestedResults = new Collection<ValidationResult>();
                    if (!TryValidateObjectRecursive(value, nestedResults, validatedObjects, validationContextItems))
                    {
                        result = false;
                        foreach (var validationResult in nestedResults)
                        {
                            results.Add(new ValidationResult(
                                validationResult.ErrorMessage,
                                validationResult.MemberNames.Select(x => property.Name + '.' + x)));
                        }
                    };
                }
            }

            return result;
        }

        private static object GetPropertyValue(this object obj, string propertyName)
        {
            object objValue = string.Empty;

            var propertyInfo = obj
                .GetType()
                .GetProperty(propertyName);

            if (propertyInfo != null)
            {
                objValue = propertyInfo.GetValue(obj, null);
            }

            return objValue;
        }
    }
}
