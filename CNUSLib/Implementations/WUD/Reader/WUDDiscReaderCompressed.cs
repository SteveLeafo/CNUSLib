using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNUSLib
{
    public class WUDDiscReaderCompressed : WUDDiscReader
    {
        public WUDDiscReaderCompressed(WUDImage wudFile)
        {
        }

        public override void readEncryptedToOutputStream(System.IO.Stream outputStream, long offset, long size)
        {
            throw new NotImplementedException();
        }
    }
}
