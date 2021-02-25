using EPiServer.ContentApi.Core.Internal;
using System;
using Xunit;

namespace EPiServer.ContentApi.Core.Tests.Internal
{
    public class GuidEncoderTest
    {
        [Fact]
        public void EncodeAsGuid_WithSameNamespaceAndSameNumber_ShouldReturnSameGuid()
        {
            var ns = Guid.NewGuid();
            var number = 666;

            var encoded1 = new GuidEncoder().EncodeAsGuid(number, ns);
            var encoded2 = new GuidEncoder().EncodeAsGuid(number, ns);

            Assert.Equal(encoded1, encoded2);
        }

        [Fact]
        public void EncodeAsGuid_WithDifferentNamespaceAndSameNumber_ShouldReturnDifferentGuids()
        {
            var ns1 = Guid.NewGuid();
            var ns2 = Guid.NewGuid();
            var number = 666;

            var encoded1 = new GuidEncoder().EncodeAsGuid(number, ns1);
            var encoded2 = new GuidEncoder().EncodeAsGuid(number, ns2);

            Assert.NotEqual(encoded1, encoded2);
        }

        [Fact]
        public void DecodeFromGuid_WithMismatchingNamespace_ShouldReturnFalse()
        {
            var originalNs = Guid.NewGuid();
            var otherNs = Guid.NewGuid();
            var number = 666;

            var encoded = new GuidEncoder().EncodeAsGuid(number, originalNs);

            var result = new GuidEncoder().TryDecodeFromGuid(encoded, otherNs, out int decoded);

            Assert.False(result);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(13)]
        [InlineData(123456789)]
        [InlineData(452867139)]
        [InlineData(1779480683)]
        [InlineData(-1)]
        [InlineData(-987654321)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void DecodeFromGuid_WhenDecodingEncodedValue_ShouldSuccessfullyDecodeOriginalValue(int number)
        {
            var ns = Guid.NewGuid();

            var encoded = new GuidEncoder().EncodeAsGuid(number, ns);

            var result = new GuidEncoder().TryDecodeFromGuid(encoded, ns, out int decoded);

            Assert.True(result);
            Assert.Equal(number, decoded);
        }

        [Theory]
        // This test uses stored generated values to verify the algorithm
        // doesn't change over time
        [InlineData(0, "9e128240-24c1-5c8a-e861-960000008096")]
        [InlineData(1, "6eab1e80-cb3c-50b8-c7e6-974334fe1ae8")]
        [InlineData(2, "6b621c28-3090-5faa-59d7-918668fc3545")]
        [InlineData(13, "16b289b5-6cdd-5387-d6a5-b167a7e85eeb")]
        [InlineData(123456789, "a6414555-6c05-5791-baaf-cd7ff0842fb3")]
        [InlineData(452867139, "ea28aa41-f623-51b1-d544-7b89499f5b95")]
        [InlineData(1779480683, "93015a84-bd41-5cac-9dd0-68010000009c")]
        [InlineData(-1, "5a5ddd2d-288c-5191-1542-29bdcb01e56c")]
        [InlineData(-987654321, "69a6ab8e-4fc2-5b96-5b92-8bada5e890d0")]
        [InlineData(int.MaxValue, "9d345cf7-f030-51a2-1183-69bdcb01651e")]
        [InlineData(int.MinValue, "f4bc36b9-d161-50bf-ba2d-120000000013")]
        public void EncodeAsGuid_WhenEncodingKnownValues_ShouldMatchPreviousEncoding(int number, string guid)
        {
            var ns = new Guid("69C3BF2C-2350-4E62-BFF4-B86521DA2FA3");

            var stored = Guid.Parse(guid);
            var encoded = new GuidEncoder().EncodeAsGuid(number, ns);

            Assert.Equal(stored, encoded);
        }
    }
}
