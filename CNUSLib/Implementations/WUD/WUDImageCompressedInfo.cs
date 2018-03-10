using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNUSLib
{
    public class WUDImageCompressedInfo
    {
        public const int WUX_HEADER_SIZE = 0x20;
        public const int WUX_MAGIC_0 = 0x30585557;
        public const int WUX_MAGIC_1 = 0x1099d02e;
        public const int SECTOR_SIZE = 0x8000;

        public int sectorSize;
        public long uncompressedSize;
        public int flags;

        public long indexTableEntryCount;
        public long offsetIndexTable = WUX_HEADER_SIZE;
        public long offsetSectorArray;
        public long indexTableSize;

        bool valid;
        public Dictionary<int, long> indexTable;

        public WUDImageCompressedInfo(byte[] headData)
        {
            if (headData.Length < WUX_HEADER_SIZE)
            {
                //MessageBox.Show("WUX header length wrong");
                return;
            }

            int magic0 = Utils.SwapEndianness(BitConverter.ToInt32(headData, 0x00));
            int magic1 = Utils.SwapEndianness(BitConverter.ToInt32(headData, 0x04));

            if (magic0 == WUX_MAGIC_0 && magic1 == WUX_MAGIC_1)
            {
                valid = true;
            }
            else
            {
                valid = false;
            }

            sectorSize = Utils.SwapEndianness(BitConverter.ToInt32(headData, 0x08));
            flags = Utils.SwapEndianness(BitConverter.ToInt32(headData, 0x0c));
            uncompressedSize = Utils.SwapEndianness(BitConverter.ToInt32(headData, 0x10));

            calculateOffsets();
        }

        private void calculateOffsets()
        {
            indexTableEntryCount = (uncompressedSize + sectorSize - 1) / sectorSize;
            //setIndexTableEntryCount(indexTableEntryCount);
            offsetSectorArray = (offsetIndexTable + ((long)indexTableEntryCount * 0x04L));
            // align to SECTOR_SIZE
            offsetSectorArray = (offsetSectorArray + (long)(sectorSize - 1));
            offsetSectorArray = offsetSectorArray - (offsetSectorArray % (long)sectorSize);
            //setOffsetSectorArray(offsetSectorArray);
            // read index table
            indexTableSize = (0x04 * indexTableEntryCount);
        }





        public bool isWUX()
        {
            return valid;
        }
    }
}
