using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using voluntLib.common.interfaces;

namespace KeySearcher.CrypCloud
{
    public class KeyResultEntry : ISerializable, IComparable
    {
        public KeyResultEntry()
        {
        }

        public KeyResultEntry(byte[] bytes)
        {
            Deserialize(bytes);
        }

        public double Costs { get; set; }
        public byte[] KeyBytes { get; set; }
        public byte[] Decryption { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as KeyResultEntry;
            if(other == null){
                return false;
            }

            return KeyBytes.SequenceEqual(other.KeyBytes);
        }

        public override int GetHashCode()
        {
            return KeyBytes.Aggregate(KeyBytes.Length, (current, t) => unchecked(current*314159 + t));
        }

        #region IComparable

        public int CompareTo(object obj)
        {
            if (Equals(obj)) {
                return 0;
            }

            var entry = (KeyResultEntry)obj;
            if (Costs == entry.Costs)
            {
                return KeyBytes[0] - entry.KeyBytes[0];
            }
                     
            var epsilon = .000001; 
            var compare = Costs - entry.Costs;    
            return (compare > epsilon) ? 1 : -1;           
        }

        #endregion IComparable

        

        public override string ToString()
        {
            return Costs + " | " + BitConverter.ToString(KeyBytes) + " | " + Encoding.UTF8.GetString(Decryption);
        }

        #region IMessageData

        public byte[] Serialize()
        {
            return BitConverter.GetBytes(Costs)
                .Concat(BitConverter.GetBytes((ushort) KeyBytes.Length))
                .Concat(KeyBytes)
                .Concat(BitConverter.GetBytes((ushort) Decryption.Length))
                .Concat(Decryption).ToArray();
        }

        public void Deserialize(byte[] bytes)
        {
            var startIndex = 0;
            Costs = BitConverter.ToDouble(bytes, startIndex);
            startIndex += sizeof (double);

            var length = BitConverter.ToUInt16(bytes, startIndex);
            startIndex += sizeof (ushort);
            KeyBytes = bytes.Skip(startIndex).Take(length).ToArray();
            startIndex += length;

            length = BitConverter.ToUInt16(bytes, startIndex);
            startIndex += sizeof (ushort);
            Decryption = bytes.Skip(startIndex).Take(length).ToArray();
        }

        #endregion IMessageData
    }
}