﻿using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;

namespace Cryptool.PluginBase.Properties
{
    public sealed partial class Settings
    {
        [UserScopedSetting()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [SettingsSerializeAs(SettingsSerializeAs.Binary)]
        public ArrayList Wizard_Storage
        {
            get
            {
                return ((ArrayList)(this["Wizard_Storage"]));
            }
            set
            {
                this["Wizard_Storage"] = value;
            }
        }

        [UserScopedSetting()]
        public System.Windows.Media.FontFamily FontFamily
        {
            get
            {
                return ((System.Windows.Media.FontFamily)(this["FontFamily"]));
            }
            set
            {
                this["FontFamily"] = value;
            }
        }

        [UserScopedSetting()]
        [global::System.Configuration.DefaultSettingValueAttribute("12")]
        public double FontSize
        {
            get
            {
                return ((double)(this["FontSize"]));
            }
            set
            {
                this["FontSize"] = value;
            }
        }
    }
}
