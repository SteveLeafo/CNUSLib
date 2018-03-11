using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNUSLib
{
    public class WUDService
    {
        private WUDService()
        {
            // Just an utility class
        }

        public static FileInfo compressWUDToWUX(WUDImage image, String outputFolder)
        {
            return compressWUDToWUX(image, outputFolder, "game.wux", false);
        }

        public static FileInfo compressWUDToWUX(WUDImage image, String outputFolder, bool overwrite)
        {
            return compressWUDToWUX(image, outputFolder, "game.wux", overwrite);
        }

        public static FileInfo compressWUDToWUX(WUDImage image, String outputFolder, String filename, bool overwrite)
        {
            if (image.isCompressed)
            {
                //log.info("Given image is already compressed");
                return null;
            }

            if (image.getWUDFileSize() != WUDImage.WUD_FILESIZE)
            {
                //log.info("Given WUD has not the expected filesize");
                return null;
            }

            String usedOutputFolder = outputFolder;
            if (usedOutputFolder == null) usedOutputFolder = "";
            Utils.createDir(usedOutputFolder);

            String filePath;
            if (usedOutputFolder == String.Empty)
            {
                filePath = filename;
            }
            else
            {
                filePath = usedOutputFolder + Path.DirectorySeparatorChar + filename;
            }

            FileInfo outputFile = new FileInfo(filePath);

            if (outputFile.Exists && !overwrite)
            {
                //log.info("Couldn't compress wud, target file already exists (" + outputFile.getAbsolutePath() + ")");
                return null;
            }

            //log.info("Writing compressed file to: " + outputFile.getAbsolutePath());
            FileStream fileStream = new FileStream(outputFile.FullName, FileMode.Create);
            BinaryWriter fileOutput = new BinaryWriter(fileStream);

            WUDImageCompressedInfo info = WUDImageCompressedInfo.getDefaultCompressedInfo();

            byte[] header = info.getHeaderAsBytes();
            //log.info("Writing header");
            fileOutput.Write(header);

            int sectorTableEntryCount = (int)((image.getWUDFileSize() + WUDImageCompressedInfo.SECTOR_SIZE - 1) / (long)WUDImageCompressedInfo.SECTOR_SIZE);

            long sectorTableStart = fileOutput.BaseStream.Position;
            long sectorTableEnd = Utils.align(sectorTableEntryCount * 0x04, WUDImageCompressedInfo.SECTOR_SIZE);
            byte[] sectorTablePlaceHolder = new byte[(int)(sectorTableEnd - sectorTableStart)];

            fileOutput.Write(sectorTablePlaceHolder);

            Dictionary<ByteArrayWrapper, int> sectorHashes = new Dictionary<ByteArrayWrapper, int>();
            Dictionary<int, int> sectorMapping = new Dictionary<int, int>();

            MemoryStream min = image.WUDDiscReader.readEncryptedToInputStream(0, image.getWUDFileSize());

            int bufferSize = WUDImageCompressedInfo.SECTOR_SIZE;
            byte[] blockBuffer = new byte[bufferSize];
            ByteArrayBuffer overflow = new ByteArrayBuffer(bufferSize);

            long written = 0;
            int curSector = 0;
            int realSector = 0;

            //log.info("Writing sectors");

            Int32 oldOffset = int.MinValue;
            min.Seek(0, SeekOrigin.Begin);
            do
            {
                //int read = StreamUtils.getChunkFromStream(in, blockBuffer, overflow, bufferSize);
                int read = min.Read(blockBuffer, 0, bufferSize);
                ByteArrayWrapper hash = new ByteArrayWrapper(HashUtil.hashSHA1(blockBuffer));

                if (!sectorHashes.TryGetValue(hash, out oldOffset))
                {
                    sectorMapping.Add(curSector, realSector);
                    sectorHashes.Add(hash, realSector);
                    fileOutput.Write(blockBuffer);
                    realSector++;
                }
                else
                {
                    sectorMapping.Add(curSector, oldOffset);
                    oldOffset = int.MinValue;
                }

                written += read;
                curSector++;
                if (curSector % 10 == 0)
                {
                    double readMB = written / 1024.0 / 1024.0;
                    double writtenMB = ((long)realSector * (long)bufferSize) / 1024.0 / 1024.0;
                    double percent = ((double)written / image.getWUDFileSize()) * 100;
                    double ratio = 1 / (writtenMB / readMB);
                    //System.out.print(String.format(Locale.ROOT, "\rCompressing into .wux | Progress %.2f%% | Ratio: 1:%.2f | Read: %.2fMB | Written: %.2fMB\t", percent, ratio, readMB, writtenMB));
                }
            } while (written < image.getWUDFileSize());
            //System.out.println();
            //System.out.println("Sectors compressed.");
            //log.info("Writing sector table");
            fileOutput.BaseStream.Seek(sectorTableStart, SeekOrigin.Begin);
            ByteBuffer buffer = ByteBuffer.allocate(sectorTablePlaceHolder.Length);
            //buffer.order(ByteOrder.LITTLE_ENDIAN);
            foreach (var e in sectorMapping)
            {
                buffer.putInt(e.Value);
            }

            fileOutput.Write(buffer.array());
            fileOutput.Close();

            return outputFile;
        }

        public static bool compareWUDImage(WUDImage firstImage, WUDImage secondImage)
        {
            //if (firstImage.getWUDFileSize() != secondImage.getWUDFileSize()) {
            //    log.info("Filesize is different");
            //    return false;
            //}
            //InputStream in1 = firstImage.getWUDDiscReader().readEncryptedToInputStream(0, WUDImage.WUD_FILESIZE);
            //InputStream in2 = secondImage.getWUDDiscReader().readEncryptedToInputStream(0, WUDImage.WUD_FILESIZE);

            bool result = true;
            //int bufferSize = 1024 * 1024 + 19;
            //long totalread = 0;
            //byte[] blockBuffer1 = new byte[bufferSize];
            //byte[] blockBuffer2 = new byte[bufferSize];
            //ByteArrayBuffer overflow1 = new ByteArrayBuffer(bufferSize);
            //ByteArrayBuffer overflow2 = new ByteArrayBuffer(bufferSize);
            //long curSector = 0;
            //do {
            //    int read1 = StreamUtils.getChunkFromStream(in1, blockBuffer1, overflow1, bufferSize);
            //    int read2 = StreamUtils.getChunkFromStream(in2, blockBuffer2, overflow2, bufferSize);
            //    if (read1 != read2) {
            //        log.info("Verification error");
            //        result = false;
            //        break;
            //    }

            //    if (!Arrays.equals(blockBuffer1, blockBuffer2)) {
            //        log.info("Verification error");
            //        result = false;
            //        break;
            //    }

            //    totalread += read1;

            //    curSector++;
            //    if (curSector % 1 == 0) {
            //        double readMB = totalread / 1024.0 / 1024.0;
            //        double percent = ((double) totalread / WUDImage.WUD_FILESIZE) * 100;
            //        System.out.print(String.format("\rVerification: %.2fMB done (%.2f%%)", readMB, percent));
            //    }
            //} while (totalread < WUDImage.WUD_FILESIZE);
            //System.out.println();
            //System.out.print("Verfication done!");
            //in1.close();
            //in2.close();

            return result;
        }

        //public static HashResult hashWUDImage(WUDImage image) 
        //{
        //    if (image == null) {
        //        log.info("Failed to calculate the hash of the given image: input was null.");
        //        return null;
        //    }

        //    if (image.isCompressed()) {
        //        log.info("The input file is compressed. The calculated hash is the hash of the corresponding .wud file, not this .wux!");
        //    } else if (image.isSplitted()) {
        //        log.info("The input file is splitted. The calculated hash is the hash of the corresponding .wud file, not this splitted .wud");
        //    }

        //    InputStream in = image.getWUDDiscReader().readEncryptedToInputStream(0, WUDImage.WUD_FILESIZE);

        //    int bufferSize = 1024 * 1024 * 10;
        //    long totalread = 0;
        //    byte[] blockBuffer1 = new byte[bufferSize];
        //    ByteArrayBuffer overflow1 = new ByteArrayBuffer(bufferSize);
        //    long curSector = 0;

        //    MessageDigest sha1 = null;
        //    MessageDigest md5 = null;
        //    Checksum checksumEngine = new CRC32();

        //    try {
        //        sha1 = MessageDigest.getInstance("SHA1");
        //        md5 = MessageDigest.getInstance("MD5");
        //    } catch (NoSuchAlgorithmException e) {
        //        e.printStackTrace();
        //    }

        //    do {
        //        int read1 = StreamUtils.getChunkFromStream(in, blockBuffer1, overflow1, bufferSize);
        //        sha1.update(blockBuffer1, 0, read1);
        //        md5.update(blockBuffer1, 0, read1);
        //        checksumEngine.update(blockBuffer1, 0, read1);

        //        totalread += read1;

        //        curSector++;
        //        if (curSector % 10 == 0) {
        //            double readMB = totalread / 1024.0 / 1024.0;
        //            double percent = ((double) totalread / WUDImage.WUD_FILESIZE) * 100;
        //            System.out.print(String.format("\rHashing: %.2fMB done (%.2f%%)", readMB, percent));
        //        }
        //    } while (totalread < WUDImage.WUD_FILESIZE);
        //    double readMB = totalread / 1024.0 / 1024.0;
        //    double percent = ((double) totalread / WUDImage.WUD_FILESIZE) * 100;

        //    System.out.println(String.format("\rHashing: %.2fMB done (%.2f%%)", readMB, percent));

        //    HashResult result = new HashResult(sha1.digest(), md5.digest(), Utils.StringToByteArray(Long.toHexString(checksumEngine.getValue())));

        //    in.close();

        //    return result;
        //}
    }
}
