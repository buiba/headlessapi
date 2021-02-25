using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.Core;
using EPiServer.ServiceLocation;

namespace EPiServer.ContentApi.Core.Serialization.Internal
{
    //plan is to obsolete this part of partial class as [Obsolete("Replaced by IPropertyModelConverter and IPropertyModelConverterProvider")]

    /// <summary>
    /// The default implementation of <see cref="IPropertyModelConverter"/>. 
    /// This class is used for handling the mapping between EPiServer property data to property models.
    /// </summary>
    [ServiceConfiguration(typeof(IPropertyModelConverter), Lifecycle = ServiceInstanceScope.Singleton)]
    public partial class DefaultPropertyModelConverter : IPropertyModelConverter
	{
        private IEnumerable<TypeModel> _typeModels;
        private readonly PropertyModelFactory _propertyModelFactory;

        /// <summary>
        /// Initialize a new instance of <see cref="DefaultPropertyModelConverter"/>
        /// </summary>
        public DefaultPropertyModelConverter() : this(ServiceLocator.Current.GetInstance<PropertyModelFactory>())
		{
		}

        /// <summary>
        /// Initialize a new instance of <see cref="DefaultPropertyModelConverter"/>
        /// </summary>
        /// <param name="reflectionService"></param>
        public DefaultPropertyModelConverter(ReflectionService reflectionService)
            :this(ServiceLocator.Current.GetInstance<PropertyModelFactory>())
		{
		}

        internal DefaultPropertyModelConverter(PropertyModelFactory propertyModelFactory)
        {
            _propertyModelFactory = propertyModelFactory;
        }


        /// <inheritdoc />
        public virtual int SortOrder { get; } = 0;

        /// <inheritdoc />
        public IEnumerable<TypeModel> ModelTypes
        {
            get
            {
                if (_typeModels == null)
                {
                    _typeModels = InitializeModelTypes();
                }
                return _typeModels;
            }
            set
            {
                _typeModels = value;
            }
        }

        /// <summary>
		/// Initialize the mapping between Content API's <see cref="IPropertyModel"/> with corresponding EpiServer.Core's <see cref="PropertyData"/>
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerable<TypeModel> InitializeModelTypes() => TypeModelResolver.ResolveModelTypes();


        /// <summary>
        /// Method to determine whether or not the provided PropertyData can be represented by
        ///  one of the implementations of IPropertyModel registered in the PropertyModelConverter
        /// </summary>
        /// <param name="propertyData">Instance of PropertyData to check against the IPropertyModels registered 
        /// with the PropertyModelConverter</param>
        /// <returns>boolean indicating whether or not the PropertyModelConverter can provide IPropertyModel
        /// for provided PropertyData</returns>
        public virtual bool HasPropertyModelAssociatedWith(PropertyData propertyData)
		{
			if (propertyData == null)
			{
				return false;
			}
			return ModelTypes.Any(x => x.PropertyType == propertyData.GetType());
		}

        /// <summary>
        /// Based on the provided PropertyData and the registered PropertyModelConverter, 
        /// an instance of IPropertyModel is generated and returned.
        /// </summary>
        /// <param name="propertyData">Instance of PropertyData which the IPropertyModel result is generated from.</param>
        /// <param name="language">language to get value of property</param>
        /// <param name="excludePersonalizedContent">Boolean to indicate whether or not to serialize personalization data.</param>
        /// <param name="expand">
        /// Booolean to indicate whether or not to expand property. 
        /// Normally, a property only contains the basic information (e.g. contentlink : {name, workid} ). But for some special ones, clients may want to get 
        /// full information of it (e.g. contentlink: {name, workid, publishdate, editdate, etc}).
        /// In this case, we will call ConvertToPropertyModel with expand set to true
        /// </param>
        /// <returns>Instance of IPropertyModel</returns>
        public virtual IPropertyModel ConvertToPropertyModel(PropertyData propertyData, CultureInfo language, bool excludePersonalizedContent, bool expand = false)
		{
            if (!(propertyData is object))
            {
                return null;
            }

            var modelType = ModelTypes.FirstOrDefault(x => x.PropertyType == propertyData.GetType());
            return modelType is object ?
                new DefaultPropertyConverter(modelType, _propertyModelFactory).Convert(propertyData, new ConverterContext(Config.Default(), string.Empty, expand ? propertyData.Name : string.Empty, excludePersonalizedContent, language)):
                null;
		}

        //exposed to be able to test obsolete methods
        private ContentApiConfiguration _config;
        internal ContentApiConfiguration Config
        {
            get { return _config ?? ServiceLocator.Current.GetInstance<ContentApiConfiguration>(); }
            set { _config = value; }
        }
	}

}
