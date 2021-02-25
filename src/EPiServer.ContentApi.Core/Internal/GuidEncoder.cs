using System;
using System.IO;
using System.Security.Cryptography;

namespace EPiServer.ContentApi.Core.Internal
{
    /// <summary>
    /// Encodes an integer, like a database ID, into a random-looking Guid of RFC4122 version 5,
    /// which means it should never collide with real random Guids.
    /// </summary>
    public class GuidEncoder
    {
        /// <summary>
        /// Encodes the initialized namespace and the input <paramref name="number"/> into a
        /// random-looking Guid of RFC4122 version 5.
        /// </summary>
        /// <param name="number">The number to encode.</param>
        /// <param name="ns">
        /// A namespace that represents the type of data encoded. This should be the same every
        /// time the same type of data is encoded, refer to RFC4122.
        /// E.g. for Cart ID:s this could be a Guid constant representing the Cart type.
        /// </param>
        /// <remarks>
        /// Example: Using to encode cart ID:s
        /// 
        /// // Separates carts from other entities like orders. Persistent, must never change.
        /// const Guid cartNamespace = new Guid("3B4EF64B-C042-4859-B7C4-47B7D2B79BDB");
        ///
        /// int cartId; // input var
        /// 
        /// var encodedGuid = _guidEncoder.EncodeAsGuid(cartId, cartNamespace);
        /// 
        /// </remarks>
        /// <returns>The Guid with the encoded number.</returns>
        public Guid EncodeAsGuid(int number, Guid ns)
        {
            // Encode Guid of version 5 according to section 4.1.3 of https://www.ietf.org/rfc/rfc4122.txt
            // Version 5 should use SHA-1 but this is opaque to anyone outside so we partly override
            // it to be able to extract the original number
            // Otherwise follow algorithm of section 4.3 of the same document

            // Step 1: Standard algorithm from RFC, get a SHA-1 hash
            var encodedBytes = CreateHash(number, ns);

            // Set the version in the four most significant bits of the time_hi_and_version field
            // (= the 4 most significant bytes of the 7th byte)
            const int versionFieldByteIndex = 6;
            // Clear 4 most significant bits containing version
            encodedBytes[versionFieldByteIndex] = (byte)(encodedBytes[versionFieldByteIndex] & 0x0F);
            // Write version 5 into 4 most significant bits
            encodedBytes[versionFieldByteIndex] = (byte)(encodedBytes[versionFieldByteIndex] | 0x5 << 4);

            // Set the variant in the two most significant bits of the clk_seq_hi_res field
            // (= the 2 most significant bits of the 9th byte)
            const int variantFieldByteIndex = 7;
            // Clear 2 most significant bits containing version
            encodedBytes[variantFieldByteIndex] = (byte)(encodedBytes[variantFieldByteIndex] & 0x3F);
            // Write 1 as the most significant bit to acheive 1 0 as the two most significant bits
            encodedBytes[variantFieldByteIndex] |= 1 << 7;

            // Step 2 (secret sauce): Overwrite the last four bytes with the encoded number so it can
            // be decoded, but encode it using the modular multiplicative inverse with makes it look
            // more "random" for sequential numbers and less empty for small numbers which are
            // both properties of auto-increment ID:s
            var inverse = MultiplicativeModularInverse(number, false);
            var inverseBytes = BitConverter.GetBytes(inverse);
            inverseBytes.CopyTo(encodedBytes, encodedBytes.Length - inverseBytes.Length - 1);

            return ToGuid(encodedBytes);
        }

        /// <summary>
        /// Decodes the original number from a Guid that was created using <see cref="EncodeAsGuid(int, Guid)" />.
        /// </summary>
        /// <param name="guid">The encoded Guid.</param>
        /// <param name="ns">
        /// A namespace that represents the type of data encoded. 
        /// This must be the same as used when <see cref="EncodeAsGuid(int, Guid)" /> was called to
        /// create the input <paramref name="guid"/> or <see cref="ArgumentException"/> will be thrown.
        /// </param>
        /// <param name="number">
        /// Output parameter that will be set to the decoded number if the operation was successful.
        /// </param>
        /// <returns><c>true</c> if the number was successfully deoced; <c>false</c> if the input
        /// <paramref name="ns"/> did not match the input <paramref name="number"/>.</returns>
        public bool TryDecodeFromGuid(Guid guid, Guid ns, out int number)
        {
            var encodedBytes = ToNetworkByteArray(guid);

            var inverse = BitConverter.ToInt32(encodedBytes, encodedBytes.Length - sizeof(int) - 1);

            number = MultiplicativeModularInverse(inverse, true);

            var validation = EncodeAsGuid(number, ns);

            if (guid != validation)
            {
                number = 0;
                return false;
            }

            return true;
        }

        private static byte[] ToNetworkByteArray(Guid guid)
        {
            var dotnetByteArray = guid.ToByteArray();
            return ConvertBetweenDotnetAndNetworkByteOrder(dotnetByteArray);
        }

        private static byte[] CreateHash(int number, Guid ns)
        {
            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(stream))
                {
                    writer.Write(ToNetworkByteArray(ns));
                    writer.Write(number);

                    using (var sha = new SHA1CryptoServiceProvider())
                    {
                        var hash = sha.ComputeHash(stream.ToArray());

                        byte[] encodedBytes = new byte[16];
                        Array.Copy(hash, 0, encodedBytes, 0, encodedBytes.Length);

                        return encodedBytes;
                    }
                }
            }
        }

        private static Guid ToGuid(byte[] networkByteArray)
        {
            var dotnetByteArray = ConvertBetweenDotnetAndNetworkByteOrder(networkByteArray);
            return new Guid(dotnetByteArray);
        }

        private static byte[] ConvertBetweenDotnetAndNetworkByteOrder(byte[] source)
        {
            if (source.Length != 16)
            {
                throw new ArgumentException("Array must have 16 elements.", nameof(source));
            }

            // The Guid struct in dotnet is stored as int32, int16, int16, byte[8]
            // When returned as a byte array, this causes the larger types (32 and 16 bit) to be represented
            // in reverse network byte order, i.e. Guid.ToByteArray returns
            // the Guid 00112233-4455-6677-8899AABBCCDDEEFF
            // as the byte array 33,22,11,00,55,44,77,66,88,99,AA,BB,CC,DD,EE,FF
            // This reversing can be seen in the source here: https://github.com/microsoft/referencesource/blob/master/mscorlib/system/guid.cs#L58

            return new byte[16]
            {
                source[0x3],
                source[0x2],
                source[0x1],
                source[0x0],
                source[0x5],
                source[0x4],
                source[0x7],
                source[0x6],
                source[0x8],
                source[0x9],
                source[0xA],
                source[0XB],
                source[0xC],
                source[0xD],
                source[0xE],
                source[0xF]
            };
        }

        private int MultiplicativeModularInverse(int input, bool inputIsInverse)
        {
            // Modulus is selected as Int32.MaxValue to uniquely map all values up to Int32.MaxValue
            // but not generate maps outside the Int32 range (so the mapped number is still Int32)
            const long modulus = 2147483648; //Int32.MaxValue + 1
            // Coprime is just a random number that is coprime to modulus
            const long modulusCoprime = 452867139;
            // Multiplicative inverse factor is calculated from the two above, so that
            // modulusCoprime * modularMultiplicativeInverse % modulus == 1
            // Example tool: https://www.dcode.fr/modular-inverse
            const long modularMultiplicativeInverse = 1779480683;

            // Special cases where the modulo is even
            if (input == 0)
            {
                return (int)-modulus;
            }
            if (input == -modulus)
            {
                return 0;
            }

            var factor = inputIsInverse ? modularMultiplicativeInverse : modulusCoprime;

            return (int)((input * factor) % modulus);
        }
    }
}
