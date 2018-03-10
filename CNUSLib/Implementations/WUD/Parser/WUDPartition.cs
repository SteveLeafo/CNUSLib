using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CNUSLib
{
    public class WUDPartition
    {
        public WUDPartitionHeader partitionHeader;

        public string partitionName;
        public long partitionOffset;

        public WUDPartition(string partitionName, long partitionOffset)
        {
            // TODO: Complete member initialization
            this.partitionName = partitionName;
            this.partitionOffset = partitionOffset;
        }
    }
}
