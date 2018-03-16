using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNUSLib
{
    public class WUDDiscReaderCompressed : WUDDiscReader
    {
        public WUDDiscReaderCompressed(WUDImage image)
            : base(image)
        {
        }

        /**
         * Expects the .wux format by Exzap. You can more infos about it here. https://gbatemp.net/threads/wii-u-image-wud-compression-tool.397901/
         */
        public override void readEncryptedToOutputStream(Stream outputStream, long offset, long size)
        {
            // make sure there is no out-of-bounds read
            WUDImageCompressedInfo info = image.compressedInfo;

            long fileBytesLeft = info.uncompressedSize - offset;

            long usedOffset = offset;
            long usedSize = size;

            if (fileBytesLeft <= 0)
            {
                //MessageBox.Show("offset too big");
                //System.exit(1);
                return;
            }
            if (fileBytesLeft < usedSize)
            {
                usedSize = fileBytesLeft;
            }
            // compressed read must be handled on a per-sector level

            int bufferSize = 0x8000;
            byte[] buffer = new byte[bufferSize];

            BinaryReader input = getRandomAccessFileStream();
            while (usedSize > 0)
            {
                long sectorOffset = (usedOffset % info.sectorSize);
                long remainingSectorBytes = info.sectorSize - sectorOffset;
                long sectorIndex = (usedOffset / info.sectorSize);
                int bytesToRead = (int)((remainingSectorBytes < usedSize) ? remainingSectorBytes : usedSize); // read only up to the end of the current sector
                // look up real sector index
                long realSectorIndex = info.getSectorIndex((int)sectorIndex);
                long offset2 = info.offsetSectorArray + realSectorIndex * info.sectorSize + sectorOffset;

                input.BaseStream.Seek(offset2, SeekOrigin.Begin);
                int read = input.Read(buffer, 0, buffer.Length);
                if (read < 0) return;
                try
                {
                    outputStream.Write(buffer, 0, bytesToRead);
                }
                catch (Exception)
                {
                    //if (e.getMessage().equals("Pipe closed")) {
                    //    break;
                    //} else {
                    //    throw e;
                    //}
                }

                usedSize -= bytesToRead;
                usedOffset += bytesToRead;
            }
            input.Close();
        }
    }
}
