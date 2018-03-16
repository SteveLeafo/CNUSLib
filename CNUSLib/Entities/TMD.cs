using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNUSLib
{
    public class TMD
    {
        private static int SIGNATURE_LENGTH = 0x100;
        private static int ISSUER_LENGTH = 0x40;
        private static int RESERVED_LENGTH = 0x3E;
        private static int SHA2_LENGTH = 0x20;

        private static int POSITION_SIGNATURE = 0x04;
        private static int POSITION_ISSUER = 0x140;
        private static int POSITION_RESERVED = 0x19A;
        private static int POSITION_SHA2 = 0x1E4;

        private static int CONTENT_INFO_ARRAY_SIZE = 0x40;

        private static int CONTENT_INFO_OFFSET = 0x204;
        private static int CONTENT_OFFSET = 0xB04;

        private static int CERT1_LENGTH = 0x400;
        private static int CERT2_LENGTH = 0x300;



        public int signatureType;                        // 0x000
        public byte[] signature;                         // 0x004
        public byte[] issuer;                            // 0x140
        public byte version;                             // 0x180
        public byte CACRLVersion;                        // 0x181
        public byte signerCRLVersion;                    // 0x182
        public long systemVersion;                       // 0x184
        public long titleID;                             // 0x18C
        public int titleType;                            // 0x194
        public short groupID;                            // 0x198
        public byte[] reserved;                          // 0x19A
        public int accessRights;                         // 0x1D8
        public short titleVersion;                       // 0x1DC
        public short contentCount;                       // 0x1DE
        public short bootIndex;                          // 0x1E0
        public byte[] SHA2;                              // 0x1E4
        public ContentInfo[] contentInfos;
        public byte[] cert1;
        public byte[] cert2;

        private Dictionary<int, Content> contentToIndex = new Dictionary<int, Content>();
        private Dictionary<int, Content> contentToID = new Dictionary<int, Content>();

        private TMD(TMDParam param)
            : base()
        {
            this.signatureType = param.signatureType;
            this.signature = param.signature;
            this.issuer = param.issuer;
            this.version = param.version;
            this.CACRLVersion = param.CACRLVersion;
            this.signerCRLVersion = param.signerCRLVersion;
            this.systemVersion = param.systemVersion;
            this.titleID = param.titleID;
            this.titleType = param.titleType;
            this.groupID = param.groupID;
            this.reserved = param.reserved;
            this.accessRights = param.accessRights;
            this.titleVersion = param.titleVersion;
            this.contentCount = param.contentCount;
            this.bootIndex = param.bootIndex;
            this.SHA2 = param.SHA2;
            this.contentInfos = param.contentInfos;
            this.cert1 = param.cert1;
            this.cert2 = param.cert2;
        }

        public static TMD parseTMD(FileInfo tmd)
        {
            if (tmd == null || !tmd.Exists)
            {
                //MessageBox.Show("TMD input file null or doesn't exist.");
                return null;
            }
            return parseTMD(File.ReadAllBytes(tmd.FullName));
        }

        public static TMD parseTMD(byte[] input)
        {
            byte[] signature = new byte[SIGNATURE_LENGTH];
            byte[] issuer = new byte[ISSUER_LENGTH];
            byte[] reserved = new byte[RESERVED_LENGTH];
            byte[] SHA2 = new byte[SHA2_LENGTH];
            byte[] cert1 = new byte[CERT1_LENGTH];
            byte[] cert2 = new byte[CERT2_LENGTH];

            ContentInfo[] contentInfos = new ContentInfo[CONTENT_INFO_ARRAY_SIZE];

            ByteBuffer buffer = ByteBuffer.allocate(input.Length);
            buffer.put(input);

            // Get Signature
            buffer.position(0);
            int signatureType = buffer.getInt();
            buffer.position(POSITION_SIGNATURE);
            buffer.get(signature, 0, SIGNATURE_LENGTH);

            // Get Issuer
            buffer.position(POSITION_ISSUER);
            buffer.get(issuer, 0, ISSUER_LENGTH);

            // Get CACRLVersion and signerCRLVersion
            buffer.position(0x180);
            byte version = buffer.get();
            byte CACRLVersion = buffer.get();
            byte signerCRLVersion = buffer.get();

            // Get title information
            buffer.position(0x184);
            long systemVersion = buffer.getLong();
            long titleID = buffer.getLong();
            int titleType = buffer.getInt();
            short groupID = buffer.getShort();

            // Get other information
            buffer.position(POSITION_RESERVED);
            buffer.get(reserved, 0, RESERVED_LENGTH);

            // Get accessRights,titleVersion,contentCount,bootIndex
            buffer.position(0x1D8);
            int accessRights = buffer.getInt();
            short titleVersion = buffer.getShort();
            short contentCount = buffer.getShort();
            short bootIndex = buffer.getShort();

            // Get hash
            buffer.position(POSITION_SHA2);
            buffer.get(SHA2, 0, SHA2_LENGTH);

            // Get contentInfos
            buffer.position(CONTENT_INFO_OFFSET);
            for (int i = 0; i < CONTENT_INFO_ARRAY_SIZE; i++)
            {
                byte[] contentInfo = new byte[ContentInfo.CONTENT_INFO_SIZE];
                buffer.get(contentInfo, 0, ContentInfo.CONTENT_INFO_SIZE);
                contentInfos[i] = ContentInfo.parseContentInfo(contentInfo);
            }

            List<Content> contentList = new List<Content>();
            // Get Contents
            for (int i = 0; i < contentCount; i++)
            {
                buffer.position(CONTENT_OFFSET + (Content.CONTENT_SIZE * i));
                byte[] content = new byte[Content.CONTENT_SIZE];
                buffer.get(content, 0, Content.CONTENT_SIZE);
                Content c = Content.parseContent(content);
                contentList.Add(c);
            }

            try
            {
                buffer.get(cert2, 0, CERT2_LENGTH);
            }
            catch (Exception)
            {

            }

            try
            {
                buffer.get(cert1, 0, CERT1_LENGTH);
            }
            catch (Exception)
            {

            }

            TMDParam param = new TMDParam();
            param.signatureType = (signatureType);
            param.signature = (signature);
            param.version = (version);
            param.CACRLVersion = (CACRLVersion);
            param.signerCRLVersion = (signerCRLVersion);
            param.systemVersion = (systemVersion);
            param.titleID = (titleID);
            param.titleType = (titleType);
            param.groupID = (groupID);
            param.accessRights = (accessRights);
            param.titleVersion = (titleVersion);
            param.contentCount = (contentCount);
            param.bootIndex = (bootIndex);
            param.SHA2 = (SHA2);
            param.contentInfos = (contentInfos);
            param.cert1 = (cert1);
            param.cert2 = (cert2);

            TMD result = new TMD(param);

            foreach (Content c in contentList)
            {
                result.setContentToIndex(c.index, c);
                result.setContentToID(c.ID, c);
            }

            return result;
        }

        public Content getContentByIndex(int index)
        {
            return contentToIndex[index];
        }

        private void setContentToIndex(int index, Content content)
        {
            contentToIndex[index] = content;
        }

        public Content getContentByID(int id)
        {
            return contentToID[id];
        }

        private void setContentToID(int id, Content content)
        {
            string s = content.ToString();
            if (contentToID.ContainsKey(id))
            {
                contentToID[id] = content;
            }
            else
            {
                contentToID.Add(id, content);
            }
        }

        /**
         * Returns all contents mapped by index
         * 
         * @return Map of Content, index/content pairs
         */
        public Dictionary<int, Content> getAllContents()
        {
            return contentToIndex;
        }

        public void printContents()
        {
            //long totalSize = 0;
            //for (Content c : contentToIndex.values()) {
            //    totalSize += c.getEncryptedFileSize();
            //    System.out.println(c);
            //}
            //System.out.println("Total size: " + totalSize);

        }

    }
}
