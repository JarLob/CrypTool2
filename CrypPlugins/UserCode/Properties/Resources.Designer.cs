﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.235
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Cryptool.Plugins.UserCode.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Cryptool.Plugins.UserCode.Properties.Resources", typeof(Resources).Assembly);
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
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to UserCode.
        /// </summary>
        internal static string PluginCaption {
            get {
                return ResourceManager.GetString("PluginCaption", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to UserCode.
        /// </summary>
        internal static string Pluginooltip {
            get {
                return ResourceManager.GetString("Pluginooltip", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to using System.Numerics;
        ///
        ///namespace Cryptool.Plugins.UserCode
        ///{
        ///
        ///    public class UserClass
        ///    {
        ///        private Cryptool.Plugins.UserCode.UserCode _userCode;
        ///        public UserClass(Cryptool.Plugins.UserCode.UserCode userCode)
        ///        {
        ///            _userCode = userCode;
        ///        }
        ///
        ///        public void UserMethod()
        ///        {
        ///            //USERCODE//
        ///        }
        ///
        ///        public void ProgressChanged(double percentage)
        ///        {
        ///            _userCode.ProgressChanged(percentage, 1.0);
        ///         [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string UserClass {
            get {
                return ResourceManager.GetString("UserClass", resourceCulture);
            }
        }
    }
}
