using System;

namespace Cryptool.PluginBase.Attributes
{
    /// <summary>
    /// When this attribute is put to an input our output property or a setting property or method, it is only visibile 
    /// when CrypTool 2 is run with command line argument "-CryptoBenchmark"
    /// Otherwise, it is hidden to the "standard user"
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
    public class CryptoBenchmarkAttribute : Attribute
    {
        public CryptoBenchmarkAttribute()
        {

        }
    }
}
