using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cryptool.PluginBase.Attributes
{
    /// <summary>
    /// When this attribute is put to an input our output property, it is only visibile 
    /// when CrypTool 2 is run with command line argument "-CryptoBenchmark"
    /// Otherwise, it is hidden to the "standard user"
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class CryptoBenchmarkPropertyAttribute : Attribute
    {
        public CryptoBenchmarkPropertyAttribute()
        {

        }
    }
}
