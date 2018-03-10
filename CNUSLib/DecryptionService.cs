using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNUSLib
{
    public class DecryptionService
    {

        private static Dictionary<NUSTitle, DecryptionService> instances = new Dictionary<NUSTitle, DecryptionService>();
        private NUSTitle NUSTitle;

        public static DecryptionService getInstance(NUSTitle nustitle)
        {
            if (!instances.ContainsKey(nustitle))
            {
                instances.Add(nustitle, new DecryptionService(nustitle));
            }
            return instances[nustitle];
        }

        private DecryptionService(NUSTitle nustitle)
        {
            this.NUSTitle = nustitle;
        }

        public Ticket getTicket()
        {
            return NUSTitle.ticket;
        }

        public void decryptFSTEntryTo(bool useFullPath, FSTEntry entry, String outputPath, bool skipExistingFile)
        {
            if (entry.isNotInPackage || entry.content == null)
            {
                return;
            }

            //MessageBox.Show("Decrypting " + entry.getFilename());

            string targetFilePath = new StringBuilder().Append(outputPath).Append("/").Append(entry.filename).ToString();
            string fullPath = new StringBuilder().Append(outputPath).ToString();

            if (useFullPath)
            {
                targetFilePath = new StringBuilder().Append(outputPath).Append(entry.getFullPath()).ToString();
                fullPath = new StringBuilder().Append(outputPath).Append(entry.path).ToString();
                if (entry.isDir)
                { // If the entry is a directory. Create it and return.
                    Directory.CreateDirectory(targetFilePath);
                    return;
                }
            }
            else if (entry.isDir)
            {
                return;
            }

            try
            {
                Directory.CreateDirectory(fullPath);
            }
            catch (Exception)
            {
                return;
            }

            FileInfo target = new FileInfo(targetFilePath);

            if (skipExistingFile)
            {
                FileInfo targetFile = new FileInfo(targetFilePath);
                if (targetFile.Exists)
                {
                    if (entry.isDir)
                    {
                        return;
                    }
                    if (targetFile.Length == entry.fileSize)
                    {
                        Content c = entry.content;
                        if (c.isHashed())
                        {
                            //MessageBox.Show("File already exists: " + entry.filename);
                            return;
                        }
                        else
                        {
                            //MessageBox.Show("File already exists");
                            //if (Arrays.Equals(HashUtil.hashSHA1(target, (int) c.getDecryptedFileSize()), c.SHA2Hash)) {
                            //    MessageBox.Show("File already exists: " + entry.filename);
                            //    return;
                            //} else {
                            //    MessageBox.Show("File already exists with the same filesize, but the hash doesn't match: " + entry.getFilename());
                            //}
                        }

                    }
                    else
                    {
                        //MessageBox.Show("File already exists but the filesize doesn't match: " + entry.filename);
                    }
                }
            }

            FileStream sr = new FileStream(targetFilePath, FileMode.Create);
            BinaryWriter outputStream = new BinaryWriter(sr);
            try
            {
                decryptFSTEntryToStream(entry, outputStream);
            }
            catch (Exception e)
            {
                if (entry.filename.EndsWith(".xml") && Utils.checkXML(new FileInfo(targetFilePath)))
                {
                    //MessageBox.Show("Hash doesn't match, but it's an XML file and it looks okay.");
                }
                else
                {
                    //MessageBox.Show("Hash doesn't match!");
                    throw e;
                }
            }
        }

        public void decryptFSTEntryToStream(FSTEntry entry, BinaryWriter outputStream)
        {
            if (entry.isNotInPackage || entry.content == null)
            {
                return;
            }

            Content c = entry.content;

            long fileSize = entry.fileSize;
            long fileOffset = entry.fileOffset;
            long fileOffsetBlock = entry.getFileOffsetBlock();

            NUSDataProvider dataProvider = NUSTitle.dataProvider;

            MemoryStream min = dataProvider.getInputStreamFromContent(c, fileOffsetBlock);

            try
            {
                decryptFSTEntryFromStreams(min, outputStream, fileSize, fileOffset, c);
            }
            catch (Exception e)
            {
                //MessageBox.Show("Hash doesn't match");
                //if (entry.getFilename().endsWith(".xml")) {
                //    if (outputStream instanceof PipedOutputStream) {
                //        MessageBox.Show("Hash doesn't match. Please check the data for " + entry.getFullPath());
                //    } else {
                //        throw e;
                //    }
                //} else if (entry.getContent().isUNKNWNFlag1Set()) {
                //    MessageBox.Show("But file is optional. Don't worry.");
                //} else {
                //    StringBuilder sb = new StringBuilder();
                //    sb.append("Detailed info:").append(System.lineSeparator());
                //    sb.append(entry).append(System.lineSeparator());
                //    sb.append(entry.getContent()).append(System.lineSeparator());
                //    sb.append(String.format("%016x", this.NUSTitle.getTMD().getTitleID()));
                //    sb.append(e.getMessage() + " Calculated Hash: " + Utils.ByteArrayToString(e.getGivenHash()) + ", expected hash: "
                //            + Utils.ByteArrayToString(e.getExpectedHash()));
                //    MessageBox.Show(sb.toString());
                //    throw e;
                //}
            }
        }

        private void decryptFSTEntryFromStreams(MemoryStream inputStream, BinaryWriter outputStream, long filesize, long fileoffset, Content content)
        {
            decryptStreams(inputStream, outputStream, filesize, fileoffset, content);
        }

        private void decryptContentFromStream(MemoryStream inputStream, BinaryWriter outputStream, Content content)
        {
            long filesize = content.getDecryptedFileSize();
            //MessageBox.Show("Decrypting Content " + String.Format(content.ID.ToString("X8")));
            decryptStreams(inputStream, outputStream, filesize, 0L, content);
        }

        private void decryptStreams(MemoryStream inputStream, BinaryWriter outputStream, long size, long offset, Content content)
        {
            NUSDecryption nusdecryption = new NUSDecryption(getTicket());
            short contentIndex = (short)content.index;

            long encryptedFileSize = content.encryptedFileSize;

            if (content.isEncrypted())
            {
                if (content.isHashed())
                {
                    NUSDataProvider dataProvider = NUSTitle.dataProvider;
                    byte[] h3 = dataProvider.getContentH3Hash(content);
                    nusdecryption.decryptFileStreamHashed(inputStream, outputStream, size, offset, (short)contentIndex, h3);
                }
                else
                {
                    nusdecryption.decryptFileStream(inputStream, outputStream, size, (short)contentIndex, content.SHA2Hash, encryptedFileSize);
                }
            }
            else
            {
                StreamUtils.saveInputStreamToOutputStreamWithHash(inputStream, outputStream, size, content.SHA2Hash, encryptedFileSize);
            }

            inputStream.Close();
            outputStream.Close();
        }

       
        public void decryptContentTo(Content content, String outPath, bool skipExistingFile)
        {
            String targetFilePath = outPath + Path.DirectorySeparatorChar + content.getFilenameDecrypted();
            if (skipExistingFile)
            {
                FileInfo targetFile = new FileInfo(targetFilePath);
                if (targetFile.Exists)
                {
                    if (targetFile.Length == content.getDecryptedFileSize())
                    {
                        //MessageBox.Show("File already exists : " + content.getFilenameDecrypted());
                        return;
                    }
                    else
                    {
                        //MessageBox.Show("File already exists but the filesize doesn't match: " + content.getFilenameDecrypted());
                    }
                }
            }

            try
            {
                Directory.CreateDirectory(outPath);
            }
            catch (Exception)
            {
                return;
            }

            //MessageBox.Show("Decrypting Content " + content.ID.ToString("X8"));

            //FileOutputStream outputStream = new FileOutputStream(new File(targetFilePath));
            FileStream sr = new FileStream(targetFilePath, FileMode.Create);
            BinaryWriter outputStream = new BinaryWriter(sr);


            decryptContentToStream(content, outputStream);
        }

        public void decryptContentToStream(Content content, BinaryWriter outputStream)
        {
            if (content == null)
            {
                return;
            }

            NUSDataProvider dataProvider = NUSTitle.dataProvider;
            MemoryStream inputStream = dataProvider.getInputStreamFromContent(content, 0);

            decryptContentFromStream(inputStream, outputStream, content);
        }

        //public PipedInputStreamWithException getDecryptedOutputAsInputStream(FSTEntry fstEntry) 
        //{
        //    PipedInputStreamWithException in = new PipedInputStreamWithException();
        //    PipedOutputStream out = new PipedOutputStream(in);

        //    new Thread(() -> {
        //        try { // Throwing it in both cases is EXTREMLY important. Otherwise it'll end in a deadlock
        //            decryptFSTEntryToStream(fstEntry, out);
        //            in.throwException(null);
        //        } catch (Exception e) {
        //            in.throwException(e);
        //        }

        //    }).start();

        //    return in;
        //}

        //    public PipedInputStreamWithException getDecryptedContentAsInputStream(Content content) 
        //{
        //        PipedInputStreamWithException in = new PipedInputStreamWithException();
        //        PipedOutputStream out = new PipedOutputStream(in);

        //        new Thread(() -> {
        //            try {// Throwing it in both cases is EXTREMLY important. Otherwise it'll end in a deadlock
        //                decryptContentToStream(content, out);
        //                in.throwException(null);
        //            } catch (Exception e) {
        //                in.throwException(e);
        //            }
        //        }).start();

        //        return in;
        //    }

        // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        // Decrypt FSTEntry to OutputStream
        // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        public void decryptFSTEntryTo(String entryFullPath, BinaryWriter outputStream)
        {
            FSTEntry entry = NUSTitle.getFSTEntryByFullPath(entryFullPath);
            if (entry == null)
            {
                //MessageBox.Show("File not found");
            }

            decryptFSTEntryToStream(entry, outputStream);
        }

        // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        // Decrypt single FSTEntry to File
        // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        public void decryptFSTEntryTo(String entryFullPath, String outputFolder)
        {
            decryptFSTEntryTo(false, entryFullPath, outputFolder);
        }

        public void decryptFSTEntryTo(bool fullPath, String entryFullPath, String outputFolder)
        {
            decryptFSTEntryTo(fullPath, entryFullPath, outputFolder, NUSTitle.skipExistingFiles);
        }

        public void decryptFSTEntryTo(String entryFullPath, String outputFolder, bool skipExistingFiles)
        {
            decryptFSTEntryTo(false, entryFullPath, outputFolder, NUSTitle.skipExistingFiles);
        }

        public void decryptFSTEntryTo(bool fullPath, String entryFullPath, String outputFolder, bool skipExistingFiles)
        {
            FSTEntry entry = NUSTitle.getFSTEntryByFullPath(entryFullPath);
            if (entry == null)
            {
                //MessageBox.Show("File not found");
                return;
            }

            decryptFSTEntryTo(fullPath, entry, outputFolder, skipExistingFiles);
        }

        public void decryptFSTEntryTo(FSTEntry entry, String outputFolder)
        {
            decryptFSTEntryTo(false, entry, outputFolder);
        }

        public void decryptFSTEntryTo(bool fullPath, FSTEntry entry, String outputFolder)
        {
            decryptFSTEntryTo(fullPath, entry, outputFolder, NUSTitle.skipExistingFiles);
        }

        public void decryptFSTEntryTo(FSTEntry entry, String outputFolder, bool skipExistingFiles)
        {
            decryptFSTEntryTo(false, entry, outputFolder, NUSTitle.skipExistingFiles);
        }

        /*
         * public void decryptFSTEntryTo(bool fullPath, FSTEntry entry,String outputFolder, bool skipExistingFiles) {
         * decryptFSTEntry(fullPath,entry,outputFolder,skipExistingFiles); }
         */

        // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        // Decrypt list of FSTEntry to Files
        // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        public void decryptAllFSTEntriesTo(String outputFolder)
        {
            Directory.CreateDirectory(outputFolder + Path.DirectorySeparatorChar + "code");
            Directory.CreateDirectory(outputFolder + Path.DirectorySeparatorChar + "content");
            Directory.CreateDirectory(outputFolder + Path.DirectorySeparatorChar + "meta");
            decryptFSTEntriesTo(true, ".*", outputFolder);
        }

        public void decryptFSTEntriesTo(String regEx, String outputFolder)
        {
            decryptFSTEntriesTo(true, regEx, outputFolder);
        }

        public void decryptFSTEntriesTo(bool fullPath, String regEx, String outputFolder)
        {
            decryptFSTEntryListTo(fullPath, NUSTitle.getFSTEntriesByRegEx(regEx), outputFolder);
        }

        public void decryptFSTEntryListTo(List<FSTEntry> list, String outputFolder)
        {
            decryptFSTEntryListTo(true, list, outputFolder);
        }

        public void decryptFSTEntryListTo(bool fullPath, List<FSTEntry> list, String outputFolder)
        {
            foreach (FSTEntry entry in list)
            {
                decryptFSTEntryTo(fullPath, entry, outputFolder, NUSTitle.skipExistingFiles);
            }
        }

        // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        // Save decrypted contents
        // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        public void decryptPlainContentByID(int ID, String outputFolder)
        {
            decryptPlainContent(getTMDFromNUSTitle().getContentByID(ID), outputFolder);
        }

        public void decryptPlainContentByIndex(int index, String outputFolder)
        {
            decryptPlainContent(getTMDFromNUSTitle().getContentByIndex(index), outputFolder);
        }

        public void decryptPlainContent(Content c, String outputFolder)
        {
            decryptPlainContents(new List<Content>(new Content[] { c }), outputFolder);
        }

        public void decryptPlainContents(List<Content> list, String outputFolder)
        {
            foreach (Content c in list)
            {
                decryptContentTo(c, outputFolder, NUSTitle.skipExistingFiles);
            }
        }

        public void decryptAllPlainContents(String outputFolder)
        {
            decryptPlainContents(new List<Content>(getTMDFromNUSTitle().getAllContents().Values.ToArray()), outputFolder);
        }

        // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        // Other
        // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        private TMD getTMDFromNUSTitle()
        {
            return NUSTitle.TMD;
        }

    }
}
