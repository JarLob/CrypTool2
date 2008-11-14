using System.IO;

namespace Org.BouncyCastle.Asn1
{
    /**
     * A Der encoded set object
     */
    public class DerSet
        : Asn1Set
    {
        /**
         * create an empty set
         */
        public DerSet()
			: base(0)
        {
        }

		/**
         * @param obj - a single object that makes up the set.
         */
        public DerSet(
            Asn1Encodable obj)
			: base(1)
        {
            AddObject(obj);
        }

		public DerSet(
			params Asn1Encodable[] v)
			: base(v.Length)
		{
			foreach (Asn1Encodable o in v)
			{
				AddObject(o);
			}

			Sort();
		}

		/**
         * @param v - a vector of objects making up the set.
         */
        public DerSet(
            Asn1EncodableVector v)
			: this(v, true)
        {
        }

		internal DerSet(
            Asn1EncodableVector	v,
            bool				needsSorting)
			: base(v.Count)
        {
			foreach (Asn1Encodable o in v)
			{
                AddObject(o);
            }

			if (needsSorting)
            {
                Sort();
            }
        }

		/*
         * A note on the implementation:
         * <p>
         * As Der requires the constructed, definite-length model to
         * be used for structured types, this varies slightly from the
         * ASN.1 descriptions given. Rather than just outputing Set,
         * we also have to specify Constructed, and the objects length.
         */
        internal override void Encode(
            DerOutputStream derOut)
        {
            MemoryStream bOut = new MemoryStream();
            DerOutputStream dOut = new DerOutputStream(bOut);

			foreach (object obj in this)
			{
				dOut.WriteObject(obj);
			}

			dOut.Close();

			byte[] bytes = bOut.ToArray();

			derOut.WriteEncoded(Asn1Tags.Set | Asn1Tags.Constructed, bytes);
        }
    }
}
