﻿/*
   Copyright 2018 Nils Kopal <Nils.Kopal<AT>Uni-Kassel.de>

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
using CrypToolStoreLib.Database;
using CrypToolStoreLib.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrpyStoreLib
{
    class Program
    {
        private static Logger logger = Logger.GetLogger();
        private static Program program = new Program();

        static void Main(string[] args)
        {
            program.Run();
        }

        public void Run()
        {
            Logger.SetLogLevel(Logtype.Debug);
            try
            {
                using (Database database = new Database("192.168.0.122", "CrypToolStore", "cryptoolstore", "123", 1))
                {
                    //database.UpdateResourceData(1005, 0, new byte[] { 1, 2, 3, 4, 5 }, DateTime.Now);
                    //database.DeleteResourceData(1005, 0);
                    var result = database.GetResourceDatas();
                    foreach (var entey in result)
                    {
                        Console.WriteLine(entey);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.ReadLine();
        }
    }
}