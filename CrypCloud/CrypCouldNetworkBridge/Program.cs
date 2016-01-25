using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using voluntLib;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Net;
using voluntLib.managementLayer.localStateManagement.states;
using voluntLib.managementLayer.localStateManagement.states.config;

namespace CrypCouldNetworkBridge
{
    class Program
    {
        private static VoluntLib voluntLib;
        private const int NB_PORT = 13377;
        private static IPAddress NB_ENDPOINT = IPAddress.Any;

        public static void Main(string[] args)
        {  
            //utilizing certificate of user named asdasd for testing. user "asdasd" supposed to be banned for production ct
            //certificate and password needs to be changed before deploying to networkbridge
            var cerificate = Properties.Resources.asdasd;
            var networkBridgeCert = new X509Certificate2(cerificate, "asdasd");
            
            var rootCert = new X509Certificate2(Properties.Resources.rootCA);

            var adminCertificates = Properties.Resources.adminCertificates.Replace("\r", "");
            var adminList = adminCertificates.Split('\n').ToList();

            var bannedCertificates = Properties.Resources.bannedCertificates.Replace("\r", "");
            var bannedList = bannedCertificates.Split('\n').ToList();

            var state = new EpochStateConfig() { BitMaskWidth = 1024 * 16 };
            state.FinalizeValues();

            voluntLib = new VoluntLib
            {
                DefaultStateConfig = state,
                EnablePersistence = true,
                LoadDataFromLocalStorage = true,
                AdminCertificateList = adminList,
                BannedCertificateList = bannedList,
                MulticastGroup = "224.0.7.100"
            };

            voluntLib.EnableNATFreeNetworkBridge(NB_PORT, NB_ENDPOINT);
            voluntLib.Init(rootCert, networkBridgeCert);
            voluntLib.Start();

            while (true)
            {
                Thread.Sleep(1000);
            }
        }    
    }    
}
