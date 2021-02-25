using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;

namespace EPiServer.ContentApi.IntegrationTests.ContentModels.Media
{
    [ContentType(DisplayName = "DefaultMedia", GUID = "aa531491-cd9f-4c4f-90e8-5af59f43c324", Description = "")]
    public class DefaultMedia : MediaData
    {

        [CultureSpecific]
        public virtual string Description { get; set; }

    }
}