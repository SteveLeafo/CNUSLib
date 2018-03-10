using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WudTool
{
    internal class TMDParam
    {
        internal int signatureType;                                  // 0x000
        internal byte[] signature;                                   // 0x004
        internal byte[] issuer;                                      // 0x140
        internal byte version;                                       // 0x180
        internal byte CACRLVersion;                                  // 0x181
        internal byte signerCRLVersion;                              // 0x182
        internal long systemVersion;                                 // 0x184
        internal long titleID;                                       // 0x18C
        internal int titleType;                                      // 0x194
        internal short groupID;                                      // 0x198
        internal byte[] reserved;                                    // 0x19A
        internal int accessRights;                                   // 0x1D8
        internal short titleVersion;                                 // 0x1DC
        internal short contentCount;                                 // 0x1DE
        internal short bootIndex;                                    // 0x1E0
        internal byte[] SHA2;                                        // 0x1E4
        internal ContentInfo[] contentInfos;                         //
        internal byte[] cert1;
        internal byte[] cert2;
    }
}
