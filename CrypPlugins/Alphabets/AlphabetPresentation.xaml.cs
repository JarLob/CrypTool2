/*
   Copyright 2008 Sebastian Przybylski, University of Siegen

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.IO;
using System.Data;
using System.ComponentModel;
using Cryptool.PluginBase;
using System.Collections.ObjectModel;

namespace Cryptool.Alphabets
{
    /// <summary>
    /// Interaction logic for AlphabetPresentation.xaml
    /// </summary>
    [Cryptool.PluginBase.Attributes.Localization("Cryptool.Alphabets.Properties.Resources")]
    public partial class AlphabetPresentation : UserControl
    {
        private ObservableCollection<AlphabetItem> alphabets = new ObservableCollection<AlphabetItem>();
        public ObservableCollection<AlphabetItem> Alphabets { get { return alphabets; } }

        public static readonly DependencyProperty SelectedAlphabetProperty = DependencyProperty.Register("SelectedAlphabet",
            typeof(AlphabetItem), typeof(AlphabetPresentation), new FrameworkPropertyMetadata(null));

        public AlphabetItem SelectedAlphabet
        {
            get { return (AlphabetItem)base.GetValue(SelectedAlphabetProperty); }
            set
            {
                base.SetValue(SelectedAlphabetProperty, value);
            }
        }

        public AlphabetPresentation()
        {
            Alphabets.Add(AlphabetItem.CyrillicAlphabet); 
            Alphabets.Add(AlphabetItem.GreekAlphabet);
            Alphabets.Add(AlphabetItem.BasicLatinAlphabet);
            SelectedAlphabet = AlphabetItem.CyrillicAlphabet; 
            InitializeComponent();
        }

        private void SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AlphabetList.SelectedItem != null)
            {
                SelectedAlphabet = (AlphabetItem)AlphabetList.SelectedItem;
                SelectedAlphabet.IsSelected = true;
            }

            if(e.RemovedItems.Count != 0)
            {
                ((AlphabetItem)e.RemovedItems[0]).IsSelected = false; 
            }

        }
    }

    public class UnicodeUtil
    {
        private int from = 0;
        private int to = 0;

        private int lowerFrom = 0;
        private int lowerTo = 0;

        public string output = string.Empty;
        public string outputLower = string.Empty;

        public string getStringFromUnicode(int from, int to)
        {
            string output = string.Empty;
            byte[] k = new byte[4];
            MemoryStream m = new MemoryStream(k);
            BinaryWriter w = new BinaryWriter(m);
            for (int i = from; i <= to; ++i)
            {
                w.Seek(0, SeekOrigin.Begin);
                w.Write(i);
                output += Encoding.UTF32.GetString(k);
            }
            return output;
        }

        public UnicodeUtil(int from, int to)
        {
            this.from = from;
            this.to = to;

            this.output = getStringFromUnicode(from, to);
        }

        public UnicodeUtil(int from, int to, int lowerFrom, int lowerTo)
        {
            this.from = from;
            this.to = to;

            this.lowerFrom = lowerFrom;
            this.lowerTo = lowerTo;

            this.output = getStringFromUnicode(from, to);
            this.outputLower = getStringFromUnicode(lowerFrom, lowerTo);
        }
    }

    public class BasicAlphabet
    {
        public string Small { get; set; }
        public string Captial { get; set; }
        public string Special { get; set; }

        public BasicAlphabet(string captial, string small)
        {
            this.Captial = captial;
            this.Small = small;
        }

        public BasicAlphabet(string captial, string small, string special)
        {
            this.Captial = captial;
            this.Small = small;
            this.Special = special;
        }
    }

    public class AlphabetItem : INotifyPropertyChanged 
    {
        //public static UnicodeUtil Cyrillic = new UnicodeUtil(0x410, 0x44f);
        //public static UnicodeUtil BasicLatin = new UnicodeUtil('a', 'Z');
        //public static UnicodeUtil Greek = new UnicodeUtil(0x370, 0x3ff);

        //public static CharOperation BasicLatin = new CharOperation("ABCDEFGHIJKLMNOPQRSTUVWXYZ", "abcdefghijklmnopqrstuvwxyz");
        //public static CharOperation Cyrillic = new CharOperation("АБВГДЕЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯЁЂЃЄЅІЇЈЉЊЋЌЎЏ", "абвгдежзийклмнопрстуфхцчшщъыьэюяёђѓєѕіїјљњћќўџ");
        //public static CharOperation Greek = new CharOperation("ΑΒΓΔΕΖΗΘΙΚΛΜΝΞΟΠΡΣΣΤΥΦΧΨΩΪΫΌΎΏ", "αβγδεζηθικλμνξοπρςστυφχψωϊϋόύώ");

        public static BasicAlphabet BasicLatin = new BasicAlphabet("ABCDEFGHIJKLMNOPQRSTUVWXYZ", "abcdefghijklmnopqrstuvwxyz");
        public static BasicAlphabet Cyrillic = new BasicAlphabet("&#x0410;&#x0411;&#x0412;&#x0413;&#x0414;&#x0415;&#x0416;&#x0417;&#x0418;&#x0419;&#x041a;&#x041b;&#x041c;&#x041d;&#x041e;&#x041f;&#x0420;&#x0421;&#x0422;&#x0423;&#x0424;&#x0425;&#x0426;&#x0427;&#x0428;&#x0429;&#x042a;&#x042b;&#x042c;&#x042d;&#x042e;&#x042f;&#x0401;&#x0402;&#x0403;&#x0404;&#x0405;&#x0406;&#x0407;&#x0408;&#x0409;&#x040a;&#x040b;&#x040c;&#x040e;&#x040f;", "&#x0430;&#x0431;&#x0432;&#x0433;&#x0434;&#x0435;&#x0436;&#x0437;&#x0438;&#x0439;&#x043a;&#x043b;&#x043c;&#x043d;&#x043e;&#x043f;&#x0440;&#x0441;&#x0442;&#x0443;&#x0444;&#x0445;&#x0446;&#x0447;&#x0448;&#x0449;&#x044a;&#x044b;&#x044c;&#x044d;&#x044e;&#x044f;&#x0451;&#x0452;&#x0453;&#x0454;&#x0455;&#x0456;&#x0457;&#x0458;&#x0459;&#x045a;&#x045b;&#x045c;&#x045e;&#x045f;");
        public static BasicAlphabet Greek = new BasicAlphabet("&#x0391;&#x0392;&#x0393;&#x0394;&#x0395;&#x0396;&#x0397;&#x0398;&#x0399;&#x039a;&#x039b;&#x039c;&#x039d;&#x039e;&#x039f;&#x03a0;&#x03a1;&#x03a3;&#x03a3;&#x03a4;&#x03a5;&#x03a6;&#x03a7;&#x03a8;&#x03a9;&#x03aa;&#x03ab;&#x038c;&#x038e;&#x038f;", "&#x03b1;&#x03b2;&#x03b3;&#x03b4;&#x03b5;&#x03b6;&#x03b7;&#x03b8;&#x03b9;&#x03ba;&#x03bb;&#x03bc;&#x03bd;&#x03be;&#x03bf;&#x03c0;&#x03c1;&#x03c2;&#x03c3;&#x03c4;&#x03c5;&#x03c6;&#x03c7;&#x03c8;&#x03c9;&#x03ca;&#x03cb;&#x03cc;&#x03cd;&#x03ce;");

        public event PropertyChangedEventHandler PropertyChanged;

        static public AlphabetItem CyrillicAlphabet = null;
        static public AlphabetItem GreekAlphabet = null;
        static public AlphabetItem BasicLatinAlphabet = null;

        static AlphabetItem()
        {
            CyrillicAlphabet = new AlphabetItem(Cyrillic, "Cyrillic", false);
            BasicLatinAlphabet = new AlphabetItem(BasicLatin, "BasicLatin", false);
            GreekAlphabet = new AlphabetItem(Greek, "Greek", false);
        }

        public AlphabetItem()
        {
            
        }

        public AlphabetItem(BasicAlphabet basic)
        {
            this.basic = basic;
            this.text = basic.Captial;
        }

        public AlphabetItem(BasicAlphabet basic, string title, bool editable)
        {
            this.editable = editable;
            this.basic = basic;
            this.text = basic.Captial;
            this.title = title;
        }

        private BasicAlphabet basic { get; set; }
        private bool editable = true;

        private bool isSelected = false;
        public bool IsSelected
        {
            get { return isSelected; }
            set { isSelected = value; OnPropertyChanged("IsSelected"); }
        }

        private string title = string.Empty;
        public string Title
        {
            get { return System.Net.WebUtility.HtmlDecode(title); }
            set { title = value; OnPropertyChanged("Title"); }
        }

        private string text = string.Empty;
        public string Text
        {
            get { return System.Net.WebUtility.HtmlDecode(text); }
            set { text = value; OnPropertyChanged("Text"); }
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
