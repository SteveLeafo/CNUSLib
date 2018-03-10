using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WudTool
{
    internal class ContentParam
    {
        internal int ID;
        internal short Index;
        internal short Type;

        internal long EncryptedFileSize;
        internal byte[] SHA2Hash;

        internal ContentFSTInfo ContentFSTInfo;
    }
}
