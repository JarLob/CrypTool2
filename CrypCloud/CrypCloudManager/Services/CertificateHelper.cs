using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using X509Certificate = Org.BouncyCastle.X509.X509Certificate;

namespace CrypCloud.Manager
{
    public class CertificateHelper
    {
        public static readonly string DefaultUserCertificateDir = CreatePathInAppdata("CryptCloud", "Certificates");
        private const string CertFileExtention = ".pfx";

        #region directory

        public static void CreateDirectory()
        {
            Directory.CreateDirectory(DefaultUserCertificateDir);
        }

        public static bool DoesDirectoryExists()
        {
            return Directory.Exists(DefaultUserCertificateDir);
        }

        #endregion

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
                return  new X509Certificate2(CreatePathToUserCertificate(name), password);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static void StoreCertificate(X509Certificate certificate, string password, string avatar)
        {
            var x509Certificate2 = new X509Certificate2();
            x509Certificate2.Import(certificate.GetEncoded(), password, X509KeyStorageFlags.Exportable);
            var export = x509Certificate2.Export(X509ContentType.Pfx, password);

            File.WriteAllBytes(DefaultUserCertificateDir + avatar + CertFileExtention, export);
        }

        public static bool UserCertificateIsUnknown(string username)
        {
            return ! File.Exists(CreatePathToUserCertificate(username));
        }

        #region pathHelper

        private static string FullPathToFilename(string fullPath)
        {
            return fullPath.Replace(DefaultUserCertificateDir, "").Replace(CertFileExtention, "").Replace("\\", "");
        }

        public static string CreatePathInAppdata(params string[] folders)
        {
            var pathToAppdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var pathInAppdata = folders.Aggregate("", (current, folder) => current + (folder + Path.DirectorySeparatorChar));
            return Path.Combine(pathToAppdata, pathInAppdata);
        }

        private static string CreatePathToUserCertificate(string username)
        {
           
            return DefaultUserCertificateDir + Path.DirectorySeparatorChar + username + CertFileExtention;
        }

        #endregion

         
    }
}
