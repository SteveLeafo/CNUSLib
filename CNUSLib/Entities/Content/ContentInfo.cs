using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WudTool
{
    class ContentInfo
    {
        public static int CONTENT_INFO_SIZE = 0x24;

        private short indexOffset;
        private short commandCount;
        private byte[] SHA2Hash;

        public ContentInfo()
            : this((short)0)
        {
        }

        public ContentInfo(short contentCount)
            : this((short)0, contentCount)
        {

        }

        public ContentInfo(short indexOffset, short commandCount)
            : this(indexOffset, commandCount, null)
        {

        }

        public ContentInfo(short indexOffset, short commandCount, byte[] SHA2Hash)
        {
            this.indexOffset = indexOffset;
            this.commandCount = commandCount;
            this.SHA2Hash = SHA2Hash;
        }

        /**
         * Creates a new ContentInfo object given be the raw byte data
         * 
         * @param input
         *            0x24 byte of data from the TMD (starting at 0x208)
         * @return ContentFSTInfo object
         */
        public static ContentInfo parseContentInfo(byte[] input)
        {
            if (input == null || input.Length != CONTENT_INFO_SIZE)
            {
                //MessageBox.Show("Error: invalid ContentInfo byte[] input");
                return null;
            }

            byte[] buffer = new byte[input.Length];
            Array.ConstrainedCopy(input, 0, buffer, 0, input.Length);

            short indexOffset = Utils.SwapEndianness(BitConverter.ToInt16(buffer, 0));
            short commandCount = Utils.SwapEndianness(BitConverter.ToInt16(buffer, 0x02));

            byte[] sha2hash = new byte[0x20];
            //buffer.position(0x04);
            Array.ConstrainedCopy(buffer, 0x04, buffer, 0, 0x20);
            //buffer.get(sha2hash, 0x00, 0x20);

            return new ContentInfo(indexOffset, commandCount, sha2hash);
        }

        public override string ToString()
        {
            return "ContentInfo [indexOffset=" + indexOffset + ", commandCount=" + commandCount + ", SHA2Hash=" + Utils.ByteArrayToString(SHA2Hash) + "]";
        }
    }
}
