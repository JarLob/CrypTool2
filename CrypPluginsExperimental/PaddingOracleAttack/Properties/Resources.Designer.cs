﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.296
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PaddingOracleAttack.Properties {
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
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("PaddingOracleAttack.Properties.Resources", typeof(Resources).Assembly);
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
        ///   Looks up a localized string similar to Decrypt completely.
        /// </summary>
        internal static string btnAll {
            get {
                return ResourceManager.GetString("btnAll", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Auto search.
        /// </summary>
        internal static string btnAuto {
            get {
                return ResourceManager.GetString("btnAuto", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Change Byte.
        /// </summary>
        internal static string btnLblP1 {
            get {
                return ResourceManager.GetString("btnLblP1", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Go to Phase 2.
        /// </summary>
        internal static string btnLblP1End {
            get {
                return ResourceManager.GetString("btnLblP1End", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Send Request.
        /// </summary>
        internal static string btnLblP1Init {
            get {
                return ResourceManager.GetString("btnLblP1Init", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Next byte.
        /// </summary>
        internal static string btnLblP2 {
            get {
                return ResourceManager.GetString("btnLblP2", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Go to Phase 3.
        /// </summary>
        internal static string btnLblP2End {
            get {
                return ResourceManager.GetString("btnLblP2End", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Start Search.
        /// </summary>
        internal static string btnLblP2Init {
            get {
                return ResourceManager.GetString("btnLblP2Init", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Decrypt Byte.
        /// </summary>
        internal static string btnLblP3Decrypt {
            get {
                return ResourceManager.GetString("btnLblP3Decrypt", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Recover Plaintext.
        /// </summary>
        internal static string btnLblP3End {
            get {
                return ResourceManager.GetString("btnLblP3End", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Change Current Byte.
        /// </summary>
        internal static string btnLblP3Find {
            get {
                return ResourceManager.GetString("btnLblP3Find", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Increase Padding.
        /// </summary>
        internal static string btnLblP3IncPad {
            get {
                return ResourceManager.GetString("btnLblP3IncPad", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Next.
        /// </summary>
        internal static string btnNext {
            get {
                return ResourceManager.GetString("btnNext", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Plaintext Recovered. Attack completed successfully..
        /// </summary>
        internal static string descDone {
            get {
                return ResourceManager.GetString("descDone", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Please wait while the message is being decrypted..
        /// </summary>
        internal static string descFinishAll {
            get {
                return ResourceManager.GetString("descFinishAll", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Phase 1 finished! Valid padding found..
        /// </summary>
        internal static string descP1Done {
            get {
                return ResourceManager.GetString("descP1Done", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Phase 1: Find a message that decrypts to a valid padding. Send the unchanged message to see if a valid padding already exists..
        /// </summary>
        internal static string descP1Init {
            get {
                return ResourceManager.GetString("descP1Init", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Task: Change the last byte until the padding is valid..
        /// </summary>
        internal static string descP1Task {
            get {
                return ResourceManager.GetString("descP1Task", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Phase 2 finished! First padding byte found! Padding length:.
        /// </summary>
        internal static string descP2Done {
            get {
                return ResourceManager.GetString("descP2Done", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Phase 2 finished! The first 7 bytes did not affect the padding, so the padding length must be 1!.
        /// </summary>
        internal static string descP2DoneSpecial {
            get {
                return ResourceManager.GetString("descP2DoneSpecial", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Phase 2: Find first padding byte. Change the bytes from left to right. If the padding turns invalid, a padding byte must have been changed..
        /// </summary>
        internal static string descP2Init {
            get {
                return ResourceManager.GetString("descP2Init", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The changed byte does not belong to the padding. Try the next byte!.
        /// </summary>
        internal static string descP2Task {
            get {
                return ResourceManager.GetString("descP2Task", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Byte Decrypted! Increase the padding to continue the decryption..
        /// </summary>
        internal static string descP3Dec {
            get {
                return ResourceManager.GetString("descP3Dec", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The byte was decrypted. Increase the padding to continue the decryption..
        /// </summary>
        internal static string descP3DecDone {
            get {
                return ResourceManager.GetString("descP3DecDone", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to All Padding Bytes Decrypted. Increase the padding to continue the decryption..
        /// </summary>
        internal static string descP3DecPadDone {
            get {
                return ResourceManager.GetString("descP3DecPadDone", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Message was decrypted! Click to see the original plaintext..
        /// </summary>
        internal static string descP3Done {
            get {
                return ResourceManager.GetString("descP3Done", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Valid padding found! Byte can be decrypted..
        /// </summary>
        internal static string descP3FindDone {
            get {
                return ResourceManager.GetString("descP3FindDone", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Task: Change the byte until the message decrypts to the desired value! If the correct value is found, the padding will turn valid..
        /// </summary>
        internal static string descP3FindTask {
            get {
                return ResourceManager.GetString("descP3FindTask", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Phase 3: Decrypt the message. Start with decrypting the padding bytes..
        /// </summary>
        internal static string descP3Init {
            get {
                return ResourceManager.GetString("descP3Init", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Response from the Padding Oracle.
        /// </summary>
        internal static string descPadIn {
            get {
                return ResourceManager.GetString("descPadIn", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to English.
        /// </summary>
        internal static string langCheck {
            get {
                return ResourceManager.GetString("langCheck", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Automatically decrypt the complete message..
        /// </summary>
        internal static string ttBtnAll {
            get {
                return ResourceManager.GetString("ttBtnAll", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Search automatically for the next valid value..
        /// </summary>
        internal static string ttBtnAuto {
            get {
                return ResourceManager.GetString("ttBtnAuto", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Perform the next step..
        /// </summary>
        internal static string ttBtnNext {
            get {
                return ResourceManager.GetString("ttBtnNext", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Undo the last performed action..
        /// </summary>
        internal static string ttBtnReturn {
            get {
                return ResourceManager.GetString("ttBtnReturn", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Return to the beginning of the current phase..
        /// </summary>
        internal static string ttBtnReturnPhase {
            get {
                return ResourceManager.GetString("ttBtnReturnPhase", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The encrypted informationblock..
        /// </summary>
        internal static string ttCipherBlock {
            get {
                return ResourceManager.GetString("ttCipherBlock", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The corrupted initializationblock (original C1 XOR O)..
        /// </summary>
        internal static string ttCorruptedBlock {
            get {
                return ResourceManager.GetString("ttCorruptedBlock", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The decrypted initializationblock (is completely known at the end). XORd with the original initializationblock C1 results in the plaintext P2..
        /// </summary>
        internal static string ttDecBlock {
            get {
                return ResourceManager.GetString("ttDecBlock", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The overlay used to modify the initializationblock..
        /// </summary>
        internal static string ttOverlayBlock {
            get {
                return ResourceManager.GetString("ttOverlayBlock", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The resulting plaintext when XORing D2 with the original initializationblock C1 and O..
        /// </summary>
        internal static string ttPlainBlock {
            get {
                return ResourceManager.GetString("ttPlainBlock", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The encrypted initializationblock..
        /// </summary>
        internal static string ttPrelBlock {
            get {
                return ResourceManager.GetString("ttPrelBlock", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Amount of requests sent to the server.
        /// </summary>
        internal static string ttSentRequests {
            get {
                return ResourceManager.GetString("ttSentRequests", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The plugin only displays 8 bytes at a time. If the block size exceeds 8 bytes, hidden bytes can be displayed by using the scrollbar..
        /// </summary>
        internal static string ttViewByte {
            get {
                return ResourceManager.GetString("ttViewByte", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Currently Viewing Bytes.
        /// </summary>
        internal static string ttViewByteDesc {
            get {
                return ResourceManager.GetString("ttViewByteDesc", resourceCulture);
            }
        }
    }
}
