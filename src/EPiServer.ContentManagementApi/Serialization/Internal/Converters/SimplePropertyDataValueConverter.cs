using System;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ContentApi.Core.Serialization.Internal;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.Core;
using EPiServer.ServiceLocation;

namespace EPiServer.ContentManagementApi.Serialization.Internal.Converters
{
    /// <summary>
    /// The converter reponsible for converting simple value <see cref="IPropertyModel"/> to <see cref="PropertyData"/>
    /// </summary>
    [ServiceConfiguration(typeof(IPropertyDataValueConverter))]
    [PropertyDataValueConverter(new Type[] {
        typeof(StringPropertyModel), typeof(LongStringPropertyModel), typeof(AppSettingsPropertyModel), typeof(BlobPropertyModel),
        typeof(DocumentUrlPropertyModel), typeof(DropDownListPropertyModel), typeof(ImageUrlPropertyModel), typeof(LanguagePropertyModel),
        typeof(UrlPropertyModel), typeof(VirtualLinkPropertyModel), typeof(NumberPropertyModel), typeof(FileSortOrderPropertyModel),
        typeof(FloatPropertyModel), typeof(FramePropertyModel), typeof(SortOrderPropertyModel), typeof(WeekdayPropertyModel),
        typeof(BooleanPropertyModel), typeof(CheckboxListPropertyModel), typeof(DateListPropertyModel), typeof(DateTimePropertyModel),
        typeof(DoubleListPropertyModel), typeof(IntegerListPropertyModel), typeof(SelectorPropertyModel),
        typeof(StringListPropertyModel), typeof(AppSettingsMultiplePropertyModel) })]
    internal class SimplePropertyDataValueConverter : IPropertyDataValueConverter
    {
        ///<inheritdoc />
        public object Convert(IPropertyModel propertyModel, PropertyData propertyData)
        {
            if (propertyModel is null)
            {
                throw new ArgumentNullException(nameof(propertyModel));
            }

            switch (propertyModel)
            {
                case CheckboxListPropertyModel checkBoxListPropertyModel:
                    return checkBoxListPropertyModel.Value is object ? string.Join(",", checkBoxListPropertyModel.Value) : null;
                case SelectorPropertyModel selectorPropertyModel:
                    return selectorPropertyModel.Value is object ? string.Join(",", selectorPropertyModel.Value) : null;
                case AppSettingsMultiplePropertyModel appSettingsMultiplePropertyModel:
                    return appSettingsMultiplePropertyModel.Value is object ? string.Join(",", appSettingsMultiplePropertyModel.Value) : null;
                case BlobPropertyModel blobPropertyModel:
                    return !string.IsNullOrEmpty(blobPropertyModel.Value) ? blobPropertyModel.Value : null;
                case DocumentUrlPropertyModel documentUrlPropertyModel:
                    return !string.IsNullOrEmpty(documentUrlPropertyModel.Value) ? documentUrlPropertyModel.Value : null;
                case ImageUrlPropertyModel imageUrlPropertyModel:
                    return !string.IsNullOrEmpty(imageUrlPropertyModel.Value) ? imageUrlPropertyModel.Value : null;
                case UrlPropertyModel urlPropertyModel:
                    return !string.IsNullOrEmpty(urlPropertyModel.Value) ? urlPropertyModel.Value : null;
                case ISimplePropertyModel simplePropertyModel:
                    return simplePropertyModel.Value;
                default:
                    throw new NotImplementedException($"The property model {propertyModel.Name} cannot be handled by this converter");
            };
        }         
    }
}
