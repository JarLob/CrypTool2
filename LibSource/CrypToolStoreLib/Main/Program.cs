/*
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

            ListDevelopersMessage message = new ListDevelopersMessage();
            message.Developer = new Developer() { Username = "kopal", Firstname = "nils", Lastname = "kopal", Email = "Nils.Kopal@uni-kassel.de", IsAdmin = true };
            message.DeveloperList.Add(new Developer() { Username = "0", Firstname = "a", Lastname = "w" });
            message.DeveloperList.Add(new Developer() { Username = "1", Firstname = "b", Lastname = "x" });
            message.DeveloperList.Add(new Developer() { Username = "2", Firstname = "c", Lastname = "y" });
            message.DeveloperList.Add(new Developer() { Username = "3", Firstname = "d", Lastname = "z" });

            var data = message.Serialize();

            ListDevelopersMessage message2 = new ListDevelopersMessage();
            message2.Deserialize(data);


            foreach (var developer in message2.DeveloperList)
            {
                Console.WriteLine(developer);
            }

            Console.WriteLine(message2.Developer);

            Console.ReadLine();
        }
    }
}
