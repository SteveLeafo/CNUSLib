using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CNUSLib
{
    class WUDDiscReaderUncompressed : WUDDiscReader
    {
        WUDImage image;
        public WUDDiscReaderUncompressed(WUDImage wudFile)
        {
            image = wudFile;
        }

        public override void readEncryptedToOutputStream(Stream outputStream, long offset, long size)
        {
            FileStream input = image.fileStream;

            //StreamUtils.skipExactly(input, offset);
            //input.Seek(offset, SeekOrigin.Begin);
            input.Seek(offset, SeekOrigin.Begin);

            int bufferSize = 0x8000;
            byte[] buffer = new byte[bufferSize];
            long totalread = 0;
            do
            {
                int read = input.Read(buffer, 0, bufferSize);
                if (read < 0) break;
                if (totalread + read > size)
                {
                    read = (int)(size - totalread);
                }
                try
                {
                    outputStream.Write(buffer, 0, read);
                }
                catch (IOException e)
                {
                    //if (e.getMessage().equals("Pipe closed")) 
                    //{
                    //    break;
                    //} else 
                    //{
                    //    input.close();
                    //    throw e;
                    //}
                }
                totalread += read;
            } while (totalread < size);
            //input.close();
            //outputStream.close();
        }
    }
}
