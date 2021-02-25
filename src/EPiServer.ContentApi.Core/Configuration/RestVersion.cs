using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPiServer.ContentApi.Core.Configuration
{
    /// <summary>
    /// Defines the different versions of the Rest API
    /// <para>
    /// Preview API: This API is current in preview state meaning it might change between minor versions
    /// </para>
    /// </summary>
    [Obsolete("Will be removed in the next major release")]
    public enum RestVersion
    {
        /// <summary>
        /// Represents the 2.0 version of the Rest API
        /// </summary>
        Version_2_0
    }
}
