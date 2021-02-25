using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ContentApi.Core.Serialization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPiServer.ContentApi.Cms.Tests.Serialization
{
    public class TestConverterContext : ConverterContext
    {
        public TestConverterContext(bool excludePersonalization = true, string select = "", string expand = "", ContentApiOptions options = null, CultureInfo language = null)
            :base(options ?? new ContentApiOptions(), select, expand, excludePersonalization, language ?? CultureInfo.InvariantCulture)
        { }
    }
}
