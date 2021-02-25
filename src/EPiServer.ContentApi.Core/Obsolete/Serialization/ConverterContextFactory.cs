using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPiServer.ContentApi.Core.Obsolete.Serialization
{
    internal static class ConverterContextFactory
    {
        public static ConverterContext ForObsolete(bool excludePersonalizedContent) => 
            new ConverterContext(ServiceLocator.Current.GetInstance<ContentApiConfiguration>().Default(), string.Empty, string.Empty, excludePersonalizedContent, CultureInfo.InvariantCulture);
    }
}
