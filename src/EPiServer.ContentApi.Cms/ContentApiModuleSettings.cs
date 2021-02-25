using System.Configuration;

namespace EPiServer.ContentApi.Cms
{
    /// <summary>
    /// Contain app settings
    /// </summary>
    public class ContentApiModuleSettings
    {
        /// <summary>
        /// Indicate whether HttpConfiguration.MapHttpAttributeRoutes() should be called in our api
        /// </summary>
        public static bool ShouldUseDefaultHttpConfiguration = IsDefaultHttpConfigurationEnabled();

        private static bool IsDefaultHttpConfigurationEnabled()
        {
            bool result;

            if (bool.TryParse(ConfigurationManager.AppSettings["episerver:contentdelivery:maphttpattributeroutes"], out result))
            {
                return result;
            }

            return true;
        }
    }
}
