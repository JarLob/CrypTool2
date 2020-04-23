﻿using System;
using common;


namespace ADFGVXAnalyzer
{
    public class ADFGVX
    {
        public String name = "unknown";
        public AlphabetVector transpositionKey;
        AlphabetVector transpositionInverseKey;
        public Alphabet36Vector substitutionKey;
        public Alphabet36Vector substitutionInverseKey;

        public ADFGVX(String name, int transpositionKeyLength)
        {
            this.name = name;
            transpositionKey = new AlphabetVector(transpositionKeyLength, false);
            transpositionInverseKey = new AlphabetVector(transpositionKeyLength, false);
            substitutionKey = new Alphabet36Vector();
            substitutionInverseKey = new Alphabet36Vector();
            substitutionKey.acceptErrors = true;
            substitutionInverseKey.acceptErrors = true;
            resetTranspositionKey();
            resetSubstitutionKey();
        }

        public ADFGVX(String name, String transpositionKeyStr, String substitutionKeyStr)

            : this(name, transpositionKeyStr, substitutionKeyStr, false)
        { }

        public ADFGVX(String name, String transpositionKeyStr, String substitutionKeyStr, bool inverseSubstitution)

            : this(name, transpositionKeyStr.Length)
        {
            setTranspositionKey(transpositionKeyStr);
            if (inverseSubstitution)
            {
                setSubstitutionInverseKey(substitutionKeyStr);
            }
            else
            {
                setSubstitutionKey(substitutionKeyStr);
            }
        }
        public void randomTranspositionKey()
        {
            this.transpositionKey.randomPermutation();
            updateTranspositionInverseKey();
        }

        private void updateTranspositionInverseKey() { this.transpositionInverseKey.inverseOf(this.transpositionKey); }
        private void updateTranspositionKeyFromInverse() { this.transpositionKey.inverseOf(this.transpositionInverseKey); }
        private void updateSubstitutionInverseKey() { this.substitutionInverseKey.inverseOf(this.substitutionKey); }
        private void updateSubstitutionKeyFromInverse() { this.substitutionKey.inverseOf(this.substitutionInverseKey); }


        public void resetTranspositionKey()
        {
            this.transpositionKey.Identity();
            this.transpositionInverseKey.Identity();
        }

        public void setTranspositionKey(String transpositionKeyStr)
        {
            this.transpositionKey.copy(transpositionKeyStr);
            updateTranspositionInverseKey();
        }

        public string getTranspositionKey()
        {
            return transpositionKey.ToString();
        }

        public void setTranspositionKey(AlphabetVector transpositionKey)
        {
            this.transpositionKey.copy(transpositionKey);
            updateTranspositionInverseKey();
        }
        public void swapInTranspositionKey(int i, int j)
        {
            this.transpositionKey.Swap(i, j);
            updateTranspositionInverseKey();
        }

        public void setTranspositionInverseKey(AlphabetVector transpositionInverseKey)
        {
            this.transpositionInverseKey.copy(transpositionInverseKey);
            updateTranspositionKeyFromInverse();
        }
        public void setTranspositionInverseKey(String transpositionInverseKeyStr)
        {
            this.transpositionInverseKey.copy(transpositionInverseKeyStr);
            updateTranspositionKeyFromInverse();
        }
        public void swapInTranspositionInverseKey(int i, int j)
        {
            this.transpositionInverseKey.Swap(i, j);
            updateTranspositionKeyFromInverse();
        }

        public void randomSubstitutionKey()
        {
            this.substitutionKey.randomPermutation();
            updateSubstitutionInverseKey();
        }

        public void resetSubstitutionKey()
        {
            this.substitutionKey.Identity();
            this.substitutionInverseKey.Identity();
        }

        public void setSubstitutionKey(String substitutionKeyStr)
        {
            this.substitutionKey.copy(substitutionKeyStr);
            updateSubstitutionInverseKey();
        }
        public void setSubstitutionKey(Alphabet36Vector substitutionKey)
        {
            this.substitutionKey.copy(substitutionKey);
            updateSubstitutionInverseKey();
        }
        public void swapInSubstitutionKey(int i, int j)
        {
            this.substitutionKey.Swap(i, j);
            updateSubstitutionInverseKey();
        }

        public void setSubstitutionInverseKey(String substitutionInverseKeyStr)
        {
            this.substitutionInverseKey.copy(substitutionInverseKeyStr);
            updateSubstitutionKeyFromInverse();
        }
        public void setSubstitutionInverseKey(Alphabet36Vector substitutionInverseKey)
        {
            this.substitutionInverseKey.copy(substitutionInverseKey);
            updateSubstitutionKeyFromInverse();
        }
        public void swapInSubstitutionInverseKey(int i, int j)
        {
            this.substitutionInverseKey.Swap(i, j);
            updateSubstitutionKeyFromInverse();
        }

        public void decodeSubstitution(ADFGVXVector interim, Alphabet36Vector plain)
        {
            if (interim.length % 2 != 0)
            {
                //hack, to allow uneven length adfgvx texts
                interim.append("A");                
            }
            plain.length = 0;
            for (int i = 0; i < interim.length; i += 2)
            {
                int v1 = interim.TextInInt[i];
                int v2 = interim.TextInInt[i + 1];
                if (v1 == -1 || v2 == -1)
                {
                    plain.append(-1);
                }
                else
                {
                    plain.append(this.substitutionInverseKey.TextInInt[v1 * 6 + v2]);
                }
            }
        }

        public void decode(ADFGVXVector cipher, ADFGVXVector interim, Alphabet36Vector plain)
        {
            Transposition.decodeWithInverseKey(this.transpositionInverseKey, cipher, interim);
            decodeSubstitution(interim, plain);
        }
    }
}
