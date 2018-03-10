using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WudTool
{
    internal class WUDDiscReaderCompressed : WUDDiscReader
    {
        internal WUDDiscReaderCompressed(WUDImage wudFile)
        {
        }

        public override void readEncryptedToOutputStream(System.IO.Stream outputStream, long offset, long size)
        {
            throw new NotImplementedException();
        }
    }
}
