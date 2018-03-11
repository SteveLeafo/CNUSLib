using System;
using System.IO;
using System.Security.Cryptography;

namespace CNUSLib
{
    public abstract class WUDDiscReader
    {
        protected WUDImage image;

        public WUDDiscReader(WUDImage image)
        {
            this.image = image;
        }

        public void readDecryptedToOutputStream(Stream outputStream, long clusterOffset, long fileOffset, long size, byte[] key, byte[] IV)
        {
            byte[] usedIV = IV;
            if (usedIV == null)
            {
                usedIV = new byte[0x10];
            }
            long usedSize = size;
            long usedFileOffset = fileOffset;
            byte[] buffer;

            long maxCopySize;
            long copySize;

            long readOffset;

            int blockSize = 0x8000;
            long totalread = 0;

            do
            {
                long blockNumber = (usedFileOffset / blockSize);
                long blockOffset = (usedFileOffset % blockSize);

                readOffset = clusterOffset + (blockNumber * blockSize);
                // (long)WiiUDisc.WIIU_DECRYPTED_AREA_OFFSET + volumeOffset + clusterOffset + (blockStructure.getBlockNumber() * 0x8000);

                buffer = readDecryptedChunk(readOffset, key, usedIV);
                maxCopySize = 0x8000 - blockOffset;
                copySize = (usedSize > maxCopySize) ? maxCopySize : usedSize;

                outputStream.Write(buffer, (int)blockOffset, (int)buffer.Length);
                totalread += copySize;

                // update counters
                usedSize -= copySize;
                usedFileOffset += copySize;
            } while (totalread < usedSize);

            //outputStream.close();
        }

        public byte[] readEncryptedToByteArray(long offset, long fileoffset, long size, long shmooker = 0)
        {
            MemoryStream outStream = new MemoryStream();
            readEncryptedToOutputStream(outStream, offset, size);
            return outStream.ToArray();
        }

        public byte[] readDecryptedToByteArray(long offset, long fileoffset, long size, byte[] key, byte[] iv)
        {
            MemoryStream outStream = new MemoryStream();

            readDecryptedToOutputStream(outStream, offset, fileoffset, size, key, iv);
            return outStream.ToArray();
        }

        public byte[] readDecryptedChunk(long readOffset, byte[] key, byte[] IV)
        {
            int chunkSize = 0x8000;

            byte[] encryptedChunk = readEncryptedToByteArray(readOffset, 0, chunkSize);
            byte[] decryptedChunk = new byte[chunkSize];

            AESDecryption aesDecryption = new AESDecryption(key, IV);
            decryptedChunk = aesDecryption.decrypt(encryptedChunk);

            return decryptedChunk;
        }

        public abstract void readEncryptedToOutputStream(Stream outputStream, long offset, long size);

        public MemoryStream readEncryptedToInputStream(long offset, long size)
        {
            MemoryStream ms = new MemoryStream();
            readEncryptedToOutputStream(ms, offset, size);
            return ms;
            //PipedInputStream in = new PipedInputStream();
            //PipedOutputStream out = new PipedOutputStream(in);

            //new Thread(() -> {
            //    try {
            //    readEncryptedToOutputStream(out, offset, size);
            //} catch (IOException e) {
            //    e.printStackTrace();
            //}
            //},"readEncryptedToInputStream@" + this.hashCode()).start();

            //return in;
        }

        public BinaryReader getRandomAccessFileStream()
        {
            if (image == null || image.fileHandle == null)
            {
                //log.warning("No image or image filehandle set.");
                //System.exit(1); // TODO: NOOOOOOOOOOOOO/
            }
            FileStream fs = new FileStream(image.fileHandle.FullName, FileMode.Open);
            return new BinaryReader(fs);
        }
    }
}
