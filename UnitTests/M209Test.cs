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
    public class M209Test
    {
        public M209Test()
        {
        }

        [TestMethod]
        public void M209TestMethod()
        {
            var pluginInstance = TestHelpers.GetPluginInstance("M209");
            var scenario = new PluginTestScenario(pluginInstance, new[] { "Text", ".Startwert", 
                ".Rotor1", ".Rotor2", ".Rotor3", ".Rotor4", ".Rotor5", ".Rotor6",
                ".Bar1", ".Bar2", ".Bar3", ".Bar4", ".Bar5", ".Bar6", ".Bar7", ".Bar8", ".Bar9", ".Bar10",
                ".Bar11", ".Bar12", ".Bar13", ".Bar14", ".Bar15", ".Bar16", ".Bar17", ".Bar18", ".Bar19", ".Bar20",
                ".Bar21", ".Bar22", ".Bar23", ".Bar24", ".Bar25", ".Bar26", ".Bar27" }, new[] { "OutputString" });
            object[] output;

            foreach (TestVector vector in testvectors)
            {
                List<object> parameters = new List<object> { vector.input, vector.key };
                foreach (var pin in vector.pins) parameters.Add(pin);
                foreach (var slider in vector.sliders) parameters.Add(slider);

                output = scenario.GetOutputs( parameters.ToArray() );
                Assert.AreEqual(vector.output, (string)output[0], "Unexpected value in test #" + vector.n + ".");
            }
        }

        struct TestVector
        {
            public string input, output, key;
            public string[] pins, sliders;
            public int n;
        }

        //
        // Source of the test vectors: http://wikipedia.qwika.com/en2ko/M-209
        //
        TestVector[] testvectors = new TestVector[] {
            new TestVector () { n=0,
                key="AAAAAA", input="AAAAAAAAAAAAAAAAAAAAAAAAAA", output="TNJUW AUQTK CZKNU TOTBC WARMI O", 
                pins = new string[] { "ABDHIKMNSTVW", "ADEGJKLORSUX", "ABGHJLMNRSTUX", "CEFHIMNPSTU", "BDEFHIMNPS", "ABDHKNOQ" }, 
                sliders = new string[] { "36","06","16","15","45","04","04","04","04","20","20","20","20","20","20","20","20","20","20","25","25","05","05","05","05","05","05" }
            },
        };

    }
}


