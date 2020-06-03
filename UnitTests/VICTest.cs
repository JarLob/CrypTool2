﻿using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.TemplateAndPluginTests
{
    [TestClass]
    public class VICTest
    {
        public VICTest()
        {
        }
        //The tests themselves are not yet implemented. I'm just trying to initializie the component correctly and run it with a testvector, but that keeps failing.
        [TestMethod]
        public void VICTestMethod()
        {
            var pluginInstance = TestHelpers.GetPluginInstance("VIC");
            var scenario = new PluginTestScenario(pluginInstance, new[] { "Input", "Phrase", "Date", "InitializingString", "Number", "Password" }, new[] { "Output" });
            object[] output;

            foreach (TestVector vector in testvectors)
            {
                output = scenario.GetOutputs(new object[] { vector.input, vector.phrase, vector.date, vector.initializingString, vector.number, vector.password });
                Assert.AreEqual(vector.output, (string)output[0], "Unexpected value in test #" + vector.n + ".");
            }

        }

        struct TestVector
        {
            public int n;
            public string input, output, phrase, date, initializingString, number, password;
        }

        TestVector[] testvectors = new TestVector[] 
        {
            new TestVector () {n=0, 
                input="Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat.",
                phrase="Met defective are allowance two perceived listening consulted contained", 
                date="391945",
                initializingString="208103",
                number="10",
                password="snowfall", 
                output="Some output value" }
        };

    }
}