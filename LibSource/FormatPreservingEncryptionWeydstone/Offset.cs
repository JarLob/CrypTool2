/*
 * WORK IN PROGRESS 
 */

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace FormatPreservingEncryptionWeydstone
//{
//    public interface Offset
//    {
//        byte[] Off(byte[] K, byte[] T);
//    }

//    public class OFF1 : Offset
//    {
//        public byte[] Off(byte[] K, byte[] T)
//        {
//            return new byte[16];
//        }
//    }

//    public class OFF2 : Offset
//    {
//        public byte[] Off(byte[] K, byte[] T)
//        {
//            //TODO validate
//            byte[] off = new byte[16];

//            // T′ = [0]^3 | [NUMradix(T)]^13
//            byte[] T_;

//            Ciphers cipher = new Ciphers();
//            return cipher.ciph(K, T);

//        }
//    }
//}
