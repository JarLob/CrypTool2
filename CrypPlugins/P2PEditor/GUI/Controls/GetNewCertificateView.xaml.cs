﻿using System;
using System.IO;
using System.Reflection;
using System.Windows;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using PeersAtPlay.CertificateLibrary.Network;
using System.Threading;
using PeersAtPlay.CertificateLibrary.Util;
using System.Windows.Threading;
using System.Windows.Media.Animation;
using PeersAtPlay.CertificateLibrary.Certificates;
using Cryptool.P2P;
using Cryptool.P2P.Internal;
using Cryptool.PluginBase.Attributes;

namespace Cryptool.P2PEditor.GUI.Controls
{
    [Localization("Cryptool.P2PEditor.Properties.Resources")]
    public partial class GetNewCertificate
    {
        public static string WorldName = ".*";

        public GetNewCertificate()
        {
            InitializeComponent();            
        }

        private void Request_Click(object sender, RoutedEventArgs e)
        {
            this.MessageLabel.Visibility = Visibility.Hidden;
            if (!Verification.IsValidAvatar(this.UsernameField.Text))
            {

                this.LogMessage(Properties.Resources.Username_is_not_valid_);                   
                this.UsernameField.Focus();
                this.UsernameField.SelectAll();
                return;
            }

            if (!Verification.IsValidEmailAddress(this.EmailField.Text))
            {
                this.LogMessage(Properties.Resources.Email_is_not_valid_);
                this.EmailField.Focus();
                this.EmailField.SelectAll();
                return;
            }
            
            if (this.PasswordField.Password != this.ConfirmField.Password)
            {
                this.LogMessage(Properties.Resources.Passwords_did_not_match_);
                this.PasswordField.Password = "";
                this.ConfirmField.Password = "";
                this.PasswordField.Focus();
                return;
            }    

            if (!Verification.IsValidPassword(this.PasswordField.Password))
            {
                this.LogMessage(Properties.Resources.Password_is_not_valid_);
                this.PasswordField.Password = "";
                this.ConfirmField.Password = "";
                this.PasswordField.Focus();
                return;
            }
            
            Requesting = true;
            Thread thread = new Thread(new ParameterizedThreadStart(RetrieveCertificate));
            CertificateRegistration certReg = new CertificateRegistration(this.UsernameField.Text,
                                                     this.EmailField.Text,
                                                     WorldName,
                                                     this.PasswordField.Password);
            thread.Start(certReg);
        }

        public void RetrieveCertificate(object o)
        {
            CertificateRegistration certReg = (CertificateRegistration)o;

            try
            {
                CertificateClient certificateClient = new CertificateClient();

                //use a proxy server:
                if (P2PSettings.Default.UseProxy)
                {
                    certificateClient.ProxyAddress = P2PSettings.Default.ProxyServer;
                    certificateClient.ProxyPort = P2PSettings.Default.ProxyPort;
                    certificateClient.ProxyAuthName = P2PSettings.Default.ProxyUser;
                    certificateClient.ProxyAuthPassword = P2PBase.DecryptString(P2PSettings.Default.ProxyPassword);
                    certificateClient.UseProxy = true;
                    certificateClient.UseSystemWideProxy = P2PSettings.Default.UseSystemWideProxy;
                    certificateClient.SslCertificateRefused += new EventHandler<EventArgs>(delegate
                    {
                        this.LogMessage(Properties.Resources.SSLCertificate_revoked__Please_update_CrypTool_2_0_);
                    });
                    certificateClient.HttpTunnelEstablished += new EventHandler<ProxyEventArgs>(delegate
                    {
                       this.LogMessage(Properties.Resources.HttpTunnel_successfully_established_);
                    });
                    certificateClient.NoProxyConfigured += new EventHandler<EventArgs>(delegate
                    {
                        this.LogMessage(Properties.Resources.No_proxy_server_configured__Please_check_your_configuration_);
                    });
                    certificateClient.ProxyErrorOccured += ProxyErrorOccured;
                }

                Assembly asm = Assembly.GetEntryAssembly();
                certificateClient.ProgramName = asm.GetName().Name;
                certificateClient.ProgramVersion = AssemblyHelper.GetVersionString(asm);

                certificateClient.CertificateAuthorizationRequired += new EventHandler<EventArgs>(delegate
                {
                    this.LogMessage(Properties.Resources.Certificate_authorization_required);
                });

                certificateClient.EmailVerificationRequired += new EventHandler<EventArgs>(delegate
                {
                    this.LogMessage(Properties.Resources.GetNewCertificate_RetrieveCertificate_);
                    this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        this.UsernameField.Text = "";
                        this.EmailField.Text = "";
                        this.PasswordField.Password = "";
                        this.ConfirmField.Password = "";
                        this.MessageLabel.Visibility = Visibility.Visible;
                    }, null);
                });

                certificateClient.CertificateReceived += CertificateReceived;

                certificateClient.InvalidCertificateRegistration += InvalidCertificateRegistration;

                certificateClient.ServerErrorOccurred += new EventHandler<ProcessingErrorEventArgs>(delegate
                {
                    this.LogMessage(Properties.Resources.Server_error_occurred__Please_try_again_later);
                });

                certificateClient.NewProtocolVersion += new EventHandler<EventArgs>(delegate
                {
                    this.LogMessage(Properties.Resources.New_ProtocolVersion__Please_update_CrypTool_2_0);
                });

                certificateClient.RegisterCertificate(certReg);

            }
            catch (NetworkException nex)
            {
               this.LogMessage(String.Format(Properties.Resources.There_was_a_communication_problem_with_the_server,nex.Message));
            }
            catch (Exception ex)
            {
               this.LogMessage(String.Format(Properties.Resources.An_exception_occured___1, ex.Message));
            }
            finally
            {
                Requesting = false;                
            }
        }

        public void InvalidCertificateRegistration(object sender, ProcessingErrorEventArgs args)
        {
            try
            {
                switch (args.Type)
                {
                    case ErrorType.AvatarAlreadyExists:
                        this.LogMessage(Properties.Resources.The_username_already_exists__Please_choose_another_one_);
                        break;
                    case ErrorType.EmailAlreadyExists:
                        this.LogMessage(Properties.Resources.The_email_already_exists__Please_choose_another_one_);
                        break;
                    case ErrorType.WrongPassword:
                        this.LogMessage(Properties.Resources.The_username_and_email_already_exist_but_the_entered_password_was_wrong_);
                        this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            this.PasswordField.Password = "";
                            this.ConfirmField.Password = "";
                            this.PasswordField.Focus();
                        }, null);        
                        break;
                    default:
                        this.LogMessage(String.Format(Properties.Resources.InvalidCertificateRegistration_Invalid_registration___1, args.Type));
                        break;
                }
            }
            catch (Exception ex) 
            {
                this.LogMessage(String.Format(Properties.Resources.InvalidCertificateRegistration_Exception_occured___1, ex.Message));
                return;
            }
            finally
            {
                Requesting = false;
            } 
        }

        public void CertificateReceived(object sender, CertificateReceivedEventArgs args)
        {
            try
            {
                try
                {
                    if (!Directory.Exists(PeerCertificate.DEFAULT_USER_CERTIFICATE_DIRECTORY))
                    {
                        Directory.CreateDirectory(PeerCertificate.DEFAULT_USER_CERTIFICATE_DIRECTORY);
                        this.P2PEditor.GuiLogMessage(String.Format(Properties.Resources.Automatic_created_account_folder_,PeerCertificate.DEFAULT_USER_CERTIFICATE_DIRECTORY), NotificationLevel.Info);
                    }
                }
                catch (Exception ex)
                {
                    this.LogMessage(String.Format(Properties.Resources.Cannot_create_default_account_data_directory_, ex.Message));
                    return;
                }

                args.Certificate.SaveCrtToAppData();
                args.Certificate.SavePkcs12ToAppData(args.Certificate.Password);                
                this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {                    
                    ((P2PEditorPresentation)((P2PEditor)GetValue(P2PEditorProperty)).Presentation).Connect.Username.Text = this.UsernameField.Text;
                    ((P2PEditorPresentation)((P2PEditor)GetValue(P2PEditorProperty)).Presentation).Connect.Password.Password = this.PasswordField.Password;
                    this.UsernameField.Text = "";
                    this.EmailField.Text = "";
                    this.PasswordField.Password = "";
                    this.ConfirmField.Password = "";
                    this.RequestPage.Visibility = System.Windows.Visibility.Hidden;
                    this.OKPage.Visibility = System.Windows.Visibility.Visible;
                }, null);                
            }
            catch (Exception ex)
            {
                this.LogMessage(String.Format(Properties.Resources.Could_not_save_the_received_certificate_to_your_AppData_folder_,
                                (ex.GetBaseException() != null && ex.GetBaseException().Message != null ? ex.GetBaseException().Message : ex.Message)));
            }
            finally
            {
                Requesting = false;
            }
        }

        private bool requesting = false;
        public bool Requesting
        {
            get { return requesting; }
            set
            {
                requesting = value;
                try
                {
                    this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        Storyboard storyboard = (Storyboard)FindResource("AnimateWorldIcon");
                        if (requesting)
                        {
                            this.RequestLabel.Visibility = System.Windows.Visibility.Visible;
                            this.RequestButton.IsEnabled = false;
                            storyboard.Begin();
                        }
                        else
                        {
                            this.RequestLabel.Visibility = System.Windows.Visibility.Hidden;
                            this.RequestButton.IsEnabled = true;
                            storyboard.Stop();
                        }
                    }, null);
                }
                catch (Exception)
                {                    
                }
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.P2PEditorPresentation.ShowConnectView();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.RequestPage.Visibility = Visibility.Visible;
            this.OKPage.Visibility = Visibility.Hidden;
            this.P2PEditorPresentation.ShowConnectView();
        }

        private void ProxyErrorOccured(object sender, ProxyEventArgs args)
        {
            this.LogMessage(String.Format(Properties.Resources.ProxyErrorOccured_Proxy_Error___1__occured_, args.Message));
        }

        /// <summary>
        /// Logs a message to the network editor gui
        /// </summary>
        /// <param name="message"></param>
        /// <param name="error"></param>
        private void LogMessage(string message, bool error = false)
        {
            this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                this.MessageLabel.Content = message;
                this.P2PEditor.GuiLogMessage(message, NotificationLevel.Info);
                this.MessageLabel.Visibility = Visibility.Visible;
            }, null);
        }
    }
}