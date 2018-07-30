using System.Security.AccessControl;
using BlockmodeVisualizer;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using Cryptool.Plugins.BlockmodeVisualizer;
using Cryptool.Plugins.Cryptography.Encryption;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tests.TemplateAndPluginTests;
using BV = Cryptool.Plugins.BlockmodeVisualizer.BlockmodeVisualizer;

namespace UnitTests
{
    /*
     * Source of the test values:
     * ECB, CBC, CFB, OFB, CTR: https://nvlpubs.nist.gov/nistpubs/Legacy/SP/nistspecialpublication800-38a.pdf
     * XTS: https://csrc.nist.gov/CSRC/media/Projects/Cryptographic-Algorithm-Validation-Program/documents/aes/XTSTestVectors.zip
     * CCM: https://csrc.nist.gov/CSRC/media/Projects/Cryptographic-Algorithm-Validation-Program/documents/mac/ccmtestvectors.zip
     * GCM: https://csrc.nist.gov/CSRC/media/Projects/Cryptographic-Algorithm-Validation-Program/documents/mac/gcmtestvectors.zip
     */
    [TestClass]
    public class BlockmodeVisualizerTest
    {
        // Test components
        private BV instance;
        private readonly BlockmodeVisualizerSettings settings;
        private readonly PluginTestScenario scenario;
        private object[] output;

        public BlockmodeVisualizerTest()
        {
            // Set up component
            instance = (BV) TestHelpers.GetPluginInstance("BlockmodeVisualizer");
            settings = (BlockmodeVisualizerSettings) instance.Settings;
            scenario = new PluginTestScenario(instance, new[]
            {
                "TextInput",
                "TagInput",
                "Key",
                "InitializationVector",
                "AssociatedData"
            }, new[]
            {
                "TextOutput",
                "TagOutput"
            });
        }

        [TestInitialize]
        public void SetUp()
        {
            // Set up Blockcipher
            instance.Blockcipher = new AESControl(new AES());
        }

        [TestMethod]
        public void ComputationWithoutBlockcipher()
        {
            // Test parameters
            string plaintext = "6bc1bee22e409f96e93d7e117393172aae2d8a571e03ac9c9eb76fac45af8e5130c81c46a35ce411e5fbc1191a0a52eff69f2445df4f9b17ad2b417be66c3710";
            string tag_in = "2d8a571e03ac9c";
            string key = "2b7e151628aed2a6abf7158809cf4f3c";
            string iv = "000102030405060708090a0b0c0d0e0f";
            string ciphertext = "6bc1bee22e409f96e93d7e117393172aae2d8a571e03ac9c9eb76fac45af8e5130c81c46a35ce411e5fbc1191a0a52eff69f2445df4f9b17ad2b417be66c3710";
            string tag_out = "2d8a571e03ac9c";

            // Prepare component
            instance.Blockcipher = null;
            settings.Blockmode = Blockmodes.GCM;

            // Test encryption
            settings.Action = Actions.ENCRYPTION;
            output = scenario.GetOutputs(new object[] { plaintext.HexToStream(), tag_in.HexToByteArray(), key.HexToByteArray(), iv.HexToByteArray(), null });
            Assert.AreEqual(ciphertext.ToUpper(), output[0].ToHex());
            Assert.AreEqual(tag_out.ToUpper(), output[1].ToHex());

            // Test decryption
            settings.Action = Actions.DECRYPTION;
            output = scenario.GetOutputs(new object[] { ciphertext.HexToStream(), tag_in.HexToByteArray(), key.HexToByteArray(), iv.HexToByteArray(), null });
            Assert.AreEqual(plaintext.ToUpper(), output[0].ToHex());
            Assert.AreEqual(tag_out.ToUpper(), output[1].ToHex());
        }

        [TestMethod]
        public void ECBModeWithFullBlocks()
        {
            // Test parameters
            string plaintext = "6bc1bee22e409f96e93d7e117393172aae2d8a571e03ac9c9eb76fac45af8e5130c81c46a35ce411e5fbc1191a0a52eff69f2445df4f9b17ad2b417be66c3710";
            string key = "2b7e151628aed2a6abf7158809cf4f3c";
            string ciphertext = "3ad77bb40d7a3660a89ecaf32466ef97f5d3d58503b9699de785895a96fdbaaf43b1cd7f598ece23881b00e3ed0306887b0c785e27e8ad3f8223207104725dd4";

            // Prepare component
            settings.Blockmode = Blockmodes.ECB;
            settings.Padding = BlockCipherHelper.PaddingType.None;

            // Test encryption
            settings.Action = Actions.ENCRYPTION;
            output = scenario.GetOutputs(new object[] { plaintext.HexToStream(), null, key.HexToByteArray(), null, null });
            Assert.AreEqual(ciphertext.ToUpper(), output[0].ToHex());

            // Test decryption
            settings.Action = Actions.DECRYPTION;
            output = scenario.GetOutputs(new object[] { ciphertext.HexToStream(), null, key.HexToByteArray(), null, null });
            Assert.AreEqual(plaintext.ToUpper(), output[0].ToHex());
        }

        [TestMethod]
        public void ECBModeWithPartialBlock()
        {
            // Test parameters
            string plaintext = "6bc1bee22e409f96e93d7e117393172aae2d8a571e03ac9c9eb76fac45af8e5130c81c46a35ce411e5fbc1191a0a52eff69f2445df4f9b17";
            string key = "2b7e151628aed2a6abf7158809cf4f3c";
            string ciphertext = "3ad77bb40d7a3660a89ecaf32466ef97f5d3d58503b9699de785895a96fdbaaf43b1cd7f598ece23881b00e3ed0306888d84ff315b5b119caa2bb3086517f39e";

            // Prepare component
            settings.Blockmode = Blockmodes.ECB;
            settings.Padding = BlockCipherHelper.PaddingType.ANSIX923;

            // Test encryption
            settings.Action = Actions.ENCRYPTION;
            output = scenario.GetOutputs(new object[] { plaintext.HexToStream(), null, key.HexToByteArray(), null, null });
            Assert.AreEqual(ciphertext.ToUpper(), output[0].ToHex());

            // Test decryption
            settings.Action = Actions.DECRYPTION;
            output = scenario.GetOutputs(new object[] { ciphertext.HexToStream(), null, key.HexToByteArray(), null, null });
            Assert.AreEqual(plaintext.ToUpper(), output[0].ToHex());
        }

        [TestMethod]
        public void CBCModeWithFullBlocks()
        {
            // Test parameters
            string plaintext = "6bc1bee22e409f96e93d7e117393172aae2d8a571e03ac9c9eb76fac45af8e5130c81c46a35ce411e5fbc1191a0a52eff69f2445df4f9b17ad2b417be66c3710";
            string key = "2b7e151628aed2a6abf7158809cf4f3c";
            string iv = "000102030405060708090a0b0c0d0e0f";
            string ciphertext = "7649abac8119b246cee98e9b12e9197d5086cb9b507219ee95db113a917678b273bed6b8e3c1743b7116e69e222295163ff1caa1681fac09120eca307586e1a7";

            // Prepare component
            settings.Blockmode = Blockmodes.CBC;
            settings.Padding = BlockCipherHelper.PaddingType.None;

            // Test encryption
            settings.Action = Actions.ENCRYPTION;
            output = scenario.GetOutputs(new object[] { plaintext.HexToStream(), null, key.HexToByteArray(), iv.HexToByteArray(), null });
            Assert.AreEqual(ciphertext.ToUpper(), output[0].ToHex());

            // Test decryption
            settings.Action = Actions.DECRYPTION;
            output = scenario.GetOutputs(new object[] { ciphertext.HexToStream(), null, key.HexToByteArray(), iv.HexToByteArray(), null });
            Assert.AreEqual(plaintext.ToUpper(), output[0].ToHex());
        }

        [TestMethod]
        public void CBCModeWithPartialBlock()
        {
            // Test parameters
            string plaintext = "6bc1bee22e409f96e93d7e117393172aae2d8a571e03ac9c9eb76fac45af8e5130c81c46a35ce411e5fbc1191a0a52eff69f2445df4f9b17";
            string key = "2b7e151628aed2a6abf7158809cf4f3c";
            string iv = "000102030405060708090a0b0c0d0e0f";
            string ciphertext = "7649abac8119b246cee98e9b12e9197d5086cb9b507219ee95db113a917678b273bed6b8e3c1743b7116e69e22229516146080ad9bd5b4fd8a6d9981b8cb8677";

            // Prepare component
            settings.Blockmode = Blockmodes.CBC;
            settings.Padding = BlockCipherHelper.PaddingType.ANSIX923;

            // Test encryption
            settings.Action = Actions.ENCRYPTION;
            output = scenario.GetOutputs(new object[] { plaintext.HexToStream(), null, key.HexToByteArray(), iv.HexToByteArray(), null });
            Assert.AreEqual(ciphertext.ToUpper(), output[0].ToHex());

            // Test decryption
            settings.Action = Actions.DECRYPTION;
            output = scenario.GetOutputs(new object[] { ciphertext.HexToStream(), null, key.HexToByteArray(), iv.HexToByteArray(), null });
            Assert.AreEqual(plaintext.ToUpper(), output[0].ToHex());
        }

        [TestMethod]
        public void CFB1Mode()
        {
            // Test parameters
            string plaintext = "6bc1bee22e409f96e93d7e117393172aae2d";
            string key = "2b7e151628aed2a6abf7158809cf4f3c";
            string iv = "000102030405060708090a0b0c0d0e0f";
            string ciphertext = "3b79424c9c0dd436bace9e0ed4586a4f32b9";

            // Prepare component
            settings.Blockmode = Blockmodes.CFB;
            settings.Padding = BlockCipherHelper.PaddingType.None;
            settings.DataSegmentLength = 1;

            // Test encryption
            settings.Action = Actions.ENCRYPTION;
            output = scenario.GetOutputs(new object[] { plaintext.HexToStream(), null, key.HexToByteArray(), iv.HexToByteArray(), null });
            Assert.AreEqual(ciphertext.ToUpper(), output[0].ToHex());

            // Test decryption
            settings.Action = Actions.DECRYPTION;
            output = scenario.GetOutputs(new object[] { ciphertext.HexToStream(), null, key.HexToByteArray(), iv.HexToByteArray(), null });
            Assert.AreEqual(plaintext.ToUpper(), output[0].ToHex());
        }

        [TestMethod]
        public void CFB3ModeWithFullBlocks()
        {
            // Test parameters
            string plaintext = "6bc1bee22e409f96e93d7e117393172aae2d8a571e03ac9c9eb76fac45af";
            string key = "2b7e151628aed2a6abf7158809cf4f3c";
            string iv = "000102030405060708090a0b0c0d0e0f";
            string ciphertext = "3b3fd9389efefde94db3b82a57cf5536aa106edc721502374e733ffd34dd";

            // Prepare component
            settings.Blockmode = Blockmodes.CFB;
            settings.Padding = BlockCipherHelper.PaddingType.None;
            settings.DataSegmentLength = 3;

            // Test encryption
            settings.Action = Actions.ENCRYPTION;
            output = scenario.GetOutputs(new object[] { plaintext.HexToStream(), null, key.HexToByteArray(), iv.HexToByteArray(), null });
            Assert.AreEqual(ciphertext.ToUpper(), output[0].ToHex());

            // Test decryption
            settings.Action = Actions.DECRYPTION;
            output = scenario.GetOutputs(new object[] { ciphertext.HexToStream(), null, key.HexToByteArray(), iv.HexToByteArray(), null });
            Assert.AreEqual(plaintext.ToUpper(), output[0].ToHex());
        }

        [TestMethod]
        public void CFB3ModeWithPartialBlock()
        {
            // Test parameters
            string plaintext = "6bc1bee22e409f96e93d7e117393172aae2d8a571e03ac9c9eb76fac";
            string key = "2b7e151628aed2a6abf7158809cf4f3c";
            string iv = "000102030405060708090a0b0c0d0e0f";
            string ciphertext = "3b3fd9389efefde94db3b82a57cf5536aa106edc721502374e733ffd7170";

            // Prepare component
            settings.Blockmode = Blockmodes.CFB;
            settings.Padding = BlockCipherHelper.PaddingType.ANSIX923;
            settings.DataSegmentLength = 3;

            // Test encryption
            settings.Action = Actions.ENCRYPTION;
            output = scenario.GetOutputs(new object[] { plaintext.HexToStream(), null, key.HexToByteArray(), iv.HexToByteArray(), null });
            Assert.AreEqual(ciphertext.ToUpper(), output[0].ToHex());

            // Test decryption
            settings.Action = Actions.DECRYPTION;
            output = scenario.GetOutputs(new object[] { ciphertext.HexToStream(), null, key.HexToByteArray(), iv.HexToByteArray(), null });
            Assert.AreEqual(plaintext.ToUpper(), output[0].ToHex());
        }

        [TestMethod]
        public void CFB16Mode()
        {
            // Test parameters
            string plaintext = "6bc1bee22e409f96e93d7e117393172aae2d8a571e03ac9c9eb76fac45af8e5130c81c46a35ce411e5fbc1191a0a52eff69f2445df4f9b17ad2b417be66c3710";
            string key = "2b7e151628aed2a6abf7158809cf4f3c";
            string iv = "000102030405060708090a0b0c0d0e0f";
            string ciphertext = "3b3fd92eb72dad20333449f8e83cfb4ac8a64537a0b3a93fcde3cdad9f1ce58b26751f67a3cbb140b1808cf187a4f4dfc04b05357c5d1c0eeac4c66f9ff7f2e6";

            // Prepare component
            settings.Blockmode = Blockmodes.CFB;
            settings.Padding = BlockCipherHelper.PaddingType.None;
            settings.DataSegmentLength = 16;

            // Test encryption
            settings.Action = Actions.ENCRYPTION;
            output = scenario.GetOutputs(new object[] { plaintext.HexToStream(), null, key.HexToByteArray(), iv.HexToByteArray(), null });
            Assert.AreEqual(ciphertext.ToUpper(), output[0].ToHex());

            // Test decryption
            settings.Action = Actions.DECRYPTION;
            output = scenario.GetOutputs(new object[] { ciphertext.HexToStream(), null, key.HexToByteArray(), iv.HexToByteArray(), null });
            Assert.AreEqual(plaintext.ToUpper(), output[0].ToHex());
        }

        [TestMethod]
        public void OFBModeWithFullBlocks()
        {
            // Test parameters
            string plaintext = "6bc1bee22e409f96e93d7e117393172aae2d8a571e03ac9c9eb76fac45af8e5130c81c46a35ce411e5fbc1191a0a52eff69f2445df4f9b17ad2b417be66c3710";
            string key = "2b7e151628aed2a6abf7158809cf4f3c";
            string iv = "000102030405060708090a0b0c0d0e0f";
            string ciphertext = "3b3fd92eb72dad20333449f8e83cfb4a7789508d16918f03f53c52dac54ed8259740051e9c5fecf64344f7a82260edcc304c6528f659c77866a510d9c1d6ae5e";

            // Prepare component
            settings.Blockmode = Blockmodes.OFB;

            // Test encryption
            settings.Action = Actions.ENCRYPTION;
            output = scenario.GetOutputs(new object[] { plaintext.HexToStream(), null, key.HexToByteArray(), iv.HexToByteArray(), null });
            Assert.AreEqual(ciphertext.ToUpper(), output[0].ToHex());

            // Test decryption
            settings.Action = Actions.DECRYPTION;
            output = scenario.GetOutputs(new object[] { ciphertext.HexToStream(), null, key.HexToByteArray(), iv.HexToByteArray(), null });
            Assert.AreEqual(plaintext.ToUpper(), output[0].ToHex());
        }

        [TestMethod]
        public void OFBModeWithPartialBlock()
        {
            // Test parameters
            string plaintext = "6bc1bee22e409f96e93d7e117393172aae2d8a571e03ac9c9eb76fac45af8e5130c81c46a35ce411e5fbc1191a0a52eff69f2445df4f9b17";
            string key = "2b7e151628aed2a6abf7158809cf4f3c";
            string iv = "000102030405060708090a0b0c0d0e0f";
            string ciphertext = "3b3fd92eb72dad20333449f8e83cfb4a7789508d16918f03f53c52dac54ed8259740051e9c5fecf64344f7a82260edcc304c6528f659c778";

            // Prepare component
            settings.Blockmode = Blockmodes.OFB;

            // Test encryption
            settings.Action = Actions.ENCRYPTION;
            output = scenario.GetOutputs(new object[] { plaintext.HexToStream(), null, key.HexToByteArray(), iv.HexToByteArray(), null });
            Assert.AreEqual(ciphertext.ToUpper(), output[0].ToHex());

            // Test decryption
            settings.Action = Actions.DECRYPTION;
            output = scenario.GetOutputs(new object[] { ciphertext.HexToStream(), null, key.HexToByteArray(), iv.HexToByteArray(), null });
            Assert.AreEqual(plaintext.ToUpper(), output[0].ToHex());
        }

        [TestMethod]
        public void CTRModeWithFullBlocks()
        {
            // Test parameters
            string plaintext = "6bc1bee22e409f96e93d7e117393172aae2d8a571e03ac9c9eb76fac45af8e5130c81c46a35ce411e5fbc1191a0a52eff69f2445df4f9b17ad2b417be66c3710";
            string key = "2b7e151628aed2a6abf7158809cf4f3c";
            string iv = "f0f1f2f3f4f5f6f7f8f9fafbfcfdfeff";
            string ciphertext = "874d6191b620e3261bef6864990db6ce9806f66b7970fdff8617187bb9fffdff5ae4df3edbd5d35e5b4f09020db03eab1e031dda2fbe03d1792170a0f3009cee";

            // Prepare component
            settings.Blockmode = Blockmodes.CTR;

            // Test encryption
            settings.Action = Actions.ENCRYPTION;
            output = scenario.GetOutputs(new object[] { plaintext.HexToStream(), null, key.HexToByteArray(), iv.HexToByteArray(), null });
            Assert.AreEqual(ciphertext.ToUpper(), output[0].ToHex());

            // Test decryption
            settings.Action = Actions.DECRYPTION;
            output = scenario.GetOutputs(new object[] { ciphertext.HexToStream(), null, key.HexToByteArray(), iv.HexToByteArray(), null });
            Assert.AreEqual(plaintext.ToUpper(), output[0].ToHex());
        }

        [TestMethod]
        public void CTRModeWithPartialBlock()
        {
            // Test parameters
            string plaintext = "6bc1bee22e409f96e93d7e117393172aae2d8a571e03ac9c9eb76fac45af8e5130c81c46a35ce411e5fbc1191a0a52eff69f2445df4f9b17";
            string key = "2b7e151628aed2a6abf7158809cf4f3c";
            string iv = "f0f1f2f3f4f5f6f7f8f9fafbfcfdfeff";
            string ciphertext = "874d6191b620e3261bef6864990db6ce9806f66b7970fdff8617187bb9fffdff5ae4df3edbd5d35e5b4f09020db03eab1e031dda2fbe03d1";

            // Prepare component
            settings.Blockmode = Blockmodes.CTR;

            // Test encryption
            settings.Action = Actions.ENCRYPTION;
            output = scenario.GetOutputs(new object[] { plaintext.HexToStream(), null, key.HexToByteArray(), iv.HexToByteArray(), null });
            Assert.AreEqual(ciphertext.ToUpper(), output[0].ToHex());

            // Test decryption
            settings.Action = Actions.DECRYPTION;
            output = scenario.GetOutputs(new object[] { ciphertext.HexToStream(), null, key.HexToByteArray(), iv.HexToByteArray(), null });
            Assert.AreEqual(plaintext.ToUpper(), output[0].ToHex());
        }

        [TestMethod]
        public void XTSModeWithFullBlocks()
        {
            // Test parameters
            string plaintext = "401efe5c41cea23da0d33caa946b916c88ad99d65fb8238047597b94bcdb88b7";
            string key = "bf14b298e9c72ca73676915a80fa2fac4fe2b56ebc4df57e3028fd4a41ac9e1c";
            string iv = "5e49263efac5451ee395083c25de2c13";
            string ciphertext = "63a98f178be85688a8a5ce00b25bf08a972d34ece95c6947260e6e44fdbaa357";

            // Prepare component
            settings.Blockmode = Blockmodes.XTS;

            // Test encryption
            settings.Action = Actions.ENCRYPTION;
            output = scenario.GetOutputs(new object[] { plaintext.HexToStream(), null, key.HexToByteArray(), iv.HexToByteArray(), null });
            Assert.AreEqual(ciphertext.ToUpper(), output[0].ToHex());

            // Test decryption
            settings.Action = Actions.DECRYPTION;
            output = scenario.GetOutputs(new object[] { ciphertext.HexToStream(), null, key.HexToByteArray(), iv.HexToByteArray(), null });
            Assert.AreEqual(plaintext.ToUpper(), output[0].ToHex());
        }

        [TestMethod]
        public void XTSModeWithPartialBlock()
        {
            // Test parameters
            string plaintext = "9c0f7eac3b89e76539fcfe16a6beef8140792b0b6b";
            string key = "79353430ac31b76e126a6643ec890f30316e90792b0b6b301f07532a06808ac8";
            string iv = "2ff8262da623ef8b52a9b1bd10d3bca9";
            string ciphertext = "186cef57185b1c5f1afe337f337198d923844d2497";

            // Prepare component
            settings.Blockmode = Blockmodes.XTS;

            // Test encryption
            settings.Action = Actions.ENCRYPTION;
            output = scenario.GetOutputs(new object[] { plaintext.HexToStream(), null, key.HexToByteArray(), iv.HexToByteArray(), null });
            Assert.AreEqual(ciphertext.ToUpper(), output[0].ToHex());

            // Test decryption
            settings.Action = Actions.DECRYPTION;
            output = scenario.GetOutputs(new object[] { ciphertext.HexToStream(), null, key.HexToByteArray(), iv.HexToByteArray(), null });
            Assert.AreEqual(plaintext.ToUpper(), output[0].ToHex());
        }

        [TestMethod]
        public void XTSModeWithTooShortInput()
        {
            // Test parameters
            string plaintext = "401efe";
            string key = "bf14b298e9c72ca73676915a80fa2fac4fe2b56ebc4df57e3028fd4a41ac9e1c";
            string iv = "5e49263efac5451ee395083c25de2c13";
            string ciphertext = "63a9";

            // Prepare component
            settings.Blockmode = Blockmodes.XTS;

            // Test encryption
            settings.Action = Actions.ENCRYPTION;
            output = scenario.GetOutputs(new object[] { plaintext.HexToStream(), null, key.HexToByteArray(), iv.HexToByteArray(), null });
            Assert.AreEqual("", output[0].ToHex());

            // Test decryption
            settings.Action = Actions.DECRYPTION;
            output = scenario.GetOutputs(new object[] { ciphertext.HexToStream(), null, key.HexToByteArray(), iv.HexToByteArray(), null });
            Assert.AreEqual("", output[0].ToHex());
        }

        [TestMethod]
        public void CCMMode()
        {
            // Test parameters
            string plaintext = "6bc1bee22e409f96e93d7e117393172aae2d8a571e03ac9c9eb76fac45af8e5130c81c46a35ce411e5fbc1191a0a52eff69f2445df4f9b17ad2b417be66c3710";
            string ad = "";
            string key = "2b7e151628aed2a6abf7158809cf4f3c";
            string iv = "000102030405060708090a0b0c0d0e0f";
            string ciphertext = "3ff1caa1681fac09120eca307586e1a7";
            string tag = "";

            // Prepare component
            settings.Blockmode = Blockmodes.CCM;
            settings.TagLength = 16;

            // Test encryption
            settings.Action = Actions.ENCRYPTION;
            output = scenario.GetOutputs(new object[] { plaintext.HexToStream(), tag.HexToByteArray(), key.HexToByteArray(), iv.HexToByteArray(), ad.HexToStream() });
            Assert.AreEqual(ciphertext.ToUpper(), output[0].ToHex());
            Assert.AreEqual(tag.ToUpper(), output[1].ToHex());

            // Test decryption
            //settings.Action = Actions.DECRYPTION;
            //output = scenario.GetOutputs(new object[] { ciphertext.HexToStream(), tag.HexToByteArray(), key.HexToByteArray(), iv.HexToByteArray(), ad.HexToStream() });
            //Assert.AreEqual(plaintext.ToUpper(), output[0].ToHex());
            //Assert.AreEqual("", output[1].ToHex());
        }

        [TestMethod]
        public void GCMModeWithNoInputAndNoAssociatedData()
        {
            // Test parameters
            string plaintext = "";
            string ad = "";
            string key = "11754cd72aec309bf52f7687212e8957";
            string iv = "3c819d9a9bed087615030b65";
            string ciphertext = "";
            string tag = "250327c674aaf477aef2675748cf6971";

            // Prepare component
            settings.Blockmode = Blockmodes.GCM;
            settings.TagLength = 16;

            // Test encryption
            settings.Action = Actions.ENCRYPTION;
            output = scenario.GetOutputs(new object[] { plaintext.HexToStream(), tag.HexToByteArray(), key.HexToByteArray(), iv.HexToByteArray(), ad.HexToStream() });
            Assert.AreEqual(ciphertext.ToUpper(), output[0].ToHex());
            Assert.AreEqual(tag.ToUpper(), output[1].ToHex());

            // Test decryption
            settings.Action = Actions.DECRYPTION;
            output = scenario.GetOutputs(new object[] { ciphertext.HexToStream(), tag.HexToByteArray(), key.HexToByteArray(), iv.HexToByteArray(), ad.HexToStream() });
            Assert.AreEqual(plaintext.ToUpper(), output[0].ToHex());
            Assert.AreEqual("", output[1].ToHex());
        }

        [TestMethod]
        public void GCMModeWithNoInputNoAssociatedDataAndShorterTag()
        {
            // Test parameters
            string plaintext = "";
            string ad = "";
            string key = "81b6844aab6a568c4556a2eb7eae752f";
            string iv = "ce600f59618315a6829bef4d";
            string ciphertext = "";
            string tag = "89b43e9dbc1b4f597dbbc7655bb5";

            // Prepare component
            settings.Blockmode = Blockmodes.GCM;
            settings.TagLength = 14;

            // Test encryption
            settings.Action = Actions.ENCRYPTION;
            output = scenario.GetOutputs(new object[] { plaintext.HexToStream(), tag.HexToByteArray(), key.HexToByteArray(), iv.HexToByteArray(), ad.HexToStream() });
            Assert.AreEqual(ciphertext.ToUpper(), output[0].ToHex());
            Assert.AreEqual(tag.ToUpper(), output[1].ToHex());

            // Test decryption
            settings.Action = Actions.DECRYPTION;
            output = scenario.GetOutputs(new object[] { ciphertext.HexToStream(), tag.HexToByteArray(), key.HexToByteArray(), iv.HexToByteArray(), ad.HexToStream() });
            Assert.AreEqual(plaintext.ToUpper(), output[0].ToHex());
            Assert.AreEqual("", output[1].ToHex());
        }

        [TestMethod]
        public void GCMModeWithNoInputNoAssociatedDataAndShorterIV()
        {
            // Test parameters
            string plaintext = "";
            string ad = "";
            string key = "81b6844aab6a568c4556a2eb7eae752f";
            string iv = "ce";
            string ciphertext = "";
            string tag = "0afa9db2584ad47cd033704829962a4a";

            // Prepare component
            settings.Blockmode = Blockmodes.GCM;
            settings.TagLength = 16;

            // Test encryption
            settings.Action = Actions.ENCRYPTION;
            output = scenario.GetOutputs(new object[] { plaintext.HexToStream(), tag.HexToByteArray(), key.HexToByteArray(), iv.HexToByteArray(), ad.HexToStream() });
            Assert.AreEqual(ciphertext.ToUpper(), output[0].ToHex());
            Assert.AreEqual(tag.ToUpper(), output[1].ToHex());

            // Test decryption
            settings.Action = Actions.DECRYPTION;
            output = scenario.GetOutputs(new object[] { ciphertext.HexToStream(), tag.HexToByteArray(), key.HexToByteArray(), iv.HexToByteArray(), ad.HexToStream() });
            Assert.AreEqual(plaintext.ToUpper(), output[0].ToHex());
            Assert.AreEqual("", output[1].ToHex());
        }

        [TestMethod]
        public void GCMModeWithNoInputNoAssociatedDataAndLongerIV()
        {
            // Test parameters
            string plaintext = "";
            string ad = "";
            string key = "1672c3537afa82004c6b8a46f6f0d026";
            string iv = "ce600f59618315a6829bef4dce600f59618315a6829bef4dce600f59618315a6829bef4dce600f59618315a6829bef4dce600f59618315a6829bef4d";
            string ciphertext = "";
            string tag = "92e136da3fd2725c98aa816023174a99";

            // Prepare component
            settings.Blockmode = Blockmodes.GCM;
            settings.TagLength = 16;

            // Test encryption
            settings.Action = Actions.ENCRYPTION;
            output = scenario.GetOutputs(new object[] { plaintext.HexToStream(), tag.HexToByteArray(), key.HexToByteArray(), iv.HexToByteArray(), ad.HexToStream() });
            Assert.AreEqual(ciphertext.ToUpper(), output[0].ToHex());
            Assert.AreEqual(tag.ToUpper(), output[1].ToHex());

            // Test decryption
            settings.Action = Actions.DECRYPTION;
            output = scenario.GetOutputs(new object[] { ciphertext.HexToStream(), tag.HexToByteArray(), key.HexToByteArray(), iv.HexToByteArray(), ad.HexToStream() });
            Assert.AreEqual(plaintext.ToUpper(), output[0].ToHex());
            Assert.AreEqual("", output[1].ToHex());
        }

        [TestMethod]
        public void GCMModeWithInputAndAssociatedData()
        {
            // Test parameters
            string plaintext = "65e621d2f7cbe8e0a66491bd4e85e98988bde56d2ac7f3caaaa56964ea755193244d623bf78e7555395bfe7148cd72de7d91fe";
            string ad = "ec7514f966f9ecf5a97283d484624889c166a323";
            string key = "5900fc1ade14cfc8b828ff98a0af1bbe";
            string iv = "e9ba224d909e2ca4a55b6a6e";
            string ciphertext = "1f9895b0af80638ec9d84292d85bc244796d85d143c1b1991244ba7cf2ec3ef586b7546d74a9a899271cb556f4df666ad49049";
            string tag = "e5d8ce216102c78a3a532dc35fc201d5";

            // Prepare component
            settings.Blockmode = Blockmodes.GCM;
            settings.TagLength = 16;

            // Test encryption
            settings.Action = Actions.ENCRYPTION;
            output = scenario.GetOutputs(new object[] { plaintext.HexToStream(), tag.HexToByteArray(), key.HexToByteArray(), iv.HexToByteArray(), ad.HexToStream() });
            Assert.AreEqual(ciphertext.ToUpper(), output[0].ToHex());
            Assert.AreEqual(tag.ToUpper(), output[1].ToHex());

            // Test decryption
            settings.Action = Actions.DECRYPTION;
            output = scenario.GetOutputs(new object[] { ciphertext.HexToStream(), tag.HexToByteArray(), key.HexToByteArray(), iv.HexToByteArray(), ad.HexToStream() });
            Assert.AreEqual(plaintext.ToUpper(), output[0].ToHex());
            Assert.AreEqual("", output[1].ToHex());
        }

        [TestMethod]
        public void GCMModeWithInvalidTag()
        {
            // Test parameters
            string plaintext = "65e621d2f7cbe8e0a66491bd4e85e98988bde56d2ac7f3caaaa56964ea755193244d623bf78e7555395bfe7148cd72de7d91fe";
            string ad = "ec7514f966f9ecf5a97283d484624889c166a323";
            string key = "5900fc1ade14cfc8b828ff98a0af1bbe";
            string iv = "e9ba224d909e2ca4a55b6a6e";
            string ciphertext = "1f9895b0af80638ec9d84292d85bc244796d85d143c1b1991244ba7cf2ec3ef586b7546d74a9a899271cb556f4df666ad49049";
            string tag = "e5d8ce216102c78a3a532dc35fc201d4";

            // Prepare component
            settings.Blockmode = Blockmodes.GCM;
            settings.TagLength = 16;

            // Test decryption
            settings.Action = Actions.DECRYPTION;
            output = scenario.GetOutputs(new object[] { ciphertext.HexToStream(), tag.HexToByteArray(), key.HexToByteArray(), iv.HexToByteArray(), ad.HexToStream() });
            // 61757468656e7469636174696f6e5f6572726f72 is the hex code of "authentication_error"
            Assert.AreEqual("61757468656e7469636174696f6e5f6572726f72".ToUpper(), output[0].ToHex());
            Assert.AreEqual("", output[1].ToHex());
        }
    }
}