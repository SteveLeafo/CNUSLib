using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNUSLib
{
    public class Utils
    {
        public static long align(long numToRound, int multiple)
        {
            if ((multiple > 0) && ((multiple & (multiple - 1)) == 0))
            {
                return alignPower2(numToRound, multiple);
            }
            else
            {
                return alignGeneric(numToRound, multiple);
            }
        }

        private static long alignGeneric(long numToRound, int multiple)
        {
            int isPositive = 0;
            if (numToRound >= 0)
            {
                isPositive = 1;
            }
            return ((numToRound + isPositive * (multiple - 1)) / multiple) * multiple;
        }

        private static long alignPower2(long numToRound, int multiple)
        {
            if (!((multiple > 0) && ((multiple & (multiple - 1)) == 0))) return 0L;
            return (numToRound + (multiple - 1)) & ~(multiple - 1);
        }

        public static int SwapEndianness(int value)
        {
            var b1 = (value >> 0) & 0xff;
            var b2 = (value >> 8) & 0xff;
            var b3 = (value >> 16) & 0xff;
            var b4 = (value >> 24) & 0xff;

            return b1 << 24 | b2 << 16 | b3 << 8 | b4 << 0;
            //return value;
        }

        public static Int16 SwapEndianness(Int16 value)
        {
            var b1 = (value >> 0) & 0xff;
            var b2 = (value >> 8) & 0xff;

            return (short)(b1 << 8 | b2 << 0);
            //return value;
        }


        public static String ByteArrayToString(byte[] ba)
        {
            if (ba == null) return null;
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
            {
                hex.Append(b.ToString("X2"));
            }
            return hex.ToString();
        }

        public static byte[] StringToByteArray(String s)
        {
            int len = s.Length;
            byte[] data = new byte[len / 2];
            for (int i = 0; i < len; i += 2)
            {
                data[i / 2] = (byte)((Convert.ToInt32(s[i].ToString(), 16) << 4) + Convert.ToInt32(s[(i + 1)].ToString(), 16));
            }
            return data;
        }

        public static bool checkXML(System.IO.FileInfo fileInfo)
        {
            throw new NotImplementedException();
        }

        internal static void createDir(string usedOutputFolder)
        {
            Directory.CreateDirectory(usedOutputFolder);
        }
    }
}
