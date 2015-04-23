﻿using System; 
using System.IO;
using CertificateLibrary.Certificates;
using PeersAtPlay.CertificateLibrary.Util;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Crypto; 
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;

namespace PeersAtPlay.CertificateLibrary.Certificates
{
    public class PeerCertificate : Certificate
    {

        #region Static readonly

        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static readonly string DefaultUserCertificateDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CrypCloud" + Path.DirectorySeparatorChar + "Certificates" + Path.DirectorySeparatorChar);
        public static readonly int CertificateVersion = 1;

        #endregion


        #region Constructor

        /// <summary>
        /// Creates a stub PeerCertificate object.
        /// </summary>
        public PeerCertificate()
        {
            this.CaX509 = null;
            this.PeerX509 = null;
            this.Issuer = null;
            this.Subject = null;
            this.Avatar = String.Empty;
            this.World = String.Empty;
            this.HashedEmail = String.Empty;
            this.Password = String.Empty;
            this.KeyPair = null;
            this.IsLoaded = false;
        }

        /// <summary>
        /// Creates a PeerCertificate object. 
        /// The CommonName of peerX509 will be used to store the avatar name.
        /// </summary>
        /// <param name="caX509"></param>
        /// <param name="peerX509"></param>
        /// <param name="world"></param>
        /// <param name="hashedEmail"></param>
        /// <param name="password"></param>
        /// <param name="keyPair"></param>
        public PeerCertificate(X509Certificate caX509, X509Certificate peerX509, string world, string hashedEmail, string password, AsymmetricCipherKeyPair keyPair)
        {
            this.CaX509 = caX509;
            this.PeerX509 = peerX509;
            this.Issuer = new DNWrapper(peerX509.IssuerDN);
            this.Subject = new DNWrapper(peerX509.SubjectDN);
            this.Avatar = this.Subject.CommonName;
            this.World = world;
            this.HashedEmail = hashedEmail;
            this.Password = password;
            this.KeyPair = keyPair;
            this.IsLoaded = true;

            if (CertificateLoaded != null)
            {
                this.CertificateLoaded.Invoke(this, new EventArgs());
            }
        }

        #endregion


        #region Load a peer certificate

        /// <summary>
        /// Loads a peer certificate by extracting information from PKCS #12 personal information exchange (also known as PFX) formatted data
        /// </summary>
        /// <param name="stream">byte array containing the PKCS #12 store</param>
        /// <param name="password">Password to open the PKCS #12 store</param>
        /// <exception cref="ArgumentNullException">if the password or bytearray is null</exception>
        /// <exception cref="IOException">The PKCS #12 store could not be opened</exception>
        /// <exception cref="PKCS12FormatException">If PKCS #12 store is not genuine</exception>
        /// <exception cref="X509CertificateFormatException">Issuer distinguished name is empty or malformed</exception>
        public void Load(byte[] pkcs12, string password)
        {
            using (MemoryStream mstream = new MemoryStream(pkcs12))
            {
                Load(mstream, password);
                mstream.Close();
            }

        }

        /// <summary>
        /// Loads a peer certificate by extracting information from PKCS #12 personal information exchange (also known as PFX) formatted data
        /// </summary>
        /// <param name="stream">Stream containing the PKCS #12 store</param>
        /// <param name="password">Password to open the PKCS #12 store</param>
        /// <exception cref="ArgumentNullException">if the password or stream is null</exception>
        /// <exception cref="IOException">The PKCS #12 store could not be opened</exception>
        /// <exception cref="PKCS12FormatException">If PKCS #12 store is not genuine</exception>
        /// <exception cref="X509CertificateFormatException">Issuer distinguished name is empty or malformed</exception>
        public override void Load(Stream stream, string password)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream can not be null!");
            }
            if (password == null)
            {
                throw new ArgumentNullException("password can not be null!");
            }

            // Load peer certificate from pfx into temp store. Throws IOException if PKCS #12 could not be opened
            Pkcs12Store tempStore = new Pkcs12StoreBuilder().Build();
            tempStore.Load(stream, password.ToCharArray());

            int aliasCount = 0;
            string tempAlias = String.Empty;
            foreach (string alias in tempStore.Aliases)
            {
                tempAlias = alias;
                aliasCount++;
            }

            if (aliasCount != 1 || tempAlias == String.Empty)
            {
                string msg = "PKCS12 store contains a wrong certificate alias. Maybe no genuine PeersAtPlay certificate?";
                Log.Error(msg);
                throw new PKCS12FormatException(msg);
            }
            if (tempStore.GetCertificateChain(tempAlias).Length < 2)
            {
                string msg = "PKCS12 store does not contain two certificates. Maybe no genuine PeersAtPlay certificate?";
                Log.Error(msg);
                throw new PKCS12FormatException(msg);
            }

            X509Certificate tempPeerCert = null;
            X509Certificate tempCACert = null;
            DNWrapper tempIssuer = null;
            DNWrapper tempSubject = null;
            AsymmetricCipherKeyPair tempKeyPair = null;
            string tempAvatar = null;
            string tempWorld = null;
            string tempHashedEmail = null;
            int tempVersion = 0;

            foreach (X509CertificateEntry certEntry in tempStore.GetCertificateChain(tempAlias))
            {
                // Check whether CertificateUsage extension is properly set
                string certificateUsage = CertificateServices.GetExtensionValue(certEntry.Certificate, PAPObjectIdentifier.CertificateUsage);
                if (certificateUsage == null)
                {
                    continue;
                }

                if (certificateUsage.Equals(CertificateUsageValue.CA) && CertificateServices.IsCaCertificate(certEntry.Certificate))
                {
                    tempCACert = certEntry.Certificate;
                }
                else if (certificateUsage.Equals(CertificateUsageValue.TLS) || certificateUsage.Equals(CertificateUsageValue.PEER))
                {
                    tempSubject = new DNWrapper(certEntry.Certificate.SubjectDN);
                    if (tempSubject.CommonName == String.Empty)
                    {
                        // No Avatar name, but we need one!
                        string msg = "Your certificate does not contain an Avatar name. Maybe not a genuine PeersAtPlay certificate?";
                        Log.Error(msg);
                        throw new X509CertificateFormatException(msg);
                    }

                    tempPeerCert = certEntry.Certificate;
                    tempIssuer = new DNWrapper(certEntry.Certificate.IssuerDN);
                    tempAvatar = tempSubject.CommonName;
                    tempKeyPair = new AsymmetricCipherKeyPair(certEntry.Certificate.GetPublicKey(), tempStore.GetKey(tempAlias).Key);

                    if (certificateUsage.Equals(CertificateUsageValue.PEER) && CertificateServices.IsPeerCertificate(certEntry.Certificate))
                    {
                        tempWorld = CertificateServices.GetExtensionValue(certEntry.Certificate, PAPObjectIdentifier.WorldName);
                        tempHashedEmail = CertificateServices.GetExtensionValue(certEntry.Certificate, PAPObjectIdentifier.HashedEmail);
                        tempVersion = Int32.Parse(CertificateServices.GetExtensionValue(certEntry.Certificate, PAPObjectIdentifier.CertificateVersion));
                    }
                    else if (certificateUsage.Equals(CertificateUsageValue.PEER) && CertificateServices.IsTlsCertificate(certEntry.Certificate))
                    {
                        tempWorld = String.Empty;
                        tempHashedEmail = String.Empty;
                        tempVersion = Int32.Parse(CertificateServices.GetExtensionValue(certEntry.Certificate, PAPObjectIdentifier.CertificateVersion));
                    }
                    else
                    {
                        tempWorld = String.Empty;
                        tempHashedEmail = String.Empty;
                        this.Version = 0;
                    }
                }
            }

            if (tempCACert == null || tempPeerCert == null)
            {
                string msg = "PKCS12 store does not contain peer and CA certificate. Maybe no genuine PeersAtPlay certificate?";
                Log.Error(msg);
                throw new PKCS12FormatException(msg);
            }

            // Everything seems ok
            this.PeerX509 = tempPeerCert;
            this.CaX509 = tempCACert;
            this.Issuer = tempIssuer;
            this.Subject = tempSubject;
            this.Avatar = tempAvatar;
            this.World = tempWorld;
            this.HashedEmail = tempHashedEmail;
            this.Version = tempVersion;
            this.Password = password;
            this.KeyPair = tempKeyPair;
            this.IsLoaded = true;
            Log.Debug("Certificate for user " + Avatar + " successfully loaded");

            if (CertificateLoaded != null)
            {
                this.CertificateLoaded.Invoke(this, new EventArgs());
            }
        }

        /// <summary>
        /// Loads a PKCS #12 certificate from users AppData directory.
        /// </summary>
        /// <param name="serial">Serialnumber of the certificate, that should be opened</param>
        /// <param name="password">Password to open the PKCS #12 store</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="IOException">The PKCS #12 store could not be opened</exception>
        /// <exception cref="PKCS12FormatException">If PKCS #12 store does not contain the right certificates</exception>
        /// <exception cref="X509CertificateFormatException">If X509 certificate does not contain the right values</exception>
        public void LoadPkcs12FromAppData(string serial, string password)
        {
            string appDataCertificate = DefaultUserCertificateDirectory + serial + ".p12";
            FileStream stream = null;

            try
            {
                stream = new FileStream(appDataCertificate, FileMode.Open, FileAccess.Read);
            }
            catch (Exception ex)
            {
                if (stream != null)
                {
                    stream.Close();
                }
                throw new IOException("Could not load PKCS #12 file from AppData", ex);
            }

            try
            {
                Load(stream, password);
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
            }
        }

        #endregion


        #region Save a peer certificate

        /// <summary>
        /// Saves the peer certificate in PKCS #12 personal information exchange (pfx) format to the given stream.
        /// Certificate will be secured with the existing password.
        /// </summary>
        /// <param name="stream">Stream used to save the PKCS #12 store</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="CertificateException">No peer certificate loaded</exception>
        /// <exception cref="PKCS12FormatException">Could not write PKCS #12 store to stream</exception>
        public override void SaveAsPkcs12(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream", "stream can not be null");
            }
            if (!this.IsLoaded)
            {
                string msg = "Nothing to export. You need to load a peer certificate first!";
                Log.Error(msg);
                throw new CertificateException(msg);
            }

            X509CertificateEntry[] certChain = new X509CertificateEntry[2];
            certChain[0] = new X509CertificateEntry(PeerX509);
            certChain[1] = new X509CertificateEntry(CaX509);
            Pkcs12Store store = new Pkcs12StoreBuilder().Build();

            try
            {
                store.SetKeyEntry(Avatar, new AsymmetricKeyEntry(KeyPair.Private), certChain);
                store.Save(stream, Password.ToCharArray(), new SecureRandom());
            }
            catch (Exception ex)
            {
                string msg = "Could not save peer PKCS #12 store!";
                Log.Error(msg, ex);
                throw new PKCS12FormatException(msg, ex);
            }
        }

        /// <summary>
        /// Saves the peer certificate in CRT format to the given stream.
        /// CRT just contains the certificate without private key.
        /// </summary>
        /// <param name="stream"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="CertificateException">No peer certificate loaded</exception>
        /// <exception cref="IOException">Error while writing to stream</exception>
        public override void SaveAsCrt(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream", "stream can not be null");
            }
            if (!this.IsLoaded)
            {
                string msg = "Nothing to export. You need to load a peer certificate first!";
                Log.Error(msg);
                throw new CertificateException(msg);
            }

            byte[] crtBytes = PeerX509.GetEncoded();
            try
            {
                stream.Write(crtBytes, 0, crtBytes.Length);
            }
            catch (Exception ex)
            {
                string msg = "Could not save peer certificate!";
                Log.Error(msg, ex);
                throw new IOException(msg, ex);
            }
        }

        /// <summary>
        /// Saves the certificate in PKCS #12 Format to users AppData directory.
        /// </summary>
        /// <param name="password"></param>
        /// <exception cref="CertificateException">No peer certificate loaded</exception>
        /// <exception cref="IOException">Error writing to AppData</exception>
        public void SavePkcs12ToAppData(string password)
        {
            if (!this.IsLoaded)
            {
                throw new CertificateException("No peer certificate loaded");
            }

            string appDataCertificate = DefaultUserCertificateDirectory + Avatar + ".p12";
            FileStream stream = null;
            try
            {
                if (!Directory.Exists(DefaultUserCertificateDirectory))
                {
                    Directory.CreateDirectory(DefaultUserCertificateDirectory);
                }
                stream = new FileStream(appDataCertificate, FileMode.Create, FileAccess.Write);
                SaveAsPkcs12(stream);
            }
            catch (CertificateException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                string msg = "Could not save peer certificate in AppData!";
                Log.Error(msg, ex);
                throw new IOException(msg, ex);
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
            }
        }

        /// <summary>
        /// Saves the certificate as crt file to users AppData directory.
        /// </summary>
        /// <exception cref="CertificateException">No peer certificate loaded</exception>
        /// <exception cref="IOException">Could not write to AppData</exception>
        public void SaveCrtToAppData()
        {
            if (!IsLoaded)
            {
                throw new CertificateException("No peer certificate loaded");
            }

            FileStream stream = null;
            try
            {
                if (!Directory.Exists(DefaultUserCertificateDirectory))
                {
                    Directory.CreateDirectory(DefaultUserCertificateDirectory);
                }

                string appDataCertificate = DefaultUserCertificateDirectory + this.PeerX509.SerialNumber.ToString() + ".crt";
                stream = new FileStream(appDataCertificate, FileMode.Create, FileAccess.Write);
                SaveAsCrt(stream);
            }
            catch (CertificateException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                string msg = "Could not save peer certificate in AppData!";
                Log.Error(msg, ex);
                throw new IOException(msg, ex);
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
            }
        }

        #endregion


        #region Accessor

        /// <summary>
        /// Returns a .Net System.Security.Cryptography.X509Certificates.X509Certificate2 object of the peer certificate.
        /// </summary>
        /// <returns>System.Security.Cryptography.X509Certificates.X509Certificate2</returns>
        /// <exception cref="CryptographicException">If the certificate could not be transformed</exception>
        public System.Security.Cryptography.X509Certificates.X509Certificate2 GetX509Certificate2()
        {
            return new System.Security.Cryptography.X509Certificates.X509Certificate2(GetPkcs12(), Password);
        }

        #endregion


        #region Properties and Events

        public X509Certificate PeerX509 { get; private set; }

        public string Avatar { get; private set; }

        public string HashedEmail { get; private set; }

        public string World { get; private set; }

        public event EventHandler<EventArgs> CertificateLoaded;

        #endregion

    }
}