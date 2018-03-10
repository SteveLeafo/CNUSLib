using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WudTool
{
    internal class FST
    {
        internal FSTEntry root = FSTEntry.getRootFSTEntry();

        internal int unknown;
        internal int contentCount;

        internal Dictionary<int, ContentFSTInfo> contentFSTInfos = new Dictionary<int, ContentFSTInfo>();

        private FST(int unknown, int contentCount)
        {
            this.unknown = unknown;
            this.contentCount = contentCount;
        }

        /**
         * Creates a FST by the given raw byte data
         * 
         * @param fstData
         *            raw decrypted FST data
         * @param contentsMappedByIndex
         *            map of index/content
         * @return
         */
        public static FST parseFST(byte[] fstData, Dictionary<int, Content> contentsMappedByIndex)
        {
            if (!Arrays.Equals(Arrays.copyOfRange(fstData, 0, 3), new byte[] { 0x46, 0x53, 0x54 }))
            {
                //throw new NullPointerException();
                return null;
                // throw new IllegalArgumentException("Not a FST. Maybe a wrong key?");
            }

            int unknownValue = Utils.SwapEndianness(BitConverter.ToInt32(fstData, 0x04));
            int contentCount = Utils.SwapEndianness(BitConverter.ToInt32(fstData, 0x08));

            FST result = new FST(unknownValue, contentCount);

            int contentfst_offset = 0x20;
            int contentfst_size = 0x20 * contentCount;

            int fst_offset = contentfst_offset + contentfst_size;

            int fileCount = Utils.SwapEndianness(BitConverter.ToInt32(fstData, fst_offset + 0x08));
            int fst_size = fileCount * 0x10;

            int nameOff = fst_offset + fst_size;
            int nameSize = nameOff + 1;

            // Get list with null-terminated Strings. Ends with \0\0.
            for (int i = nameOff; i < fstData.Length - 1; i++)
            {
                if (fstData[i] == 0 && fstData[i + 1] == 0)
                {
                    nameSize = i - nameOff;
                }
            }

            Dictionary<int, ContentFSTInfo> contentFSTInfos = result.contentFSTInfos;
            for (int i = 0; i < contentCount; i++)
            {
                byte[] contentFST = Arrays.copyOfRange(fstData, contentfst_offset + (i * 0x20), contentfst_offset + ((i + 1) * 0x20));
                contentFSTInfos.Add(i, ContentFSTInfo.parseContentFST(contentFST));
            }

            byte[] fstSection = Arrays.copyOfRange(fstData, fst_offset, fst_offset + fst_size);
            byte[] nameSection = Arrays.copyOfRange(fstData, nameOff, nameOff + nameSize);

            FSTEntry root = result.root;

            FSTService.parseFST(root, fstSection, nameSection, contentsMappedByIndex, contentFSTInfos);

            return result;
        }
    }
}
