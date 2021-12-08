using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Moonlight.Api
{
    public static class Extensions
    {
        public static string ToJson(this object obj, JsonSerializerOptions? jsonSerializerOptions = null) => JsonSerializer.Serialize(obj, jsonSerializerOptions ?? new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull });

        public static int GetVarLength(this object value)
        {
            int amount;
            switch (value)
            {
                case int numValue:
                    amount = 0;
                    do
                    {
                        numValue >>= 7;
                        amount++;
                    } while (numValue != 0);

                    return amount;
                case long numValue:
                    amount = 0;
                    do
                    {
                        numValue >>= 7;
                        amount++;
                    } while (numValue != 0);

                    return amount;
                case string stringValue:
                    return stringValue.Length.GetVarLength() + stringValue.Length;
                case object[] byteArray:
                    return byteArray.Length.GetVarLength() + byteArray.Length;
                default:
                    throw new ArgumentException($"Invalid type: {value.GetType()}. Expected int, long or string.", nameof(value));
            }
        }

        /// <summary>
        /// Calculates the Minecraft SHA1 Digest from an array of bytes. Slightly modified from https://gist.github.com/ammaraskar/7b4a3f73bee9dc4136539644a0f27e63
        /// </summary>
        /// <param name="input">The byte array to calculate the hash from.</param>
        /// <returns>The Minecraft SHA1 hash.</returns>
        [SuppressMessage("Roslyn", "CA5350", Justification = "Minecraft protocol and Mojang Session Servers require a SHA1 hash.")]
        public static string MinecraftShaDigest(this byte[] input)
        {
            byte[] hash = SHA1.Create().ComputeHash(input);
            // Reverse the bytes since BigInteger uses little endian
            Array.Reverse(hash);

            BigInteger b = new(hash);
            // Very annoyingly, BigInteger in C# tries to be smart and puts in a leading 0 when formatting as a hex number to allow roundtripping of negative numbers, thus we have to trim it off.
            if (b < 0)
            {
                // toss in a negative sign if the interpreted number is negative
                return "-" + (-b).ToString("x", CultureInfo.InvariantCulture).TrimStart('0');
            }
            else
            {
                return b.ToString("x", CultureInfo.InvariantCulture).TrimStart('0');
            }
        }
    }
}