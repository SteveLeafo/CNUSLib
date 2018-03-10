using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNUSLib
{
    public class NUSDataProviderWUD : NUSDataProvider
    {
        private WUDInfo WUDInfo;

        private TMD tmd;

        public NUSDataProviderWUD(NUSTitle title, WUDInfo wudinfo)
            : base(title)
        {
            //super(title);
            this.WUDInfo = wudinfo;
            this.tmd = TMD.parseTMD(getRawTMD());
        }

        public long getOffsetInWUD(Content content)
        {
            if (content.contentFSTInfo == null)
            {
                return getAbsoluteReadOffset();
            }
            else
            {
                return getAbsoluteReadOffset() + content.contentFSTInfo.getOffset();
            }
        }

        public long getAbsoluteReadOffset()
        {
            return (long)Settings.WIIU_DECRYPTED_AREA_OFFSET + getGamePartition().partitionOffset;
        }

        public override MemoryStream getInputStreamFromContent(Content content, long fileOffsetBlock)
        {
            WUDDiscReader discReader = getDiscReader();
            long offset = getOffsetInWUD(content) + fileOffsetBlock;
            return discReader.readEncryptedToInputStream(offset, content.encryptedFileSize);
        }

        public override byte[] getContentH3Hash(Content content)
        {

            if (getGamePartitionHeader() == null)
            {
                //MessageBox.Show("GamePartitionHeader is null");
                return null;
            }

            if (!getGamePartitionHeader().calculatedHashes)
            {
                //MessageBox.Show("Calculating h3 hashes");
                getGamePartitionHeader().calculateHashes(getTMD().getAllContents());

            }
            return getGamePartitionHeader().getH3Hash(content);
        }

        public TMD getTMD()
        {
            return tmd;
        }

        public override byte[] getRawTMD()
        {
            return getGamePartition().rawTMD;
        }

        public override byte[] getRawTicket()
        {
            return getGamePartition().rawTicket;
        }

        public override byte[] getRawCert()
        {
            return getGamePartition().rawCert;
        }

        public WUDGamePartition getGamePartition()
        {
            return WUDInfo.getGamePartition();
        }

        public WUDPartitionHeader getGamePartitionHeader()
        {
            return getGamePartition().partitionHeader;
        }

        public WUDDiscReader getDiscReader()
        {
            return WUDInfo.WUDDiscReader;
        }

        public override void cleanup()
        {
            // We don't need it
        }

        public override string ToString()
        {
            return "NUSDataProviderWUD [WUDInfo=" + WUDInfo + "]";
        }
    }
}
