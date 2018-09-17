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
using CrypToolStoreLib.DataObjects;
using CrypToolStoreLib.Tools;
using CrypToolStoreLib.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrypToolStoreLib.Client;
using System.Security.Cryptography.X509Certificates;
using System.Net;

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
            Logger.SetLogLevel(Logtype.Info);
            CrypToolStoreDatabase database = CrypToolStoreDatabase.GetDatabase();
            if (!database.InitAndConnect("192.168.0.122", "CrypToolStore", "cryptoolstore", "123", 5))
            {
                logger.LogText("Shutting down as we could not connect to mysql database", this, Logtype.Info);
                return;
            }

            CrypToolStoreServer server = null;
            
            try
            {
                X509Certificate2 cert = new X509Certificate2("anonymous.p12", "anonymous");                
                server = new CrypToolStoreServer();
                server.ServerKey = cert;
                server.Start();

                while (true)
                {
                    try
                    {
                        CrypToolStoreClient client = new CrypToolStoreClient();
                        client.Connect();
                        client.Login("Test", "");

                        Source source = new Source();
                        source.PluginId = 7;
                        source.PluginVersion = 2;
                        source.Assembly = new byte[] { 1, 2, 3 };
                        source.BuildDate = DateTime.Now;
                        source.BuildLog = "empty2";
                        source.BuildState = "state2";
                        source.BuildVersion = 0;
                        source.ZipFile = new byte[] { 1, 2, 3 };

                        client.CreateSource(source);

                        client.UpdateSource(source);

                        client.Disconnect();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                    Console.ReadLine();
                }
            }
            catch (Exception ex)
            {
                logger.LogText(String.Format("Exception while running the server: {0}", ex.Message), this, Logtype.Info);
            }
            finally
            {
                if (server != null)
                {
                    server.Stop();
                }
                database.Close();
            }
            Console.ReadLine();
        }
    }
}
