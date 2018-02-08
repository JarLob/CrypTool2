﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30128.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PKCS1.Resources.lang.Gui {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class SigGenRsaCtrl {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal SigGenRsaCtrl() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("PKCS1.Resources.lang.Gui.SigGenRsaCtrl", typeof(SigGenRsaCtrl).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unverschlüsselt.
        /// </summary>
        public static string decrypted {
            get {
                return ResourceManager.GetString("decrypted", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Verschlüsselt.
        /// </summary>
        public static string encrypted {
            get {
                return ResourceManager.GetString("encrypted", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Erzeugte Signatur.
        /// </summary>
        public static string generatedSig {
            get {
                return ResourceManager.GetString("generatedSig", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Signatur erzeugen.
        /// </summary>
        public static string genSig {
            get {
                return ResourceManager.GetString("genSig", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Öffentlicher Schlüssel:.
        /// </summary>
        public static string pubKey {
            get {
                return ResourceManager.GetString("pubKey", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to RSA-Schlüsselgröße (in Bit):.
        /// </summary>
        public static string rsaKeySize {
            get {
                return ResourceManager.GetString("rsaKeySize", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Struktur.
        /// </summary>
        public static string structure {
            get {
                return ResourceManager.GetString("structure", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Sichere PKCS#1-Signatur erzeugen.
        /// </summary>
        public static string title {
            get {
                return ResourceManager.GetString("title", resourceCulture);
            }
        }
    }
}
