using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.TemplateAndPluginTests
{
    [TestClass]
    public class SpeckTest
    {
        [TestMethod]
        public void SpeckTestMethod()
        {
            var pluginInstance = TestHelpers.GetPluginInstance("Speck");
            var scenario = new PluginTestScenario(pluginInstance, new[] { "InputStream", "InputKey", "InputIV", ".ChoiceOfVariant", ".OperationMode", ".OpMode", ".PadMode" }, new[] { "OutputStream" });

            foreach (TestVector vector in testvectors)
            {
                object[] output = scenario.GetOutputs(new object[] { vector.input.HexToStream(), vector.key.HexToByteArray(), vector.iv.HexToByteArray(), vector.alg, vector.operationMode, vector.opMode, vector.padMode });
                Assert.AreEqual(vector.output.ToUpper(), output[0].ToHex(), "Unexpected value in test #" + vector.n + ".");
            }
        }

        struct TestVector
        {
            public string input, key, output, iv;
            public int n, alg, operationMode, opMode, padMode;
        }

        //alg = Speck variant
        //operationMode = Mode of operation
        //opMode = Encrypt/Decrypt
        //padMode = Padding mode

        //
        // Source of the test vectors: https://eprint.iacr.org/2013/404.pdf
        //
        TestVector[] testvectors = new TestVector[] {

            new TestVector () { n=1, alg=0, operationMode = 0, opMode=0, padMode = 0, key="0001080910111819", input="4c697465", output="f24268a8", iv="" }, //Speck32/64 Encryption ECB 
            new TestVector () { n=2, alg=1, operationMode = 0, opMode=0, padMode = 0, key="00010208090a101112", input="72616c6c7920", output="dc5a38a549c0", iv="" }, //Speck48/72 Encryption ECB
            new TestVector () { n=3, alg=2, operationMode = 0, opMode=0, padMode = 0, key="00010208090a10111218191a", input="74686973206d", output="5d44b6105e73", iv="" }, //Speck48/96 Encryption ECB
            new TestVector () { n=4, alg=3, operationMode = 0, opMode=0, padMode = 0, key="0001020308090a0b10111213", input="65616e7320466174", output="6c947541ec52799f", iv="" }, //Speck64/96 Encryption ECB
            new TestVector () { n=5, alg=4, operationMode = 0, opMode=0, padMode = 0, key="0001020308090a0b1011121318191a1b", input="2d4375747465723b", output="8b024e4548a56f8c", iv="" }, //Speck64/128 Encryption ECB
            new TestVector () { n=6, alg=5, operationMode = 0, opMode=0, padMode = 0, key="00010203040508090a0b0c0d", input="2075736167652c20686f7765", output="aa798fdebd627871ab094d9e", iv="" }, //Speck96/96 Encryption ECB
            new TestVector () { n=7, alg=6, operationMode = 0, opMode=0, padMode = 0, key="00010203040508090a0b0c0d101112131415", input="7665722c20696e2074696d65", output="e62e2540e47a8a227210f32b", iv="" }, //Speck96/144 Encryption ECB
            new TestVector () { n=8, alg=7, operationMode = 0, opMode=0, padMode = 0, key="000102030405060708090a0b0c0d0e0f", input="206d616465206974206571756976616c", output="180d575cdffe60786532787951985da6", iv="" }, //Speck128/128 Encryption ECB
            new TestVector () { n=9, alg=8, operationMode = 0, opMode=0, padMode = 0, key="000102030405060708090a0b0c0d0e0f1011121314151617", input="656e7420746f20436869656620486172", output="86183ce05d18bcf9665513133acfe41b", iv="" }, //Speck128/192 Encryption ECB
            new TestVector () { n=10, alg=9, operationMode = 0, opMode=0, padMode = 0, key="000102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f", input="706f6f6e65722e20496e2074686f7365", output="438f189c8db4ee4e3ef5c00504010941", iv="" }, //Speck128/256 Encryption ECB

            new TestVector () { n=11, alg=0, operationMode = 0, opMode=1, padMode = 0, key="0001080910111819", input="f24268a8", output="4c697465", iv="" }, //Speck32/64 Decryption ECB 
            new TestVector () { n=12, alg=1, operationMode = 0, opMode=1, padMode = 0, key="00010208090a101112", input="dc5a38a549c0", output="72616c6c7920", iv="" }, //Speck48/72 Decryption ECB
            new TestVector () { n=13, alg=2, operationMode = 0, opMode=1, padMode = 0, key="00010208090a10111218191a", input="5d44b6105e73", output="74686973206d", iv="" }, //Speck48/96 Decryption ECB
            new TestVector () { n=14, alg=3, operationMode = 0, opMode=1, padMode = 0, key="0001020308090a0b10111213", input="6c947541ec52799f", output="65616e7320466174", iv="" }, //Speck64/96 Decryption ECB
            new TestVector () { n=15, alg=4, operationMode = 0, opMode=1, padMode = 0, key="0001020308090a0b1011121318191a1b", input="8b024e4548a56f8c", output="2d4375747465723b", iv="" }, //Speck64/128 Decryption ECB
            new TestVector () { n=16, alg=5, operationMode = 0, opMode=1, padMode = 0, key="00010203040508090a0b0c0d", input="aa798fdebd627871ab094d9e", output="2075736167652c20686f7765", iv="" }, //Speck96/96 Decryption ECB
            new TestVector () { n=17, alg=6, operationMode = 0, opMode=1, padMode = 0, key="00010203040508090a0b0c0d101112131415", input="e62e2540e47a8a227210f32b", output="7665722c20696e2074696d65", iv="" }, //Speck96/144 Decryption ECB
            new TestVector () { n=18, alg=7, operationMode = 0, opMode=1, padMode = 0, key="000102030405060708090a0b0c0d0e0f", input="180d575cdffe60786532787951985da6", output="206d616465206974206571756976616c", iv="" }, //Speck128/128 Decryption ECB
            new TestVector () { n=19, alg=8, operationMode = 0, opMode=1, padMode = 0, key="000102030405060708090a0b0c0d0e0f1011121314151617", input="86183ce05d18bcf9665513133acfe41b", output="656e7420746f20436869656620486172", iv="" }, //Speck128/192 Decryption ECB
            new TestVector () { n=20, alg=9, operationMode = 0, opMode=1, padMode = 0, key="000102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f", input="438f189c8db4ee4e3ef5c00504010941", output="706f6f6e65722e20496e2074686f7365", iv="" } //Speck128/256 Decryption ECB
        };
    }
}

