using System;
using System.Collections.Generic;
using System.Globalization;
using Mediachase.Commerce;

namespace EPiServer.ContentApi.Commerce.Internal.Models.Markets
{
    /// <summary>
    /// Represents market information.
    /// </summary>
    public class MarketApiModel
    {
        /// <summary>
        /// The id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The default language.
        /// </summary>
        public CultureInfo DefaultLanguage { get; set; }

        /// <summary>
        /// The default currency.
        /// </summary>
        public string DefaultCurrency { get; set; }

        /// <summary>
        /// The languages.
        /// </summary>
        public IEnumerable<CultureInfo> Languages { get; set; }

        /// <summary>
        /// The currencies.
        /// </summary>
        public IEnumerable<string> Currencies { get; set; }

        /// <summary>
        /// The countries.
        /// </summary>
        public IEnumerable<string> Countries { get; set; }
    }
}
