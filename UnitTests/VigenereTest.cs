﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Cryptool.PluginBase.IO;
using Cryptool.Plugins.Cryptography.Encryption;

namespace Tests.TemplateAndPluginTests
{
    [TestClass]
    public class VigenereTest
    {
        public VigenereTest()
        {
        }

        [TestMethod]
        public void VigenereTestMethod()
        {
            var pluginInstance = TestHelpers.GetPluginInstance("Vigenere");
            var scenario = new PluginTestScenario(pluginInstance, new[] { "InputString", "ShiftValue" }, new[] { "OutputString" });
            object[] output;

            foreach (TestVector vector in testvectors)
            {
                output = scenario.GetOutputs(new object[] { vector.input, vector.key });
                Assert.AreEqual(vector.output, (string)output[0], "Unexpected value in test #" + vector.n + ".");
            }

        }

        struct TestVector
        {
            public string input, output, key;
            public int n;
        }

        //
        // Sources of the test vectors:
        //  http://courses.ece.ubc.ca/412/previous_years/2004/modules/sessions/EECE_412-03-crypto_intro-viewable.pdf
        //  http://en.wikipedia.org/wiki/Vigenère_cipher
        //
        TestVector[] testvectors = new TestVector[] {
            new TestVector () { n=0, key="LEMON", input="ATTACKATDAWN", output="LXFOPVEFRNHR" },
            new TestVector () { n=0, key="ABCD", input="CRYPTOISSHORTFORCRYPTOGRAPHY", output="CSASTPKVSIQUTGQUCSASTPIUAQJB" },
            new TestVector () { n=0, key="RELATIONS", input="TOBEORNOTTOBETHATISTHEQUESTION", output="KSMEHZBBLKSMEMPOGAJXSEJCSFLZSY" },
        };

    }
}