using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WudTool
{
    internal class ByteBuffer
    {
        byte[] buffer;
        internal int currentPos;

        public ByteBuffer(int length)
        {
            buffer = new byte[length];
        }

        internal static ByteBuffer allocate(int length)
        {
            return new ByteBuffer(length);
        }

        internal void put(byte[] input)
        {
            if (input.Length == buffer.Length)
            {
                Array.ConstrainedCopy(input, 0, buffer, 0, input.Length);
                currentPos = 0;
            }
        }

        internal void position(int p)
        {
            currentPos = p;
        }

        internal int getInt(int p)
        {
            currentPos = p + 4;
            return Utils.SwapEndianness(BitConverter.ToInt32(buffer, p));
        }

        internal int getInt()
        {
            int l = Utils.SwapEndianness(BitConverter.ToInt32(buffer, currentPos));
            currentPos += 4;
            return l;
        }

        internal byte get()
        {
            currentPos++;
            return buffer[currentPos - 1];
        }

        internal short getShort(int p)
        {
            currentPos = p + 2;
            return Utils.SwapEndianness(BitConverter.ToInt16(buffer, p));
        }

        internal short getShort()
        {
            short l = Utils.SwapEndianness(BitConverter.ToInt16(buffer, currentPos));
            currentPos += 2;
            return l;
        }

        internal long getLong(int p)
        {
            currentPos = p + 4;
            byte[] bytes = new byte[8];
            Array.ConstrainedCopy(buffer, p, bytes, 0, 8);
            bytes = bytes.Reverse().ToArray();
            long l = BitConverter.ToInt64(bytes, 0);
            return l;
        }

        internal long getLong()
        {
            byte[] bytes = new byte[8];
            Array.ConstrainedCopy(buffer, currentPos, bytes, 0, 8);
            //bytes = bytes.Reverse().ToArray();
            long l2 = BitConverter.ToInt64(bytes, 0);
            bytes = bytes.Reverse().ToArray();
            long l1 = BitConverter.ToInt64(bytes, 0);
            currentPos += 8;
            return l1;
        }
        internal long getLong2()
        {
            byte[] bytes = new byte[8];
            Array.ConstrainedCopy(buffer, currentPos, bytes, 0, 8);
           // bytes = bytes.Reverse().ToArray();
            long l = BitConverter.ToInt64(bytes, 0);
            currentPos += 8;
            return l;
        }

        internal void get(byte[] dst, int offset, int length)
        {
            if (offset == 0)
            {
                currentPos += offset;
            }
            else
            {
                currentPos += offset;
            }
            if (dst.Length <= length)
            {
                Array.ConstrainedCopy(buffer, currentPos, dst, 0, length);
                currentPos += length;
            }
        }

        internal byte[] putLong(long l)
        {
            byte[] ar = BitConverter.GetBytes(l).Reverse().ToArray();
            Array.ConstrainedCopy(ar, 0, buffer, currentPos, 0x08);
            return buffer;
        }


        internal byte[] putShort(short p)
        {
            byte[] ar = BitConverter.GetBytes(p).Reverse().ToArray();
            Array.ConstrainedCopy(ar, 0, buffer, currentPos, 0x08);
            return buffer;
        }
    }
}
