﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AESVisualisation.Properties {
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
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("AESVisualisation.Properties.Resources", typeof(Resources).Assembly);
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
        ///   Looks up a localized string similar to The round key is added to the current state by XORing the bytes..
        /// </summary>
        internal static string addKeyExplanation {
            get {
                return ResourceManager.GetString("addKeyExplanation", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to First, the last four bytes of the previous key are taken. Afterwards, the last byte is placed at the front..
        /// </summary>
        internal static string expansionExplanation {
            get {
                return ResourceManager.GetString("expansionExplanation", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Every byte is exchanged with the corresponding byte from the S-box.
        /// </summary>
        internal static string expansionExplanation1 {
            get {
                return ResourceManager.GetString("expansionExplanation1", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A round constant is added.
        /// </summary>
        internal static string expansionExplanation2 {
            get {
                return ResourceManager.GetString("expansionExplanation2", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Adding the first four bytes of the previous key gives you the first four bytes of the next key..
        /// </summary>
        internal static string expansionExplanation3 {
            get {
                return ResourceManager.GetString("expansionExplanation3", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to For column x of the new key you XOR column x from the previous key with column x-1 from the new key..
        /// </summary>
        internal static string expansionExplanation4 {
            get {
                return ResourceManager.GetString("expansionExplanation4", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Input of the key used for the encryption.
        /// </summary>
        internal static string inputKeyDescription {
            get {
                return ResourceManager.GetString("inputKeyDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Key input.
        /// </summary>
        internal static string inputKeyName {
            get {
                return ResourceManager.GetString("inputKeyName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Input of the text that is to be encrypted.
        /// </summary>
        internal static string inputTextDescription {
            get {
                return ResourceManager.GetString("inputTextDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Text input.
        /// </summary>
        internal static string inputTextName {
            get {
                return ResourceManager.GetString("inputTextName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Keysize.
        /// </summary>
        internal static string KeysizeCaption {
            get {
                return ResourceManager.GetString("KeysizeCaption", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Select the size of the key..
        /// </summary>
        internal static string KeysizeTooltip {
            get {
                return ResourceManager.GetString("KeysizeTooltip", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to One column is taken from the current state and placed next to the multiplication matrix..
        /// </summary>
        internal static string mixColExplanation {
            get {
                return ResourceManager.GetString("mixColExplanation", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Then it is multiplied with the multiplication matrix to determine the next column of the next state..
        /// </summary>
        internal static string mixColExplanation1 {
            get {
                return ResourceManager.GetString("mixColExplanation1", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Text output.
        /// </summary>
        internal static string OutputStreamCaption {
            get {
                return ResourceManager.GetString("OutputStreamCaption", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Output of the ciphertext.
        /// </summary>
        internal static string OutputStreamTooltip {
            get {
                return ResourceManager.GetString("OutputStreamTooltip", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to AES Visualization.
        /// </summary>
        internal static string PluginCaption {
            get {
                return ResourceManager.GetString("PluginCaption", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Visualization of AES encryption.
        /// </summary>
        internal static string PluginTooltip {
            get {
                return ResourceManager.GetString("PluginTooltip", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Round 1.
        /// </summary>
        internal static string Round1 {
            get {
                return ResourceManager.GetString("Round1", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Round 10.
        /// </summary>
        internal static string Round10 {
            get {
                return ResourceManager.GetString("Round10", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Round 2.
        /// </summary>
        internal static string Round2 {
            get {
                return ResourceManager.GetString("Round2", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Round 3.
        /// </summary>
        internal static string Round3 {
            get {
                return ResourceManager.GetString("Round3", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Round 4.
        /// </summary>
        internal static string Round4 {
            get {
                return ResourceManager.GetString("Round4", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Round 5.
        /// </summary>
        internal static string Round5 {
            get {
                return ResourceManager.GetString("Round5", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Round 6.
        /// </summary>
        internal static string Round6 {
            get {
                return ResourceManager.GetString("Round6", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Round 7.
        /// </summary>
        internal static string Round7 {
            get {
                return ResourceManager.GetString("Round7", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Round 8.
        /// </summary>
        internal static string Round8 {
            get {
                return ResourceManager.GetString("Round8", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Round 9.
        /// </summary>
        internal static string Round9 {
            get {
                return ResourceManager.GetString("Round9", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to First, the second row is shifted once to the left. Then, the third row is shifted twice towards the left, and finally the forth row is shifted three times to the left. The overlapping bytes are transferred to the right to form a 4 x 4 matrix..
        /// </summary>
        internal static string shiftRowExplanation {
            get {
                return ResourceManager.GetString("shiftRowExplanation", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to First, a byte is transferred from the state matrix to the transition spot and split up..
        /// </summary>
        internal static string subBytesExplanation {
            get {
                return ResourceManager.GetString("subBytesExplanation", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The corresponding byte in the S-box is determined and placed into the result matrix..
        /// </summary>
        internal static string subBytesExplanation1 {
            get {
                return ResourceManager.GetString("subBytesExplanation1", resourceCulture);
            }
        }
    }
}
