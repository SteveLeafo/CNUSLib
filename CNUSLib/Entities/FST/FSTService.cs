using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WudTool
{
    public class FSTService
    {
        private FSTService()
        {
        }

        internal static void parseFST(FSTEntry rootEntry, byte[] fstSection, byte[] namesSection, Dictionary<int, Content> contentsByIndex,
                Dictionary<int, ContentFSTInfo> contentsFSTByIndex)
        {
            int totalEntries = Utils.SwapEndianness(BitConverter.ToInt32(fstSection, 0x08));

            int level = 0;
            int[] LEntry = new int[16];
            int[] Entry = new int[16];
            String[] pathStrings = new String[16];
            for (int i = 0; i < 16; i++)
            {
                pathStrings[i] = "";
            }

            Dictionary<int, FSTEntry> fstEntryToOffsetMap = new Dictionary<int, FSTEntry>();
            Entry[level] = 0;
            LEntry[level++] = 0;

            fstEntryToOffsetMap.Add(0, rootEntry);

            int lastlevel = level;
            String path = "\\";

            FSTEntry last = null;
            for (int i = 1; i < totalEntries; i++)
            {

                int entryOffset = i;
                if (level > 0)
                {
                    while (LEntry[level - 1] == i)
                    {
                        level--;
                    }
                }

                byte[] curEntry = Arrays.copyOfRange(fstSection, i * 0x10, (i + 1) * 0x10);

                FSTEntryParam entryParam = new FSTEntryParam();

                if (lastlevel != level)
                {
                    path = pathStrings[level] + getFullPath(level - 1, level, fstSection, namesSection, Entry);
                    lastlevel = level;
                }

                String filename = getName(curEntry, namesSection);

                long fileOffset = Utils.SwapEndianness(BitConverter.ToInt32(curEntry, 0x04));
                long fileSize = (uint)Utils.SwapEndianness(BitConverter.ToInt32(curEntry, 0x08));

                short flags = Utils.SwapEndianness(BitConverter.ToInt16(curEntry, 0x0C));
                short contentIndex = Utils.SwapEndianness(BitConverter.ToInt16(curEntry, 0x0E));

                if ((curEntry[0] & FSTEntry.FSTEntry_notInNUS) == FSTEntry.FSTEntry_notInNUS)
                {
                    entryParam.notInPackage = (true);
                }
                FSTEntry parent = null;
                if ((curEntry[0] & FSTEntry.FSTEntry_DIR) == FSTEntry.FSTEntry_DIR)
                {
                    entryParam.isDir = (true);
                    int parentOffset = (int)fileOffset;
                    int nextOffset = (int)fileSize;

                    parent = fstEntryToOffsetMap[parentOffset];
                    Entry[level] = i;
                    LEntry[level++] = nextOffset;
                    pathStrings[level] = path;

                    if (level > 15)
                    {
                        //MessageBox.Show("level > 15");
                        break;
                    }
                }
                else
                {
                    entryParam.FileOffset = (fileOffset << 5);

                    entryParam.FileSize = (fileSize);
                    parent = fstEntryToOffsetMap[(Entry[level - 1])];
                }

                entryParam.Flags = (flags);
                entryParam.Filename = (filename);
                entryParam.Path = (path);

                if (contentsByIndex != null)
                {
                    Content content = contentsByIndex[contentIndex];
                    if (content == null)
                    {
                        //MessageBox.Show("Content for FST Entry not found");
                    }
                    else
                    {

                        if (content.isHashed() && (content.getDecryptedFileSize() < (fileOffset << 5)))
                        { // TODO: Figure out how this works...
                            entryParam.FileOffset = (fileOffset);
                        }

                        entryParam.Content = (content);

                        ContentFSTInfo contentFSTInfo = contentsFSTByIndex[(int)contentIndex];
                        if (contentFSTInfo == null)
                        {
                            //MessageBox.Show("ContentFSTInfo for FST Entry not found");
                        }
                        else
                        {
                            content.contentFSTInfo = (contentFSTInfo);
                        }
                    }
                }

                entryParam.ContentFSTID = (contentIndex);
                entryParam.Parent = (parent);

                FSTEntry entry = new FSTEntry(entryParam);
                last = entry;
                fstEntryToOffsetMap.Add(entryOffset, entry);
            }

        }

        private static int getNameOffset(byte[] curEntry)
        {
            // Its a 24bit number. We overwrite the first byte, then we can read it as an Integer.
            // But at first we make a copy.
            byte[] entryData = Arrays.copyOf(curEntry, curEntry.Length);
            entryData[0] = 0;
            return Utils.SwapEndianness(BitConverter.ToInt32(entryData, 0));
        }

        public static String getName(byte[] data, byte[] namesSection)
        {
            int nameOffset = getNameOffset(data);
            int j = 0;

            while ((nameOffset + j) < namesSection.Length && namesSection[nameOffset + j] != 0)
            {
                j++;
            }

            return System.Text.Encoding.UTF8.GetString((Arrays.copyOfRange(namesSection, nameOffset, nameOffset + j)));
        }

        public static String getFullPath(int startlevel, int endlevel, byte[] fstSection, byte[] namesSection, int[] Entry)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = startlevel; i < endlevel; i++)
            {
                int entryOffset = Entry[i] * 0x10;
                byte[] entryData = Arrays.copyOfRange(fstSection, entryOffset, entryOffset + 10);
                String entryName = getName(entryData, namesSection);

                sb.Append(entryName).Append(Path.DirectorySeparatorChar);
            }
            return sb.ToString();
        }
    }


}
