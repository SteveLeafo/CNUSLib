using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WudTool
{
    internal class WUDInfoParser
    {
        public static byte[] DECRYPTED_AREA_SIGNATURE = new byte[] { (byte)0xCC, (byte)0xA6, (byte)0xE6, 0x7B };
        public static byte[] PARTITION_FILE_TABLE_SIGNATURE = new byte[] { 0x46, 0x53, 0x54, 0x00 }; // "FST"
        public static int PARTITION_TOC_OFFSET = 0x800;
        public static int PARTITION_TOC_ENTRY_SIZE = 0x80;

        public static String WUD_TMD_FILENAME = "title.tmd";
        public static String WUD_TICKET_FILENAME = "title.tik";
        public static String WUD_CERT_FILENAME = "title.cert";

        public static WUDInfo createAndLoad(WUDDiscReader discReader, byte[] titleKey)
        {
            WUDInfo result = new WUDInfo(titleKey, discReader);

            byte[] PartitionTocBlock = discReader.readDecryptedToByteArray(Settings.WIIU_DECRYPTED_AREA_OFFSET, 0, 0x8000, titleKey, null);

            // verify DiscKey before proceeding
            byte[] copy = new byte[4];

            Array.ConstrainedCopy(PartitionTocBlock, 0, copy, 0, 4);
            if (!ArraysEqual(copy, DECRYPTED_AREA_SIGNATURE))
            {
                //MessageBox.Show("Decryption of PartitionTocBlock failed");
                return null;
            }

            Dictionary<string, WUDPartition> partitions = readPartitions(result, PartitionTocBlock);
            result.partitions.Clear();
            foreach (var v in partitions)
            {
                result.partitions.Add(v.Key, v.Value);
            }

            return result;
        }

        private static bool ArraysEqual(byte[] copy, byte[] DECRYPTED_AREA_SIGNATURE)
        {
            if (copy.Length == DECRYPTED_AREA_SIGNATURE.Length)
            {
                for (int i = 0; i < copy.Length; ++i)
                {
                    if (copy[i] != DECRYPTED_AREA_SIGNATURE[i])
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        private static Dictionary<string, WUDPartition> readPartitions(WUDInfo wudInfo, byte[] partitionTocBlock)
        {
            byte[] buffer = new byte[partitionTocBlock.Length];

            int partitionCount = (int)SwapEndianness(BitConverter.ToUInt32(partitionTocBlock, 0x1C));

            Dictionary<string, WUDPartition> partitions = new Dictionary<string, WUDPartition>();

            byte[] gamePartitionTMD = new byte[0];
            byte[] gamePartitionTicket = new byte[0];
            byte[] gamePartitionCert = new byte[0];

            String realGamePartitionName = "";
            // populate partition information from decrypted TOC
            for (int i = 0; i < partitionCount; i++)
            {

                int offset = (PARTITION_TOC_OFFSET + (i * PARTITION_TOC_ENTRY_SIZE));
                byte[] partitionIdentifier = new byte[0x19];
                Array.ConstrainedCopy(partitionTocBlock, offset, partitionIdentifier, 0, 0x19);

                int j = 0;
                for (j = 0; j < partitionIdentifier.Length; j++)
                {
                    if (partitionIdentifier[j] == 0)
                    {
                        break;
                    }
                }

                byte[] partitionNameArray = new byte[j];
                Array.ConstrainedCopy(partitionIdentifier, 0, partitionNameArray, 0, j);
                String partitionName = Encoding.ASCII.GetString(partitionNameArray);

                // calculate partition offset (relative from WIIU_DECRYPTED_AREA_OFFSET) from decrypted TOC
                long tmp = SwapEndianness(BitConverter.ToUInt32(partitionTocBlock, (PARTITION_TOC_OFFSET + (i * PARTITION_TOC_ENTRY_SIZE) + 0x20)));

                long partitionOffset = ((tmp * (long)0x8000) - 0x10000);

                WUDPartition partition = new WUDPartition(partitionName, partitionOffset);

                if (partitionName.StartsWith("SI"))
                {
                    byte[] fileTableBlock = wudInfo.WUDDiscReader.readDecryptedToByteArray(Settings.WIIU_DECRYPTED_AREA_OFFSET + partitionOffset, 0, 0x8000,
                            wudInfo.titleKey, null);

                    byte[] copy = new byte[4];

                    Array.ConstrainedCopy(fileTableBlock, 0, copy, 0, 4);
                    if (!ArraysEqual(copy, PARTITION_FILE_TABLE_SIGNATURE))
                    {
                        //MessageBox.Show("FST Decrpytion failed");
                        continue;
                    }

                    //if (!Arrays.equals(Arrays.copyOfRange(fileTableBlock, 0, 4), PARTITION_FILE_TABLE_SIGNATURE))
                    //{
                    //    log.info("FST Decrpytion failed");
                    //    continue;
                    //}

                    FST fst = FST.parseFST(fileTableBlock, null);

                    byte[] rawTIK = getFSTEntryAsByte(WUD_TICKET_FILENAME, partition, fst, wudInfo.WUDDiscReader, wudInfo.titleKey);
                    byte[] rawTMD = getFSTEntryAsByte(WUD_TMD_FILENAME, partition, fst, wudInfo.WUDDiscReader, wudInfo.titleKey);
                    byte[] rawCert = getFSTEntryAsByte(WUD_CERT_FILENAME, partition, fst, wudInfo.WUDDiscReader, wudInfo.titleKey);

                    gamePartitionTMD = rawTMD;
                    gamePartitionTicket = rawTIK;
                    gamePartitionCert = rawCert;

                    // We want to use the real game partition
                    realGamePartitionName = partitionName = "GM" + Utils.ByteArrayToString(Arrays.copyOfRange(rawTIK, 0x1DC, 0x1DC + 0x08));
                }
                else if (partitionName.StartsWith(realGamePartitionName))
                {
                    wudInfo.gamePartitionName = partitionName;
                    partition = new WUDGamePartition(partitionName, partitionOffset, gamePartitionTMD, gamePartitionCert, gamePartitionTicket);
                }
                byte[] header = wudInfo.WUDDiscReader.readEncryptedToByteArray(partition.partitionOffset + 0x10000, 0, 0x8000);
                WUDPartitionHeader partitionHeader = WUDPartitionHeader.parseHeader(header);
                partition.partitionHeader = (partitionHeader);

                partitions.Add(partitionName, partition);
            }

            return partitions;
        }

        private static byte[] getFSTEntryAsByte(String filename, WUDPartition partition, FST fst, WUDDiscReader discReader, byte[] key)
        {
            FSTEntry entry = getEntryByName(fst.root, filename);
            ContentFSTInfo info = fst.contentFSTInfos[((int)entry.contentFSTID)];

            // Calculating the IV
            ByteBuffer byteBuffer = ByteBuffer.allocate(0x10);
            byteBuffer.position(0x08);
            long l = entry.fileOffset >> 16;
            byte[] ar = BitConverter.GetBytes(l);
            byte[] IV = new byte[0x10];//= copybyteBuffer.putLong(entry.fileOffset >> 16).ToArray();
            Array.ConstrainedCopy(ar, 0, IV, 0x08, 0x08);

            return discReader.readDecryptedToByteArray(Settings.WIIU_DECRYPTED_AREA_OFFSET + (long)partition.partitionOffset + (long)info.getOffset(),
                    entry.fileOffset, (int)entry.fileSize, key, IV);
            return null;
        }

        private static FSTEntry getEntryByName(FSTEntry root, String name)
        {
            foreach (FSTEntry cur in root.getFileChildren())
            {
                if (cur.filename.Equals(name))
                {
                    return cur;
                }
            }
            foreach (FSTEntry cur in root.getDirChildren())
            {
                FSTEntry dir_result = getEntryByName(cur, name);
                if (dir_result != null)
                {
                    return dir_result;
                }
            }
            return null;
        }
        public static uint SwapEndianness(uint value)
        {
            var b1 = (value >> 0) & 0xff;
            var b2 = (value >> 8) & 0xff;
            var b3 = (value >> 16) & 0xff;
            var b4 = (value >> 24) & 0xff;

            return b1 << 24 | b2 << 16 | b3 << 8 | b4 << 0;
            //return value;
        }
    }
}
