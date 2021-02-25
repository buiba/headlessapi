using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.Web;

namespace EPiServer.ContentApi.Core.Serialization
{
    /// <summary>
    /// Interface for mapping <see cref="SiteDefinition"/> to <see cref="SiteDefinitionModel"/>
    /// <para>
    /// Preview API: This API is current in preview state meaning it might change between minor versions
    /// </para>
    /// </summary>
    public interface ISiteDefinitionConverter
    {
        /// <summary>
        /// Maps an instance of <see cref="SiteDefinition"/> to <see cref="SiteDefinitionModel"/>
        /// </summary>
        /// <param name="site">The site definition object that the model is generated from</param>
        ///<param name="context">The context for current conversion</param>
        /// <returns>A new <see cref="SiteDefinitionModel"/> instance.</returns>
        SiteDefinitionModel Convert(SiteDefinition site, ConverterContext context);
    }
}
