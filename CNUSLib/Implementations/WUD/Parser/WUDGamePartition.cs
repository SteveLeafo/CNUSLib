using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WudTool
{
    class WUDGamePartition : WUDPartition
    {
        internal byte[] rawTMD;
        internal byte[] rawCert;
        internal byte[] rawTicket;

        public WUDGamePartition(String partitionName, long partitionOffset, byte[] rawTMD, byte[] rawCert, byte[] rawTicket) : base(partitionName, partitionOffset)
        {
            this.rawTMD = rawTMD;
            this.rawCert = rawCert;
            this.rawTicket = rawTicket;
        }
    }
}
