﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cryptool.PluginBase.Miscellaneous;
using CrypTool.CertificateLibrary.Network;

namespace CrypCloud.Manager.Services
{
    class CAServerHelper
    {
        public static void ResetPassword(PasswordReset request, Action successAction, Action<ProcessingErrorEventArgs> processingError, Action<string> errorHandler)
        {
            var certificateClient = GetACertificateClient(errorHandler);

            certificateClient.InvalidPasswordReset += (sender, arg) => processingError(arg);
            certificateClient.PasswordResetVerificationRequired += delegate { successAction(); }; 

            TryAsyncCommunication(() => certificateClient.ResetPassword(request), errorHandler);
        }

        public static void VerifyPasswordReset(PasswordResetVerification request, Action<CertificateReceivedEventArgs> successAction, 
                                               Action<ProcessingErrorEventArgs> processingError, Action<string> errorHandler)
        {
            var certificateClient = GetACertificateClient(errorHandler); 

            certificateClient.InvalidPasswordResetVerification += (sender, arg) => processingError(arg);
            certificateClient.CertificateReceived += (sender, arg) => successAction(arg);


            TryAsyncCommunication(() => certificateClient.VerifyPasswordReset(request), errorHandler);
        } 
        
        public static void VerifyEmail(string verificationCode, Action successAction, Action<ProcessingErrorEventArgs> processingError, Action<string> errorHandler)
        {
            var certificateClient = GetACertificateClient(errorHandler);

            certificateClient.InvalidEmailVerification += (sender, arg) => processingError(arg);
            certificateClient.CertificateAuthorizationRequired += delegate { successAction(); };
            certificateClient.EmailVerified += delegate { successAction(); };

            TryAsyncCommunication(() => certificateClient.VerifyEmail(new EmailVerification(verificationCode, false)), errorHandler);
        }

        public static void RegisterCertificate(CertificateRegistration request, Action successAction, Action<ProcessingErrorEventArgs> processingError, Action<string> errorHandler)
        {
            var certificateClient = GetACertificateClient(errorHandler);

            certificateClient.InvalidCertificateRegistration += (sender, arg) => processingError(arg);
            certificateClient.InvalidEmailVerification += (sender, arg) => processingError(arg);
            certificateClient.EmailVerificationRequired += delegate { successAction(); }; 

            TryAsyncCommunication(() => certificateClient.RegisterCertificate(request), errorHandler);
        }

        public static void RequestCertificate(CertificateRequest request, Action<CertificateReceivedEventArgs> successAction,
                                              Action<ProcessingErrorEventArgs> processingError,  Action<string> errorAction)
        {
            var certificateClient = GetACertificateClient(errorAction);

            certificateClient.InvalidCertificateRequest += (sender, arg) => processingError(arg);
            certificateClient.CertificateReceived += (sender, arg) => successAction(arg);

            TryAsyncCommunication(() => certificateClient.RequestCertificate(request), errorAction);
        }


        private static CertificateClient GetACertificateClient(Action<string> errorHandler)
        {
            var certificateClient = new CertificateClient
            {
                ProgramName = AssemblyHelper.ProductName,
                ProgramVersion = AssemblyHelper.BuildType + " " + AssemblyHelper.Version + " " + AssemblyHelper.InstallationType 
            };

            certificateClient.ServerErrorOccurred += delegate { errorHandler("Internal Server Error Occurred"); };
            certificateClient.ServerDisconnected += delegate { errorHandler("Server Disconnected"); };
            certificateClient.NewProtocolVersion += delegate { errorHandler("Deprecated Protocol Version"); };
            return certificateClient;
        }

        private static void TryAsyncCommunication(Action request, Action<string> error)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    request();
                }
                catch (Exception ex)
                {
                    error("Could not Communicate with the server: " + ex.Message);
                }
            });
        }
    }
}
