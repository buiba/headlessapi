using EPiServer.Framework;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
using System;
using System.IO;
using System.Reflection;

namespace EPiServer.ContentApi.Forms.Internal
{
    /// <summary>
    ///     Contains utility methods for Content Delivery Forms
    /// </summary>
    [ServiceConfiguration(typeof(CommonUtility))]
    public class CommonUtility
    {
        private static readonly ILogger _logger = LogManager.GetLogger(typeof(CommonUtility));

        /// <summary>
        ///      Get the specified manifest script resource from an assembly.
        /// </summary>
        public virtual string LoadResourceFromAssemblyByName(string resourceName, string assemblyName)
        {
            Validator.ThrowIfNullOrEmpty(nameof(resourceName), resourceName);
            Validator.ThrowIfNullOrEmpty(nameof(assemblyName), assemblyName);

            try
            {
                var assembly = Assembly.Load(assemblyName);
                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Could not load script from assembly {assemblyName}. Script name: {resourceName}", ex);
            }

            return string.Empty;
        }
    }
}
