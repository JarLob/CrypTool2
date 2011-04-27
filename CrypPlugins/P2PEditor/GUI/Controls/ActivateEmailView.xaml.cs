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
using System.Windows.Media;

namespace Cryptool.P2PEditor.GUI.Controls
{
    [Localization("Cryptool.P2PEditor.Properties.Resources")]
    public partial class ActivateEmailView
    {
        public static string WorldName = ".*";

        public ActivateEmailView()
        {
            InitializeComponent();            
        }

        private void ActivateButton_Click(object sender, RoutedEventArgs e)
        {
            this.MessageLabel.Visibility = Visibility.Hidden;
            this.MessageBox.Visibility = Visibility.Hidden;
            if (string.IsNullOrEmpty(this.ActivationCodeField.Text))
            {

                this.MessageLabel.Text = "Activation code may not be empty.";
                this.P2PEditor.GuiLogMessage(this.MessageLabel.Text.ToString(), NotificationLevel.Info);
                this.MessageLabel.Visibility = Visibility.Visible;
                this.ActivationCodeField.Focus();
                return;
            }
          
            if (!Verification.IsValidPassword(this.PasswordField.Password))
            {
                this.MessageLabel.Text = "Password is not valid.";
                this.P2PEditor.GuiLogMessage(this.MessageLabel.Text.ToString(), NotificationLevel.Info);
                this.MessageLabel.Visibility = Visibility.Visible;
                this.PasswordField.Password = "";
                this.PasswordField.Focus();
                return;
            }
            
            Requesting = true;
            Thread thread = new Thread(new ParameterizedThreadStart(ActivateEmail));
            thread.CurrentCulture = Thread.CurrentThread.CurrentCulture;
            thread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
            EmailVerification emailVer = new EmailVerification(this.ActivationCodeField.Text, false);
            thread.Start(emailVer);
        }

        public void ActivateEmail(object o)
        {
            EmailVerification emailVer = (EmailVerification)o;

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
                        this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            this.MessageLabel.Text = "SSLCertificate revoked. Please update CrypTool 2.0.";
                            this.P2PEditor.GuiLogMessage(this.MessageLabel.Text.ToString(), NotificationLevel.Info);
                            this.MessageLabel.Visibility = Visibility.Visible;
                        }, null);
                    });
                    certificateClient.HttpTunnelEstablished += new EventHandler<ProxyEventArgs>(delegate
                    {
                        this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            this.P2PEditor.GuiLogMessage("HttpTunnel successfully established", NotificationLevel.Debug);
                        }, null);
                    });
                    certificateClient.NoProxyConfigured += new EventHandler<EventArgs>(delegate
                    {
                        this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            this.MessageLabel.Text = "No proxy server configured. Please check your configuration.";
                            this.P2PEditor.GuiLogMessage(this.MessageLabel.Text.ToString(), NotificationLevel.Info);
                            this.MessageLabel.Visibility = Visibility.Visible;
                        }, null);
                    });
                    certificateClient.ProxyErrorOccured += ProxyErrorOccured;
                }

                Assembly asm = Assembly.GetEntryAssembly();
                certificateClient.ProgramName = asm.GetName().Name;
                certificateClient.ProgramVersion = AssemblyHelper.GetVersionString(asm);

                certificateClient.CertificateAuthorizationRequired += new EventHandler<EventArgs>(delegate
                {
                    this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        this.MessageLabel.Text = "Certificate authorization required";
                        this.P2PEditor.GuiLogMessage(this.MessageLabel.Text.ToString(), NotificationLevel.Info);
                        this.MessageLabel.Visibility = Visibility.Visible;
                    }, null); 
                });

                certificateClient.CertificateReceived += CertificateReceived;

                certificateClient.InvalidEmailVerification += InvalidEmailVerification;

                certificateClient.ServerErrorOccurred += new EventHandler<ProcessingErrorEventArgs>(delegate
                {
                    this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        this.MessageLabel.Text = "Server error occurred. Please try again later";
                        this.P2PEditor.GuiLogMessage(this.MessageLabel.Text.ToString(), NotificationLevel.Info);
                        this.MessageLabel.Visibility = Visibility.Visible;
                    }, null); 
                });

                certificateClient.NewProtocolVersion += new EventHandler<EventArgs>(delegate
                {
                    this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        this.MessageLabel.Text = "New ProtocolVersion. Please update CrypTool 2.0";
                        this.P2PEditor.GuiLogMessage(this.MessageLabel.Text.ToString(), NotificationLevel.Info);
                        this.MessageLabel.Visibility = Visibility.Visible;
                    }, null); 
                });

                certificateClient.VerifyEmail(emailVer);
            }
            catch (NetworkException nex)
            {
                this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    this.MessageLabel.Text = "There was a communication problem with the server: " + nex.Message + "\n" + "Please try again later";
                    this.P2PEditor.GuiLogMessage(this.MessageLabel.Text.ToString(), NotificationLevel.Error);
                    this.MessageLabel.Visibility = Visibility.Visible;
                }, null);
            }
            catch (Exception ex)
            {
                this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    this.MessageLabel.Text = "An exception occured: " + ex.Message;
                    this.P2PEditor.GuiLogMessage(this.MessageLabel.Text.ToString(), NotificationLevel.Error);
                    this.MessageLabel.Visibility = Visibility.Visible;
                }, null);
            }
            finally
            {
                Requesting = false;                
            }
        }

        public void InvalidEmailVerification(object sender, ProcessingErrorEventArgs args)
        {
            try
            {
                switch (args.Type)
                {
                    case ErrorType.NoCertificateFound:
                        this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            this.MessageLabel.Text = "You have entered a wrong verification code.";
                            this.P2PEditor.GuiLogMessage(this.MessageLabel.Text.ToString(), NotificationLevel.Info);
                            this.MessageLabel.Visibility = Visibility.Visible;
                        }, null);
                        break;
                    case ErrorType.WrongPassword:
                        this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            this.MessageLabel.Text = "The verification code is ok but the entered password was wrong.";
                            this.P2PEditor.GuiLogMessage(this.MessageLabel.Text.ToString(), NotificationLevel.Info);
                            this.MessageLabel.Visibility = Visibility.Visible;
                        }, null);
                        this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {                           ;
                            this.PasswordField.Password = "";
                            this.PasswordField.Focus();
                        }, null);        
                        break;
                    default:
                        this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            this.MessageLabel.Text = "Invalid certificate request: " + args.Message ?? args.Type.ToString();
                            this.P2PEditor.GuiLogMessage(this.MessageLabel.Text.ToString(), NotificationLevel.Error);
                            this.MessageLabel.Visibility = Visibility.Visible;
                        }, null);
                        break;
                }
            }
            catch (Exception) 
            { 
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
                if (!Directory.Exists(PeerCertificate.DEFAULT_USER_CERTIFICATE_DIRECTORY))
                {
                    Directory.CreateDirectory(PeerCertificate.DEFAULT_USER_CERTIFICATE_DIRECTORY);
                    this.P2PEditor.GuiLogMessage("Automatic created account folder: " + PeerCertificate.DEFAULT_USER_CERTIFICATE_DIRECTORY, NotificationLevel.Info);
                }
            }
            catch (Exception ex)
            {
                this.MessageLabel.Text = "Cannot create default account data directory '" + PeerCertificate.DEFAULT_USER_CERTIFICATE_DIRECTORY + "':\n" + ex.Message;
                this.P2PEditor.GuiLogMessage(this.MessageLabel.Text.ToString(), NotificationLevel.Error);
                this.MessageLabel.Visibility = Visibility.Visible;
                return;
            }

            try
            {
                args.Certificate.SaveCrtToAppData();
                args.Certificate.SavePkcs12ToAppData(args.Certificate.Password);                
                this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {                    
                    this.PasswordField.Password = "";
                    this.ActivatePage.Visibility = System.Windows.Visibility.Hidden;
                    this.OKPage.Visibility = System.Windows.Visibility.Visible;
                }, null);                
            }
            catch (Exception ex)
            {
                 this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            this.MessageLabel.Text = "Could not save the received certificate to your AppData folder:\n\n" +
                                (ex.GetBaseException() != null && ex.GetBaseException().Message != null ? ex.GetBaseException().Message : ex.Message);
                            this.P2PEditor.GuiLogMessage(this.MessageLabel.Text.ToString(), NotificationLevel.Error);
                            this.MessageLabel.Visibility = Visibility.Visible;
                        }, null);
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
                            this.ActivateButton.IsEnabled = false;
                            storyboard.Begin();
                        }
                        else
                        {
                            this.RequestLabel.Visibility = System.Windows.Visibility.Hidden;
                            this.ActivateButton.IsEnabled = true;
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
            this.P2PEditorPresentation.ShowGetNewCertificateView();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.ActivatePage.Visibility = Visibility.Visible;
            this.OKPage.Visibility = Visibility.Hidden;
            this.P2PEditorPresentation.ShowConnectView();
        }

        private void ProxyErrorOccured(object sender, ProxyEventArgs args)
        {
            this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                this.MessageLabel.Text = "Proxy Error (" + args.StatusCode + ") occured:" + args.Message;
                this.P2PEditor.GuiLogMessage(this.MessageLabel.Text.ToString(), NotificationLevel.Error);
                this.MessageLabel.Visibility = Visibility.Visible;
            }, null);
        }

        /// <summary>
        /// Logs a message to the network editor gui
        /// </summary>
        /// <param name="message"></param>
        /// <param name="error"></param>
        public void LogMessage(string message, bool error = false)
        {
            this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                if (error)
                {
                    this.Erroricon.Visibility = Visibility.Visible;
                    this.MessageLabel.Foreground = Brushes.Red;
                }
                else
                {
                    this.Erroricon.Visibility = Visibility.Hidden;
                    this.MessageLabel.Foreground = Brushes.Black;
                }
                this.MessageLabel.Text = message;
                this.P2PEditor.GuiLogMessage(message, NotificationLevel.Info);
                this.MessageLabel.Visibility = Visibility.Visible;
                this.MessageBox.Visibility = Visibility.Visible;
            }, null);
        }
    }
}