using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WudTool
{
    internal class WUDInfo
    {
        internal byte[] titleKey;

        internal WUDDiscReader WUDDiscReader;
        internal Dictionary<string, WUDPartition> partitions = new Dictionary<string, WUDPartition>();

        internal string gamePartitionName;

        private WUDGamePartition cachedGamePartition = null;

        public WUDInfo(byte[] titleKey, WudTool.WUDDiscReader discReader)
        {
            // TODO: Complete member initialization
            this.titleKey = titleKey;
            this.WUDDiscReader = discReader;
        }

        public void addPartion(String partitionName, WUDGamePartition partition)
        {
            partitions.Add(partitionName, partition);
        }
        public WUDGamePartition getGamePartition()
        {
            if (cachedGamePartition == null)
            {
                cachedGamePartition = findGamePartition();
            }
            return cachedGamePartition;
        }

        private WUDGamePartition findGamePartition()
        {
            foreach (var e in partitions)
            {
                if (e.Key.Equals(gamePartitionName))
                {
                    return (WUDGamePartition)e.Value;
                }
            }
            return null;
        }

    }
}
