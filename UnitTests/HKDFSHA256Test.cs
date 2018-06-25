using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Cryptool.PluginBase.IO;
using Cryptool.Plugins.Cryptography.Encryption;
using System.Numerics;
using System.Threading;
using Cryptool.Plugins.HKDFSHA256;

namespace Tests.TemplateAndPluginTests
{
    [TestClass]
    public class HKDFSHA256Test
    {
        public HKDFSHA256Test()
        {
        }

        byte[] outputData = null;
        Cryptool.Plugins.HKDFSHA256.HKDFSHA256 pluginInstance;

        [TestMethod]
        public void HKDFSHA256TestMethod()
        {
            pluginInstance = (Cryptool.Plugins.HKDFSHA256.HKDFSHA256)TestHelpers.GetPluginInstance("HKDFSHA256");
            pluginInstance.PropertyChanged += PluginInstance_PropertyChanged;
            var scenario = new PluginTestScenario(pluginInstance, new[] { "SKM", "CTXInfo", "Salt", "OutputBytes", ".InfinityOutput", ".DisplayPres" }, new[] { "KeyMaterial" });
            object[] output;
           

            foreach (TestVector vector in testvectors)
            {
                DateTime startTime = DateTime.Now;
                outputData = null;
                output = scenario.GetOutputs(new object[] { vector.SKM.ToLower().HexToByteArray(), vector.CTXInfo.ToLower().HexToByteArray(), vector.Salt.ToLower().HexToByteArray(), vector.OutputBytes, true, true });
                do
                {
                    Thread.Sleep(1);
                    if (DateTime.Now > startTime.AddSeconds(60))
                    {
                        throw new Exception("TestCase #" + vector.n + " running to long. Aborted it.");
                    }
                } while (outputData == null);
                Assert.AreEqual(vector.KeyMaterial.ToLower(), outputData.ToHex(), "Unexpected value in test #" + vector.n + ".");
            }

        }

        private void PluginInstance_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("KeyMaterial"))
            {
                outputData = pluginInstance.KeyMaterial;
            }
        }

        struct TestVector
        {
            public string SKM, CTXInfo, Salt, KeyMaterial;
            public BigInteger OutputBytes;
            public int n;
        }

        //
        // Sources of the test vectors:
        //  http://en.wikipedia.org/wiki/HKDFSHA256_cipher
        //  Cryptool1-Testvectors
        //
        TestVector[] testvectors = new TestVector[] {
            new TestVector () { n=0, SKM="0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b", CTXInfo="f0f1f2f3f4f5f6f7f8f9", Salt="000102030405060708090a0b0c", KeyMaterial="3cb25f25faacd57a90434f64d0362f2a2d2d0a90cf1a5a4c5db02d56ecc4c5bf34007208d5b887185865", OutputBytes=42 },

        };

    }
}