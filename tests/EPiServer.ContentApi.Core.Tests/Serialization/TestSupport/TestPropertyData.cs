using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer.Core;

namespace EPiServer.ContentApi.Core.Tests.Serialization.TestSupport
{
    internal class TestPropertyData : PropertyData
    {
        public override object Value { get; set; }

        public override PropertyDataType Type { get; }

        public override Type PropertyValueType { get;  }

        public override void ParseToSelf(string value)
        {
            
        }

        protected override void SetDefaultValue()
        {
            
        }
    }
}
