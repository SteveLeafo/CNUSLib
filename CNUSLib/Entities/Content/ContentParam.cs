using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNUSLib
{
    public class ContentParam
    {
        public int ID;
        public short Index;
        public short Type;

        public long EncryptedFileSize;
        public byte[] SHA2Hash;

        public ContentFSTInfo ContentFSTInfo;
    }
}
