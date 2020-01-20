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
            new TestVector () { n=20, alg=9, operationMode = 0, opMode=1, padMode = 0, key="000102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f", input="438f189c8db4ee4e3ef5c00504010941", output="706f6f6e65722e20496e2074686f7365", iv="" }, //Speck128/256 Decryption ECB

            new TestVector () { n=21, alg=0, operationMode = 1, opMode=0, padMode = 1, key="0001080910111819", input="4c697465", output="3D46C2E2", iv="80af127a" }, //Speck32/64 full block Encryption CBC 
            new TestVector () { n=22, alg=0, operationMode = 1, opMode=0, padMode = 1, key="0001080910111819", input="4c6974", output="EC0D3CEF", iv="80af127a" }, //Speck32/64 partial block Encryption CBC 
            new TestVector () { n=23, alg=1, operationMode = 1, opMode=0, padMode = 1, key="00010208090a101112", input="72616c6c7920", output="3416BC2BF216", iv="80af127a6c79" }, //Speck48/72 full block Encryption CBC
            new TestVector () { n=24, alg=1, operationMode = 1, opMode=0, padMode = 1, key="00010208090a101112", input="72616c6c79", output="0124DB980FEE", iv="80af127a6c79" }, //Speck48/72 partial block Encryption CBC
            new TestVector () { n=25, alg=2, operationMode = 1, opMode=0, padMode = 1, key="00010208090a10111218191a", input="74686973206d", output="FAE00E1DFFB4", iv="80af127a6c79" }, //Speck48/96 full block Encryption CBC
            new TestVector () { n=26, alg=2, operationMode = 1, opMode=0, padMode = 1, key="00010208090a10111218191a", input="7468697320", output="5B055B0B7A8C", iv="80af127a6c79" }, //Speck48/96 partial block Encryption CBC
            new TestVector () { n=27, alg=3, operationMode = 1, opMode=0, padMode = 1, key="0001020308090a0b10111213", input="65616e7320466174", output="A557811192F732D6", iv="80af127a6c798a17" }, //Speck64/96 full block Encryption CBC
            new TestVector () { n=28, alg=3, operationMode = 1, opMode=0, padMode = 1, key="0001020308090a0b10111213", input="65616e73204661", output="48D67FA83FDA602D", iv="80af127a6c798a17" }, //Speck64/96 partial block Encryption CBC
            new TestVector () { n=29, alg=4, operationMode = 1, opMode=0, padMode = 1, key="0001020308090a0b1011121318191a1b", input="2d4375747465723b", output="F845E9A81758A527", iv="80af127a6c798a17" }, //Speck64/128 full block Encryption CBC
            new TestVector () { n=30, alg=4, operationMode = 1, opMode=0, padMode = 1, key="0001020308090a0b1011121318191a1b", input="2d437574746572", output="24DB95192E354EDB", iv="80af127a6c798a17" }, //Speck64/128 partial block Encryption CBC
            new TestVector () { n=31, alg=5, operationMode = 1, opMode=0, padMode = 1, key="00010203040508090a0b0c0d", input="2075736167652c20686f7765", output="71B802BAE7813C4F69FC53CA", iv="80af127a6c798a17971ad51f" }, //Speck96/96 full block Encryption CBC
            new TestVector () { n=32, alg=5, operationMode = 1, opMode=0, padMode = 1, key="00010203040508090a0b0c0d", input="2075736167652c20686f77", output="878F4787E595BF8224820BD8", iv="80af127a6c798a17971ad51f" }, //Speck96/96 partial block Encryption CBC
            new TestVector () { n=33, alg=6, operationMode = 1, opMode=0, padMode = 1, key="00010203040508090a0b0c0d101112131415", input="7665722c20696e2074696d65", output="9D6A9BE0551FFA7D248EEAAE", iv="80af127a6c798a17971ad51f" }, //Speck96/144 full block Encryption CBC
            new TestVector () { n=34, alg=6, operationMode = 1, opMode=0, padMode = 1, key="00010203040508090a0b0c0d101112131415", input="7665722c20696e2074696d", output="FAD0B2247FFA216AD2398DE3", iv="80af127a6c798a17971ad51f" }, //Speck96/144 partial block Encryption CBC
            new TestVector () { n=35, alg=7, operationMode = 1, opMode=0, padMode = 1, key="000102030405060708090a0b0c0d0e0f", input="206d616465206974206571756976616c", output="760E253656F1DFA5493F5D3471028E70", iv="80af127a6c798a17971ad51fafde151b" }, //Speck128/128 full block Encryption CBC
            new TestVector () { n=36, alg=7, operationMode = 1, opMode=0, padMode = 1, key="000102030405060708090a0b0c0d0e0f", input="206d61646520697420657175697661", output="E2EF0F4FB958D274B183AB96DBE78EA7", iv="80af127a6c798a17971ad51fafde151b" }, //Speck128/128 partial block Encryption CBC
            new TestVector () { n=37, alg=8, operationMode = 1, opMode=0, padMode = 1, key="000102030405060708090a0b0c0d0e0f1011121314151617", input="656e7420746f20436869656620486172", output="3EA46146090239FEADE6BB20F6101770", iv="80af127a6c798a17971ad51fafde151b" }, //Speck128/192 full block Encryption CBC
            new TestVector () { n=38, alg=8, operationMode = 1, opMode=0, padMode = 1, key="000102030405060708090a0b0c0d0e0f1011121314151617", input="656e7420746f204368696566204861", output="C3D673F06F0B4E4CE2CD5D34BBB7EAC8", iv="80af127a6c798a17971ad51fafde151b" }, //Speck128/192 partial block Encryption CBC
            new TestVector () { n=39, alg=9, operationMode = 1, opMode=0, padMode = 1, key="000102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f", input="706f6f6e65722e20496e2074686f7365", output="733518941C2B92ECD9E59A7C9E485921", iv="80af127a6c798a17971ad51fafde151b" }, //Speck128/256 full block Encryption CBC
            new TestVector () { n=40, alg=9, operationMode = 1, opMode=0, padMode = 1, key="000102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f", input="706f6f6e65722e20496e2074686f73", output="6C5AD838AEF9BF32C29EA172678EBAA0", iv="80af127a6c798a17971ad51fafde151b" }, //Speck128/256 full block Encryption CBC
            
            new TestVector () { n=41, alg=0, operationMode = 1, opMode=1, padMode = 1, key="0001080910111819", input="3D46C2E2", output="4c697465", iv="80af127a" }, //Speck32/64 full block Decryption CBC 
            new TestVector () { n=42, alg=0, operationMode = 1, opMode=1, padMode = 1, key="0001080910111819", input="EC0D3CEF", output="4c6974", iv="80af127a" }, //Speck32/64 partial block Decryption CBC 
            new TestVector () { n=43, alg=1, operationMode = 1, opMode=1, padMode = 1, key="00010208090a101112", input="3416BC2BF216", output="72616c6c7920", iv="80af127a6c79" }, //Speck48/72 full block Decryption CBC
            new TestVector () { n=44, alg=1, operationMode = 1, opMode=1, padMode = 1, key="00010208090a101112", input="0124DB980FEE", output="72616c6c79", iv="80af127a6c79" }, //Speck48/72 partial block Decryption CBC
            new TestVector () { n=45, alg=2, operationMode = 1, opMode=1, padMode = 1, key="00010208090a10111218191a", input="FAE00E1DFFB4", output="74686973206d", iv="80af127a6c79" }, //Speck48/96 full block Decryption CBC
            new TestVector () { n=46, alg=2, operationMode = 1, opMode=1, padMode = 1, key="00010208090a10111218191a", input="5B055B0B7A8C", output="7468697320", iv="80af127a6c79" }, //Speck48/96 partial block Decryption CBC
            new TestVector () { n=47, alg=3, operationMode = 1, opMode=1, padMode = 1, key="0001020308090a0b10111213", input="A557811192F732D6", output="65616e7320466174", iv="80af127a6c798a17" }, //Speck64/96 full block Decryption CBC
            new TestVector () { n=48, alg=3, operationMode = 1, opMode=1, padMode = 1, key="0001020308090a0b10111213", input="48D67FA83FDA602D", output="65616e73204661", iv="80af127a6c798a17" }, //Speck64/96 partial block Decryption CBC
            new TestVector () { n=49, alg=4, operationMode = 1, opMode=1, padMode = 1, key="0001020308090a0b1011121318191a1b", input="F845E9A81758A527", output="2d4375747465723b", iv="80af127a6c798a17" }, //Speck64/128 full block Decryption CBC
            new TestVector () { n=50, alg=4, operationMode = 1, opMode=1, padMode = 1, key="0001020308090a0b1011121318191a1b", input="24DB95192E354EDB", output="2d437574746572", iv="80af127a6c798a17" }, //Speck64/128 partial block Decryption CBC
            new TestVector () { n=51, alg=5, operationMode = 1, opMode=1, padMode = 1, key="00010203040508090a0b0c0d", input="71B802BAE7813C4F69FC53CA", output="2075736167652c20686f7765", iv="80af127a6c798a17971ad51f" }, //Speck96/96 full block Decryption CBC
            new TestVector () { n=52, alg=5, operationMode = 1, opMode=1, padMode = 1, key="00010203040508090a0b0c0d", input="878F4787E595BF8224820BD8", output="2075736167652c20686f77", iv="80af127a6c798a17971ad51f" }, //Speck96/96 partial block Decryption CBC
            new TestVector () { n=53, alg=6, operationMode = 1, opMode=1, padMode = 1, key="00010203040508090a0b0c0d101112131415", input="9D6A9BE0551FFA7D248EEAAE", output="7665722c20696e2074696d65", iv="80af127a6c798a17971ad51f" }, //Speck96/144 full block Decryption CBC
            new TestVector () { n=54, alg=6, operationMode = 1, opMode=1, padMode = 1, key="00010203040508090a0b0c0d101112131415", input="FAD0B2247FFA216AD2398DE3", output="7665722c20696e2074696d", iv="80af127a6c798a17971ad51f" }, //Speck96/144 partial block Decryption CBC
            new TestVector () { n=55, alg=7, operationMode = 1, opMode=1, padMode = 1, key="000102030405060708090a0b0c0d0e0f", input="760E253656F1DFA5493F5D3471028E70", output="206d616465206974206571756976616c", iv="80af127a6c798a17971ad51fafde151b" }, //Speck128/128 full block Decryption CBC
            new TestVector () { n=56, alg=7, operationMode = 1, opMode=1, padMode = 1, key="000102030405060708090a0b0c0d0e0f", input="E2EF0F4FB958D274B183AB96DBE78EA7", output="206d61646520697420657175697661", iv="80af127a6c798a17971ad51fafde151b" }, //Speck128/128 partial block Decryption CBC
            new TestVector () { n=57, alg=8, operationMode = 1, opMode=1, padMode = 1, key="000102030405060708090a0b0c0d0e0f1011121314151617", input="3EA46146090239FEADE6BB20F6101770", output="656e7420746f20436869656620486172", iv="80af127a6c798a17971ad51fafde151b" }, //Speck128/192 full block Decryption CBC
            new TestVector () { n=58, alg=8, operationMode = 1, opMode=1, padMode = 1, key="000102030405060708090a0b0c0d0e0f1011121314151617", input="C3D673F06F0B4E4CE2CD5D34BBB7EAC8", output="656e7420746f204368696566204861", iv="80af127a6c798a17971ad51fafde151b" }, //Speck128/192 partial block Decryption CBC
            new TestVector () { n=59, alg=9, operationMode = 1, opMode=1, padMode = 1, key="000102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f", input="733518941C2B92ECD9E59A7C9E485921", output="706f6f6e65722e20496e2074686f7365", iv="80af127a6c798a17971ad51fafde151b" }, //Speck128/256 full block Decryption CBC
            new TestVector () { n=60, alg=9, operationMode = 1, opMode=1, padMode = 1, key="000102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f", input="6C5AD838AEF9BF32C29EA172678EBAA0", output="706f6f6e65722e20496e2074686f73", iv="80af127a6c798a17971ad51fafde151b" }, //Speck128/256 full block Decryption CBC
        };
    }
}

