using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CNUSLib
{
    public abstract class NUSTitleLoader
    {
        protected NUSTitleLoader()
        {
            // should be empty
        }

        public NUSTitle loadNusTitle(NUSTitleConfig config)
        {
            NUSTitle result = new NUSTitle();

            NUSDataProvider dataProvider = getDataProvider(result, config);
            result.dataProvider = (dataProvider);

            TMD tmd = TMD.parseTMD(dataProvider.getRawTMD());
            result.TMD = (tmd);

            if (tmd == null)
            {
                //MessageBox.Show("TMD not found.");
                throw new Exception();
            }

            Ticket ticket = config.ticket;
            if (ticket == null)
            {
                ticket = Ticket.parseTicket(dataProvider.getRawTicket());
            }
            result.ticket = ticket;
            // System.out.println(ticket);

            Content fstContent = tmd.getContentByIndex(0);

            MemoryStream fstContentEncryptedStream = dataProvider.getInputStreamFromContent(fstContent, 0);
            if (fstContentEncryptedStream == null)
            {

                return null;
            }

            byte[] fstBytes = fstContentEncryptedStream.ToArray();// StreamUtils.getBytesFromStream(fstContentEncryptedStream, (int)fstContent.getEncryptedFileSize());

            if (fstContent.isEncrypted())
            {
                AESDecryption aesDecryption = new AESDecryption(ticket.decryptedKey, new byte[0x10]);
                fstBytes = aesDecryption.decrypt(fstBytes);
            }

            Dictionary<int, Content> contents = tmd.getAllContents();

            FST fst = FST.parseFST(fstBytes, contents);
            result.FST = (fst);

            return result;
        }

        protected abstract NUSDataProvider getDataProvider(NUSTitle title, NUSTitleConfig config);
    }
}
