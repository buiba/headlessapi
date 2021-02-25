using System.Configuration;

namespace EPiServer.ContentApi.Search
{
    /// <summary>
    /// Contain app settings
    /// </summary>
    public class ContentApiSearchModuleSettings
    {
        /// <summary>
        /// Indicate whether HttpConfiguration.MapHttpAttributeRoutes() should be called in our api
        /// </summary>
        public static bool ShouldUseDefaultHttpConfiguration = IsDefaultHttpConfigurationEnabled();

        private static bool IsDefaultHttpConfigurationEnabled()
        {
            bool result;

            if (bool.TryParse(ConfigurationManager.AppSettings["episerver:contentdeliverysearch:maphttpattributeroutes"], out result))
            {
                return result;
            }

            return true;
        }
    }
}
