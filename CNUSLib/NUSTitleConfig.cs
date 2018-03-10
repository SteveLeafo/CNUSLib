using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WudTool
{
    internal class NUSTitleConfig
    {
        internal String inputPath;
        internal WUDInfo WUDInfo;
        internal Ticket ticket;

        internal int version = Settings.LATEST_TMD_VERSION;
        internal long titleID = 0x0L;

        //private WoomyInfo woomyInfo;
    }
}
