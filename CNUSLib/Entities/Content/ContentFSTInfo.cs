using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WudTool
{
    public class ContentFSTInfo
    {
        private long offsetSector;
        private long sizeSector;
        private long ownerTitleID;
        private int groupID;
        private byte unkown;

        private static int SECTOR_SIZE = 0x8000;

        private ContentFSTInfo(ContentFSTInfoParam param)
        {
            this.offsetSector = param.offsetSector;
            this.sizeSector = param.sizeSector;
            this.ownerTitleID = param.ownerTitleID;
            this.groupID = param.groupID;
            this.unkown = param.unkown;
        }

        /**
         * Creates a new ContentFSTInfo object given be the raw byte data
         * 
         * @param input
         *            0x20 byte of data from the FST (starting at 0x20)
         * @return ContentFSTInfo object
         */
        public static ContentFSTInfo parseContentFST(byte[] input)
        {
            if (input == null || input.Length != 0x20)
            {
                //MessageBox.Show("Error: invalid ContentFSTInfo byte[] input");
                return null;
            }
            ContentFSTInfoParam param = new ContentFSTInfoParam();
            ByteBuffer buffer = ByteBuffer.allocate(input.Length);
            buffer.put(input);

            buffer.position(0);
            int offset = buffer.getInt();
            int size = buffer.getInt();
            long ownerTitleID = buffer.getLong();
            int groupID = buffer.getInt();
            byte unkown = buffer.get();

            param.offsetSector = (offset);
            param.sizeSector = (size);
            param.ownerTitleID = (ownerTitleID);
            param.groupID = (groupID);
            param.unkown = (unkown);

            return new ContentFSTInfo(param);
        }

        /**
         * Returns the offset of of the Content in the partition
         * 
         * @return offset of the content in the partition in bytes
         */
        public long getOffset()
        {
            long result = (offsetSector * SECTOR_SIZE) - SECTOR_SIZE;
            if (result < 0)
            {
                return 0;
            }
            return result;
        }

        /**
         * Returns the size in bytes, not in sectors
         * 
         * @return size in bytes
         */
        public int getSize()
        {
            return (int)(sizeSector * SECTOR_SIZE);
        }

        public override string ToString()
        {
            return "ContentFSTInfo [offset=" + offsetSector.ToString("X8") + ", size=" + sizeSector.ToString("X8") + ", ownerTitleID="
                    + ownerTitleID.ToString("X16") + ", groupID=" + groupID.ToString("X8") + ", unkown=" + unkown + "]";
        }

    }
}
