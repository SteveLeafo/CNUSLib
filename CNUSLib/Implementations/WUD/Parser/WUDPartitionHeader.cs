using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNUSLib
{
    public class WUDPartitionHeader
    {
        private static TraceSource log = new TraceSource("WUDPartitionHeader");

        public bool calculatedHashes = false;
        public Dictionary<short, byte[]> h3Hashes = new Dictionary<short, byte[]>();
        public byte[] rawData;

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
                log.TraceEvent(TraceEventType.Error, 0, "Can't find h3 hash, given content is null.");
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
            contents.Sort(delegate(Content c1, Content c2) { return c1.index.CompareTo(c2.index); });

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
                if (!Arrays.Equals(HashUtil.hashSHA1(hash), c.SHA2Hash))
                {
                    log.TraceEvent(TraceEventType.Error, 0, "h3 incorrect from WUD");
                }

                addH3Hashes(c.index, hash);
                offset += cnt_hashes;
            }

            calculatedHashes = (true);
        }
    }
}
