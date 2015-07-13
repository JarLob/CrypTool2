using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using voluntLib.common.interfaces;

namespace KeySearcher.CrypCloud
{
    public class KeyResultEntry : ISerializable, IComparable
    {
        public double Costs { get; set; }
        public byte[] KeyBytes { get; set; }
        public byte[] Decryption { get; set; }

        public KeyResultEntry() { }
        public KeyResultEntry(byte[] bytes)
        {
            Deserialize(bytes);
        }

        public override string ToString()
        {
            return Costs + " | " + BitConverter.ToString(KeyBytes) + " | " + Encoding.UTF8.GetString(Decryption);
        }

        #region IComparable

        public int CompareTo(object obj)
        {
            var entry = (KeyResultEntry)obj;             
            return (entry.Costs > Costs) ? -1 : 1;
        }

        #endregion IComparable

        #region IMessageData

        public byte[] Serialize()
        {
            return BitConverter.GetBytes(Costs)
                .Concat(BitConverter.GetBytes((ushort)KeyBytes.Length))
                .Concat(KeyBytes)
                .Concat(BitConverter.GetBytes((ushort)Decryption.Length))
                .Concat(Decryption).ToArray();
        }

        public void Deserialize(byte[] bytes)
        {
            var startIndex = 0;
            Costs = BitConverter.ToDouble(bytes, startIndex);
            startIndex += sizeof(double);

            var length = BitConverter.ToUInt16(bytes, startIndex);
            startIndex += sizeof(ushort);
            KeyBytes = bytes.Skip(startIndex).Take(length).ToArray();
            startIndex += length;

            length = BitConverter.ToUInt16(bytes, startIndex);
            startIndex += sizeof(ushort);
            Decryption = bytes.Skip(startIndex).Take(length).ToArray();
        }

        #endregion IMessageData

    }
}
