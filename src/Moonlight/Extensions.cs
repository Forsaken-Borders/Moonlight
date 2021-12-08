using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.X509;

namespace Moonlight
{
    public static class Extensions
    {
        public static byte[] ToDerFormat(this AsymmetricKeyParameter key) => SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(key).ToAsn1Object().GetDerEncoded();

        public static byte[] ProcessEncryption(this AsymmetricKeyParameter key, byte[] data, bool encrypt)
        {
            IAsymmetricBlockCipher cipher = new Pkcs1Encoding(new RsaEngine());
            cipher.Init(encrypt, key);
            return cipher.ProcessBlock(data, 0, cipher.GetInputBlockSize());
        }
    }
}