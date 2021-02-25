using EPiServer.ContentApi.Core.Serialization.Models;

namespace EPiServer.ContentApi.Forms.Model
{
    /// <summary>
    /// FormContentApiModel that contains form related data transformed from IContent for serialization
    /// </summary>
    public class FormContentApiModel: ContentApiModel
    {
        /// <summary>
        /// Form model that contains all data to render and operate a form at viewmode
        /// </summary>
        public FormContainerBlockModel FormModel;
    }
}
