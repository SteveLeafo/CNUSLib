using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CNUSLib
{
    class AESDecryption
    {
        private Aes cipher;

        protected byte[] AESKey;
        protected byte[] IV;

        public AESDecryption(byte[] AESKeyIn, byte[] IVIn)
        {
            try
            {
                cipher = Aes.Create();
            }
            catch (Exception)
            {
            }
            AESKey = AESKeyIn;
            IV = IVIn;
            init();
        }

        protected void init()
        {
            init(AESKey, IV);
        }

        protected void init(byte[] decryptedKey, byte[] iv)
        {
            cipher.Key = AESKey;
            cipher.IV = IV;
            cipher.Mode = CipherMode.CBC;
            cipher.Padding = PaddingMode.None;
        }

        public byte[] decrypt(byte[] input)
        {
            try
            {
                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms, cipher.CreateDecryptor(), CryptoStreamMode.Write);
                cs.Write(input, 0, input.Length);
                return ms.ToArray();
            }
            catch (Exception)
            {
            }
            return input;
        }

        public byte[] decrypt(byte[] input, int len)
        {
            try
            {
                if (len <= input.Length)
                {
                    MemoryStream ms = new MemoryStream();
                    CryptoStream cs = new CryptoStream(ms, cipher.CreateDecryptor(), CryptoStreamMode.Write);
                    cs.Write(input, 0, len);
                    return ms.ToArray();
                }
            }
            catch (Exception)
            {
            }
            return input;
        }

        public byte[] decrypt(byte[] input, int offset, int len)
        {
            byte[] buffer = new byte[len];
            Array.ConstrainedCopy(input, offset, buffer, 0, len);
            try
            {
                if (len <= input.Length)
                {
                    MemoryStream ms = new MemoryStream();
                    CryptoStream cs = new CryptoStream(ms, cipher.CreateDecryptor(), CryptoStreamMode.Write);
                    cs.Write(buffer, 0, len);
                    return ms.ToArray();
                }
            }
            catch (Exception)
            {
            }
            return buffer;
        }
    }
}
