using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNUSLib
{
    class ByteArrayWrapper
    {
        private byte[] data;

        public ByteArrayWrapper(byte[] data) {
        if (data == null) {
            throw new ArgumentNullException();
        }
        this.data = data;
    }

        public override bool Equals(Object other)
        {
            if (!(other is ByteArrayWrapper))
            {
                return false;
            }
            return Arrays.Equals(data, ((ByteArrayWrapper)other).data);
        }

        public override int GetHashCode()
        {
            return data.GetHashCode();
        }
    }
}
