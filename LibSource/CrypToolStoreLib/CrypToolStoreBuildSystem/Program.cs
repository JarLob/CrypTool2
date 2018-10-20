using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrypToolStoreBuildSystem
{
    class Program
    {
        static void Main(string[] args)
        {
            CrypToolStoreBuildServer server = new CrypToolStoreBuildServer();
            server.Start();
            Console.ReadLine();
        }
    }
}
