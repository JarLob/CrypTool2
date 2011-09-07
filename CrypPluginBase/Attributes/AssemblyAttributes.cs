﻿using System;

namespace Cryptool.PluginBase.Attributes
{
    /************************ Ct2BuildType ************************/
    public enum Ct2BuildType
    {
        Developer = 0,
        Nightly = 1,
        Beta = 2,
        Stable = 3
    }

    [AttributeUsage(AttributeTargets.Assembly)]
    public class AssemblyCt2BuildTypeAttribute : Attribute
    {
        public Ct2BuildType BuildType
        {
            get; set;
        }

        public AssemblyCt2BuildTypeAttribute(Ct2BuildType type)
        {
            this.BuildType = type;
        }

        public AssemblyCt2BuildTypeAttribute(int type)
        {
            this.BuildType = (Ct2BuildType) type;
        }
    }

    /********************** Ct2InstallationType ********************/
    public enum Ct2InstallationType
    {
        Developer = 0,
        ZIP = 1,
        MSI = 2,
        NSIS = 3
    }

    [AttributeUsage(AttributeTargets.Assembly)]
    public class AssemblyCt2InstallationTypeAttribute : Attribute
    {
        public Ct2InstallationType InstallationType
        {
            get;
            set;
        }

        public AssemblyCt2InstallationTypeAttribute(Ct2InstallationType type)
        {
            this.InstallationType = type;
        }

        public AssemblyCt2InstallationTypeAttribute(int type)
        {
            this.InstallationType = (Ct2InstallationType)type;
        }
    }
}
