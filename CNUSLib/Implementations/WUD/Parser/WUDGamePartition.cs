using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CNUSLib
{
    public class WUDGamePartition : WUDPartition
    {
        public byte[] rawTMD;
        public byte[] rawCert;
        public byte[] rawTicket;

        public WUDGamePartition(String partitionName, long partitionOffset, byte[] rawTMD, byte[] rawCert, byte[] rawTicket) : base(partitionName, partitionOffset)
        {
            this.rawTMD = rawTMD;
            this.rawCert = rawCert;
            this.rawTicket = rawTicket;
        }
    }
}
