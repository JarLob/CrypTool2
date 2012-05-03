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
    public class TranspositionTest
    {
        public TranspositionTest()
        {
        }

        [TestMethod]
        public void TranspositionTestMethod()
        {
            var pluginInstance = TestHelpers.GetPluginInstance("Transposition");
            var scenario = new PluginTestScenario(pluginInstance, new[] { "Input", "Keyword", ".ReadIn", ".Permutation", ".ReadOut" }, new[] { "Output" });
            object[] output;

            foreach (TestVector vector in testvectors)
            {
                output = scenario.GetOutputs(new object[] { vector.input.ToByteArray(), vector.key, vector.readin, vector.perm, vector.readout });
                Assert.AreEqual(vector.output, ((byte[])output[0]).ToString2(), "Unexpected value in test #" + vector.n + ".");
            }

        }

        struct TestVector
        {
            public string input, output, key;
            public int readin, readout, perm;
            public int n;
        }

        //
        // Source of the test vectors: http://crackthebbccode.wikispaces.com/Useful+code
        //
        TestVector[] testvectors = new TestVector[] {
            new TestVector () { n=0, readin=0, perm=1, readout=1, key="ZEBRAS", input="WEAREDISCOVEREDFLEEATONCE", output="EVLNACDTESEAROFODEECWIREE" }, 
            new TestVector () { n=1, readin=0, perm=1, readout=1, key="ZEBRAS", input="Wir wurden entdeckt, flieh wenn du kannst", output="w cle tred  dnidt,h n nefwusuekinkWrntena" },
            new TestVector () { n=2, readin=1, perm=1, readout=1, key="ZEBRAS", input="Wir wurden entdeckt, flieh wenn du kannst", output="enn du deckt, den entflieh wkannstWir wur" },
            new TestVector () { n=3, readin=1, perm=0, readout=1, key="ZEBRAS", input="Wir wurden entdeckt, flieh wenn du kannst", output="wri uW ednercdteknl ,fite hwne d ukntnnsa" },
            new TestVector () { n=4, readin=0, perm=0, readout=1, key="ZEBRAS", input="Wir wurden entdeckt, flieh wenn du kannst", output="eddfkWneelaincninr k en dtehswu,n tu  twr" },
            new TestVector () { n=5, readin=0, perm=1, readout=1, key="HUHN", input="A", output="A" },
            new TestVector () { n=6, readin=1, perm=1, readout=1, key="HUHN", input="A", output="A" },
            new TestVector () { n=7, readin=1, perm=0, readout=1, key="HUHN", input="A", output="A" },
            new TestVector () { n=8, readin=0, perm=0, readout=1, key="HUHN", input="A", output="A" },
        };

    }
}