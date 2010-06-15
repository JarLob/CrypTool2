﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Org.BouncyCastle.Math;


namespace PKCS1.Library
{
    class BleichenbacherSignature : Signature
    {
        protected int m_dataBlockStartPos = 2072;
        public int DataBlockStartPos
        {
            set
            {
                //TODO zulässigen Wertebereich weiter einschraenken?
                if ((int)value > 0 && (int)value < RSAKeyManager.getInstance().RsaKeySize)
                {
                    this.m_dataBlockStartPos = (int)value;
                }
                else
                {
                    this.m_dataBlockStartPos = 2072;
                }
            }
            get
            {
                return this.m_dataBlockStartPos;
            }
        }

        public override void GenerateSignature()
        {
            this.m_KeyLength = RSAKeyManager.getInstance().RsaKeySize;

            int hashDigestLength = Hashfunction.getDigestSize() * 8; // weil Size in Byte zurückgegeben wird
            int hashIdentLength = Datablock.getInstance().HashFunctionIdent.DERIdent.Length * 4; // weil ein zeichen im string = 4 bit            
            int keyLength = this.m_KeyLength;
            BigInteger derIdent = new BigInteger(Datablock.getInstance().HashFunctionIdent.DERIdent, 16);
            BigInteger datablockLength = BigInteger.ValueOf(hashDigestLength + hashIdentLength + 8); // Länge Datenblock inkl 0-Byte (=8Bit)

            bool isDivByThree = false;
            BigInteger N = null;
            BigInteger datablock = null;

            while (false == isDivByThree)
            {
                BigInteger hashDigest = new BigInteger(1, Hashfunction.generateHashDigest(Datablock.getInstance().Message, Datablock.getInstance().HashFunctionIdent));
                // T*2^160 + H
                datablock = derIdent.Multiply(BigInteger.Two.Pow(hashDigestLength)).Add(hashDigest); // Datablock erstellen; T=HashFuncIdent; H=HashDigest
                N = (BigInteger.Two.Pow(datablockLength.IntValue)).Subtract(datablock); // N muss vielfaches von 3 sein

                if (0 == (N.Mod(BigInteger.Three)).IntValue)
                {
                    isDivByThree = true;
                }
                else
                {
                    Datablock.getInstance().Message = Datablock.getInstance().Message + " ";
                }
            }

            BigInteger sigLengthWithoutZeros = BigInteger.ValueOf(this.m_KeyLength - 15); // 15 weil die ersten 15 bit 0 sind
            BigInteger startPos = BigInteger.ValueOf(this.m_dataBlockStartPos);
            BigInteger endPos = startPos.Add(datablockLength);
            BigInteger sigLengthDivThree = sigLengthWithoutZeros.Divide(BigInteger.Three); // TODO muss Ganzzahl sein
            
            BigInteger testbeta = endPos.Subtract(BigInteger.Two.Multiply(sigLengthDivThree)).Subtract(datablockLength); // sollte 34 seinbei keylength 3072

            //2^3057 - 2^ (2038 + 288 + 34) == 2^3057 - 2^2360
            BigInteger fakeSig = (BigInteger.Two.Pow(sigLengthWithoutZeros.IntValue)).Subtract( BigInteger.Two.Pow( 2 * sigLengthDivThree.IntValue + datablockLength.IntValue + testbeta.IntValue ));

            // 2^3057 - 2^2360 + 2^2072 * N
            fakeSig = fakeSig.Add( (BigInteger.Two.Pow( startPos.IntValue )).Multiply(datablock) );
            fakeSig = fakeSig.Add( BigInteger.Two.Pow(startPos.IntValue-8).Multiply(BigInteger.ValueOf(125)) ); // add garbage

            BigInteger fakeSigResult = MathFunctions.cuberoot2(fakeSig);

            byte[] returnByteArray = new byte[this.m_KeyLength / 8]; // KeyLength is in bit
            Array.Copy(fakeSigResult.ToByteArray(), 0, returnByteArray, returnByteArray.Length - fakeSigResult.ToByteArray().Length, fakeSigResult.ToByteArray().Length);

            this.m_Signature = returnByteArray;
            this.m_bSigGenerated = true;
            this.OnRaiseSigGenEvent(SignatureType.Bleichenbacher);
        }

        /*
        public void GenerateSignature2()
        {
            // RSA Schlüssellänge setzen für Methode in Oberklasse
            this.m_KeyLength = RSAKeyManager.getInstance().RsaKeySize;

            // TODO prüfen ob Rsa Schlüssel generiert wurde
            // TODO User mitteilen dass evtl Leerzeichen an message angehangen wurden und in HwControl Digest korrigieren
            BigInteger T = new BigInteger(Datablock.getInstance().HashFunctionIdent.DERIdent, 16);
            string test1 = T.ToString(16);
            BigInteger a = null;
            bool isDivByThree = false;

            int hashDigestLength = Hashfunction.getDigestSize() * 8; // weil Size in Byte zurückgegeben wird
            int hashIdentLength = Datablock.getInstance().HashFunctionIdent.DERIdent.Length * 4; // weil ein zeichen im string = 4 bit
            int datablocklength = hashDigestLength + hashIdentLength + 8; // Länge Datenblock inkl 0-Byte (=8Bit)

            while (false == isDivByThree)
            {
                //byte[] hashDigest = Hashfunction.generateHashDigest(Datablock.getInstance().Message, Datablock.getInstance().HashFunctionIdent);
                //BigInteger testBigInt2 = new BigInteger(1, hashDigest);
                //BigInteger testBigInt = new BigInteger( Hashfunction.generateHashDigest(Datablock.getInstance().Message, Datablock.getInstance().HashFunctionIdent) );
                //string testHashDigest = testBigInt.ToString(16);
                //string testHashDigest2 = testBigInt2.ToString(16);

                BigInteger H = new BigInteger(1, Hashfunction.generateHashDigest(Datablock.getInstance().Message, Datablock.getInstance().HashFunctionIdent));
                BigInteger helpSubst = T.Multiply(BigInteger.Two.Pow(hashDigestLength)).Add(H); // Datablock erstellen; T=HashFuncIdent; H=HashDigest
                //string test = helpSubst.ToString(16);
                a = (BigInteger.Two.Pow(datablocklength)).Subtract(helpSubst); // a = 2^288- (helpsubst); muss div by three sein

                if (0 == (a.Mod(BigInteger.Three)).IntValue)
                {
                    isDivByThree = true;
                }
                else
                {
                    Datablock.getInstance().Message = Datablock.getInstance().Message + " ";
                }
            }
            // s = 2^1019-a/3*2^34
            // 1019 ist fix wenn Modulus 3072 lang ist
            BigInteger fakedSignature = (BigInteger.Two.Pow(1019)).Subtract(a.Divide(BigInteger.Three).Multiply(BigInteger.Two.Pow(34)));
            //string test2 = fakedSignature.ToString(16);

            byte[] returnByteArray = new byte[this.m_KeyLength / 8]; // KeyLength is in bit
            Array.Copy(fakedSignature.ToByteArray(), 0, returnByteArray, returnByteArray.Length - fakedSignature.ToByteArray().Length, fakedSignature.ToByteArray().Length);

            this.m_Signature = returnByteArray;
            this.m_bSigGenerated = true;
            this.OnRaiseSigGenEvent(SignatureType.Bleichenbacher);

            //this.generateSignature2();
        }

        public void GenerateSignature3()
        {
            this.m_KeyLength = RSAKeyManager.getInstance().RsaKeySize;

            int hashDigestLength = Hashfunction.getDigestSize() * 8; // weil Size in Byte zurückgegeben wird
            int hashIdentLength = Datablock.getInstance().HashFunctionIdent.DERIdent.Length * 4; // weil ein zeichen im string = 4 bit            
            BigInteger derIdent = new BigInteger(Datablock.getInstance().HashFunctionIdent.DERIdent, 16);

            BigInteger datablockLength = BigInteger.ValueOf(hashDigestLength + hashIdentLength + 8); // Länge Datenblock inkl 0-Byte (=8Bit)
            bool isDivByThree = false;
            BigInteger N = null;

            while (false == isDivByThree)
            {
                BigInteger hashDigest = new BigInteger(1, Hashfunction.generateHashDigest(Datablock.getInstance().Message, Datablock.getInstance().HashFunctionIdent));
                BigInteger D = derIdent.Multiply(BigInteger.Two.Pow(hashDigestLength)).Add(hashDigest); // Datablock erstellen; T=HashFuncIdent; H=HashDigest
                N = (BigInteger.Two.Pow(datablockLength.IntValue)).Subtract(D); // N muss vielfaches von 3 sein

                if (0 == (N.Mod(BigInteger.Three)).IntValue)
                {
                    isDivByThree = true;
                }
                else
                {
                    Datablock.getInstance().Message = Datablock.getInstance().Message + " ";
                }
            }
            BigInteger x = BigInteger.ValueOf(this.m_KeyLength - 15); // 15 weil die ersten 15 bit 0 sind
            BigInteger startPos = BigInteger.ValueOf(2072); // durch Eingabe ersetzen
            BigInteger endPos = startPos.Add(datablockLength);

            ///////// nicht zur Berechnung nötig
            BigInteger lengthDivThree = x.Divide(BigInteger.Three);
            BigInteger beta = endPos.Subtract(BigInteger.Two.Multiply(lengthDivThree)).Subtract(datablockLength); // sollte 34 sein
            ////////// 

            //BigInteger garbage = (N.Pow(2).Multiply(
            //BigInteger fakeSig = BigInteger.Two.Pow(this.m_KeyLength).Subtract(N.Multiply(BigInteger.Two.Pow(startPos)));
            BigInteger A = BigInteger.Two.Pow(lengthDivThree.IntValue);
            BigInteger B = (N.Multiply(BigInteger.Two.Pow(beta.IntValue)).Divide(BigInteger.Three));

            BigInteger fakeSig = A.Pow(3).Subtract((BigInteger.Three.Multiply((A.Pow(2))).Multiply(B))).Add((BigInteger.Three.Multiply(A.Multiply((B.Pow(2)))))).Subtract((B.Pow(3)));
            string test = fakeSig.ToString(16);
            BigInteger fakeSigResult = MathFunctions.cuberoot(fakeSig);


            byte[] returnByteArray = new byte[this.m_KeyLength / 8]; // KeyLength is in bit
            Array.Copy(fakeSigResult.ToByteArray(), 0, returnByteArray, returnByteArray.Length - fakeSigResult.ToByteArray().Length, fakeSigResult.ToByteArray().Length);

            this.m_Signature = returnByteArray;
            this.m_bSigGenerated = true;
            this.OnRaiseSigGenEvent(SignatureType.Bleichenbacher);
        }
        */
    }
}
