using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPiServer.ContentApi.Core.Serialization.Models
{
    public class HostDefinitionModel
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public LanguageModel Language { get; set; }
    }
}
