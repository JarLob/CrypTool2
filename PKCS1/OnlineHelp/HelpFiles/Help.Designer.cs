﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30128.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PKCS1.OnlineHelp.HelpFiles {
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
    internal class Help {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Help() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("PKCS1.OnlineHelp.HelpFiles.Help", typeof(Help).Assembly);
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
        ///   Looks up a localized string similar to &lt;h2&gt;Bleichenbacher Signatur generieren&lt;/h2&gt;
        ///In diesem Tab wird eine gefälschte Signatur generiert, die in ver- und entschlüsselter Form dargestellt wird. 
        ///Eine Signatur, die von fehlerhaften Implementierungen als valide erkannt wird, hat folgende Struktur: 
        ///&apos;00&apos; &apos;01&apos; PS &apos;00&apos; HI HW GG. &lt;/br&gt;
        ///Im Einzelnen bedeutet dies:
        ///&lt;ul&gt;
        ///&lt;li&gt;
        ///&lt;strong&gt;&apos;00&apos;&lt;/strong&gt; 
        ///Einleitender Nullblock (8 Bit). Dadurch wird gewährleistet dass, der numerische Wert der Signatur kleiner ist als das 
        ///&lt;a href=&quot;help://KeyGen_ModulusS [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string Gen_Bleichenb_Sig_Tab {
            get {
                return ResourceManager.GetString("Gen_Bleichenb_Sig_Tab", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;h2&gt;Datenblock generieren&lt;/h2&gt;
        ///In diesem Tab kann der Datenblock einer Signatur generiert werden. Der Datenblock besteht aus den zwei Teilen &quot;Hashfunction-Identifier&quot; und &quot;Hashwert&quot;.
        ///&lt;ul&gt;
        ///&lt;li&gt;
        ///&lt;strong&gt;Hashfunction-Identifier&lt;br /&gt;&lt;/strong&gt;
        ///Der Hashfunction-Identifier ist ein ASN.1-codierter Datenblock, der unter anderem Informationen wie den Namen der verwendeten Hashfunktion (Algorithmidentifier), die Länge des gesamten Datenblocks, und die Länge des Hashwertes beinhaltet.&lt;/br&gt;
        ///Die Länge und der Wert [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string Gen_Datablock_Tab {
            get {
                return ResourceManager.GetString("Gen_Datablock_Tab", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;h2&gt;Kuehn Signatur generieren&lt;/h2&gt;
        ///In diesem Tab können gefälschte Signaturen, nach der Methode wie sie Ulrich Kühn beschrieben hat, erstellt werden.
        ///Die Signaturen ähneln in der Struktur denen von &lt;a href=&quot;help://Gen_Bleichenb_Sig_Tab&quot;&gt;Bleichenbacher&lt;/a&gt;, machen sich jedoch die Rechenkraft von
        ///Computern zu nutze und sind auch auf Signaturen von 1024 Bit Länge anwendbar. Auch hier liegt folgende Struktur zugrunde: &apos;00&apos; &apos;01&apos; PS &apos;00&apos; HI HW GG. &lt;/br&gt;
        ///
        ///Die Unterschiede zu den Bleichenbacher Signaturen sind [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string Gen_Kuehn_Iterations {
            get {
                return ResourceManager.GetString("Gen_Kuehn_Iterations", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;h2&gt;Kuehn Signatur generieren&lt;/h2&gt;
        ///In diesem Tab können gefälschte Signaturen, nach der Methode wie sie Ulrich Kühn beschrieben hat, erstellt werden.
        ///Die Signaturen ähneln in der Struktur denen von &lt;a href=&quot;help://Gen_Bleichenb_Sig_Tab&quot;&gt;Bleichenbacher&lt;/a&gt;, machen sich jedoch die Rechenkraft von
        ///Computern zu nutze und sind auch auf Signaturen von 1024 Bit Länge anwendbar. Auch hier liegt folgende Struktur zugrunde: &apos;00&apos; &apos;01&apos; PS &apos;00&apos; HI HW GG. &lt;/br&gt;
        ///
        ///Die Unterschiede zu den Bleichenbacher Signaturen sind [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string Gen_Kuehn_Sig_Tab {
            get {
                return ResourceManager.GetString("Gen_Kuehn_Sig_Tab", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;h2&gt;Signatur generieren&lt;/h2&gt;
        ///In diesem Tab wird die komplette PKCS#1-Signatur erstellt. Die Signatur hat folgende Struktur: &apos;00&apos; &apos;01&apos; PS &apos;00&apos; HI HW. &lt;/br&gt;
        ///Im Einzelnen bedeutet dies:
        ///&lt;ul&gt;
        ///&lt;li&gt;
        ///&lt;strong&gt;&apos;00&apos;&lt;/strong&gt; 
        ///Einleitender Nullblock (8 Bit). Dadurch wird gewährleistet dass der numerische Wert der Signatur kleiner ist als das 
        ///&lt;a href=&quot;help://KeyGen_ModulusSize&quot;&gt;RSA-Modul.&lt;/a&gt;
        ///&lt;/li&gt;
        ///&lt;li&gt;
        ///&lt;strong&gt;&apos;01&apos;&lt;/strong&gt;
        ///Block Type. Dieser Block gibt an, ob es sich um eine Operation mit dem privaten ode [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string Gen_PKCS1_Sig_Tab {
            get {
                return ResourceManager.GetString("Gen_PKCS1_Sig_Tab", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;h2&gt;RSA-Schlüsselgenerierung&lt;/h2&gt;
        ///Um PKCS#1-Signaturen erzeugen und validieren zu können, ist ein RSA-Schlüsselpaar notwendig. Dieses besteht aus einem privaten und einem öffentlichen Schlüssel, sowie einem sog. RSA-Modul, der bei beiden Schlüsseln gleich ist.&lt;/br&gt;
        ///Für die hier dargestellten Angriffe auf die PKCS#1-Signaturen sind der Wert des öffentlichen Schlüssels und die Länge des Moduls (in Bit) wichtig. Diese Parameter können hier konfiguriert werden. Der öffentliche Schlüssel sowie der Modul werden [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string KeyGen {
            get {
                return ResourceManager.GetString("KeyGen", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;h2&gt;RSA-Modul&lt;/h2&gt;
        ///Der Modul ist Teil des öffentlichen RSA-Schlüssels. Der Modul wird auch bei der Operation mit dem privaten Schlüssel gebraucht.&lt;/br&gt;
        ///
        ///Da für die Angriffe auf die PKCS#1-Signaturen nicht der Wert, sondern nur die Länge in Bit nötig ist, kann hier die Länge angegeben werden und es wird ein Modul erzeugt.
        ///In dem Bleichenbacher Angriff wurde von einer Bitlänge des Moduls von 3072 ausgegangen. Bei den Angriffen mit kürzeren Schlüsseln kann hier die Schlüssellänge reduziert werden.
        ///.
        /// </summary>
        internal static string KeyGen_ModulusSize {
            get {
                return ResourceManager.GetString("KeyGen_ModulusSize", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;h2&gt;RSA öffentlicher Schlüssel&lt;/h2&gt;
        ///Der öffentliche Schlüssel (public key) des RSA-Schlüsselpaares wird genutzt, um die mit dem privaten Schlüssel 
        ///erstellten Signaturen zu validieren. Aus Performance-Gründen wird gewöhnlich ein öffentlicher Schlüssel mit einem geringen
        ///Hamming-Gewicht genutzt (z.B. 3, 17 oder 65537). Voraussetzung für den Bleichenbacher Angriff ist der spezielle Fall, dass der
        ///öffentliche Schlüssel drei ist.
        ///.
        /// </summary>
        internal static string KeyGen_PubExponent {
            get {
                return ResourceManager.GetString("KeyGen_PubExponent", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;h2&gt;PKCS #1-Signaturgenerierung&lt;/h2&gt;
        ///&lt;strong&gt;Um PKCS#1-Signaturen erzeugen zu können, muss zuerst ein RSA-Schlüsselpaar in der entsprechenden Maske
        /// erzeugt werden&lt;/strong&gt;&lt;/br&gt;
        /// &lt;strong&gt;Zuerst muss der Datenblock erzeugt werden, bevor die komplette Signatur generiert werden kann!&lt;/strong&gt;
        /// &lt;/br&gt;&lt;/br&gt;
        ///Die PKCS#1-Signaturen basieren auf dem asymmetrischen Verschlüsselungsalgorithmus RSA. Daher ist es notwendig, einen
        ///RSA-Schlüssel zu erzeugen.&lt;/br&gt;
        ///Um eine PKCS#1-Signatur zu erzeugen, wird zunächst de [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string SigGen {
            get {
                return ResourceManager.GetString("SigGen", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;h2&gt;Bleichenbacher Angriff&lt;/h2&gt;
        ///&lt;strong&gt;Um den Bleichenbacher Angriff durchführen zu können, muss zuerst ein RSA-Schlüsselpaar in der entsprechenden Maske erzeugt werden&lt;/strong&gt;&lt;/br&gt;
        /// &lt;strong&gt;Zuerst muss der Datenblock erzeugt werden, bevor die komplette Signatur generiert werden kann!&lt;/strong&gt;
        /// &lt;/br&gt;&lt;/br&gt;
        /// Um eine gefälschte Signatur zu erzeugen, wird zunächst der &lt;a href=&quot;help://Gen_Datablock_Tab&quot;&gt;Datenblock&lt;/a&gt; wie in
        /// einer regulären PKCS#1-Signatur generiert. Allerdings unterscheidet sich die &lt;a  [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string SigGenFakeBleichenbacher {
            get {
                return ResourceManager.GetString("SigGenFakeBleichenbacher", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;h2&gt;Angriff mit kuerzeren Schlüsseln&lt;/h2&gt;
        ///&lt;strong&gt;Um den Angriff mit kuerzeren Schlüsseln durchführen zu können, muss zuerst ein RSA-Schlüsselpaar in der entsprechenden Maske erzeugt werden&lt;/strong&gt;&lt;/br&gt;
        ///&lt;strong&gt;Zuerst muss der Datenblock erzeugt werden, bevor die komplette Signatur generiert werden kann!&lt;/strong&gt;
        ///&lt;/br&gt;
        ///&lt;/br&gt;
        ///Um eine gefälschte Signatur nach der Kuehn Methode zu erzeugen, wird zunächst der &lt;a href=&quot;help://Gen_Datablock_Tab&quot;&gt;Datenblock&lt;/a&gt; generiert. Dies
        ///ist gleich zu dem Datenblock e [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string SigGenFakeKuehn {
            get {
                return ResourceManager.GetString("SigGenFakeKuehn", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;h2&gt;Signaturvalidierung&lt;/h2&gt;
        ///Bei der Validierung einer PKCS#1-Signatur wird eine Operation mit dem öffentlichen Schlüssel durchgeführt.
        ///Das Ergebnis dieser Operation sollte eine Struktur aufweisen, wie &lt;a href=&quot;help://Gen_PKCS1_Sig_Tab&quot;&gt;hier&lt;/a&gt; beschrieben.
        ///Als nächster Schritt wird der &lt;a href=&quot;help://Gen_Datablock_Tab&quot;&gt;Datenblock&lt;/a&gt; ausgelesen.&lt;/br&gt;
        ///Dieses Extrahieren des Datenblock kann auf eine korrekte oder auf eine fehlerhafte Art und Weise geschehen. Die fehlerhafte
        ///Implementierung war bis zum [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string SigVal {
            get {
                return ResourceManager.GetString("SigVal", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;!DOCTYPE HTML PUBLIC &quot;-//W3C//DTD HTML 4.0 Transitional//EN&quot;&gt;
        ///&lt;html&gt;
        ///	&lt;head&gt;
        ///		&lt;title&gt;&lt;/title&gt;
        ///		&lt;style type=&quot;text/css&quot;&gt;
        ///		  body
        ///		  {
        ///		  	font-family:Arial,Verdana,Georgia;
        ///		  	font-size:smaller;
        ///		  }
        ///		&lt;/style&gt;
        ///	&lt;/head&gt;
        ///	&lt;body&gt;
        ///	&lt;h2&gt;PKCS#1-Signaturen / Bleichenbacher Angriff&lt;/h2&gt;
        ///	&lt;p align=&quot;justify&quot;&gt;
        ///	PKCS#1-Signaturen basieren auf dem RSA-Verschlüsselungsverfahren. Der Angriff von Daniel Bleichenbacher zielt nicht
        ///	auf das Verschlüsselungsverfahren selbst, sondern auf Implementierung [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string Start {
            get {
                return ResourceManager.GetString("Start", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;h2&gt;PKCS#1 / Bleichenbacher Angriff - Hilfe&lt;/h2&gt;
        ///Willkommen in der Hilfe des PKCS#1 / Bleichenbacher Angriff Plugins.&lt;/br&gt;
        ///Hier finden Sie detaillierte Informationen zu PKCS#1-Signaturen und dem Bleichenbacher Angriff.&lt;/br&gt;&lt;/br&gt;
        ///In die verschiedenen Masken dieses Plugins gelangen Sie mit Hilfe der Navigation auf der linken Seite. In den verschiedenen Masken
        ///wiederum finden Sie mehrere Hilfebuttons. Wenn Sie auf diese klicken, bekommen Sie detaillierte Informationen über das jeweilige Thema.
        ///.
        /// </summary>
        internal static string StartControl {
            get {
                return ResourceManager.GetString("StartControl", resourceCulture);
            }
        }
    }
}
