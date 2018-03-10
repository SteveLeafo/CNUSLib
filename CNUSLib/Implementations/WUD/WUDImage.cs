using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNUSLib
{
    public class WUDImage
    {
        public static long WUD_FILESIZE = 0x5D3A00000L;

        public FileInfo fileHandle;
        public WUDImageCompressedInfo compressedInfo = null;
        public FileStream fileStream;
        public bool isCompressed;
        public bool isSplitted;

        public long inputFileSize = 0L;
        public WUDDiscReader WUDDiscReader;

        public WUDImage(FileInfo file)
        {
            if (file == null || !file.Exists)
            {
                //MessageBox.Show("WUD file is null or does not exist");
                //System.exit(1);
            }

            fileStream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read);
            BinaryReader br1 = new BinaryReader(fileStream);
            br1.BaseStream.Seek(0, SeekOrigin.Begin);
            byte[] wuxheader = new byte[WUDImageCompressedInfo.WUX_HEADER_SIZE];
            int n = br1.Read(wuxheader, 0, WUDImageCompressedInfo.WUX_HEADER_SIZE);
            WUDImageCompressedInfo compressedInfo = new WUDImageCompressedInfo(wuxheader);


            if (compressedInfo.isWUX())
            {
                //MessageBox.Show("Image is compressed");
                this.isCompressed = true;
                this.isSplitted = false;
                Dictionary<int, long> indexTable = new Dictionary<int, long>();
                long offsetIndexTable = compressedInfo.offsetIndexTable;
                br1.BaseStream.Seek(offsetIndexTable, SeekOrigin.Begin);

                byte[] tableData = new byte[(int)(compressedInfo.indexTableEntryCount * 0x04)];
                br1.Read(tableData, 0, tableData.Length);
                int cur_offset = 0x00;
                for (long i = 0; i < compressedInfo.indexTableEntryCount; i++)
                {
                    indexTable[(int)i] = Utils.SwapEndianness(BitConverter.ToInt32(tableData, (int)cur_offset));
                    cur_offset += 0x04;
                }
                compressedInfo.indexTable = indexTable;
                //compressedInfo = compressedInfo;
            }
            else
            {
                this.isCompressed = false;
                if (file.Name.Equals(String.Format(WUDDiscReaderSplitted.WUD_SPLITTED_DEFAULT_FILEPATTERN, 1)) && (file.Length == WUDDiscReaderSplitted.WUD_SPLITTED_FILE_SIZE))
                {
                    this.isSplitted = true;
                    //MessageBox.Show("Image is splitted");
                }
                else
                {
                    //MessageBox.Show("Image is not splitted");
                    this.isSplitted = false;
                }
            }

            if (isCompressed)
            {
                this.WUDDiscReader = new WUDDiscReaderCompressed(this);
            }
            else if (isSplitted)
            {
                this.WUDDiscReader = new WUDDiscReaderSplitted(this);
            }
            else
            {
                this.WUDDiscReader = new WUDDiscReaderUncompressed(this);
            }

            //fileStream.close();
            this.fileHandle = file;
        }

        public long getWUDFileSize()
        {
            if (inputFileSize == 0)
            {
                if (isSplitted)
                {
                    inputFileSize = calculateSplittedFileSize();
                }
                else if (isCompressed)
                {
                    inputFileSize = compressedInfo.uncompressedSize;
                }
                else
                {
                    inputFileSize = fileHandle.Length;
                }
            }
            return inputFileSize;
        }

        private long calculateSplittedFileSize()
        {
            long result = 0;
            //File filehandlePart1 = getFileHandle();
            //String pathToFiles = filehandlePart1.getParentFile().getAbsolutePath();
            //for (int i = 1; i <= WUDDiscReaderSplitted.NUMBER_OF_FILES; i++)
            //{
            //    String filePartPath = pathToFiles + File.separator + String.format(WUDDiscReaderSplitted.WUD_SPLITTED_DEFAULT_FILEPATTERN, i);
            //    File part = new File(filePartPath);
            //    if (part.exists())
            //    {
            //        result += part.length();
            //    }
            //}
            return result;
        }
    }
}
