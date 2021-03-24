﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Dieser Code wurde von einem Tool generiert.
//     Laufzeitversion:4.0.30319.42000
//
//     Änderungen an dieser Datei können falsches Verhalten verursachen und gehen verloren, wenn
//     der Code erneut generiert wird.
// </auto-generated>
//------------------------------------------------------------------------------

namespace HKDFSHA256.Properties {
    using System;
    
    
    /// <summary>
    ///   Eine stark typisierte Ressourcenklasse zum Suchen von lokalisierten Zeichenfolgen usw.
    /// </summary>
    // Diese Klasse wurde von der StronglyTypedResourceBuilder automatisch generiert
    // -Klasse über ein Tool wie ResGen oder Visual Studio automatisch generiert.
    // Um einen Member hinzuzufügen oder zu entfernen, bearbeiten Sie die .ResX-Datei und führen dann ResGen
    // mit der /str-Option erneut aus, oder Sie erstellen Ihr VS-Projekt neu.
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
        ///   Gibt die zwischengespeicherte ResourceManager-Instanz zurück, die von dieser Klasse verwendet wird.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("HKDFSHA256.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Überschreibt die CurrentUICulture-Eigenschaft des aktuellen Threads für alle
        ///   Ressourcenzuordnungen, die diese stark typisierte Ressourcenklasse verwenden.
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
        ///   Sucht eine lokalisierte Zeichenfolge, die Orientate implementation to RFC 5869 ähnelt.
        /// </summary>
        internal static string ConfigInfinityOutput {
            get {
                return ResourceManager.GetString("ConfigInfinityOutput", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die Configuration is based on RFC 5869. This means that an 8-bit counter is used and the length of the output bytes is limited to 8192. Further information can be found in the help. ähnelt.
        /// </summary>
        internal static string ConfigInfinityOutputTooltip {
            get {
                return ResourceManager.GetString("ConfigInfinityOutputTooltip", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die Explain the HKDF SHA-256 ähnelt.
        /// </summary>
        internal static string ConfigPresCaption {
            get {
                return ResourceManager.GetString("ConfigPresCaption", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die Activates or deactivates the explanations ähnelt.
        /// </summary>
        internal static string ConfigPresTooltip {
            get {
                return ResourceManager.GetString("ConfigPresTooltip", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die Save generated key material to file ähnelt.
        /// </summary>
        internal static string ConfigPrintKMToFileCaption {
            get {
                return ResourceManager.GetString("ConfigPrintKMToFileCaption", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die Defines, that the generated key material will be saved to a file, so that it can be used in other tools. ähnelt.
        /// </summary>
        internal static string ConfigPrintKMToFileTooltip {
            get {
                return ResourceManager.GetString("ConfigPrintKMToFileTooltip", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die While requesting memory from your system, there was an exception. Please try a smaller value for the output bytes. ähnelt.
        /// </summary>
        internal static string ExSystemOutOfMemory {
            get {
                return ResourceManager.GetString("ExSystemOutOfMemory", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die Please specify a smaller value for requested length of outputbytes. You specified: {0}. Allowed is only: {1} ähnelt.
        /// </summary>
        internal static string ExToMuchOutputRequested {
            get {
                return ResourceManager.GetString("ExToMuchOutputRequested", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die Visualization of the Key Derivation Function HKDF SHA-256 ähnelt.
        /// </summary>
        internal static string HKDFSHA256Tooltip {
            get {
                return ResourceManager.GetString("HKDFSHA256Tooltip", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die CTXInfo ähnelt.
        /// </summary>
        internal static string InputCtxInfoCaption {
            get {
                return ResourceManager.GetString("InputCtxInfoCaption", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die Input of the applicationspecific constant CTXInfo ähnelt.
        /// </summary>
        internal static string InputCtxInfoToolTip {
            get {
                return ResourceManager.GetString("InputCtxInfoToolTip", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die Length of the key material ähnelt.
        /// </summary>
        internal static string InputOutputLengthCaption {
            get {
                return ResourceManager.GetString("InputOutputLengthCaption", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die Length of the key material (in byte) ähnelt.
        /// </summary>
        internal static string InputOutputLengthToolTip {
            get {
                return ResourceManager.GetString("InputOutputLengthToolTip", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die Salt ähnelt.
        /// </summary>
        internal static string InputSaltCaption {
            get {
                return ResourceManager.GetString("InputSaltCaption", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die Input of the salt ähnelt.
        /// </summary>
        internal static string InputSaltToolTip {
            get {
                return ResourceManager.GetString("InputSaltToolTip", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die Source Key Material ähnelt.
        /// </summary>
        internal static string InputSKMCaption {
            get {
                return ResourceManager.GetString("InputSKMCaption", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die Input of source key material ähnelt.
        /// </summary>
        internal static string InputSKMToolTip {
            get {
                return ResourceManager.GetString("InputSKMToolTip", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die Output of the {0}{ord} calculation round:
        ///
        ///Byte 1 - 8:     {1}
        ///Byte 9 - 16:   {2}
        ///Byte 17 - 24: {3}
        ///Byte 25 - 32: {4} ähnelt.
        /// </summary>
        internal static string KeyMaterialDebugTextTemplate {
            get {
                return ResourceManager.GetString("KeyMaterialDebugTextTemplate", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die Key Material ähnelt.
        /// </summary>
        internal static string OutputKeyMaterialCaption {
            get {
                return ResourceManager.GetString("OutputKeyMaterialCaption", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die Debug ähnelt.
        /// </summary>
        internal static string OutputKeyMaterialDebugCaption {
            get {
                return ResourceManager.GetString("OutputKeyMaterialDebugCaption", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die Output of the single steps of the key derivation ähnelt.
        /// </summary>
        internal static string OutputKeyMaterialDebugToolTip {
            get {
                return ResourceManager.GetString("OutputKeyMaterialDebugToolTip", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die Output of the to generate key material ähnelt.
        /// </summary>
        internal static string OutputKeyMaterialToolTip {
            get {
                return ResourceManager.GetString("OutputKeyMaterialToolTip", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die HKDF SHA-256 ähnelt.
        /// </summary>
        internal static string PluginCaption {
            get {
                return ResourceManager.GetString("PluginCaption", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die Skip calculation ähnelt.
        /// </summary>
        internal static string PresCalc {
            get {
                return ResourceManager.GetString("PresCalc", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die The calculation of the key material can be described as following: ähnelt.
        /// </summary>
        internal static string PresConstructionPart1Text {
            get {
                return ResourceManager.GetString("PresConstructionPart1Text", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die The HKDF procedure consists of two steps. In the first step, a pseudo random key (PRK) is generated. It is calculated on the basis of the SKM and an optional salt. ähnelt.
        /// </summary>
        internal static string PresConstructionPart2Text {
            get {
                return ResourceManager.GetString("PresConstructionPart2Text", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die In step 2, the key material is calculated. For this purpose, the prk from step 1 is used as a secret key for the HMAC. The KM is calculated step by step. The HKDF method uses a feedback loop and a counter for this purpose: The respective precalculated value is used as input for the subsequent calculation. The counter is incremented in each step. The input CTXinfo is an application-specific constant, which can also be empty. ähnelt.
        /// </summary>
        internal static string PresConstructionPart3Text {
            get {
                return ResourceManager.GetString("PresConstructionPart3Text", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die The following picture shows the second step: ähnelt.
        /// </summary>
        internal static string PresConstructionPart4Text {
            get {
                return ResourceManager.GetString("PresConstructionPart4Text", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die Step 1: Extract
        ///        PRK = HMAC-SHA256(Salt, SKM)
        ///
        ///Step 2: Expand
        ///        KM(0) = empty string (zero length)
        ///        KM(1) = HMAC-SHA256(PRK, KM(0) || info || CTR)
        ///        KM(2) = HMAC-SHA256(PRK, KM (1) || info || CTR)
        ///        …
        ///        KM(n) = HMAC-SHA256(PRK, KM (n-1) || info || CTR) ähnelt.
        /// </summary>
        internal static string PresConstructionScheme {
            get {
                return ResourceManager.GetString("PresConstructionScheme", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die Construction ähnelt.
        /// </summary>
        internal static string PresConstructionSectionHeading {
            get {
                return ResourceManager.GetString("PresConstructionSectionHeading", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die 2. Construction of HKDF SHA-256 ähnelt.
        /// </summary>
        internal static string PresConstructionSectionHeadingNum {
            get {
                return ResourceManager.GetString("PresConstructionSectionHeadingNum", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die An error occured. Please find detailed information in the log of CrypTool2. ähnelt.
        /// </summary>
        internal static string PresErrorText {
            get {
                return ResourceManager.GetString("PresErrorText", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die HKDF SHA-256 ähnelt.
        /// </summary>
        internal static string PresExplanationSectionHeading {
            get {
                return ResourceManager.GetString("PresExplanationSectionHeading", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die 4. Calculation finished ähnelt.
        /// </summary>
        internal static string PresFinishedSectionHeading {
            get {
                return ResourceManager.GetString("PresFinishedSectionHeading", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die To repeat the calculation, there are 3 alternatives:
        ///
        ///- either stop the execution and start it again
        ///- or change one of the inputs 
        ///- or click on the &quot;Restart&quot; button (then the calculation will restart automatically) ähnelt.
        /// </summary>
        internal static string PresFinishedText {
            get {
                return ResourceManager.GetString("PresFinishedText", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die Skip chapter ähnelt.
        /// </summary>
        internal static string PresIntro {
            get {
                return ResourceManager.GetString("PresIntro", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die The function HKDF SHA-256 is a key derivation function (KDF). It uses the Keyed-Hash Message Authentication Code (HMAC) as a pseudorandom function (PRF). As inputs it gets the SKM and the secret key. In this implementation, the SHA-256 is used in the HMAC. This key derivation fuction is recommended by the National Institute of Standards and Technology (NIST). ähnelt.
        /// </summary>
        internal static string PresIntroductionPart1Text {
            get {
                return ResourceManager.GetString("PresIntroductionPart1Text", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die Introduction ähnelt.
        /// </summary>
        internal static string PresIntroductionSectionHeading {
            get {
                return ResourceManager.GetString("PresIntroductionSectionHeading", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die 1. Introduction ähnelt.
        /// </summary>
        internal static string PresIntroductionSectionHeadingNum {
            get {
                return ResourceManager.GetString("PresIntroductionSectionHeadingNum", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die Step 1: Calculation of PRK with following inputs:
        ///
        ///SKM: {0} 
        ///Salt: {1} ähnelt.
        /// </summary>
        internal static string PresIterationPRKCalc {
            get {
                return ResourceManager.GetString("PresIterationPRKCalc", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die Step 2: Calculation round {0} of {1}
        ///
        ///PRK: {2} 
        ///SKM: {3}
        ///CTXInfo: {4}
        ///Counter: {5} ähnelt.
        /// </summary>
        internal static string PresIterationRounds {
            get {
                return ResourceManager.GetString("PresIterationRounds", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die Calculation ähnelt.
        /// </summary>
        internal static string PresIterationSectionHeading {
            get {
                return ResourceManager.GetString("PresIterationSectionHeading", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die 3. Calculation ähnelt.
        /// </summary>
        internal static string PresIterationSectionHeadingNum {
            get {
                return ResourceManager.GetString("PresIterationSectionHeadingNum", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die Output of the {0}{ord} calculation round:
        ///
        ///Byte 1 - 8: {1}
        ///Byte 9 - 16: {2}
        ///Byte 17 - 24: {3}
        ///Byte 25 - 32: {4} ähnelt.
        /// </summary>
        internal static string PresKeyMaterialDebugTextTemplate {
            get {
                return ResourceManager.GetString("PresKeyMaterialDebugTextTemplate", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die Next ähnelt.
        /// </summary>
        internal static string PresNext {
            get {
                return ResourceManager.GetString("PresNext", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die Output of PRK:
        ///
        ///Byte 1 - 8: {1}
        ///Byte 9 - 16: {2}
        ///Byte 17 - 24: {3}
        ///Byte 25 - 32: {4} ähnelt.
        /// </summary>
        internal static string PresPRKDebugTextTemplate {
            get {
                return ResourceManager.GetString("PresPRKDebugTextTemplate", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die Restart ähnelt.
        /// </summary>
        internal static string PresRestart {
            get {
                return ResourceManager.GetString("PresRestart", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die &lt;Bold&gt;&lt;Underline&gt;Inputs:&lt;/Underline&gt;&lt;/Bold&gt;
        ///&lt;Bold&gt;Source Key Material:&lt;/Bold&gt; Specifies the source key material (SKM)
        ///
        ///&lt;Bold&gt;Salt:&lt;/Bold&gt; Specifies the salt value for the calculation of the pseudorandom key (PRK). Does not have to be secret.
        ///
        ///&lt;Bold&gt;Context Information:&lt;/Bold&gt; Specifies the applicationspecific constant context information (CTXInfo). Can be empty.
        ///
        ///&lt;Bold&gt;Length of the key material (in byte):&lt;/Bold&gt; Specifies the length of the key material in byte.
        ///
        ///&lt;Bold&gt;&lt;Underline&gt;Outputs:&lt;/Underlin [Rest der Zeichenfolge wurde abgeschnitten]&quot;; ähnelt.
        /// </summary>
        internal static string PresSectionIntroductionText {
            get {
                return ResourceManager.GetString("PresSectionIntroductionText", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die Start ähnelt.
        /// </summary>
        internal static string PresStart {
            get {
                return ResourceManager.GetString("PresStart", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die {0}/{1} ähnelt.
        /// </summary>
        internal static string PresStepText {
            get {
                return ResourceManager.GetString("PresStepText", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die HKDF SHA-256
        ///--
        ///a Key Derivation Function with extendable output length ähnelt.
        /// </summary>
        internal static string PresTitleHeading {
            get {
                return ResourceManager.GetString("PresTitleHeading", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die Saving-Parameters ähnelt.
        /// </summary>
        internal static string PrintToFileGroup {
            get {
                return ResourceManager.GetString("PrintToFileGroup", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die Output of PRK:
        ///
        ///Byte 1 - 8:   {1}
        ///Byte 9 - 16:  {2}
        ///Byte 17 - 24: {3}
        ///Byte 25 - 32: {4} ähnelt.
        /// </summary>
        internal static string PRKDebugTextTemplate {
            get {
                return ResourceManager.GetString("PRKDebugTextTemplate", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die Save key material to file: ähnelt.
        /// </summary>
        internal static string SaveFileDialogCaption {
            get {
                return ResourceManager.GetString("SaveFileDialogCaption", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die Specifies the output file for the generated key material, if you want to save it to a file ähnelt.
        /// </summary>
        internal static string SaveFileDialogTooltip {
            get {
                return ResourceManager.GetString("SaveFileDialogTooltip", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die The maximum amout of outputbytes are 8160 in case of implementation refered to  RFC 5869 . The requested amout of {0} byte was set to the maximum. ähnelt.
        /// </summary>
        internal static string TooMuchOutputRequestedLogForKPFStd {
            get {
                return ResourceManager.GetString("TooMuchOutputRequestedLogForKPFStd", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die The maximum amount of output are 5 MB 5242880 byte). The requested amount of  {0} byte was set to the maximum. ähnelt.
        /// </summary>
        internal static string TooMuchOutputRequestedLogMSG {
            get {
                return ResourceManager.GetString("TooMuchOutputRequestedLogMSG", resourceCulture);
            }
        }
    }
}
