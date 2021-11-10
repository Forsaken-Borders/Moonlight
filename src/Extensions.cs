using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.X509;

namespace Moonlight
{
    public static class Extensions
    {
        public static int GetVarIntLength(this int val)
        {
            int amount = 0;
            do
            {
                val >>= 7;
                amount++;
            } while (val != 0);

            return amount;
        }

        public static int GetVarLongLength(this long val)
        {
            int amount = 0;
            do
            {
                val >>= 7;
                amount++;
            } while (val != 0);

            return amount;
        }

        public static string ToJson(this object obj) => JsonSerializer.Serialize(obj, new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull });

        // Slightly modified from https://gist.github.com/ammaraskar/7b4a3f73bee9dc4136539644a0f27e63
        [SuppressMessage("Roslyn", "CA5350", Justification = "Minecraft protocol and Mojang Session Servers require a SHA1 hash.")]
        public static string MinecraftShaDigest(this string input)
        {
            byte[] hash = SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(input));
            // Reverse the bytes since BigInteger uses little endian
            Array.Reverse(hash);

            BigInteger b = new(hash);
            // very annoyingly, BigInteger in C# tries to be smart and puts in
            // a leading 0 when formatting as a hex number to allow roundtripping
            // of negative numbers, thus we have to trim it off.
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

        public static byte[] ToDerFormat(this AsymmetricKeyParameter key) => SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(key).ToAsn1Object().GetDerEncoded();

        public static byte[] ProcessEncryption(this AsymmetricKeyParameter key, byte[] data, bool encrypt)
        {
            IAsymmetricBlockCipher cipher = new Pkcs1Encoding(new RsaEngine());
            cipher.Init(encrypt, key);
            return cipher.ProcessBlock(data, 0, cipher.GetInputBlockSize());
        }
    }
}