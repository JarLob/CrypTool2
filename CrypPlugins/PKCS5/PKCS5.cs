﻿//////////////////////////////////////////////////////////////////////////////////////////////////
// CrypTool V2
// © 2008 - Gerhard Junker
// Apache License see http://www.apache.org/licenses/
//
// $HeadURL$
//////////////////////////////////////////////////////////////////////////////////////////////////
// $Revision::                                                                                $://
// $Author::                                                                                  $://
// $Date::                                                                                    $://
//////////////////////////////////////////////////////////////////////////////////////////////////

// see http://tools.ietf.org/html/rfc2898
// based on ftp://ftp.rsasecurity.com/pub/pkcs/pkcs-5v2/pkcs5v2_1.pdf


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Security.Cryptography;
using System.Text;

using Cryptool.PluginBase;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase.Miscellaneous;

namespace PKCS5
{
  [Author("Gerhard Junker", null, "private project member", null)]
  //"http://tools.ietf.org/html/rfc2898"
  [PluginInfo("PKCS5.Properties.Resources", false, "PluginCaption", "PluginTooltip", "PluginDescriptionURL", "PKCS5/PKCS5.png")]
  [ComponentCategory(ComponentCategory.HashFunctions)]
  public class PKCS5 : ICrypComponent
  {
    private enum argType
    {
      Object, // unknown - default
      Stream,
      ByteArray,
      String
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="PKCS5"/> class.
    /// </summary>
    public PKCS5()
    {
      this.settings = new PKCS5Settings();
    }

    #region Settings

    private PKCS5Settings settings;

    /// <summary>
    /// Gets or sets the settings.
    /// </summary>
    /// <value>The settings.</value>
    public ISettings Settings
    {
      get
      {
        return settings;
      }
      set
      {
        settings = (PKCS5Settings)value;
        OnPropertyChanged("Settings");
        GuiLogMessage("Settings changed.", NotificationLevel.Debug);
      }
    }


    /// <summary>
    /// Gets or sets a value indicating whether this instance has changes.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this instance has changes; otherwise, <c>false</c>.
    /// </value>
    public bool HasChanges
    {
      get
      {
        return settings.HasChanges;
      }

      set
      {
        settings.HasChanges = value;
        GuiLogMessage("HasChanges changed.", NotificationLevel.Debug);
      }
    }

    #endregion

    private string GetStringForSelectedEncoding(byte[] arrByte)
    {
      if (arrByte != null)
      {
        string returnValue;

        // here conversion happens
        switch (settings.Encoding)
        {
          case PKCS5Settings.EncodingTypes.Unicode:
            returnValue = Encoding.Unicode.GetString(arrByte, 0, arrByte.Length);
            break;
          case PKCS5Settings.EncodingTypes.UTF7:
            returnValue = Encoding.UTF7.GetString(arrByte, 0, arrByte.Length);
            break;
          case PKCS5Settings.EncodingTypes.UTF8:
            returnValue = Encoding.UTF8.GetString(arrByte, 0, arrByte.Length);
            break;
          case PKCS5Settings.EncodingTypes.UTF32:
            returnValue = Encoding.UTF32.GetString(arrByte, 0, arrByte.Length);
            break;
          case PKCS5Settings.EncodingTypes.ASCII:
            returnValue = Encoding.ASCII.GetString(arrByte, 0, arrByte.Length);
            break;
          case PKCS5Settings.EncodingTypes.BigEndianUnicode:
            returnValue = Encoding.BigEndianUnicode.GetString(arrByte, 0, arrByte.Length);
            break;
          case PKCS5Settings.EncodingTypes.Default:
          default:
            returnValue = Encoding.Default.GetString(arrByte, 0, arrByte.Length);
            break;
        }
        return returnValue;
      }
      return null;
    }


    public byte[] GetByteArrayForSelectedEncodingByteArray(string data)
    {
      byte[] byteArrayOutput = null;

      if (!string.IsNullOrEmpty(data))
      {
        // here conversion happens        
        switch (settings.Encoding)
        {
          case PKCS5Settings.EncodingTypes.Unicode:
            byteArrayOutput = Encoding.Unicode.GetBytes(data.ToCharArray());
            break;
          case PKCS5Settings.EncodingTypes.UTF7:
            byteArrayOutput = Encoding.UTF7.GetBytes(data.ToCharArray());
            break;
          case PKCS5Settings.EncodingTypes.UTF8:
            byteArrayOutput = Encoding.UTF8.GetBytes(data.ToCharArray());
            break;
          case PKCS5Settings.EncodingTypes.UTF32:
            byteArrayOutput = Encoding.UTF32.GetBytes(data.ToCharArray());
            break;
          case PKCS5Settings.EncodingTypes.ASCII:
            byteArrayOutput = Encoding.ASCII.GetBytes(data.ToCharArray());
            break;
          case PKCS5Settings.EncodingTypes.BigEndianUnicode:
            byteArrayOutput = Encoding.BigEndianUnicode.GetBytes(data.ToCharArray());
            break;
          case PKCS5Settings.EncodingTypes.Default:
          default:
            byteArrayOutput = Encoding.Default.GetBytes(data.ToCharArray());
            break;
        }
        return byteArrayOutput;
      }

      return null;
    }


    #region Input key / password

    // Input key
    private byte[] key = { };
    private argType keyType = argType.Object;

    /// <summary>
    /// Gets or sets the input data.
    /// </summary>
    /// <value>The input key.</value>
    [PropertyInfo(Direction.InputData, "KeyCaption", "KeyTooltip", "", true, false, QuickWatchFormat.Hex, null)]
    public System.Object Key
    {
      get
      {
        switch(keyType)
        {
          default:
          //case argType.Object:
          //case argType.Stream:
            {
                return new CStreamWriter(key);
            }

          case argType.ByteArray:
            return key;

          case argType.String:
            return GetStringForSelectedEncoding(key);
        }
      }
      set
      {
        if (null == value)
          return;

        if (value is ICryptoolStream)
        {
          keyType = argType.Stream;

          using (CStreamReader cst = ((ICryptoolStream)value).CreateReader())
          {
              cst.WaitEof();

              long len = cst.Length;
              key = new byte[len];

              for (long i = 0; i < len; i++)
                  key[i] = (byte)cst.ReadByte();
          }

        }
        else if (value is byte[])
        {
          keyType = argType.ByteArray;

          byte[] ba = (byte[])value;
          
          long len = ba.Length;
          key = new byte[len];

          for (long i = 0; i < len; i++)
            key[i] = ba[i];
        }
        else if (value is string)
        {
          string str = (string) value;
          key = GetByteArrayForSelectedEncodingByteArray(str);
        }
        else
        {
          throw new InvalidCastException("Invalid data type for Key property.");
        }

        OnPropertyChanged("Key");
        GuiLogMessage("Key changed.", NotificationLevel.Debug);
      }
    }

    #endregion

    #region Salt data / Seed data

    // Salt Data
    private byte[] salt = { };
    argType saltType = argType.Object;

    /// <summary>
    /// Gets or sets the salt data.
    /// </summary>
    /// <value>The salt data.</value>
    [PropertyInfo(Direction.InputData, "SaltCaption", "SaltTooltip", "", false, false, QuickWatchFormat.Hex, null)]
    public Object Salt
    {
      get
      {
        switch (saltType)
        {
          default:
          //case argType.Object:
          //case argType.Stream:
            {
              return new CStreamWriter(salt);
            }

          case argType.ByteArray:
            return salt;

          case argType.String:
            return GetStringForSelectedEncoding(salt);
        }
      }
      set
      {
        if (null == value)
          return;

        if (value is ICryptoolStream)
        {
          saltType = argType.Stream;

          using (CStreamReader cst = ((ICryptoolStream)value).CreateReader())
          {
              cst.WaitEof();
              long len = cst.Length;
              salt = new byte[len];

              for (long i = 0; i < len; i++)
                  salt[i] = (byte)cst.ReadByte();
          }
        }
        else if (value is byte[])
        {
          saltType = argType.ByteArray;

          byte[] ba = (byte[])value;

          long len = ba.Length;
          salt = new byte[len];

          for (long i = 0; i < len; i++)
            salt[i] = ba[i];
        }
        else if (value is string)
        {
          string str = (string)value;
          salt = GetByteArrayForSelectedEncodingByteArray(str);
        }

        else
        {
          throw new InvalidCastException("Invalid data type for Salt property.");
        }

        OnPropertyChanged("Salt");
        GuiLogMessage("Salt changed.", NotificationLevel.Debug);
      }
    }
 
    #endregion

    #region Output

    // Output
    private byte[] outputData = { };

    /// <summary>
    /// Notifies the update output.
    /// </summary>
    private void NotifyUpdateOutput()
    {
      OnPropertyChanged("HashOutputStream");
      OnPropertyChanged("HashOutputData");
    }


    /// <summary>
    /// Gets or sets the output data stream.
    /// </summary>
    /// <value>The output data stream.</value>
    [PropertyInfo(Direction.OutputData, "HashOutputStreamCaption", "HashOutputStreamTooltip", "", true, false, QuickWatchFormat.Hex, null)]
    public ICryptoolStream HashOutputStream
    {
      get
      {
          return new CStreamWriter(outputData);
        }
      //set
      //{
      //} //readonly
    }

    /// <summary>
    /// Gets the output data.
    /// </summary>
    /// <value>The output data.</value>
    [PropertyInfo(Direction.OutputData, "HashOutputDataCaption", "HashOutputDataTooltip", "", true, false, QuickWatchFormat.Hex, null)]
    public byte[] HashOutputData
    {
      get
      {
        GuiLogMessage("Got HashOutputData.", NotificationLevel.Debug);
        return this.outputData;
      }
    }

    /// <summary>
    /// Hashes this instance.
    /// </summary>
    public void Hash()
    {
      System.Security.Cryptography.PKCS5MaskGenerationMethod pkcs5Hash = 
							new System.Security.Cryptography.PKCS5MaskGenerationMethod();

      pkcs5Hash.SelectedShaFunction =
        (PKCS5MaskGenerationMethod.ShaFunction)settings.SHAFunction;

      outputData =
        pkcs5Hash.GenerateMask(this.key, this.salt, settings.Count, settings.Length >> 3);

      NotifyUpdateOutput();
    }
    #endregion

    #region IPlugin Member

#pragma warning disable 67
    public event StatusChangedEventHandler OnPluginStatusChanged;
    public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
    public event PluginProgressChangedEventHandler OnPluginProgressChanged;
#pragma warning restore

    private void Progress(double value, double max)
    {
        EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
    }

    /// <summary>
    /// Provide all presentation stuff in this user control, it will be opened in an tab.
    /// Return null if your plugin has no presentation.
    /// </summary>
    /// <value>The presentation.</value>
    public System.Windows.Controls.UserControl Presentation
    {
      get
      {
        return null;
      }
    }

      /// <summary>
    /// Will be called from editor after restoring settings and before adding to workspace.
    /// </summary>
    public void Initialize()
    {
      GuiLogMessage("Initialize.", NotificationLevel.Debug);
    }

    /// <summary>
    /// Will be called from editor before right before chain-run starts
    /// </summary>
    public void PreExecution()
    {
      GuiLogMessage("PreExecution.", NotificationLevel.Debug);
    }

    /// <summary>
    /// Will be called from editor while chain-run is active and after last necessary input
    /// for plugin has been set.
    /// </summary>
    public void Execute()
    {
      Progress(0.0, 1.0);

      GuiLogMessage("Execute.", NotificationLevel.Debug);
      Hash();

      Progress(1.0, 1.0);
    }

    /// <summary>
    /// Will be called from editor after last plugin in chain has finished its work.
    /// </summary>
    public void PostExecution()
    {
      GuiLogMessage("PostExecution.", NotificationLevel.Debug);
    }

    /// <summary>
    /// Not defined yet.
    /// </summary>
    public void Pause()
    {
      GuiLogMessage("Pause.", NotificationLevel.Debug);
    }

    /// <summary>
    /// Will be called from editor while chain-run is active. Plugin hast to stop work immediately.
    /// </summary>
    public void Stop()
    {
      GuiLogMessage("Stop.", NotificationLevel.Debug);
    }

    /// <summary>
    /// Will be called from editor when element is deleted from worksapce.
    /// Releases unmanaged and - optionally - managed resources
    /// </summary>
    public void Dispose()
    {
      GuiLogMessage("Dispose.", NotificationLevel.Debug);
    }

    #endregion

    #region INotifyPropertyChanged Member

    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// Called when [property changed].
    /// </summary>
    /// <param name="name">The name.</param>
    protected void OnPropertyChanged(string name)
    {
      if (PropertyChanged != null)
      {
        if (name == "Settings")
          Hash();
        else
          PropertyChanged(this, new PropertyChangedEventArgs(name));
      }
    }

    /// <summary>
    /// GUIs the log message.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="logLevel">The log level.</param>
    private void GuiLogMessage(string message, NotificationLevel logLevel)
    {
      EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this,
        new GuiLogEventArgs(message, this, logLevel));
    }

    #endregion
  }
}
