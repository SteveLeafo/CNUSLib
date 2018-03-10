using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WudTool
{
    internal class Arrays
    {
        internal static byte[] copyOfRange(byte[] namesSection, int from, int to)
        {
            if (from == to || from >= namesSection.Length)
            {
                return new byte[0];
            }
            byte[] returnValue = new byte[to - from];
            Array.ConstrainedCopy(namesSection, from, returnValue, 0, to - from);
            return returnValue;
        }

        internal static byte[] copyOf(byte[] curEntry, int p)
        {
            byte[] returnValue = new byte[p];
            Array.ConstrainedCopy(curEntry, 0, returnValue, 0, p);
            return returnValue;
        }

        public static bool Equals(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
            {
                return false;
            }

            for (int i = 0; i < a.Length; ++i)
            {
                if (a[i] != b[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
