using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WudTool
{
    internal class FSTEntryParam
    {
        internal String Filename = "";
        internal String Path = "";

        internal FSTEntry Parent = null;

        internal short Flags;

        internal long FileSize = 0;
        internal long FileOffset = 0;

        internal Content Content = null;

        internal bool isDir = false;
        internal bool isRoot = false;
        internal bool notInPackage = false;

        internal short ContentFSTID = 0;
    }
}
