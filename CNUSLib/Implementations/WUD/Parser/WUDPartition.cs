using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WudTool
{
    internal class WUDPartition
    {
        internal WUDPartitionHeader partitionHeader;

        internal string partitionName;
        internal long partitionOffset;

        public WUDPartition(string partitionName, long partitionOffset)
        {
            // TODO: Complete member initialization
            this.partitionName = partitionName;
            this.partitionOffset = partitionOffset;
        }
    }
}
