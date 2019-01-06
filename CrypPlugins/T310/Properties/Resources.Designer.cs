﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace T_310.Properties {
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
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("T_310.Properties.Resources", typeof(Resources).Assembly);
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
        ///   Looks up a localized string similar to Bit selector.
        /// </summary>
        internal static string BitSelectorCaption {
            get {
                return ResourceManager.GetString("BitSelectorCaption", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Most significant bits.
        /// </summary>
        internal static string BitSelectorList1 {
            get {
                return ResourceManager.GetString("BitSelectorList1", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Least significant bits.
        /// </summary>
        internal static string BitSelectorList2 {
            get {
                return ResourceManager.GetString("BitSelectorList2", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Choose if the most or least significant bits of certain vectors should be used.
        /// </summary>
        internal static string BitSelectorTooltip {
            get {
                return ResourceManager.GetString("BitSelectorTooltip", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Both keys are even. Odd keys are required. Check the documentation for more information..
        /// </summary>
        internal static string ErrorBothKeysEvenParity {
            get {
                return ResourceManager.GetString("ErrorBothKeysEvenParity", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Both keys have an incorrect length, provide 15 byte (120 bit) keys at both connectors. Both of these keys have to have an odd parity..
        /// </summary>
        internal static string ErrorBothKeysLength {
            get {
                return ResourceManager.GetString("ErrorBothKeysLength", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No keys connected, provide 15 byte (120 bit) keys at both connectors. Both these keys have to have an odd parity..
        /// </summary>
        internal static string ErrorBothKeysNull {
            get {
                return ResourceManager.GetString("ErrorBothKeysNull", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to After converting the message into CCITT-2 character encoding, no characters of the messages were left. (The message consists completely of invalid characters).
        /// </summary>
        internal static string ErrorEmptyConversion {
            get {
                return ResourceManager.GetString("ErrorEmptyConversion", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The &quot;magic number bytes&quot; of the message header were invalid (It consists of 4 byte 0x19, 25 bytes inizialization vector and 4 byte 0x0F).
        /// </summary>
        internal static string ErrorHeaderBytes {
            get {
                return ResourceManager.GetString("ErrorHeaderBytes", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The message header is invalid. (It consists of 4 byte 0x19, 25 bytes inizialization vector and 4 byte 0x0F). The 4 byte at the beginning 0x19 and 0x0F at the end were valid..
        /// </summary>
        internal static string ErrorHeaderIntegrity {
            get {
                return ResourceManager.GetString("ErrorHeaderIntegrity", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Message header could not be parsed, it is too short. Required Length: {0}; Given Length: {1}.
        /// </summary>
        internal static string ErrorHeaderLength {
            get {
                return ResourceManager.GetString("ErrorHeaderLength", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The input connector is empty, no action can be performed..
        /// </summary>
        internal static string ErrorInputEmpty {
            get {
                return ResourceManager.GetString("ErrorInputEmpty", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No input connected, please connect a byte[] as input..
        /// </summary>
        internal static string ErrorInputNull {
            get {
                return ResourceManager.GetString("ErrorInputNull", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The parity of Key {0} is even. An odd key is required. Check the documentation for more information..
        /// </summary>
        internal static string ErrorKeyEvenParity {
            get {
                return ResourceManager.GetString("ErrorKeyEvenParity", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Key {0} needs to be exactly 15 bytes long (120 bits). The given Key {0} is {1} bytes long..
        /// </summary>
        internal static string ErrorKeyLength {
            get {
                return ResourceManager.GetString("ErrorKeyLength", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Key {0} is not connected, provide a 15 byte (120 bit) key with an odd parity..
        /// </summary>
        internal static string ErrorKeyNull {
            get {
                return ResourceManager.GetString("ErrorKeyNull", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There were {0} unconvertable characters in the given message.
        /// </summary>
        internal static string ErrorUnconvertableBeginningPlural {
            get {
                return ResourceManager.GetString("ErrorUnconvertableBeginningPlural", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There was {0} unconvertable character in the given message.
        /// </summary>
        internal static string ErrorUnconvertableBeginningSingular {
            get {
                return ResourceManager.GetString("ErrorUnconvertableBeginningSingular", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ; they were truncated before encrypting..
        /// </summary>
        internal static string ErrorUnconvertableEndPlural {
            get {
                return ResourceManager.GetString("ErrorUnconvertableEndPlural", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ; it was truncated before encrypting..
        /// </summary>
        internal static string ErrorUnconvertableEndSingular {
            get {
                return ResourceManager.GetString("ErrorUnconvertableEndSingular", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Input data.
        /// </summary>
        internal static string InputDataCaption {
            get {
                return ResourceManager.GetString("InputDataCaption", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Enter plaintext (for encryption) or ciphertext (for decryption).
        /// </summary>
        internal static string InputDataTooltip {
            get {
                return ResourceManager.GetString("InputDataTooltip", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Subkey 1.
        /// </summary>
        internal static string InputKeyCaption1 {
            get {
                return ResourceManager.GetString("InputKeyCaption1", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Subkey 2.
        /// </summary>
        internal static string InputKeyCaption2 {
            get {
                return ResourceManager.GetString("InputKeyCaption2", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Enter a T-310 subkey as binary data. It needs to be exactly 120 bits (15 bytes) long and its parity must be odd..
        /// </summary>
        internal static string InputKeyTooltip1 {
            get {
                return ResourceManager.GetString("InputKeyTooltip1", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Enter a T-310 subkey as binary data. It needs to be exactly 120 bits (15 bytes) long and its parity must be odd..
        /// </summary>
        internal static string InputKeyTooltip2 {
            get {
                return ResourceManager.GetString("InputKeyTooltip2", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Long term key 14.
        /// </summary>
        internal static string Key14 {
            get {
                return ResourceManager.GetString("Key14", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Long term key 15.
        /// </summary>
        internal static string Key15 {
            get {
                return ResourceManager.GetString("Key15", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Long term key 16.
        /// </summary>
        internal static string Key16 {
            get {
                return ResourceManager.GetString("Key16", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Long term key 17.
        /// </summary>
        internal static string Key17 {
            get {
                return ResourceManager.GetString("Key17", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Long term key 21.
        /// </summary>
        internal static string Key21 {
            get {
                return ResourceManager.GetString("Key21", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Long term key 26.
        /// </summary>
        internal static string Key26 {
            get {
                return ResourceManager.GetString("Key26", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Long term key 29.
        /// </summary>
        internal static string Key29 {
            get {
                return ResourceManager.GetString("Key29", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Long term key 30.
        /// </summary>
        internal static string Key30 {
            get {
                return ResourceManager.GetString("Key30", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Long term key 31.
        /// </summary>
        internal static string Key31 {
            get {
                return ResourceManager.GetString("Key31", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Long term key 32.
        /// </summary>
        internal static string Key32 {
            get {
                return ResourceManager.GetString("Key32", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Long term key 33.
        /// </summary>
        internal static string Key33 {
            get {
                return ResourceManager.GetString("Key33", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Long term key.
        /// </summary>
        internal static string LongTermKeyCaption {
            get {
                return ResourceManager.GetString("LongTermKeyCaption", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Long term keys were special permutations used in the T-310 block cipher (Wurmreihe D-W). They were fixed to the machine, but could be changed..
        /// </summary>
        internal static string LongTermKeyTooltip {
            get {
                return ResourceManager.GetString("LongTermKeyTooltip", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Action.
        /// </summary>
        internal static string ModeCaption {
            get {
                return ResourceManager.GetString("ModeCaption", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Encrypt.
        /// </summary>
        internal static string ModeList1 {
            get {
                return ResourceManager.GetString("ModeList1", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Decrypt.
        /// </summary>
        internal static string ModeList2 {
            get {
                return ResourceManager.GetString("ModeList2", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Do you want the input data to be encrypted or decrypted?.
        /// </summary>
        internal static string ModeTooltip {
            get {
                return ResourceManager.GetString("ModeTooltip", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Output data.
        /// </summary>
        internal static string OutputDataCaption {
            get {
                return ResourceManager.GetString("OutputDataCaption", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Resulting ciphertext (when encrypting) or plaintext (when decrypting).
        /// </summary>
        internal static string OutputDataTooltip {
            get {
                return ResourceManager.GetString("OutputDataTooltip", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to T-310.
        /// </summary>
        internal static string PluginCaption {
            get {
                return ResourceManager.GetString("PluginCaption", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Historic Cipher Machine used by German Democratic Republic.
        /// </summary>
        internal static string PluginTooltip {
            get {
                return ResourceManager.GetString("PluginTooltip", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Version of the T-310.
        /// </summary>
        internal static string VersionCaption {
            get {
                return ResourceManager.GetString("VersionCaption", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to T-310/50 telex encryption.
        /// </summary>
        internal static string VersionList1 {
            get {
                return ResourceManager.GetString("VersionList1", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to T-310/51 data encryption.
        /// </summary>
        internal static string VersionList2 {
            get {
                return ResourceManager.GetString("VersionList2", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Die T310 was available in the models T-310/50 und T-310/51. Version 50 can only decrypt CCITT-2 encoded characters; version 51 any data. .
        /// </summary>
        internal static string VersionTooltip {
            get {
                return ResourceManager.GetString("VersionTooltip", resourceCulture);
            }
        }
    }
}
