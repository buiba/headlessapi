using System;
using System.Collections.Generic;
using System.Web.Http.ValueProviders.Providers;
using System.Web.Http.Metadata.Providers;
using System.Web.Http.ModelBinding;
using EPiServer.ContentApi.Core.Internal;
using Xunit;

namespace EPiServer.ContentApi.Commerce.Tests.Controller
{
    public class CommaDelimitedArrayModelBinderTest
    {
        [Fact]
        public void BindModel_GuidArray_ShouldBindValuesToModel()
        {
            var rawInput = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("model", "BA999EC2-B6EF-4EB4-B2FB-158369838684,AD3CD5A5-D0EC-4D9A-BE23-448FBCD4690F")
            };

            var bindingContext = SetupBindingContext<Guid[]>(rawInput);
            var binder = new CommaDelimitedArrayModelBinder();
            var result = binder.BindModel(null, bindingContext);

            Assert.True(result);
            Assert.True(bindingContext.ModelState.IsValid);
            Assert.Equal(new[] { Guid.Parse("BA999EC2-B6EF-4EB4-B2FB-158369838684"), Guid.Parse("AD3CD5A5-D0EC-4D9A-BE23-448FBCD4690F") }, bindingContext.Model);
        }

        [Fact]
        public void BindModel_GuidArray_WhenInputIsEmpty_ShouldBindValuesToModel()
        {
            var rawInput = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("model", "")
            };

            var bindingContext = SetupBindingContext<Guid[]>(rawInput);
            var binder = new CommaDelimitedArrayModelBinder();
            var result = binder.BindModel(null, bindingContext);

            Assert.True(result);
            Assert.Equal(Array.Empty<Guid>(), bindingContext.Model);
            Assert.True(bindingContext.ModelState.IsValid);
        }

        [Fact]
        public void BindModel_GuidArray_WhenInputIsInvalid_ShouldAddModelError()
        {
            var rawInput = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("model", "BA999EC2-B6EF-4EB4-B2FB-158369838684,InvalidGuid")
            };

            var bindingContext = SetupBindingContext<Guid[]>(rawInput);
            var binder = new CommaDelimitedArrayModelBinder();
            var result = binder.BindModel(null, bindingContext);

            Assert.False(result);
            Assert.False(bindingContext.ModelState.IsValid);
        }

        [Fact]
        public void BindModel_StringArray_ShouldBindValuesToModel()
        {
            var rawInput = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("model", "string1,string2")
            };

            var bindingContext = SetupBindingContext<string[]>(rawInput);
            var binder = new CommaDelimitedArrayModelBinder();
            var result = binder.BindModel(null, bindingContext);

            Assert.True(result);
            Assert.True(bindingContext.ModelState.IsValid);
            Assert.Equal(new[] { "string1", "string2" }, bindingContext.Model);
        }

        [Fact]
        public void BindModel_StringArray_WhenInputIsEmpty_ShouldBindValuesToModel()
        {
            var rawInput = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("model", "")
            };

            var bindingContext = SetupBindingContext<string[]>(rawInput);
            var binder = new CommaDelimitedArrayModelBinder();
            var result = binder.BindModel(null, bindingContext);

            Assert.True(result);
            Assert.True(bindingContext.ModelState.IsValid);
            Assert.Equal(Array.Empty<string>(), bindingContext.Model);
        }

        [Fact]
        public void BindModel_String_ShouldAddModelError()
        {
            var rawInput = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("model", "hello")
            };

            var bindingContext = SetupBindingContext<string>(rawInput);
            var binder = new CommaDelimitedArrayModelBinder();
            var result = binder.BindModel(null, bindingContext);

            Assert.False(result);
            Assert.False(bindingContext.ModelState.IsValid);
        }

        ModelBindingContext SetupBindingContext<T>(IEnumerable<KeyValuePair<string, string>> rawInput)
        {
            return new ModelBindingContext
            {
                ModelName = "model",
                ValueProvider = new NameValuePairsValueProvider(rawInput, null),
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(T))
            };
        }
    }
}
