using ADFGVXAnalyzer.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cryptool.ADFGVXAnalyzer
{
    public class ADFGVXVector : Vector
    {
        private static readonly ADFGVXAnalyzerSettings settings = new ADFGVXAnalyzerSettings();
        static String ALPHABET = settings.CiphertextAlphabet;
        //static String ALPHABET = "ADFGVXZ";
        static StringBuilder ALPHABET_BUILDER = new StringBuilder(ALPHABET);

        static int ALPHABET_SIZE = ALPHABET.Length;

        public ADFGVXVector(String s, bool withStats)
            : base(ALPHABET_BUILDER, s.ToUpper(), withStats)
        { }

        public ADFGVXVector(int length, bool withStats)
            : base(ALPHABET_BUILDER, length, withStats)
        { }

        public ADFGVXVector(bool withStats)
           : this(ALPHABET_SIZE, withStats)
        { }

        public ADFGVXVector()
            : this(ALPHABET_SIZE, false)
        { }
    }
}
