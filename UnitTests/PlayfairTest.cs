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
    public class PlayfairTest
    {
        public PlayfairTest()
        {
        }

        [TestMethod]
        public void PlayfairTestMethod()
        {
            var pluginInstance = TestHelpers.GetPluginInstance("Playfair");
            var scenario = new PluginTestScenario(pluginInstance, new[] { "InputString", ".Key", ".MatrixSize" }, new[] { "OutputString" });
            object[] output;

            foreach (TestVector vector in testvectors)
            {
                output = scenario.GetOutputs(new object[] { vector.input, vector.key, vector.size });
                Assert.AreEqual(vector.output, (string)output[0], "Unexpected value in test #" + vector.n + ".");
            }

        }

        struct TestVector
        {
            public string input, key, output;
            public int size;
            public int n;
        }

        //
        // Source of the test vectors: http://en.wikipedia.org/wiki/Talk%3APlayfair_cipher
        //
        TestVector[] testvectors = new TestVector[] {
            new TestVector () { n=0, size=0, key="playfairexample", input="Hide the gold in the tree stump", output="BMODZBXDNABEKUDMUIXMMOUVIF" },
            new TestVector () { n=1, size=0, key="playfair example", input="Hide the gold in the tree stump", output="BMODZBXDNABEKUDMUIXMMOUVIF" },
        };

    }
}