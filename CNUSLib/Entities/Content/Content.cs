using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WudTool
{
    class Content
    {
        public static short CONTENT_FLAG_UNKWN1 = 0x4000;
        public static short CONTENT_HASHED = 0x0002;
        public static short CONTENT_ENCRYPTED = 0x0001;
        public static int CONTENT_SIZE = 0x30;

        internal int ID;
        internal short index;
        internal short type;

        internal long encryptedFileSize;
        internal byte[] SHA2Hash;

        internal List<FSTEntry> entries = new List<FSTEntry>();

        internal ContentFSTInfo contentFSTInfo;

        internal Content(ContentParam param)
        {
            this.ID = param.ID;
            this.index = param.Index;
            this.type = param.Type;
            this.encryptedFileSize = param.EncryptedFileSize;
            this.SHA2Hash = param.SHA2Hash;
        }

        /**
         * Creates a new Content object given be the raw byte data
         * 
         * @param input
         *            0x30 byte of data from the TMD (starting at 0xB04)
         * @return content object
         */
        public static Content parseContent(byte[] input)
        {
            if (input == null || input.Length != CONTENT_SIZE)
            {
                //MessageBox.Show("Error: invalid Content byte[] input");
                return null;
            }
            ByteBuffer buffer = ByteBuffer.allocate(input.Length);
            buffer.put(input);
            buffer.position(0);

            int ID = buffer.getInt(0x00);
            short index = buffer.getShort(0x04);
            short type = buffer.getShort(0x06);
            long encryptedFileSize = buffer.getLong(0x08);
            buffer.position(0x10);
            byte[] hash = new byte[0x14];
            buffer.get(hash, 0x00, 0x14);
            byte[] hash2 = new byte[0x06];
            buffer.get(hash2, 0x00, 0x06);


            ContentParam param = new ContentParam();
            param.ID = (ID);
            param.Index = (index);
            param.Type = (type);
            param.EncryptedFileSize = (encryptedFileSize);
            param.SHA2Hash = (hash);

            return new Content(param);
        }

        /**
         * Returns if the content is hashed
         * 
         * @return true if hashed
         */
        public bool isHashed()
        {
            return (type & CONTENT_HASHED) == CONTENT_HASHED;
        }

        /**
         * Returns if the content is encrypted
         * 
         * @return true if encrypted
         */
        public bool isEncrypted()
        {
            return (type & CONTENT_ENCRYPTED) == CONTENT_ENCRYPTED;
        }

        public bool isUNKNWNFlag1Set()
        {
            return (type & CONTENT_FLAG_UNKWN1) == CONTENT_FLAG_UNKWN1;
        }

        /**
         * Return the filename of the encrypted content.
         * It's the ID as hex with an extension
         * For example: 00000000.app
         * 
         * @return filename of the encrypted content
         */
        public String getFilename()
        {
            return String.Format("%08X%s", ID, Settings.ENCRYPTED_CONTENT_EXTENTION);
        }

        /**
         * Adds a content to the internal entry list.
         * 
         * @param entry
         *            that will be added to the content list
         */
        public void addEntry(FSTEntry entry)
        {
            entries.Add(entry);
        }

        /**
         * Returns the size of the decrypted content.
         * 
         * @return size of the decrypted content
         */
        public long getDecryptedFileSize()
        {
            if (isHashed())
            {
                return encryptedFileSize / 0x10000 * 0xFC00;
            }
            else
            {
                return encryptedFileSize;
            }
        }

        /**
         * Return the filename of the decrypted content.
         * It's the ID as hex with an extension
         * For example: 00000000.dec
         * 
         * @return filename of the decrypted content
         */
        public String getFilenameDecrypted()
        {
            return String.Format("%08X%s", ID, Settings.DECRYPTED_CONTENT_EXTENTION);
        }


        public override int GetHashCode()
        {
            int prime = 31;
            int result = 1;
            result = prime * result + ID;
            result = prime * result + SHA2Hash.GetHashCode();// Arrays.hashCode(SHA2Hash);
            return result;
        }

        public bool Equals(Object obj)
        {
            if (this == obj) return true;
            if (obj == null) return false;
            if (GetType() != obj.GetType()) return false;
            Content other = (Content)obj;
            if (ID != other.ID) return false;
            if (SHA2Hash.Length != other.SHA2Hash.Length)
            {
                return false;
            }
            for (int i = 0; i < SHA2Hash.Length; ++i)
            {
                if (SHA2Hash[i] != other.SHA2Hash[i])
                {
                    return false;
                }
            }
            return Arrays.Equals(SHA2Hash, other.SHA2Hash);
        }

        public long getEncryptedFileSizeAligned()
        {
            return Utils.align(encryptedFileSize, 16);
        }

        public override String ToString()
        {
            string s = "Content [ID=" + ID.ToString("X4") + ", index=" + index.ToString("X4")  + ", type=" + type.ToString("X4") 
                    + ", encryptedFileSize=" + encryptedFileSize + ", SHA2Hash=" + Utils.ByteArrayToString(SHA2Hash) + "]";
            return s;
        }
    }
}
