using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WudTool
{
    class WUDPartitionHeader
    {
        internal bool calculatedHashes = false;
        internal Dictionary<short, byte[]> h3Hashes = new Dictionary<short, byte[]>();
        internal byte[] rawData;

        private WUDPartitionHeader()
        {
        }

        // TODO: real processing. Currently we are ignoring everything except the hashes
        public static WUDPartitionHeader parseHeader(byte[] header)
        {
            WUDPartitionHeader result = new WUDPartitionHeader();
            result.rawData = (header);
            return result;
        }

        public void addH3Hashes(short index, byte[] hash)
        {
            h3Hashes.Add(index, hash);
        }

        public byte[] getH3Hash(Content content)
        {
            if (content == null)
            {
                //MessageBox.Show("Can't find h3 hash, given content is null.");
                return null;
            }

            return h3Hashes[(content.index)];
        }

        public void calculateHashes(Dictionary<int, Content> allContents)
        {
            byte[] header = rawData;

            // Calculating offset for the hashes

            int cnt = Utils.SwapEndianness(BitConverter.ToInt32(header, 0x10));
            int start_offset = 0x40 + cnt * 0x04;

            int offset = 0;

            // We have to make sure, that the list is ordered by index
            List<Content> contents = new List<Content>(allContents.Values);
            // TODO
            //Collections.sort(contents, new Comparator<Content>() {
            //    @Override
            //    public int compare(Content o1, Content o2) {
            //        return Short.compare(o1.getIndex(), o2.getIndex());
            //    }
            //});

            foreach (Content c in allContents.Values)
            {
                if (!c.isHashed() || !c.isEncrypted())
                {
                    continue;
                }

                // The encrypted content are splitted in 0x10000 chunk. For each 0x1000 chunk we need one entry in the h3
                int cnt_hashes = (int)(c.encryptedFileSize / 0x10000 / 0x1000) + 1;

                byte[] hash = Arrays.copyOfRange(header, start_offset + offset * 0x14, start_offset + (offset + cnt_hashes) * 0x14);

                // Checking the hash of the h3 file.
                // TODO
                //if (!Arrays.Equals(HashUtil.hashSHA1(hash), c.getSHA2Hash()))
                //{
                //    MessageBox.Show("h3 incorrect from WUD");
                //}

                addH3Hashes(c.index, hash);
                offset += cnt_hashes;
            }

            calculatedHashes = (true);
        }
    }
}
