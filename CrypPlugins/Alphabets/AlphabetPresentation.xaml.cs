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
        public event EventHandler AlphabetChanged;

        private ObservableCollection<AlphabetItem> alphabets = new ObservableCollection<AlphabetItem>();
        public ObservableCollection<AlphabetItem> Alphabets { get { return alphabets; } }

        private ObservableCollection<BasicAlphabet> basic = new ObservableCollection<BasicAlphabet>();
        public ObservableCollection<BasicAlphabet> Basic { get { return basic; } }

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

        public static readonly DependencyProperty IsConfigOpenProperty = DependencyProperty.Register("IsConfigOpen",
            typeof(bool), typeof(AlphabetPresentation), new FrameworkPropertyMetadata(false));

        public bool IsConfigOpen
        {
            get { return (bool)base.GetValue(IsConfigOpenProperty); }
            set
            {
                base.SetValue(IsConfigOpenProperty, value);
            }
        }

        private void addItem(AlphabetItem item)
        {
            item.PropertyChanged += new PropertyChangedEventHandler(item_PropertyChanged);
            Alphabets.Add(item);
            saveToSettings();
        }

        private void removeItem(AlphabetItem item)
        {
            item.PropertyChanged -= new PropertyChangedEventHandler(item_PropertyChanged);
            Alphabets.Remove(item);
            saveToSettings();
        }

        void saveToSettings()
        {
            var tmp = new List<AlphabetItemData>();
            foreach (var item in Alphabets)
            {
                tmp.Add(item.Data);
            }

            setting.Data = AlphabetSettings.Serialize(tmp);
        }

        void item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            saveToSettings();
            if(AlphabetChanged != null)
                this.AlphabetChanged.Invoke(this, null);
        }

        public void SetNewItems(List<AlphabetItemData> data)
        {
            var x = Alphabets.Count - 1;
            for (int i = x; i >= 0; --i)
            {
                var item = Alphabets[i];
                removeItem(item);
            }

            foreach (var item in data)
            {
                var tmp = new AlphabetItem(item);
                addItem(tmp);
                if (tmp.IsSelected == true)
                {
                    ActiveAlphabet = tmp;
                }
            }
        }

        public AlphabetPresentation(AlphabetSettings setting)
        {
            this.setting = setting;

            InitializeComponent();
        }

        public static readonly DependencyProperty ActiveAlphabetProperty = DependencyProperty.Register("ActiveAlphabet",
            typeof(AlphabetItem), typeof(AlphabetPresentation), new FrameworkPropertyMetadata(null, OnActiveAlphabetChanged));
        private AlphabetSettings setting;

        public AlphabetItem ActiveAlphabet
        {
            get { return (AlphabetItem)base.GetValue(ActiveAlphabetProperty); }
            set
            {
                base.SetValue(ActiveAlphabetProperty, value);
            }
        }

        private static void OnActiveAlphabetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AlphabetPresentation b = (AlphabetPresentation)d;
            AlphabetItem newItem = (AlphabetItem)e.NewValue;
            AlphabetItem oldItem = (AlphabetItem)e.OldValue;

            if (newItem != null)
                newItem.IsSelected = true;

            if(oldItem != null)
                oldItem.IsSelected = false;
        }

        private void SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }

        private void DeleteItemClick(object sender, RoutedEventArgs e)
        {
            FrameworkElement ele = sender as FrameworkElement;
            if (ele == null)
                return;

            var alp = (AlphabetItem)ele.DataContext;
            removeItem(alp);
        }

        private void AddItemClick(object sender, RoutedEventArgs e)
        {
            var item = new AlphabetItem();
            addItem(item);
            SelectedAlphabet = item;
        }

        private void EditItemClick(object sender, RoutedEventArgs e)
        {
            SelectedAlphabet = ActiveAlphabet;
        }

        private void ReturnToOverviewClick(object sender, RoutedEventArgs e)
        {
            SelectedAlphabet = null;
        }

        public string GetAlphabet()
        {
            if (ActiveAlphabet == null)
                return "";

            return ActiveAlphabet.GetAlphabet();
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
    [Serializable]
    public class BasicAlphabet
    {
        public static BasicAlphabet BasicLatin;
        public static BasicAlphabet Cyrillic;
        public static BasicAlphabet Greek;

        public string Name { get; set; } 

        public string Small { get; set; }
        public string Capital { get; set; }
        public string Special { get; set; }
        private Dictionary<char, char> toLowerDict = new Dictionary<char, char>();
        private Dictionary<char, char> toUpperDict = new Dictionary<char, char>();

        static BasicAlphabet()
        {
            BasicLatin = new BasicAlphabet("ABCDEFGHIJKLMNOPQRSTUVWXYZ", "abcdefghijklmnopqrstuvwxyz", "Basic Latin");
            Cyrillic = new BasicAlphabet("АБВГДЕЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯЁЂЃЄЅІЇЈЉЊЋЌЎЏ", "абвгдежзийклмнопрстуфхцчшщъыьэюяёђѓєѕіїјљњћќўџ", "Cyrillic");
            Greek = new BasicAlphabet("ΑΒΓΔΕΖΗΘΙΚΛΜΝΞΟΠΡΣΣΤΥΦΧΨΩΪΫΌΎΏ", "αβγδεζηθικλμνξοπρςστυφχψωϊϋόύώ", "Greek");
        }

        public BasicAlphabet(string capital, string small, string name)
        {
            if(capital.Length != small.Length)
                throw new Exception();

            this.Capital = capital;
            this.Small = small;
            this.Name = name;



            var smallArray = small.ToArray();
            var capitalArray = capital.ToArray();

            for (var i = 0; i < capital.Length; ++i)
            {
                char val;
                if (!(toLowerDict.TryGetValue(capitalArray[i], out val)))
                    toLowerDict.Add(capitalArray[i], smallArray[i]);

                if (!(toUpperDict.TryGetValue(capitalArray[i], out val)))
                    toUpperDict.Add(smallArray[i], capitalArray[i]);
            }
        }

        public string ToLower(string text)
        {
            var charArray = text.ToArray();
            for (var i = 0; i < charArray.Length; ++i)
            {
                var c = charArray[i];
                char val;
                if (toLowerDict.TryGetValue(c, out val))
                {
                    charArray[i] = val;
                }
            }
            return new String(charArray);
        }

        public string ToUpper(string text)
        {
            var charArray = text.ToArray();
            for (var i = 0; i < charArray.Length; ++i)
            {
                var c = charArray[i];
                char val;
                if (toUpperDict.TryGetValue(c, out val))
                {
                    charArray[i] = val;
                }
            }
            return new String(charArray);
        }
    }



    [Serializable]
    public class AlphabetItemData
    {

        private bool editable = true;
        public bool Editable
        {
            get { return editable; }
            set { editable = value; }
        }

        private bool useNormal = true;
        public bool UseNormal
        {
            get { return useNormal; }
            set { useNormal = value;  }
        }

        private bool useLower = true;
        public bool UseLower
        {
            get { return useLower; }
            set { useLower = value;  }
        }

        private bool useUpper = true;
        public bool UseUpper
        {
            get { return useUpper; }
            set { useUpper = value; }
        }

        private bool useSpecial = true;
        public bool UseSpecial
        {
            get { return useSpecial; }
            set { useSpecial = value; }
        }

        private bool useNumeric = true;
        public bool UseNumeric
        {
            get { return useNumeric; }
            set { useNumeric = value;  }
        }

        private bool isSelected = false;
        public bool IsSelected
        {
            get { return isSelected; }
            set { isSelected = value;  }
        }

        private string title = "New Title";
        public string Title
        {
            get { return title; }
            set { title = value;  }
        }

        private string upper = null;
        public string Upper
        {
            get { return upper; }
            set { upper = value; }
        }

        private string lower = null;
        public string Lower
        {
            get { return lower; }
            set { lower = value;  }
        }

        private string numeric = "123456789";
        public string Numeric
        {
            get { return numeric; }
            set { numeric = value;  }
        }

        private string special = ".,:;!?()-+*/[]{}@_><#~=\"&%$§";
        public string Special
        {
            get { return special; }
            set { special = value; }
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

        public event PropertyChangedEventHandler PropertyChanged;

        static public AlphabetItem CyrillicAlphabet;
        static public AlphabetItem GreekAlphabet;
        static public AlphabetItem BasicLatinAlphabet;

        HashSet<string> set = new HashSet<string>();

        static AlphabetItem()
        {
            CyrillicAlphabet = new AlphabetItem(BasicAlphabet.Cyrillic, "Cyrillic", true);
            GreekAlphabet = new AlphabetItem(BasicAlphabet.BasicLatin, "Basic Latin", true);
            BasicLatinAlphabet = new AlphabetItem(BasicAlphabet.Greek, "Greek", true);
        }

        public AlphabetItem(AlphabetItem item)
        {
            this.Data = item.Data;
        }

        public AlphabetItem()
        {
            this.Data = new AlphabetItemData();
        }

        public AlphabetItem(AlphabetItemData data)
        {
            this.Data = data;
        }

        public string GetAlphabet()
        {
            string tmp = string.Empty;
            if (UseNormal)
            {
                if (UseLower)
                    tmp += Lower;

                if (UseUpper)
                    tmp += Upper;
            }

            if (UseNumeric)
                tmp += Numeric;

            if (UseSpecial)
                tmp += Special;

            return tmp;
        }

        public AlphabetItem(BasicAlphabet basic, string title, bool editable)
        {
            this.Data = new AlphabetItemData();
            this.Editable = editable;
            this.Lower = basic.Small;
            this.Upper = basic.Capital;
            this.Title = title;
        }

        public AlphabetItemData data;

        public AlphabetItemData Data
        {
            get { return data; }
            set 
            { 
                data = value; 
                OnPropertyChanged("Editable");
                OnPropertyChanged("UseNormal");
                OnPropertyChanged("UseLower");
                OnPropertyChanged("UseUpper");
                OnPropertyChanged("UseSpecial");
                OnPropertyChanged("UseNumeric");
                OnPropertyChanged("IsSelected");
                OnPropertyChanged("Title");
                OnPropertyChanged("Upper");
                OnPropertyChanged("Lower");
                OnPropertyChanged("Numeric");
                OnPropertyChanged("Special");
            }
        }


        public bool Editable
        {
            get { return Data.Editable; }
            set { Data.Editable = value; OnPropertyChanged("Editable"); }
        }


        public bool UseNormal
        {
            get { return Data.UseNormal; }
            set { Data.UseNormal = value; OnPropertyChanged("UseNormal"); }
        }


        public bool UseLower
        {
            get { return Data.UseLower; }
            set { Data.UseLower = value; OnPropertyChanged("UseLower"); }
        }


        public bool UseUpper
        {
            get { return Data.UseUpper; }
            set { Data.UseUpper = value; OnPropertyChanged("UseUpper"); }
        }


        public bool UseSpecial
        {
            get { return Data.UseSpecial; }
            set { Data.UseSpecial = value; OnPropertyChanged("UseSpecial"); }
        }


        public bool UseNumeric
        {
            get { return Data.UseNumeric; }
            set { Data.UseNumeric = value; OnPropertyChanged("UseNumeric"); }
        }


        public bool IsSelected
        {
            get { return Data.IsSelected; }
            set { Data.IsSelected = value; OnPropertyChanged("IsSelected"); }
        }


        public string Title
        {
            get { return Data.Title; }
            set { Data.Title = value; OnPropertyChanged("Title"); }
        }


        public string Upper
        {
            get { return Data.Upper; }
            set { Data.Upper = value; OnPropertyChanged("Upper"); }
        }


        public string Lower
        {
            get { return Data.Lower; }
            set { Data.Lower = value; OnPropertyChanged("Lower"); }
        }


        public string Numeric
        {
            get { return Data.Numeric; }
            set { Data.Numeric = value; OnPropertyChanged("Numeric"); }
        }


        public string Special
        {
            get { return Data.Special; }
            set { Data.Special = value; OnPropertyChanged("Special"); }
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
