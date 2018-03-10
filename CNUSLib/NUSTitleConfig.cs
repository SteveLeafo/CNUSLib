using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNUSLib
{
    public class NUSTitleConfig
    {
        public String inputPath;
        public WUDInfo WUDInfo;
        public Ticket ticket;

        public int version = Settings.LATEST_TMD_VERSION;
        public long titleID = 0x0L;

        //private WoomyInfo woomyInfo;
    }
}
