using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;


namespace WudTool
{
    class NUSDecryption : AESDecryption
    {
        public NUSDecryption(byte[] AESKey, byte[] IV)
            : base(AESKey, IV)
        {

        }

        public NUSDecryption(Ticket ticket)
            : this(ticket.decryptedKey, ticket.IV)
        {

        }

        private byte[] decryptFileChunk(byte[] blockBuffer, int BLOCKSIZE, byte[] IV)
        {
            return decryptFileChunk(blockBuffer, 0, BLOCKSIZE, IV);
        }

        private byte[] decryptFileChunk(byte[] blockBuffer, int offset, int BLOCKSIZE, byte[] IV)
        {
            if (IV != null)
            {
                base.IV = (IV);
                init();
            }
            return decrypt(blockBuffer, offset, BLOCKSIZE);
        }

        public void decryptFileStream(MemoryStream inputStream, BinaryWriter outputStream, long filesize, short contentIndex, byte[] h3hash,
                long expectedSizeForHash)
        {
            HashAlgorithm sha1 = null;
            HashAlgorithm sha1fallback = null;
            if (h3hash != null)
            {
                try
                {
                    sha1 = new SHA1CryptoServiceProvider();
                    sha1fallback = new SHA1CryptoServiceProvider();
                }
                catch (Exception)
                {
                    //e.printStackTrace();
                }
            }

            int BLOCKSIZE = 0x8000;
            // long dlFileLength = filesize;
            // if(dlFileLength > (dlFileLength/BLOCKSIZE)*BLOCKSIZE){
            // dlFileLength = ((dlFileLength/BLOCKSIZE)*BLOCKSIZE) +BLOCKSIZE;
            // }

            byte[] IV = new byte[0x10];

            IV[0] = (byte)((contentIndex >> 8) & 0xFF);
            IV[1] = (byte)(contentIndex);

            byte[] blockBuffer = new byte[BLOCKSIZE];

            int inBlockBuffer;
            long written = 0;

            //ByteArrayBuffer overflow = new ByteArrayBuffer(BLOCKSIZE);

            int offset = 0;
            inputStream.Seek(0, SeekOrigin.Begin);
            do
            {
                //inBlockBuffer = StreamUtils.getChunkFromStream(inputStream, blockBuffer, overflow, BLOCKSIZE);
                inBlockBuffer = inputStream.Read(blockBuffer, 0, BLOCKSIZE);
                offset += inBlockBuffer;

                byte[] output = decryptFileChunk(blockBuffer, (int)Utils.align(inBlockBuffer, 16), IV);

                IV = Arrays.copyOfRange(blockBuffer, BLOCKSIZE - 16, BLOCKSIZE);

                int toWrite = inBlockBuffer;
                if ((written + inBlockBuffer) > filesize)
                {
                    toWrite = (int)(filesize - written);
                }

                written += toWrite;
                outputStream.Write(output, 0, toWrite);

                if (sha1 != null && sha1fallback != null)
                {
                    sha1.ComputeHash(output, 0, toWrite);
                    sha1fallback.ComputeHash(output, 0, toWrite);
                }
            } while (inBlockBuffer == BLOCKSIZE);

            if (sha1 != null && sha1fallback != null)
            {
                long missingInHash = expectedSizeForHash - written;
                if (missingInHash > 0)
                {
                    sha1fallback.ComputeHash(new byte[(int)missingInHash]);
                }

                byte[] calculated_hash1 = sha1.Hash;
                byte[] calculated_hash2 = sha1fallback.Hash;
                byte[] expected_hash = h3hash;
                if (!Arrays.Equals(calculated_hash1, expected_hash) && !Arrays.Equals(calculated_hash2, expected_hash))
                {
                    //outputStream.close();
                    //inputStream.close();
                    //throw new CheckSumWrongException("hash checksum failed", calculated_hash1, expected_hash);
                }
                else
                {
                    // log.warning("Hash DOES match saves output stream.");
                }
            }

            outputStream.Close();
            inputStream.Close();
        }

        public void decryptFileStreamHashed(MemoryStream inputStream, BinaryWriter outputStream, long filesize, long fileoffset, short contentIndex, byte[] h3Hash)
        {
            int BLOCKSIZE = 0x10000;
            int HASHBLOCKSIZE = 0xFC00;

            long writeSize = HASHBLOCKSIZE;

            long block = (fileoffset / HASHBLOCKSIZE);
            long soffset = fileoffset - (fileoffset / HASHBLOCKSIZE * HASHBLOCKSIZE);

            if (soffset + filesize > writeSize) writeSize = writeSize - soffset;

            byte[] encryptedBlockBuffer = new byte[BLOCKSIZE];
            //ByteArrayBuffer overflow = new ByteArrayBuffer(BLOCKSIZE);

            long wrote = 0;
            int inBlockBuffer;
            do
            {
                //inBlockBuffer = StreamUtils.getChunkFromStream(inputStream, encryptedBlockBuffer, overflow, BLOCKSIZE);
                inBlockBuffer = inputStream.Read(encryptedBlockBuffer, 0, BLOCKSIZE);
                if (writeSize > filesize) writeSize = filesize;

                byte[] output;
                try
                {
                    output = decryptFileChunkHash(encryptedBlockBuffer, (int)block, contentIndex, h3Hash);
                }
                catch (Exception e)
                {
                    outputStream.Close();
                    inputStream.Close();
                    throw e;
                }

                if ((wrote + writeSize) > filesize)
                {
                    writeSize = (int)(filesize - wrote);
                }

                outputStream.Write(output, (int)(0 + soffset), (int)writeSize);

                wrote += writeSize;

                block++;

                if (soffset > 0)
                {
                    writeSize = HASHBLOCKSIZE;
                    soffset = 0;
                }
            } while (wrote < filesize && (inBlockBuffer == BLOCKSIZE));
            // System.out.println("Decryption okay");
            outputStream.Close();
            inputStream.Close();
        }

        private byte[] decryptFileChunkHash(byte[] blockBuffer, int block, int contentIndex, byte[] h3_hashes)
        {
            int hashSize = 0x400;
            int blocksize = 0xFC00;
            byte[] IV = ByteBuffer.allocate(16).putShort((short)contentIndex).ToArray();

            byte[] hashes = decryptFileChunk(blockBuffer, hashSize, IV);

            hashes[0] ^= (byte)((contentIndex >> 8) & 0xFF);
            hashes[1] ^= (byte)(contentIndex & 0xFF);

            int H0_start = (block % 16) * 20;

            IV = Arrays.copyOfRange(hashes, H0_start, H0_start + 16);
            byte[] output = decryptFileChunk(blockBuffer, hashSize, blocksize, IV);

            //HashUtil.checkFileChunkHashes(hashes, h3_hashes, output, block);

            return output;
        }
    }
}
