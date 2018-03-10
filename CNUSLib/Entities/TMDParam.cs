using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNUSLib
{
    public class TMDParam
    {
        public int signatureType;                                  // 0x000
        public byte[] signature;                                   // 0x004
        public byte[] issuer;                                      // 0x140
        public byte version;                                       // 0x180
        public byte CACRLVersion;                                  // 0x181
        public byte signerCRLVersion;                              // 0x182
        public long systemVersion;                                 // 0x184
        public long titleID;                                       // 0x18C
        public int titleType;                                      // 0x194
        public short groupID;                                      // 0x198
        public byte[] reserved;                                    // 0x19A
        public int accessRights;                                   // 0x1D8
        public short titleVersion;                                 // 0x1DC
        public short contentCount;                                 // 0x1DE
        public short bootIndex;                                    // 0x1E0
        public byte[] SHA2;                                        // 0x1E4
        public ContentInfo[] contentInfos;                         //
        public byte[] cert1;
        public byte[] cert2;
    }
}
