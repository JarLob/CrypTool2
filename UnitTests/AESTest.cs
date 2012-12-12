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
    public class AESTest
    {
        public AESTest()
        {
        }

        [TestMethod]
        public void AESTestMethod()
        {
            var pluginInstance = TestHelpers.GetPluginInstance("AES");
            var scenario = new PluginTestScenario(pluginInstance, new[] { "InputStream", "InputIV", "InputKey", ".Action", ".Blocksize", ".Keysize", ".CryptoAlgorithm", ".Mode", ".Padding", ".Keysize" }, new[] { "OutputStream" });

            foreach (TestVector vector in testvectors)
            {
                object[] output = scenario.GetOutputs(new object[] { vector.input.HexToStream(), vector.IV.HexToByteArray(), vector.key.HexToByteArray(), vector.mode, 0, (vector.key.Length * 4 - 128) / 64, vector.alg, vector.chainmode, 0, vector.keysize });
                Assert.AreEqual(vector.output.ToUpper(), output[0].ToHex(), "Unexpected value in test #" + vector.n + ".");
            }
        }

        struct TestVector
        {
            public string key, IV, input, output;
            public int n, mode, alg, chainmode, keysize;
        }

        //
        // Source of the test vectors: http://csrc.nist.gov/groups/STM/cavp/documents/aes/KAT_AES.zip
        //
        TestVector[] testvectors = new TestVector[] {
            new TestVector () { n=46, mode=0, key="0000000000000000000000000000000000000000000000000000000000000000", IV="80000000000000000000000000000000", input="00000000000000000000000000000000", output="ddc6bf790c15760d8d9aeb6f9a75fd4e", alg=0, chainmode=3, keysize=2 }, // CFB128VarTxt256.rsp
            new TestVector () { n=0, mode=0, key="00000000000000000000000000000000", IV="00000000000000000000000000000000", input="f34481ec3cc627bacd5dc3fb08f273e6", output="0336763e966d92595a567cc9ce537f5e", alg=0, chainmode=0, keysize=0 }, // CBCGFSbox128.rsp
            new TestVector () { n=1, mode=1, key="00000000000000000000000000000000", IV="00000000000000000000000000000000", input="0336763e966d92595a567cc9ce537f5e", output="f34481ec3cc627bacd5dc3fb08f273e6", alg=0, chainmode=0, keysize=0 }, // CBCGFSbox128.rsp
            new TestVector () { n=2, mode=0, key="000000000000000000000000000000000000000000000000", IV="00000000000000000000000000000000", input="1b077a6af4b7f98229de786d7516b639", output="275cfc0413d8ccb70513c3859b1d0f72", alg=0, chainmode=0, keysize=1 }, // CBCGFSbox192.rsp
            new TestVector () { n=3, mode=1, key="000000000000000000000000000000000000000000000000", IV="00000000000000000000000000000000", input="275cfc0413d8ccb70513c3859b1d0f72", output="1b077a6af4b7f98229de786d7516b639", alg=0, chainmode=0, keysize=1 }, // CBCGFSbox192.rsp
            new TestVector () { n=4, mode=0, key="0000000000000000000000000000000000000000000000000000000000000000", IV="00000000000000000000000000000000", input="014730f80ac625fe84f026c60bfd547d", output="5c9d844ed46f9885085e5d6a4f94c7d7", alg=0, chainmode=0, keysize=2 }, // CBCGFSbox256.rsp
            new TestVector () { n=5, mode=1, key="0000000000000000000000000000000000000000000000000000000000000000", IV="00000000000000000000000000000000", input="5c9d844ed46f9885085e5d6a4f94c7d7", output="014730f80ac625fe84f026c60bfd547d", alg=0, chainmode=0, keysize=2 }, // CBCGFSbox256.rsp
            new TestVector () { n=6, mode=0, key="10a58869d74be5a374cf867cfb473859", IV="00000000000000000000000000000000", input="00000000000000000000000000000000", output="6d251e6944b051e04eaa6fb4dbf78465", alg=0, chainmode=0, keysize=0 }, // CBCKeySbox128.rsp
            new TestVector () { n=7, mode=1, key="10a58869d74be5a374cf867cfb473859", IV="00000000000000000000000000000000", input="6d251e6944b051e04eaa6fb4dbf78465", output="00000000000000000000000000000000", alg=0, chainmode=0, keysize=0 }, // CBCKeySbox128.rsp
            new TestVector () { n=8, mode=0, key="e9f065d7c13573587f7875357dfbb16c53489f6a4bd0f7cd", IV="00000000000000000000000000000000", input="00000000000000000000000000000000", output="0956259c9cd5cfd0181cca53380cde06", alg=0, chainmode=0, keysize=1 }, // CBCKeySbox192.rsp
            new TestVector () { n=9, mode=1, key="e9f065d7c13573587f7875357dfbb16c53489f6a4bd0f7cd", IV="00000000000000000000000000000000", input="0956259c9cd5cfd0181cca53380cde06", output="00000000000000000000000000000000", alg=0, chainmode=0, keysize=1 }, // CBCKeySbox192.rsp
            new TestVector () { n=10, mode=0, key="c47b0294dbbbee0fec4757f22ffeee3587ca4730c3d33b691df38bab076bc558", IV="00000000000000000000000000000000", input="00000000000000000000000000000000", output="46f2fb342d6f0ab477476fc501242c5f", alg=0, chainmode=0, keysize=2 }, // CBCKeySbox256.rsp
            new TestVector () { n=11, mode=1, key="c47b0294dbbbee0fec4757f22ffeee3587ca4730c3d33b691df38bab076bc558", IV="00000000000000000000000000000000", input="46f2fb342d6f0ab477476fc501242c5f", output="00000000000000000000000000000000", alg=0, chainmode=0, keysize=2 }, // CBCKeySbox256.rsp
            new TestVector () { n=12, mode=0, key="80000000000000000000000000000000", IV="00000000000000000000000000000000", input="00000000000000000000000000000000", output="0edd33d3c621e546455bd8ba1418bec8", alg=0, chainmode=0, keysize=0 }, // CBCVarKey128.rsp
            new TestVector () { n=13, mode=1, key="80000000000000000000000000000000", IV="00000000000000000000000000000000", input="0edd33d3c621e546455bd8ba1418bec8", output="00000000000000000000000000000000", alg=0, chainmode=0, keysize=0 }, // CBCVarKey128.rsp
            new TestVector () { n=14, mode=0, key="800000000000000000000000000000000000000000000000", IV="00000000000000000000000000000000", input="00000000000000000000000000000000", output="de885dc87f5a92594082d02cc1e1b42c", alg=0, chainmode=0, keysize=1 }, // CBCVarKey192.rsp
            new TestVector () { n=15, mode=1, key="800000000000000000000000000000000000000000000000", IV="00000000000000000000000000000000", input="de885dc87f5a92594082d02cc1e1b42c", output="00000000000000000000000000000000", alg=0, chainmode=0, keysize=1 }, // CBCVarKey192.rsp
            new TestVector () { n=16, mode=0, key="8000000000000000000000000000000000000000000000000000000000000000", IV="00000000000000000000000000000000", input="00000000000000000000000000000000", output="e35a6dcb19b201a01ebcfa8aa22b5759", alg=0, chainmode=0, keysize=2 }, // CBCVarKey256.rsp
            new TestVector () { n=17, mode=1, key="8000000000000000000000000000000000000000000000000000000000000000", IV="00000000000000000000000000000000", input="e35a6dcb19b201a01ebcfa8aa22b5759", output="00000000000000000000000000000000", alg=0, chainmode=0, keysize=2 }, // CBCVarKey256.rsp
            new TestVector () { n=18, mode=0, key="00000000000000000000000000000000", IV="00000000000000000000000000000000", input="80000000000000000000000000000000", output="3ad78e726c1ec02b7ebfe92b23d9ec34", alg=0, chainmode=0, keysize=0 }, // CBCVarTxt128.rsp
            new TestVector () { n=19, mode=1, key="00000000000000000000000000000000", IV="00000000000000000000000000000000", input="3ad78e726c1ec02b7ebfe92b23d9ec34", output="80000000000000000000000000000000", alg=0, chainmode=0, keysize=0 }, // CBCVarTxt128.rsp
            new TestVector () { n=20, mode=0, key="000000000000000000000000000000000000000000000000", IV="00000000000000000000000000000000", input="80000000000000000000000000000000", output="6cd02513e8d4dc986b4afe087a60bd0c", alg=0, chainmode=0, keysize=1 }, // CBCVarTxt192.rsp
            new TestVector () { n=21, mode=1, key="000000000000000000000000000000000000000000000000", IV="00000000000000000000000000000000", input="6cd02513e8d4dc986b4afe087a60bd0c", output="80000000000000000000000000000000", alg=0, chainmode=0, keysize=1 }, // CBCVarTxt192.rsp
            new TestVector () { n=22, mode=0, key="0000000000000000000000000000000000000000000000000000000000000000", IV="00000000000000000000000000000000", input="80000000000000000000000000000000", output="ddc6bf790c15760d8d9aeb6f9a75fd4e", alg=0, chainmode=0, keysize=2 }, // CBCVarTxt256.rsp
            new TestVector () { n=23, mode=1, key="0000000000000000000000000000000000000000000000000000000000000000", IV="00000000000000000000000000000000", input="ddc6bf790c15760d8d9aeb6f9a75fd4e", output="80000000000000000000000000000000", alg=0, chainmode=0, keysize=2 }, // CBCVarTxt256.rsp
            new TestVector () { n=24, mode=0, key="00000000000000000000000000000000", IV="f34481ec3cc627bacd5dc3fb08f273e6", input="00000000000000000000000000000000", output="0336763e966d92595a567cc9ce537f5e", alg=0, chainmode=3, keysize=0 }, // CFB128GFSbox128.rsp
            new TestVector () { n=25, mode=1, key="00000000000000000000000000000000", IV="f34481ec3cc627bacd5dc3fb08f273e6", input="0336763e966d92595a567cc9ce537f5e", output="00000000000000000000000000000000", alg=0, chainmode=3, keysize=0 }, // CFB128GFSbox128.rsp
            new TestVector () { n=26, mode=0, key="000000000000000000000000000000000000000000000000", IV="1b077a6af4b7f98229de786d7516b639", input="00000000000000000000000000000000", output="275cfc0413d8ccb70513c3859b1d0f72", alg=0, chainmode=3, keysize=1 }, // CFB128GFSbox192.rsp
            new TestVector () { n=27, mode=1, key="000000000000000000000000000000000000000000000000", IV="1b077a6af4b7f98229de786d7516b639", input="275cfc0413d8ccb70513c3859b1d0f72", output="00000000000000000000000000000000", alg=0, chainmode=3, keysize=1 }, // CFB128GFSbox192.rsp
            new TestVector () { n=28, mode=0, key="0000000000000000000000000000000000000000000000000000000000000000", IV="014730f80ac625fe84f026c60bfd547d", input="00000000000000000000000000000000", output="5c9d844ed46f9885085e5d6a4f94c7d7", alg=0, chainmode=3, keysize=2 }, // CFB128GFSbox256.rsp
            new TestVector () { n=29, mode=1, key="0000000000000000000000000000000000000000000000000000000000000000", IV="014730f80ac625fe84f026c60bfd547d", input="5c9d844ed46f9885085e5d6a4f94c7d7", output="00000000000000000000000000000000", alg=0, chainmode=3, keysize=2 }, // CFB128GFSbox256.rsp
            new TestVector () { n=30, mode=0, key="10a58869d74be5a374cf867cfb473859", IV="00000000000000000000000000000000", input="00000000000000000000000000000000", output="6d251e6944b051e04eaa6fb4dbf78465", alg=0, chainmode=3, keysize=0 }, // CFB128KeySbox128.rsp
            new TestVector () { n=31, mode=1, key="10a58869d74be5a374cf867cfb473859", IV="00000000000000000000000000000000", input="6d251e6944b051e04eaa6fb4dbf78465", output="00000000000000000000000000000000", alg=0, chainmode=3, keysize=0 }, // CFB128KeySbox128.rsp
            new TestVector () { n=32, mode=0, key="e9f065d7c13573587f7875357dfbb16c53489f6a4bd0f7cd", IV="00000000000000000000000000000000", input="00000000000000000000000000000000", output="0956259c9cd5cfd0181cca53380cde06", alg=0, chainmode=0, keysize=1 }, // CFB128KeySbox192.rsp
            new TestVector () { n=33, mode=1, key="e9f065d7c13573587f7875357dfbb16c53489f6a4bd0f7cd", IV="00000000000000000000000000000000", input="0956259c9cd5cfd0181cca53380cde06", output="00000000000000000000000000000000", alg=0, chainmode=0, keysize=1 }, // CFB128KeySbox192.rsp
            new TestVector () { n=34, mode=0, key="c47b0294dbbbee0fec4757f22ffeee3587ca4730c3d33b691df38bab076bc558", IV="00000000000000000000000000000000", input="00000000000000000000000000000000", output="46f2fb342d6f0ab477476fc501242c5f", alg=0, chainmode=0, keysize=2 }, // CFB128KeySbox256.rsp
            new TestVector () { n=35, mode=1, key="c47b0294dbbbee0fec4757f22ffeee3587ca4730c3d33b691df38bab076bc558", IV="00000000000000000000000000000000", input="46f2fb342d6f0ab477476fc501242c5f", output="00000000000000000000000000000000", alg=0, chainmode=0, keysize=2 }, // CFB128KeySbox256.rsp
            new TestVector () { n=36, mode=0, key="80000000000000000000000000000000", IV="00000000000000000000000000000000", input="00000000000000000000000000000000", output="0edd33d3c621e546455bd8ba1418bec8", alg=0, chainmode=0, keysize=0 }, // CFB128VarKey128.rsp
            new TestVector () { n=37, mode=1, key="80000000000000000000000000000000", IV="00000000000000000000000000000000", input="0edd33d3c621e546455bd8ba1418bec8", output="00000000000000000000000000000000", alg=0, chainmode=0, keysize=0 }, // CFB128VarKey128.rsp
            new TestVector () { n=38, mode=0, key="800000000000000000000000000000000000000000000000", IV="00000000000000000000000000000000", input="00000000000000000000000000000000", output="de885dc87f5a92594082d02cc1e1b42c", alg=0, chainmode=0, keysize=1 }, // CFB128VarKey192.rsp
            new TestVector () { n=39, mode=1, key="800000000000000000000000000000000000000000000000", IV="00000000000000000000000000000000", input="de885dc87f5a92594082d02cc1e1b42c", output="00000000000000000000000000000000", alg=0, chainmode=0, keysize=1 }, // CFB128VarKey192.rsp
            new TestVector () { n=40, mode=0, key="8000000000000000000000000000000000000000000000000000000000000000", IV="00000000000000000000000000000000", input="00000000000000000000000000000000", output="e35a6dcb19b201a01ebcfa8aa22b5759", alg=0, chainmode=0, keysize=2 }, // CFB128VarKey256.rsp
            new TestVector () { n=41, mode=1, key="8000000000000000000000000000000000000000000000000000000000000000", IV="00000000000000000000000000000000", input="e35a6dcb19b201a01ebcfa8aa22b5759", output="00000000000000000000000000000000", alg=0, chainmode=0, keysize=2 }, // CFB128VarKey256.rsp
            new TestVector () { n=42, mode=0, key="00000000000000000000000000000000", IV="80000000000000000000000000000000", input="00000000000000000000000000000000", output="3ad78e726c1ec02b7ebfe92b23d9ec34", alg=0, chainmode=3, keysize=0 }, // CFB128VarTxt128.rsp
            new TestVector () { n=43, mode=1, key="00000000000000000000000000000000", IV="80000000000000000000000000000000", input="3ad78e726c1ec02b7ebfe92b23d9ec34", output="00000000000000000000000000000000", alg=0, chainmode=3, keysize=0 }, // CFB128VarTxt128.rsp
            new TestVector () { n=44, mode=0, key="000000000000000000000000000000000000000000000000", IV="80000000000000000000000000000000", input="00000000000000000000000000000000", output="6cd02513e8d4dc986b4afe087a60bd0c", alg=0, chainmode=3, keysize=1 }, // CFB128VarTxt192.rsp
            new TestVector () { n=45, mode=1, key="000000000000000000000000000000000000000000000000", IV="80000000000000000000000000000000", input="6cd02513e8d4dc986b4afe087a60bd0c", output="00000000000000000000000000000000", alg=0, chainmode=3, keysize=1 }, // CFB128VarTxt192.rsp
            new TestVector () { n=46, mode=0, key="0000000000000000000000000000000000000000000000000000000000000000", IV="80000000000000000000000000000000", input="00000000000000000000000000000000", output="ddc6bf790c15760d8d9aeb6f9a75fd4e", alg=0, chainmode=3, keysize=2 }, // CFB128VarTxt256.rsp
            new TestVector () { n=47, mode=1, key="0000000000000000000000000000000000000000000000000000000000000000", IV="80000000000000000000000000000000", input="ddc6bf790c15760d8d9aeb6f9a75fd4e", output="00000000000000000000000000000000", alg=0, chainmode=3, keysize=2 }, // CFB128VarTxt256.rsp
            //new TestVector () { n=48, mode=0, key="00000000000000000000000000000000", IV="f34481ec3cc627bacd5dc3fb08f273e6", input="0", output="0", alg=0, chainmode=0, keysize=0 }, // CFB1GFSbox128.rsp
            //new TestVector () { n=49, mode=1, key="00000000000000000000000000000000", IV="f34481ec3cc627bacd5dc3fb08f273e6", input="0", output="0", alg=0, chainmode=0, keysize=0 }, // CFB1GFSbox128.rsp
            //new TestVector () { n=50, mode=0, key="000000000000000000000000000000000000000000000000", IV="1b077a6af4b7f98229de786d7516b639", input="0", output="0", alg=0, chainmode=0, keysize=1 }, // CFB1GFSbox192.rsp
            //new TestVector () { n=51, mode=1, key="000000000000000000000000000000000000000000000000", IV="1b077a6af4b7f98229de786d7516b639", input="0", output="0", alg=0, chainmode=0, keysize=1 }, // CFB1GFSbox192.rsp
            //new TestVector () { n=52, mode=0, key="0000000000000000000000000000000000000000000000000000000000000000", IV="014730f80ac625fe84f026c60bfd547d", input="0", output="0", alg=0, chainmode=0, keysize=2 }, // CFB1GFSbox256.rsp
            //new TestVector () { n=53, mode=1, key="0000000000000000000000000000000000000000000000000000000000000000", IV="014730f80ac625fe84f026c60bfd547d", input="0", output="0", alg=0, chainmode=0, keysize=2 }, // CFB1GFSbox256.rsp
            //new TestVector () { n=54, mode=0, key="10a58869d74be5a374cf867cfb473859", IV="00000000000000000000000000000000", input="0", output="0", alg=0, chainmode=0, keysize=0 }, // CFB1KeySbox128.rsp
            //new TestVector () { n=55, mode=1, key="10a58869d74be5a374cf867cfb473859", IV="00000000000000000000000000000000", input="0", output="0", alg=0, chainmode=0, keysize=0 }, // CFB1KeySbox128.rsp
            //new TestVector () { n=56, mode=0, key="e9f065d7c13573587f7875357dfbb16c53489f6a4bd0f7cd", IV="00000000000000000000000000000000", input="0", output="0", alg=0, chainmode=0, keysize=1 }, // CFB1KeySbox192.rsp
            //new TestVector () { n=57, mode=1, key="e9f065d7c13573587f7875357dfbb16c53489f6a4bd0f7cd", IV="00000000000000000000000000000000", input="0", output="0", alg=0, chainmode=0, keysize=1 }, // CFB1KeySbox192.rsp
            //new TestVector () { n=58, mode=0, key="c47b0294dbbbee0fec4757f22ffeee3587ca4730c3d33b691df38bab076bc558", IV="00000000000000000000000000000000", input="0", output="0", alg=0, chainmode=0, keysize=2 }, // CFB1KeySbox256.rsp
            //new TestVector () { n=59, mode=1, key="c47b0294dbbbee0fec4757f22ffeee3587ca4730c3d33b691df38bab076bc558", IV="00000000000000000000000000000000", input="0", output="0", alg=0, chainmode=0, keysize=2 }, // CFB1KeySbox256.rsp
            //new TestVector () { n=60, mode=0, key="80000000000000000000000000000000", IV="00000000000000000000000000000000", input="0", output="0", alg=0, chainmode=0, keysize=0 }, // CFB1VarKey128.rsp
            //new TestVector () { n=61, mode=1, key="80000000000000000000000000000000", IV="00000000000000000000000000000000", input="0", output="0", alg=0, chainmode=0, keysize=0 }, // CFB1VarKey128.rsp
            //new TestVector () { n=62, mode=0, key="800000000000000000000000000000000000000000000000", IV="00000000000000000000000000000000", input="0", output="1", alg=0, chainmode=0, keysize=1 }, // CFB1VarKey192.rsp
            //new TestVector () { n=63, mode=1, key="800000000000000000000000000000000000000000000000", IV="00000000000000000000000000000000", input="1", output="0", alg=0, chainmode=0, keysize=1 }, // CFB1VarKey192.rsp
            //new TestVector () { n=64, mode=0, key="8000000000000000000000000000000000000000000000000000000000000000", IV="00000000000000000000000000000000", input="0", output="1", alg=0, chainmode=0, keysize=2 }, // CFB1VarKey256.rsp
            //new TestVector () { n=65, mode=1, key="8000000000000000000000000000000000000000000000000000000000000000", IV="00000000000000000000000000000000", input="1", output="0", alg=0, chainmode=0, keysize=2 }, // CFB1VarKey256.rsp
            //new TestVector () { n=66, mode=0, key="00000000000000000000000000000000", IV="80000000000000000000000000000000", input="0", output="0", alg=0, chainmode=0, keysize=0 }, // CFB1VarTxt128.rsp
            //new TestVector () { n=67, mode=1, key="00000000000000000000000000000000", IV="80000000000000000000000000000000", input="0", output="0", alg=0, chainmode=0, keysize=0 }, // CFB1VarTxt128.rsp
            //new TestVector () { n=68, mode=0, key="000000000000000000000000000000000000000000000000", IV="80000000000000000000000000000000", input="0", output="0", alg=0, chainmode=0, keysize=1 }, // CFB1VarTxt192.rsp
            //new TestVector () { n=69, mode=1, key="000000000000000000000000000000000000000000000000", IV="80000000000000000000000000000000", input="0", output="0", alg=0, chainmode=0, keysize=1 }, // CFB1VarTxt192.rsp
            //new TestVector () { n=70, mode=0, key="0000000000000000000000000000000000000000000000000000000000000000", IV="80000000000000000000000000000000", input="0", output="1", alg=0, chainmode=0, keysize=2 }, // CFB1VarTxt256.rsp
            //new TestVector () { n=71, mode=1, key="0000000000000000000000000000000000000000000000000000000000000000", IV="80000000000000000000000000000000", input="1", output="0", alg=0, chainmode=0, keysize=2 }, // CFB1VarTxt256.rsp
            //new TestVector () { n=72, mode=0, key="00000000000000000000000000000000", IV="f34481ec3cc627bacd5dc3fb08f273e6", input="00", output="03", alg=0, chainmode=0, keysize=0 }, // CFB8GFSbox128.rsp
            //new TestVector () { n=73, mode=1, key="00000000000000000000000000000000", IV="f34481ec3cc627bacd5dc3fb08f273e6", input="03", output="00", alg=0, chainmode=0, keysize=0 }, // CFB8GFSbox128.rsp
            //new TestVector () { n=74, mode=0, key="000000000000000000000000000000000000000000000000", IV="1b077a6af4b7f98229de786d7516b639", input="00", output="27", alg=0, chainmode=0, keysize=1 }, // CFB8GFSbox192.rsp
            //new TestVector () { n=75, mode=1, key="000000000000000000000000000000000000000000000000", IV="1b077a6af4b7f98229de786d7516b639", input="27", output="00", alg=0, chainmode=0, keysize=1 }, // CFB8GFSbox192.rsp
            //new TestVector () { n=76, mode=0, key="0000000000000000000000000000000000000000000000000000000000000000", IV="014730f80ac625fe84f026c60bfd547d", input="00", output="5c", alg=0, chainmode=0, keysize=2 }, // CFB8GFSbox256.rsp
            //new TestVector () { n=77, mode=1, key="0000000000000000000000000000000000000000000000000000000000000000", IV="014730f80ac625fe84f026c60bfd547d", input="5c", output="00", alg=0, chainmode=0, keysize=2 }, // CFB8GFSbox256.rsp
            //new TestVector () { n=78, mode=0, key="10a58869d74be5a374cf867cfb473859", IV="00000000000000000000000000000000", input="00", output="6d", alg=0, chainmode=0, keysize=0 }, // CFB8KeySbox128.rsp
            //new TestVector () { n=79, mode=1, key="10a58869d74be5a374cf867cfb473859", IV="00000000000000000000000000000000", input="6d", output="00", alg=0, chainmode=0, keysize=0 }, // CFB8KeySbox128.rsp
            //new TestVector () { n=80, mode=0, key="e9f065d7c13573587f7875357dfbb16c53489f6a4bd0f7cd", IV="00000000000000000000000000000000", input="00", output="09", alg=0, chainmode=0, keysize=1 }, // CFB8KeySbox192.rsp
            //new TestVector () { n=81, mode=1, key="e9f065d7c13573587f7875357dfbb16c53489f6a4bd0f7cd", IV="00000000000000000000000000000000", input="09", output="00", alg=0, chainmode=0, keysize=1 }, // CFB8KeySbox192.rsp
            //new TestVector () { n=82, mode=0, key="c47b0294dbbbee0fec4757f22ffeee3587ca4730c3d33b691df38bab076bc558", IV="00000000000000000000000000000000", input="00", output="46", alg=0, chainmode=0, keysize=2 }, // CFB8KeySbox256.rsp
            //new TestVector () { n=83, mode=1, key="c47b0294dbbbee0fec4757f22ffeee3587ca4730c3d33b691df38bab076bc558", IV="00000000000000000000000000000000", input="46", output="00", alg=0, chainmode=0, keysize=2 }, // CFB8KeySbox256.rsp
            //new TestVector () { n=84, mode=0, key="80000000000000000000000000000000", IV="00000000000000000000000000000000", input="00", output="0e", alg=0, chainmode=0, keysize=0 }, // CFB8VarKey128.rsp
            //new TestVector () { n=85, mode=1, key="80000000000000000000000000000000", IV="00000000000000000000000000000000", input="0e", output="00", alg=0, chainmode=0, keysize=0 }, // CFB8VarKey128.rsp
            //new TestVector () { n=86, mode=0, key="800000000000000000000000000000000000000000000000", IV="00000000000000000000000000000000", input="00", output="de", alg=0, chainmode=0, keysize=1 }, // CFB8VarKey192.rsp
            //new TestVector () { n=87, mode=1, key="800000000000000000000000000000000000000000000000", IV="00000000000000000000000000000000", input="de", output="00", alg=0, chainmode=0, keysize=1 }, // CFB8VarKey192.rsp
            //new TestVector () { n=88, mode=0, key="8000000000000000000000000000000000000000000000000000000000000000", IV="00000000000000000000000000000000", input="00", output="e3", alg=0, chainmode=0, keysize=2 }, // CFB8VarKey256.rsp
            //new TestVector () { n=89, mode=1, key="8000000000000000000000000000000000000000000000000000000000000000", IV="00000000000000000000000000000000", input="e3", output="00", alg=0, chainmode=0, keysize=2 }, // CFB8VarKey256.rsp
            //new TestVector () { n=90, mode=0, key="00000000000000000000000000000000", IV="80000000000000000000000000000000", input="00", output="3a", alg=0, chainmode=0, keysize=0 }, // CFB8VarTxt128.rsp
            //new TestVector () { n=91, mode=1, key="00000000000000000000000000000000", IV="80000000000000000000000000000000", input="3a", output="00", alg=0, chainmode=0, keysize=0 }, // CFB8VarTxt128.rsp
            //new TestVector () { n=92, mode=0, key="000000000000000000000000000000000000000000000000", IV="80000000000000000000000000000000", input="00", output="6c", alg=0, chainmode=0, keysize=1 }, // CFB8VarTxt192.rsp
            //new TestVector () { n=93, mode=1, key="000000000000000000000000000000000000000000000000", IV="80000000000000000000000000000000", input="6c", output="00", alg=0, chainmode=0, keysize=1 }, // CFB8VarTxt192.rsp
            //new TestVector () { n=94, mode=0, key="0000000000000000000000000000000000000000000000000000000000000000", IV="80000000000000000000000000000000", input="00", output="dd", alg=0, chainmode=0, keysize=2 }, // CFB8VarTxt256.rsp
            //new TestVector () { n=95, mode=1, key="0000000000000000000000000000000000000000000000000000000000000000", IV="80000000000000000000000000000000", input="dd", output="00", alg=0, chainmode=0, keysize=2 }, // CFB8VarTxt256.rsp
            new TestVector () { n=96, mode=0, key="00000000000000000000000000000000", IV="f34481ec3cc627bacd5dc3fb08f273e6", input="00000000000000000000000000000000", output="0336763e966d92595a567cc9ce537f5e", alg=0, chainmode=3, keysize=0 }, // OFBGFSbox128.rsp
            new TestVector () { n=97, mode=1, key="00000000000000000000000000000000", IV="f34481ec3cc627bacd5dc3fb08f273e6", input="0336763e966d92595a567cc9ce537f5e", output="00000000000000000000000000000000", alg=0, chainmode=3, keysize=0 }, // OFBGFSbox128.rsp
            new TestVector () { n=98, mode=0, key="000000000000000000000000000000000000000000000000", IV="1b077a6af4b7f98229de786d7516b639", input="00000000000000000000000000000000", output="275cfc0413d8ccb70513c3859b1d0f72", alg=0, chainmode=3, keysize=1 }, // OFBGFSbox192.rsp
            new TestVector () { n=99, mode=1, key="000000000000000000000000000000000000000000000000", IV="1b077a6af4b7f98229de786d7516b639", input="275cfc0413d8ccb70513c3859b1d0f72", output="00000000000000000000000000000000", alg=0, chainmode=3, keysize=1 }, // OFBGFSbox192.rsp
            new TestVector () { n=100, mode=0, key="0000000000000000000000000000000000000000000000000000000000000000", IV="014730f80ac625fe84f026c60bfd547d", input="00000000000000000000000000000000", output="5c9d844ed46f9885085e5d6a4f94c7d7", alg=0, chainmode=3, keysize=2 }, // OFBGFSbox256.rsp
            new TestVector () { n=101, mode=1, key="0000000000000000000000000000000000000000000000000000000000000000", IV="014730f80ac625fe84f026c60bfd547d", input="5c9d844ed46f9885085e5d6a4f94c7d7", output="00000000000000000000000000000000", alg=0, chainmode=3, keysize=2 }, // OFBGFSbox256.rsp
            new TestVector () { n=102, mode=0, key="10a58869d74be5a374cf867cfb473859", IV="00000000000000000000000000000000", input="00000000000000000000000000000000", output="6d251e6944b051e04eaa6fb4dbf78465", alg=0, chainmode=3, keysize=0 }, // OFBKeySbox128.rsp
            new TestVector () { n=103, mode=1, key="10a58869d74be5a374cf867cfb473859", IV="00000000000000000000000000000000", input="6d251e6944b051e04eaa6fb4dbf78465", output="00000000000000000000000000000000", alg=0, chainmode=3, keysize=0 }, // OFBKeySbox128.rsp
            new TestVector () { n=104, mode=0, key="e9f065d7c13573587f7875357dfbb16c53489f6a4bd0f7cd", IV="00000000000000000000000000000000", input="00000000000000000000000000000000", output="0956259c9cd5cfd0181cca53380cde06", alg=0, chainmode=3, keysize=1 }, // OFBKeySbox192.rsp
            new TestVector () { n=105, mode=1, key="e9f065d7c13573587f7875357dfbb16c53489f6a4bd0f7cd", IV="00000000000000000000000000000000", input="0956259c9cd5cfd0181cca53380cde06", output="00000000000000000000000000000000", alg=0, chainmode=3, keysize=1 }, // OFBKeySbox192.rsp
            new TestVector () { n=106, mode=0, key="c47b0294dbbbee0fec4757f22ffeee3587ca4730c3d33b691df38bab076bc558", IV="00000000000000000000000000000000", input="00000000000000000000000000000000", output="46f2fb342d6f0ab477476fc501242c5f", alg=0, chainmode=3, keysize=2 }, // OFBKeySbox256.rsp
            new TestVector () { n=107, mode=1, key="c47b0294dbbbee0fec4757f22ffeee3587ca4730c3d33b691df38bab076bc558", IV="00000000000000000000000000000000", input="46f2fb342d6f0ab477476fc501242c5f", output="00000000000000000000000000000000", alg=0, chainmode=3, keysize=2 }, // OFBKeySbox256.rsp
            new TestVector () { n=108, mode=0, key="80000000000000000000000000000000", IV="00000000000000000000000000000000", input="00000000000000000000000000000000", output="0edd33d3c621e546455bd8ba1418bec8", alg=0, chainmode=3, keysize=0 }, // OFBVarKey128.rsp
            new TestVector () { n=109, mode=1, key="80000000000000000000000000000000", IV="00000000000000000000000000000000", input="0edd33d3c621e546455bd8ba1418bec8", output="00000000000000000000000000000000", alg=0, chainmode=3, keysize=0 }, // OFBVarKey128.rsp
            new TestVector () { n=110, mode=0, key="800000000000000000000000000000000000000000000000", IV="00000000000000000000000000000000", input="00000000000000000000000000000000", output="de885dc87f5a92594082d02cc1e1b42c", alg=0, chainmode=3, keysize=1 }, // OFBVarKey192.rsp
            new TestVector () { n=111, mode=1, key="800000000000000000000000000000000000000000000000", IV="00000000000000000000000000000000", input="de885dc87f5a92594082d02cc1e1b42c", output="00000000000000000000000000000000", alg=0, chainmode=3, keysize=1 }, // OFBVarKey192.rsp
            new TestVector () { n=112, mode=0, key="8000000000000000000000000000000000000000000000000000000000000000", IV="00000000000000000000000000000000", input="00000000000000000000000000000000", output="e35a6dcb19b201a01ebcfa8aa22b5759", alg=0, chainmode=3, keysize=2 }, // OFBVarKey256.rsp
            new TestVector () { n=113, mode=1, key="8000000000000000000000000000000000000000000000000000000000000000", IV="00000000000000000000000000000000", input="e35a6dcb19b201a01ebcfa8aa22b5759", output="00000000000000000000000000000000", alg=0, chainmode=3, keysize=2 }, // OFBVarKey256.rsp
            new TestVector () { n=114, mode=0, key="00000000000000000000000000000000", IV="80000000000000000000000000000000", input="00000000000000000000000000000000", output="3ad78e726c1ec02b7ebfe92b23d9ec34", alg=0, chainmode=3, keysize=0 }, // OFBVarTxt128.rsp
            new TestVector () { n=115, mode=1, key="00000000000000000000000000000000", IV="80000000000000000000000000000000", input="3ad78e726c1ec02b7ebfe92b23d9ec34", output="00000000000000000000000000000000", alg=0, chainmode=3, keysize=0 }, // OFBVarTxt128.rsp
            new TestVector () { n=116, mode=0, key="000000000000000000000000000000000000000000000000", IV="80000000000000000000000000000000", input="00000000000000000000000000000000", output="6cd02513e8d4dc986b4afe087a60bd0c", alg=0, chainmode=3, keysize=1 }, // OFBVarTxt192.rsp
            new TestVector () { n=117, mode=1, key="000000000000000000000000000000000000000000000000", IV="80000000000000000000000000000000", input="6cd02513e8d4dc986b4afe087a60bd0c", output="00000000000000000000000000000000", alg=0, chainmode=3, keysize=1 }, // OFBVarTxt192.rsp
            new TestVector () { n=118, mode=0, key="0000000000000000000000000000000000000000000000000000000000000000", IV="80000000000000000000000000000000", input="00000000000000000000000000000000", output="ddc6bf790c15760d8d9aeb6f9a75fd4e", alg=0, chainmode=3, keysize=2 }, // OFBVarTxt256.rsp
            new TestVector () { n=119, mode=1, key="0000000000000000000000000000000000000000000000000000000000000000", IV="80000000000000000000000000000000", input="ddc6bf790c15760d8d9aeb6f9a75fd4e", output="00000000000000000000000000000000", alg=0, chainmode=3, keysize=2 }, // OFBVarTxt256.rsp
        };
    }
}

