using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNUSLib
{
    public class FSTEntryParam
    {
        public String Filename = "";
        public String Path = "";

        public FSTEntry Parent = null;

        public short Flags;

        public long FileSize = 0;
        public long FileOffset = 0;

        public Content Content = null;

        public bool isDir = false;
        public bool isRoot = false;
        public bool notInPackage = false;

        public short ContentFSTID = 0;
    }
}
