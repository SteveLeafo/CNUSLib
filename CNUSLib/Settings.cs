using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WudTool
{
    public class Settings
    {
        public static String URL_BASE = "http://ccs.cdn.c.shop.nintendowifi.net/ccs/download";
        public static int LATEST_TMD_VERSION = 0;
        public static String TMD_FILENAME = "title.tmd";
        public static String TICKET_FILENAME = "title.tik";
        public static String CERT_FILENAME = "title.cert";

        public static String ENCRYPTED_CONTENT_EXTENTION = ".app";
        public static String DECRYPTED_CONTENT_EXTENTION = ".dec";
        public static String WUD_KEY_FILENAME = "game.key";
        public static String WOOMY_METADATA_FILENAME = "metadata.xml";
        public static String H3_EXTENTION = ".h3";

        public static byte[] commonKey = new byte[0x10];
        public static long WIIU_DECRYPTED_AREA_OFFSET = 0x18000;
    }
}
