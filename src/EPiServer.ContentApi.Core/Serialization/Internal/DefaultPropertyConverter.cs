using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.Core;
using EPiServer.ServiceLocation;

namespace EPiServer.ContentApi.Core.Serialization.Internal
{
    /// <summary>
    /// The default implementation of <see cref="IPropertyConverter"/>. 
    /// This class is used for handling the mapping between EPiServer property data to property models.
    /// </summary>
	internal class DefaultPropertyConverter : IPropertyConverter
	{
        private readonly TypeModel _typeModel;
        private readonly PropertyModelFactory _propertyModelFactory;

        /// <summary>
        /// Initialize a new instance of <see cref="DefaultPropertyConverter"/>
        /// </summary>
        public DefaultPropertyConverter(TypeModel typeModel, PropertyModelFactory propertyModelFactory)
		{
            _typeModel = typeModel;
            _propertyModelFactory = propertyModelFactory;
		}

        /// <inheritdoc />
        public IPropertyModel Convert(PropertyData propertyData, ConverterContext contentMappingContext)
        {

            var model = _propertyModelFactory.Create(_typeModel.ModelType, propertyData, contentMappingContext);

            //If property need to be expand and is an instance of IExpandableProperty, call Expand() of this property
            if (contentMappingContext.ShouldExpand(propertyData.Name))
            {
                var expandableModel = model as IExpandableProperty;
                if (expandableModel != null)
                {
                    expandableModel.Expand(contentMappingContext.Language);
                }
            }

            return model;
        }
    }
}
