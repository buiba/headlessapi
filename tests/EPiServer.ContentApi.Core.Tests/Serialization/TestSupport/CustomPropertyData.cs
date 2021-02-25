using EPiServer.Core;
using System;

namespace EPiServer.ContentApi.Core.Tests.Serialization.TestSupport
{
	internal class CustomPropertyData : PropertyData
	{
		public override object Value { get; set; }

		public override PropertyDataType Type { get; }

		public override Type PropertyValueType { get; }

		public override void ParseToSelf(string value)
		{

		}

		protected override void SetDefaultValue()
		{

		}
	}
}
