using System.Collections.Generic;

namespace EPiServer.ContentApi.Forms.Model
{
    /// <summary>
    /// Model that contains neccessary form data to render and operate forms at viewmode
    /// </summary>
    public class FormContainerBlockModel
    {
        /// <summary>
        /// Html form template
        /// </summary>
        public string Template;

        /// <summary>
        /// Contains all required js and css to render and operate forms at viewmode
        /// </summary>
        public IDictionary<string, string> Assets;
    }
}
