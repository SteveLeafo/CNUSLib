using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CNUSLib
{
    public class HashUtil
    {
        public static byte[] hashSHA1(byte[] data)
        {
            HashAlgorithm sha1;
            try
            {
                sha1 = new SHA1CryptoServiceProvider();
            }
            catch (Exception  e)
            {
                //e.printStackTrace();
                return new byte[0x14];
            }

            return sha1.ComputeHash(data, 0, data.Length);
        }
    }
}
