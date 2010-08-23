﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Cryptool.Plugins.CostFunction;
using Cryptool.PluginBase.Cryptography;
namespace Tests
{
    [TestClass]
    public class CostFunctionTest
    {
        private TestContext testContextInstance;
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }
        [TestMethod]
        public void CostFunctionTests()
        {
            string input = "In der Kryptographie ist die Transposition ein Verschluesselungsverfahren, bei dem die Zeichen einer Botschaft (des Klartextes) umsortiert werden. Jedes Zeichen bleibt unveraendert erhalten, jedoch wird die Stelle, an der es steht, geaendert. Dies steht im Gegensatz zu den Verfahren der (monoalphabetischen oder polyalphabetischen) Substitution, bei denen jedes Zeichen des Klartextes seinen Platz behaelt, jedoch durch ein anderes Zeichen ersetzt (substituiert) wird.";
            double target = 7.38051470588235;
            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding(); // String to Byte Conversion
            CostFunction cf = new CostFunction();


            //Index of Conincidence
            cf.Initialize();

            cf.InputText = enc.GetBytes(input);
            
            testContextInstance.WriteLine(enc.GetString(cf.InputText));
            cf.changeFunctionType(0);
            cf.PreExecution(); // MUST BE CALLED!
            cf.Execute();

            Assert.AreEqual(target, cf.Value, 0.00000000000001); // Uhm. Close enough? Oo

            //Entropy
            target = 4.25374598691653;
            cf.Initialize();
            cf.InputText = enc.GetBytes(input);
            cf.changeFunctionType(1);
            cf.PreExecution(); // MUST BE CALLED!
            cf.Execute();

            Assert.AreEqual(target, cf.Value, 0.00000000000001); // Uhm. Close enough? Oo

            
            //Bigrams: log 2
            target = 265.318365029242;
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            path = Environment.CurrentDirectory;
            for (int i = 0; i < 3; i++)
            {
                path = System.IO.Path.GetDirectoryName(path);
            }
            path = Path.Combine(path, "CrypBuild\\x86\\Debug\\Data\\StatisticsCorpusDE");
            
            this.testContextInstance.WriteLine(path);
            cf.setDataPath(path);
            cf.Initialize();
            cf.InputText = enc.GetBytes(input);
            cf.changeFunctionType(2);
            cf.PreExecution(); // MUST BE CALLED!
            cf.Execute();
            testContextInstance.WriteLine(cf.Value.ToString());
            Assert.AreEqual(target, cf.Value, 0.000000000001); // Seriously what's whats up with the tolerances?
            
            //Bigrams: Sinkov
            target = -548.360297827531;
            cf.Initialize();
            cf.InputText = enc.GetBytes(input);
            cf.changeFunctionType(3);
            cf.PreExecution(); // MUST BE CALLED!
            cf.Execute();
            testContextInstance.WriteLine(cf.Value.ToString());
            Assert.AreEqual(target, cf.Value, 0.000000000001); // Uhm. Close enough? Oo
            
            //Bigrams: Percentaged
            target = 0.20132645541636;
            cf.Initialize();
            cf.InputText = enc.GetBytes(input);
            cf.changeFunctionType(4);
            cf.PreExecution(); // MUST BE CALLED!
            cf.Execute();
            testContextInstance.WriteLine(cf.Value.ToString());
            Assert.AreEqual(target, cf.Value, 0.00000000000001); // Uhm. Close enough? Oo
            /*
            //RegEx
            target = 4.25374598691653;
            cf.Initialize();
            cf.InputText = enc.GetBytes(input);
            cf.changeFunctionType(5);
            cf.PreExecution(); // MUST BE CALLED!
            cf.Execute();
            testContextInstance.WriteLine(cf.Value.ToString());
            Assert.AreEqual(target, cf.Value, 0.00000000000001); // Uhm. Close enough? Oo
            */
            //Weighted Bigrams/Trigrams
            target = -777.230685764826;
            cf.Initialize();
            cf.InputText = enc.GetBytes(input);
            cf.changeFunctionType(6);
            cf.PreExecution(); // MUST BE CALLED!
            cf.Execute();
            testContextInstance.WriteLine(cf.Value.ToString());
            Assert.AreEqual(target, cf.Value, 0.000000000001); // Uhm. Close enough? Oo 
        }

       
    }
}
