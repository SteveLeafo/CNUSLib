using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CNUSLib
{
    public class Ticket
    {
        private static int POSITION_KEY = 0x1BF;
        private static int POSITION_TITLEID = 0x1DC;

        public byte[] encryptedKey;
        public byte[] decryptedKey;

        public byte[] IV;

        private Ticket(byte[] encryptedKey, byte[] decryptedKey, byte[] IV)
        {
            this.encryptedKey = encryptedKey;
            this.decryptedKey = decryptedKey;
            this.IV = IV;
        }

        public static Ticket parseTicket(FileInfo ticket)
        {
            if (ticket == null || !ticket.Exists)
            {
                //MessageBox.Show("Ticket input file null or doesn't exist.");
                return null;
            }
            return parseTicket(File.ReadAllBytes(ticket.FullName));
        }

        public static Ticket parseTicket(byte[] ticket)
        {
            if (ticket == null)
            {
                return null;
            }

            ByteBuffer buffer = ByteBuffer.allocate(ticket.Length);
            buffer.put(ticket);

            // read key
            byte[] encryptedKey = new byte[0x10];
            buffer.position(POSITION_KEY);
            buffer.get(encryptedKey, 0x00, 0x10);

            // read titleID
            buffer.position(POSITION_TITLEID);
            long titleID = buffer.getLong();

            Ticket result = createTicket(encryptedKey, titleID);

            return result;
        }

        public static Ticket createTicket(byte[] encryptedKey, long titleID)
        {
            byte[] IV = ByteBuffer.allocate(0x10).putLong(titleID);
            byte[] decryptedKey = calculateDecryptedKey(encryptedKey, IV);

            Ticket t = new Ticket(encryptedKey, decryptedKey, IV);
            return t;
        }

        private static byte[] calculateDecryptedKey(byte[] encryptedKey, byte[] IV)
        {
            AESDecryption decryption = new AESDecryption(Settings.commonKey, IV) {};
            return decryption.decrypt(encryptedKey);
        }

        public override int GetHashCode()
        {
            int prime = 31;
            int result = 1;
            result = prime * result + encryptedKey.GetHashCode();
            return result;
        }

        public override bool Equals(Object obj)
        {
            if (this == obj) return true;
            if (obj == null) return false;
            if (GetType() != obj.GetType()) return false;
            Ticket other = (Ticket)obj;
            return Arrays.Equals(encryptedKey, other.encryptedKey);
        }

        public override string ToString()
        {
            return "Ticket [encryptedKey=" + Utils.ByteArrayToString(encryptedKey) + ", decryptedKey=" + Utils.ByteArrayToString(decryptedKey) + "]";
        }
    }
}
