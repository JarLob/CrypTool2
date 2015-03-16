 
using CertificateLibrary.Certificates;
using CrypCloud.Manager;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CrypCloudTests
{
    [TestClass]
    public class CertificatHelperTest
    {
        [Ignore] // depends on enviroment
        [TestMethod]
        public void GetNamesOfKnownCertificates()
        {
            var userNames = CertificateHelper.GetNamesOfKnownCertificates();
            Assert.AreEqual(2, userNames.Count);
            Assert.AreEqual("alice", userNames[0]);
            Assert.AreEqual("bob", userNames[1]);
        }

        [TestMethod]
        public void CertificateIsKnown_unknownCertificate()
        {
            var userCertificateIsKnown = CertificateHelper.UserCertificateIsUnknown("alfred J._Kwack");

            Assert.IsTrue(userCertificateIsKnown);
        }
        [Ignore] // depends on environment
        [TestMethod]
        public void CertificateIsUnknown_knownCertificate()
        {
            var userCertificateIsKnown = CertificateHelper.UserCertificateIsUnknown("alice");

            Assert.IsFalse(userCertificateIsKnown);
        }    
        
        [TestMethod]
        public void CertificateServices_WontCrash()
        {
            CertificateServices.GetCertificateCount(CertificateHelper.DefaultUserCertificateDir);
        }


    }
}
