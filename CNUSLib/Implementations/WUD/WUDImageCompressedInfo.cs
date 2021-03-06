﻿using System;
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

            int magic0 = BitConverter.ToInt32(headData, 0x00);
            int magic1 = BitConverter.ToInt32(headData, 0x04);

            if (magic0 == WUX_MAGIC_0 && magic1 == WUX_MAGIC_1)
            {
                valid = true;
            }
            else
            {
                valid = false;
            }

            sectorSize = BitConverter.ToInt32(headData, 0x08);
            flags = BitConverter.ToInt32(headData, 0x0c);
            uncompressedSize = BitConverter.ToInt64(headData, 0x10);

            calculateOffsets();
        }

        public static WUDImageCompressedInfo getDefaultCompressedInfo()
        {
            return new WUDImageCompressedInfo(SECTOR_SIZE, 0, WUDImage.WUD_FILESIZE);
        }

        public WUDImageCompressedInfo(int sectorSize, int flags, long uncompressedSize)
        {
            this.sectorSize = sectorSize;
            this.flags = flags;
            this.uncompressedSize = uncompressedSize;
            valid = true;
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

        public long getSectorIndex(int sectorIndex)
        {
            return indexTable[sectorIndex];
        }

        public void setIndexTable(Dictionary<int, long> indexTable)
        {
            this.indexTable = indexTable;
        }

        public byte[] getHeaderAsBytes()
        {
            ByteBuffer result = ByteBuffer.allocate(WUX_HEADER_SIZE);
            //result.order(ByteOrder.LITTLE_ENDIAN);
            result.putInt(WUX_MAGIC_0);
            result.putInt(WUX_MAGIC_1);
            result.putInt(sectorSize);
            result.putInt(flags);
            result.putLong(uncompressedSize);
            return result.array();
        }
    }
}
