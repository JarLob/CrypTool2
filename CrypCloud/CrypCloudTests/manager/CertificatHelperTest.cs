 
using CrypCloud.Manager;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CrypCloudTests
{
    [TestClass]
    public class CertificatHelperTest
    {
        [Ignore] // depends on enviroment
        [TestMethod]
        public void GetNamesOfKnownCertificats()
        {
            var userNames = CertificatHelper.GetNamesOfKnownCertificats();
            Assert.AreEqual(2, userNames.Count);
            Assert.AreEqual("alice", userNames[0]);
            Assert.AreEqual("bob", userNames[1]);
        }
        [Ignore] // depends on enviroment
        [TestMethod]
        public void LoadCertificateFromFile_invalidPassword()
        {
           // var certificat = CertificatHelper.LoadPrivateCertificat("alice", "terminato2r5");
           // Assert.IsNull(certificat); 
            
        }
        [Ignore] // depends on enviroment
        [TestMethod]
        public void LoadCertificateFromFile_validPassword()
        {
          //  var certificat = CertificatHelper.LoadPrivateCertificat("alice", "terminator5");
          //  Assert.IsNotNull(certificat);
        }

        [TestMethod]
        public void CertificatIsKnown_unknownCertificat()
        {
            var userCertificatIsKnown = CertificatHelper.UserCertificatIsUnknown("alfred J._Kwack");

            Assert.IsTrue(userCertificatIsKnown);
        }
        [Ignore] // depends on enviroment
        [TestMethod]
        public void CertificatIsUnknown_knownCertificat()
        {
            var userCertificatIsKnown = CertificatHelper.UserCertificatIsUnknown("alice");

            Assert.IsFalse(userCertificatIsKnown);
        }
    }
}
