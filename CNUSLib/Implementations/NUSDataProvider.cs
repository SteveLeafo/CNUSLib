using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WudTool
{
    internal abstract class NUSDataProvider
    {
        private NUSTitle NUSTitle;

        public NUSDataProvider(NUSTitle title)
        {
            this.NUSTitle = title;
        }

        /**
         * Saves the given content encrypted with his .h3 file in the given directory.
         * The Target directory will be created if it's missing.
         * If the content is not hashed, no .h3 will be saved
         * 
         * @param content
         *            Content that should be saved
         * @param outputFolder
         *            Target directory where the files will be stored in.
         * @throws IOException
         */
        public void saveEncryptedContentWithH3Hash(Content content, String outputFolder)
        {
            saveContentH3Hash(content, outputFolder);
            saveEncryptedContent(content, outputFolder);
        }

        /**
         * Saves the .h3 file of the given content into the given directory.
         * The Target directory will be created if it's missing.
         * If the content is not hashed, no .h3 will be saved
         * 
         * @param content
         *            The content of which the h3 hashes should be saved
         * @param outputFolder
         * @throws IOException
         */
        public void saveContentH3Hash(Content content, String outputFolder)
        {
            if (!content.isHashed())
            {
                return;
            }
            byte[] hash = getContentH3Hash(content);
            if (hash == null || hash.Length == 0)
            {
                return;
            }
            String h3Filename = String.Format("%08X%s", content.ID, Settings.H3_EXTENTION);
            FileInfo output = new FileInfo(outputFolder + Path.DirectorySeparatorChar + h3Filename);
            if (output.Exists && output.Length == hash.Length)
            {
                //MessageBox.Show(h3Filename + " already exists");
                return;
            }

            //MessageBox.Show("Saving " + h3Filename + " ");

            File.WriteAllBytes(output.FullName, hash);
            //FileUtils.saveByteArrayToFile(output, hash);
        }

        /**
         * Saves the given content encrypted in the given directory.
         * The Target directory will be created if it's missing.
         * If the content is not encrypted at all, it will be just saved anyway.
         * 
         * @param content
         *            Content that should be saved
         * @param outputFolder
         *            Target directory where the files will be stored in.
         * @throws IOException
         */
        public void saveEncryptedContent(Content content, String outputFolder)
        {
            Directory.CreateDirectory(outputFolder);
            MemoryStream inputStream = getInputStreamFromContent(content, 0);
            if (inputStream == null)
            {
                //MessageBox.Show("Couldn't save encrypted content. Input stream was null");
                return;
            }

            FileInfo output = new FileInfo(outputFolder + Path.DirectorySeparatorChar + content.getFilename());
            if (output.Exists)
            {
                if (output.Length == content.getEncryptedFileSizeAligned())
                {
                    //MessageBox.Show("Encrypted content alreadys exists, skipped");
                    return;
                }
                else
                {
                    //MessageBox.Show("Encrypted content alreadys exists, but the length is not as expected. Saving it again");
                }
            }

            File.WriteAllBytes(output.FullName, inputStream.ToArray());
            //FileUtils.saveInputStreamToFile(output, inputStream, content.getEncryptedFileSizeAligned());
        }

        /**
         * 
         * @param content
         * @param offset
         * @return
         * @throws IOException
         */
        public abstract MemoryStream getInputStreamFromContent(Content content, long offset);

        // TODO: JavaDocs
        public abstract byte[] getContentH3Hash(Content content);

        public abstract byte[] getRawTMD();

        public abstract byte[] getRawTicket();

        public abstract byte[] getRawCert();

        public abstract void cleanup();
    }
}
