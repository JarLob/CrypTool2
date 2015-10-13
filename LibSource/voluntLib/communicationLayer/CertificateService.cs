// Copyright 2014 Christopher Konze, University of Kassel
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//      http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#region

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;
using NLog;
using voluntLib.communicationLayer.messages.messageWithCertificate;

#endregion

namespace voluntLib.communicationLayer
{
    public enum CertificateValidationState
    {
        Valid,
        Unknown,
        Invalid
    }

    [SecurityPermission(SecurityAction.LinkDemand,Flags = SecurityPermissionFlag.Assertion)]
    public class CertificateService
    {
        #region member

        private const string NameOfHashAlgorithm = "SHA256";
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly X509Certificate2 caCertificate;
        private readonly RSACryptoServiceProvider csProvider;
        public X509Certificate2 OwnCertificate { get; private set; }

        public string OwnName { get; private set; }
        public List<string> AdminCertificateList { get; set; }
        public List<string> BannedCertificateList { get; set; }

        #endregion

        public CertificateService(X509Certificate2 caCertificate, X509Certificate2 ownCertificate)
        {
            this.caCertificate = caCertificate;
            OwnCertificate = ownCertificate;

            if ( ! IsValidCertificate(ownCertificate))
                throw new InvalidDataException("ownCertificate is not valid");
            
            if ( ! ownCertificate.HasPrivateKey)
                throw new KeyNotFoundException("Private Key not found");

            //create the CSP from the private key
            csProvider = (RSACryptoServiceProvider) ownCertificate.PrivateKey;
            if (csProvider.CspKeyContainerInfo.ProviderType == 1)
            {
                //if the Certificate's CSP does not support SHA2 create a new CSP that does
                csProvider = new RSACryptoServiceProvider(new CspParameters
                {
                    KeyContainerName = csProvider.CspKeyContainerInfo.KeyContainerName,
                    KeyNumber = csProvider.CspKeyContainerInfo.KeyNumber == KeyNumber.Exchange ? 1 : 2
                }) {PersistKeyInCsp = true};
            }

            OwnName = GetSubjectNameFromCertificate(ownCertificate);
        }

        private static string GetSubjectNameFromCertificate(X509Certificate2 cert)
        {
            return cert.SubjectName.Name != null ? cert.SubjectName.Name.Split('=').Last() : "";
        }

        public CertificateValidationState VerifySignature(AMessage message)
        {
            //extract certificate 
            var senderCertificate = ExtractValidCertificate(message);
            if (senderCertificate == null)
                return CertificateValidationState.Invalid;

            if (IsBannedCertificate(senderCertificate))
                return CertificateValidationState.Invalid; 

            //extract signature and replace with empty signature
            var originalSignature = message.Header.SignatureData;
            var signature = message.Header.SignatureData;
            message.ClearSignature();
            var data = message.Serialize();
            message.Header.SignatureData = originalSignature;


            // Verify the signature with the hash
            var provider = (RSACryptoServiceProvider) senderCertificate.PublicKey.Key;
            if (provider.VerifyData(data, NameOfHashAlgorithm, signature))
            {
                Logger.Debug("[" + OwnName + "] Signature is valid");
                return CertificateValidationState.Valid;
            }

            Logger.Error("Signature is invalid");
            return CertificateValidationState.Invalid;
        }

        /// <summary>
        ///   Signs the message, adds the sendername and the certificate
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public AMessage SignAndAddInformation(AMessage message)
        {
            message.Header.SenderName = OwnName;
            message.Header.CertificateData = ExportOwnCertificate();

            //remove old signature
            message.ClearSignature();

            // sign data
            var data = message.Serialize();
            message.Header.SignatureData = csProvider.SignData(data, NameOfHashAlgorithm);
            return message;
        }

        /// <summary>
        ///   Determines whether the specified message has been signed by an admin.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public bool IsAdmin(AMessage message)
        {
            var senderCertificate = ExtractValidCertificate(message);
            if (senderCertificate == null)
                return false;

            return IsAdminCertificate(senderCertificate);
        }

        public bool IsAdminCertificate(X509Certificate2 senderCertificate)
        {
            //by name
            var senderName = GetSubjectNameFromCertificate(senderCertificate);
            if (AdminCertificateList.Contains("N:" + senderName))
                return true;

            //by serial number
            if (AdminCertificateList.Contains("SN:" + senderCertificate.SerialNumber))
                return true;
            return false;
        }
        
        public bool IsBannedCertificate(X509Certificate2 senderCertificate)
        {
            //by name
            var senderName = GetSubjectNameFromCertificate(senderCertificate);
            if (BannedCertificateList.Contains("N:" + senderName))
                return true;

            //by serial number
            if (BannedCertificateList.Contains("SN:" + senderCertificate.SerialNumber))
                return true;
            return false;
        }

        #region helper

        /// <summary>
        ///   returns a valid certificate or null
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        private X509Certificate2 ExtractValidCertificate(AMessage message)
        {
            X509Certificate2 senderCertificate;
            try
            {
                senderCertificate = new X509Certificate2(message.Header.CertificateData);
            } catch
            {
                Logger.Error("Received Certificate not wellformed!");
                return null;
            }

            //check if its valid
            if (IsValidCertificate(senderCertificate))
                return senderCertificate;

            Logger.Error("Received Certificate is invalid!");
            return null;
        }

        /// <summary>
        ///   Determines whether the certificate is issued by the given CA.
        /// </summary>
        /// <param name="certificate">The certificate.</param>
        /// <returns></returns> 
        private bool IsValidCertificate(X509Certificate2 certificate)
        {
            var chain = new X509Chain();
            chain.ChainPolicy.ExtraStore.Add(caCertificate);
            chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
            chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;
            return chain.Build(certificate) && certificate.SubjectName.Name != null;
        }

        /// <summary>
        ///   Exports the own certificate.
        /// </summary>
        /// <returns></returns>
        private byte[] ExportOwnCertificate()
        {
            return OwnCertificate.Export(X509ContentType.Cert);
        }

        #endregion
    }
}