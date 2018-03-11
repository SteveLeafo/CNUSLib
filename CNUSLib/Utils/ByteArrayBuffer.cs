using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNUSLib
{
    class ByteArrayBuffer
    {
        public byte[] buffer;
        int lengthOfDataInBuffer;

        public ByteArrayBuffer(int length)
        {
            buffer = new byte[(int)length];
        }

        public int getSpaceLeft()
        {
            return buffer.Length - lengthOfDataInBuffer;
        }

        public void addLengthOfDataInBuffer(int bytesRead)
        {
            lengthOfDataInBuffer += bytesRead;
        }

        public void resetLengthOfDataInBuffer()
        {
            lengthOfDataInBuffer = (0);
        }
    }
}
