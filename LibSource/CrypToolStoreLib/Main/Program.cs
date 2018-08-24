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

            Source source = new Source();
            source.BuildState = "123";
            source.BuildVersion = 12;
            source.BuildDate = DateTime.Now.Subtract(new TimeSpan(1, 1, 1));
            Source source2 = new Source();
            source2.Deserialize(source.Serialize());
            Console.WriteLine(source2);

            Resource resource = new Resource();
            resource.Name = "fubar";
            resource.Publish = true;
            resource.Id = 1000;
            Resource resource2 = new Resource();
            resource2.Deserialize(resource.Serialize());
            Console.WriteLine(resource2);

            ResourceData data = new ResourceData();
            data.ResourceId = 10000;
            data.UploadDate = DateTime.Now.Subtract(new TimeSpan(1, 1, 1));
            data.Version = 50;
            data.Data = new byte[] { 254, 55, 11, 1, 127 };
            ResourceData data2 = new ResourceData();
            data2.Deserialize(data.Serialize());
            Console.WriteLine(data2);

            Console.ReadLine();
        }
    }
}
