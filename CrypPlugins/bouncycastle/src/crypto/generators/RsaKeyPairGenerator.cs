using System;

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;

namespace Org.BouncyCastle.Crypto.Generators
{
    /**
     * an RSA key pair generator.
     */
    public class RsaKeyPairGenerator
		: IAsymmetricCipherKeyPairGenerator
    {
		private static readonly BigInteger DefaultPublicExponent = BigInteger.ValueOf(0x10001);
		private const int DefaultTests = 12;

		private RsaKeyGenerationParameters param;

		public void Init(
            KeyGenerationParameters parameters)
        {
			if (parameters is RsaKeyGenerationParameters)
			{
				this.param = (RsaKeyGenerationParameters)parameters;
			}
			else
			{
				this.param = new RsaKeyGenerationParameters(
					DefaultPublicExponent, parameters.Random, parameters.Strength, DefaultTests);
			}
        }

		public AsymmetricCipherKeyPair GenerateKeyPair()
        {
            BigInteger p, q, n, d, e, pSub1, qSub1, phi;

            //
            // p and q values should have a length of half the strength in bits
            //
            int pbitlength = (param.Strength + 1) / 2;
            int qbitlength = (param.Strength - pbitlength);
            e = param.PublicExponent;

            //
            // Generate p, prime and (p-1) relatively prime to e
            //
            for (;;)
            {
//				p = new BigInteger(pbitlength, param.Certainty, param.Random);
//				if (e.Gcd(p.Subtract(BigInteger.One)).Equals(BigInteger.One)) break;

				p = new BigInteger(pbitlength, 1, param.Random);

				if (p.Mod(e).Equals(BigInteger.One))
					continue;

				if (!p.IsProbablePrime(param.Certainty))
					continue;

				if (e.Gcd(p.Subtract(BigInteger.One)).Equals(BigInteger.One)) 
					break;
			}

            //
            // Generate a modulus of the required length
            //
            for (;;)
            {
                // Generate q, prime and (q-1) relatively prime to e,
                // and not equal to p
                //
                for (;;)
                {
//					q = new BigInteger(qbitlength, param.Certainty, param.Random);
//					if (e.Gcd(q.Subtract(BigInteger.One)).Equals(BigInteger.One) && !p.Equals(q)) break;

					q = new BigInteger(qbitlength, 1, param.Random);

					if (q.Equals(p))
						continue;

					if (q.Mod(e).Equals(BigInteger.One))
						continue;

					if (!q.IsProbablePrime(param.Certainty))
						continue;

					if (e.Gcd(q.Subtract(BigInteger.One)).Equals(BigInteger.One)) 
						break;
				}

                //
                // calculate the modulus
                //
                n = p.Multiply(q);

                if (n.BitLength == param.Strength)
					break;

                //
                // if we Get here our primes aren't big enough, make the largest
                // of the two p and try again
                //
                p = p.Max(q);
            }

			if (p.CompareTo(q) < 0)
			{
				phi = p;
				p = q;
				q = phi;
			}

            pSub1 = p.Subtract(BigInteger.One);
            qSub1 = q.Subtract(BigInteger.One);
            phi = pSub1.Multiply(qSub1);

            //
            // calculate the private exponent
            //
            d = e.ModInverse(phi);

            //
            // calculate the CRT factors
            //
            BigInteger dP, dQ, qInv;

            dP = d.Remainder(pSub1);
            dQ = d.Remainder(qSub1);
            qInv = q.ModInverse(p);

            return new AsymmetricCipherKeyPair(
                new RsaKeyParameters(false, n, e),
                new RsaPrivateCrtKeyParameters(n, e, d, p, q, dP, dQ, qInv));
        }
    }

}
