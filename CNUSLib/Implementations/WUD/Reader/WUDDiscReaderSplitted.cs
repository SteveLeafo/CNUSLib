using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CNUSLib
{
    public class WUDDiscReaderSplitted : WUDDiscReader
    {
        public static String WUD_SPLITTED_DEFAULT_FILEPATTERN = "game_part%d.wud";
        public static long WUD_SPLITTED_FILE_SIZE = 0x100000L * 0x800L;
        public static long NUMBER_OF_FILES = 12;

        public WUDDiscReaderSplitted(WUDImage wudFile)
        {
        }

        public override void readEncryptedToOutputStream(System.IO.Stream outputStream, long offset, long size)
        {
            throw new NotImplementedException();
        }
    }
}
