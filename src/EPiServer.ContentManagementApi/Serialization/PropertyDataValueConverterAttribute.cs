using System;

namespace EPiServer.ContentManagementApi.Serialization
{
    /// <summary>
    /// Define a list of property model that can be converted by <see cref="IPropertyDataValueConverter" />
    /// <para>
    /// Preview API: This API is current in preview state meaning it might change between minor versions
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class PropertyDataValueConverterAttribute : Attribute
    {
        /// <summary>
        /// Register property models using the given parameter as an array of property model.
        /// </summary>
        public PropertyDataValueConverterAttribute(params Type[] propertyModelTypes)
        {
            PropertyModelTypes = propertyModelTypes;
        }

        /// <summary>
        /// Gets or sets the type of property model the attributed class represents.
        /// </summary>
        public Type[] PropertyModelTypes { get; set; }
    }
}
