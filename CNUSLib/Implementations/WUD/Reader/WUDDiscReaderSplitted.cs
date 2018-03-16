using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CNUSLib
{
    public class WUDDiscReaderSplitted : WUDDiscReader
    {
        public static long WUD_SPLITTED_FILE_SIZE = 0x100000L * 0x800L;
        public static long NUMBER_OF_FILES = 12;
        public static String WUD_SPLITTED_DEFAULT_FILEPATTERN = "game_part{0}.wud";

        public WUDDiscReaderSplitted(WUDImage image)
            : base(image)
        {
        }

        public override void readEncryptedToOutputStream(Stream outputStream, long offset, long size)
        {
            BinaryReader input = getFileByOffset(offset);

            int bufferSize = 0x8000;
            byte[] buffer = new byte[bufferSize];
            long totalread = 0;
            long curOffset = offset;

            int part = getFilePartByOffset(offset);
            long offsetInFile = getOffsetInFilePart(part, curOffset);

            do
            {
                offsetInFile = getOffsetInFilePart(part, curOffset);
                int curReadSize = bufferSize;
                if ((offsetInFile + bufferSize) >= WUD_SPLITTED_FILE_SIZE)
                { // Will we read above the part?
                    long toRead = WUD_SPLITTED_FILE_SIZE - offsetInFile;
                    if (toRead == 0)
                    { // just load the new file
                        input.Close();
                        input = getFileByOffset(curOffset);
                        part++;
                        offsetInFile = getOffsetInFilePart(part, curOffset);
                    }
                    else
                    {
                        curReadSize = (int)toRead; // And first only read until the part ends
                    }
                }

                int read = input.Read(buffer, 0, curReadSize);
                if (read < 0) break;
                if (totalread + read > size)
                {
                    read = (int)(size - totalread);
                }
                try
                {
                    outputStream.Write(buffer, 0, read);
                }
                catch (IOException)
                {
                    //if (e.getMessage().equals("Pipe closed")) {
                    //    break;
                    //} else {
                    //    input.close();
                    //    throw e;
                    //}
                }
                totalread += read;
                curOffset += read;
            } while (totalread < size);

            input.Close();
            //outputStream.Close();
        }

        private int getFilePartByOffset(long offset)
        {
            return (int)(offset / WUD_SPLITTED_FILE_SIZE) + 1;
        }

        private long getOffsetInFilePart(int part, long offset)
        {
            return offset - ((long)(part - 1) * WUD_SPLITTED_FILE_SIZE);
        }

        private BinaryReader getFileByOffset(long offset)
        {
            FileInfo filehandlePart1 = image.fileHandle;
            String pathToFiles = Path.GetDirectoryName(filehandlePart1.FullName);

            int filePart = getFilePartByOffset(offset);

            String filePartPath = pathToFiles + Path.DirectorySeparatorChar + String.Format(WUD_SPLITTED_DEFAULT_FILEPATTERN, filePart);

            FileInfo part = new FileInfo(filePartPath);

            if (!part.Exists)
            {
                //log.info("File does not exist");
                return null;
            }
            FileStream fileStream = new FileStream(part.FullName, FileMode.Open);
            BinaryReader result = new BinaryReader(fileStream);
            result.BaseStream.Seek(getOffsetInFilePart(filePart, offset), SeekOrigin.Begin);
            return result;
        }
    }
}
