using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using PeersAtPlay.CertificateLibrary.Certificates;

namespace CrypCloud.Manager
{
    public class CertificateHelper
    {
        public static readonly string DefaultUserCertificateDir = PeerCertificate.DefaultUserCertificateDirectory;
        private const string CertFileExtention = ".p12";

        public static List<string> GetNamesOfKnownCertificates()
        {
            if ( ! DoesDirectoryExists())
            {
                return new List<string>();
            }

            var files = Directory.GetFiles(DefaultUserCertificateDir, "*" + CertFileExtention);
            var usernames = files.Select(FullPathToFilename);
            return usernames.ToList(); 
        } 

        public static X509Certificate2 LoadPrivateCertificate(string name, SecureString password)
        {
            try
            { 
                return new X509Certificate2(CreatePathToUserCertificate(name), password);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static Boolean StoreCertificate(PeerCertificate certificate, string password, string avatar)
        {
            try
            {
                certificate.SavePkcs12ToAppData(password);
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }

        public static bool UserCertificateIsUnknown(string username)
        {
            return ! File.Exists(CreatePathToUserCertificate(username));
        }

        #region pathHelper

        public static void CreateDirectory()
        {
            Directory.CreateDirectory(DefaultUserCertificateDir);
        }

        public static bool DoesDirectoryExists()
        {
            return Directory.Exists(DefaultUserCertificateDir);
        }

        private static string FullPathToFilename(string fullPath)
        {
            return fullPath.Replace(DefaultUserCertificateDir, "").Replace(CertFileExtention, "").Replace("\\", "");
        }
 
        private static string CreatePathToUserCertificate(string username)
        {
           
            return DefaultUserCertificateDir + Path.DirectorySeparatorChar + username + CertFileExtention;
        }

        #endregion
         
    }
}
