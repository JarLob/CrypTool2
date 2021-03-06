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
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;

using System.Collections;

using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using System.IO;

namespace CrypTool.Alphabets
{
    /// <summary>
    /// Interaction logic for AlphabetPresentation.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("CrypTool.Alphabets.Properties.Resources")]
    public partial class AlphabetPresentation : UserControl, INotifyPropertyChanged
    {
        public event EventHandler AlphabetChanged;

        private ObservableCollection<AlphabetItem> alphabets = new ObservableCollection<AlphabetItem>();
        public ObservableCollection<AlphabetItem> Alphabets
        {
            get { return alphabets; }
            private set
            {
                alphabets = value; 
                //OnPropertyChanged("Alphabets"); 
;
            }
        }

        private ObservableCollection<OutputOrder> outputOrder = new ObservableCollection<OutputOrder>();
        public ObservableCollection<OutputOrder> OutputOrder
        {
            get { return outputOrder; }
            private set
            {
                outputOrder = value; //OnPropertyChanged("OutputOrder"); 
                
            }
        }

        private ObservableCollection<BasicAlphabet> basic = new ObservableCollection<BasicAlphabet>();
        public ObservableCollection<BasicAlphabet> Basic { get { return basic; } private set { basic = value; OnPropertyChanged("Basic"); } }

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

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }


        DispatcherTimer timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1) };
        public AlphabetPresentation(AlphabetSettings setting)
        {
            this.setting = setting;
            this.setting.PropertyChanged += new PropertyChangedEventHandler(setting_PropertyChanged);

            this.OutputOrderView = CollectionViewSource.GetDefaultView(OutputOrder) as ListCollectionView;
            OutputOrderView.CustomSort = new IndexSorter<OutputOrder>(OutputOrder, OutputOrderView);

            this.AlphabetCollectionView = CollectionViewSource.GetDefaultView(Alphabets) as ListCollectionView;
            AlphabetCollectionView.CustomSort = new IndexSorter<AlphabetItem>(Alphabets, AlphabetCollectionView);

            timer.Tick += delegate(object o, EventArgs args)
            {
                timer.Stop();
                this.setting.PropertyChanged -= new PropertyChangedEventHandler(setting_PropertyChanged);
                this.SetAlphabetItems(setting.Default.AlphabetData);
                this.SetOutputOrders(setting.Default.OutputOrderData);
            };

            timer.Start();
            InitializeComponent();
        }

        void setting_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Data")
            {
                this.setting.PropertyChanged -= new PropertyChangedEventHandler(setting_PropertyChanged);
                timer.Stop();
                var data = AlphabetSettings.DeserializeData<Data>(setting.Data);
                this.SetAlphabetItems(data.AlphabetData);
                this.SetOutputOrders(data.OutputOrderData);
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
            var tmp2 = new List<OutputOrder>();
            if (Alphabets != null)
            {
                foreach (var item in Alphabets)
                {
                    tmp.Add(item.Data);
                }
            }

            if (OutputOrder != null)
            {
                foreach (var item in OutputOrder)
                {
                    tmp2.Add(item);
                }
            }

            Data data = new Data() { AlphabetData = tmp, OutputOrderData = tmp2 };
            setting.Data = AlphabetSettings.SerializeData<Data>(data);
        }

        void item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            raiseChange();
        }

        public void SetOutputOrders(List<OutputOrder> data)
        {
           // OutputOrder = new ObservableCollection<OutputOrder>();
            foreach (var item in data)
            {
                OutputOrder.Add(item);
            }
            saveToSettings();
        }

        public void SetAlphabetItems(List<AlphabetItemData> data)
        {
            if (Alphabets != null)
            {
                var x = Alphabets.Count - 1;
                for (int i = x; i >= 0; --i)
                {
                    var item = Alphabets[i];
                    item.PropertyChanged -= new PropertyChangedEventHandler(item_PropertyChanged);
                }
            }

            Alphabets.Clear();

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

        internal class IndexSorter<T> : IComparer
        {
            private ListCollectionView view;

            public IndexSorter(ObservableCollection<T> collection, ListCollectionView view)
            {
                this.view = view;
                applyIndex((IList)collection);
                collection.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(ConnectorCollectionItemChanged);
            }

            private void applyIndex(IList collection)
            {
                var tmp = collection.OfType<IIndex>();
                if (tmp.Any(x => x.Index == int.MaxValue))
                {
                    foreach (var item in tmp)
                        item.Index = collection.IndexOf(item);
                }
            }

            public int Compare(object x, object y)
            {
                var connA = x as IIndex;
                var connB = y as IIndex;
                var val = connA.Index.CompareTo(connB.Index);
                return val;
            }

            private void assignIndex(IList collection)
            {
                var tmp = collection.OfType<IIndex>();
                foreach (var connector in tmp)
                {
                    int index = collection.IndexOf(connector);
                    connector.Index = index;
                }
                view.Refresh();
            }

            void ConnectorCollectionItemChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
            {
                assignIndex((IList)sender);
            }
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
            var ele = sender as FrameworkElement;
            if (ele == null)
            {
                return;
            }
            var alphabetItem = (AlphabetItem)ele.DataContext;
            var result = MessageBox.Show(string.Format(Properties.Resources.DeleteAlphabetMessageBoxText, alphabetItem.Title), Properties.Resources.DeleteAlphabetMessageBoxTitle, MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                removeItem(alphabetItem);
            }
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
                return string.Empty;

            var alp = ActiveAlphabet.GetAlphabet();
            var builder = new StringBuilder();

            foreach (var item in OutputOrder.OrderBy(x=> x.Index).Where( x=> x.IsActive))
            {
                try
                {
                    var obj = alp.SingleOrDefault(x => x.OutputOrderType == item.OutputType);
                    builder.Append(obj.Output);
                }
                catch (Exception)
                { }
            }

            return builder.ToString();
        }

        private void UpClick(object sender, RoutedEventArgs e)
        {
            FrameworkElement ele = sender as FrameworkElement;
            if (ele == null)
                return;

            var alp = (AlphabetItem)ele.DataContext;
            var index = Alphabets.IndexOf(alp);
            AlphabetItem tmp = null;
            if (ActiveAlphabet == alp)
                tmp = alp; 
            index++;
            if(index < Alphabets.Count)
            {
                Alphabets.Remove(alp);
                Alphabets.Insert(index, alp);
                if (tmp != null)
                    ActiveAlphabet = alp;
            }

            AlphabetCollectionView.Refresh();
        }

        private void DownClick(object sender, RoutedEventArgs e)
        {
            FrameworkElement ele = sender as FrameworkElement;
            if (ele == null)
                return;

            var alp = (AlphabetItem)ele.DataContext;
            var index = Alphabets.IndexOf(alp);
            AlphabetItem tmp = null;
            if (ActiveAlphabet == alp)
                tmp = alp; 
            index--;
            if (index > -1)
            {
                Alphabets.Remove(alp);
                Alphabets.Insert(index, alp);
                if (tmp != null)
                    ActiveAlphabet = alp;
            }
                

            AlphabetCollectionView.Refresh();
           
        }

        public ListCollectionView AlphabetCollectionView { get; set; }
        private ListCollectionView OutputOrderView;


        private void handleIndex(int index, OutputOrder item)
        {
            if (index > -1)
            {
                OutputOrder.Remove(item);
                OutputOrder.Insert(index, item);
            }
        }

        private void ToggleButton_Drop(object sender, DragEventArgs e)
        {
            var btn = ((ToggleButton)sender);
            var target = (OutputOrder)btn.DataContext;
            var source = (OutputOrder)e.Data.GetData("CrypTool.Alphabets.OutputOrder");

            if (target == source)
            {
                btn.IsChecked = !btn.IsChecked;
                e.Handled = true;
                return;
            }

            var targetIndex = OutputOrder.IndexOf(target);
            var sourceIndex = OutputOrder.IndexOf(source);

            handleIndex(targetIndex, source);
            handleIndex(sourceIndex, target);
                
            OutputOrderView.Refresh();
            raiseChange();
        }

        private void raiseChange()
        {
            saveToSettings();
            if(AlphabetChanged != null)
                this.AlphabetChanged.Invoke(this, null);
        }

        private void ToggleButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var btn = (ToggleButton)sender;
            var source = (OutputOrder)btn.DataContext;
            DragDrop.DoDragDrop((ToggleButton)sender, source, DragDropEffects.Copy);
            raiseChange();
        }


        public event PropertyChangedEventHandler PropertyChanged;
        private bool isLoading = true;
    }

    [Serializable]
    public enum OutputType
    {
        Upper,
        Lower,
        Special,
        Numeric
    }

    [Serializable]
    public class Data
    {
        private List<AlphabetItemData> alphabetData = null;
        public List<AlphabetItemData> AlphabetData
        {
            get
            {
                return alphabetData;
            }
            set
            {
                alphabetData = value;
            }
        }

        private List<OutputOrder> autputOrderData = null;
        public List<OutputOrder> OutputOrderData
        {
            get
            {
                return autputOrderData;
            }
            set
            {
                autputOrderData = value;
            }
        }
    }

    [Serializable]
    public class OutputOrder : IIndex, INotifyPropertyChanged
    {
        public OutputType OutputType;

        private string caption = string.Empty;
        public string Caption
        {
            get
            {
                return caption;
            }
            set
            {
                caption = value;
                OnPropertyChanged("Caption");
            }
        }

        private bool isActive = true;
        public bool IsActive
        {
            get
            {
                return isActive;
            }
            set
            {
                isActive = value;
                OnPropertyChanged("IsActive");
            }
        }

        private int index = int.MaxValue;
        public int Index
        {
            get
            {
                return index;
            }
            set
            {
                index = value;
            }
        }

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
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

        private int index = int.MaxValue;
        public int Index
        {
            get { return index; }
            set { index = value; }
        }

        private bool isSelected = false;
        public bool IsSelected
        {
            get { return isSelected; }
            set { isSelected = value;  }
        }

        private string title = Properties.Resources.NewTitle;
        public string Title
        {
            get { return title; }
            set { title = value;  }
        }

        private CrypTool.Alphabets.AlphabetItem.AlphabetType upper = new AlphabetItem.AlphabetType() { OutputOrderType = OutputType.Upper };
        public CrypTool.Alphabets.AlphabetItem.AlphabetType Upper
        {
            get { return upper; }
            set { upper = value; }
        }

        private CrypTool.Alphabets.AlphabetItem.AlphabetType lower = new AlphabetItem.AlphabetType() { OutputOrderType = OutputType.Lower };
        public CrypTool.Alphabets.AlphabetItem.AlphabetType Lower
        {
            get { return lower; }
            set { lower = value;  }
        }

        private CrypTool.Alphabets.AlphabetItem.AlphabetType numeric = new AlphabetItem.AlphabetType() { Output = "0123456789", OutputOrderType = OutputType.Numeric };
        public CrypTool.Alphabets.AlphabetItem.AlphabetType Numeric
        {
            get { return numeric; }
            set { numeric = value;  }
        }

        private CrypTool.Alphabets.AlphabetItem.AlphabetType special = new AlphabetItem.AlphabetType() { Output = ".,:;!?()-+*/[]{}@_><#~=\"&%$§", OutputOrderType = OutputType.Special } ;
        public CrypTool.Alphabets.AlphabetItem.AlphabetType Special
        {
            get { return special; }
            set { special = value; }
        }
    }

    public interface IIndex
    {
        int Index
        {
            get;
            set;
        }
    }

    public class AlphabetItem : INotifyPropertyChanged, IIndex
    {
        //public static UnicodeUtil Cyrillic = new UnicodeUtil(0x410, 0x44f);
        //public static UnicodeUtil BasicLatin = new UnicodeUtil('a', 'Z');
        //public static UnicodeUtil Greek = new UnicodeUtil(0x370, 0x3ff);

        //public static CharOperation BasicLatin = new CharOperation("ABCDEFGHIJKLMNOPQRSTUVWXYZ", "abcdefghijklmnopqrstuvwxyz");
        //public static CharOperation Cyrillic = new CharOperation("АБВГДЕЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯЁЂЃЄЅІЇЈЉЊЋЌЎЏ", "абвгдежзийклмнопрстуфхцчшщъыьэюяёђѓєѕіїјљњћќўџ");
        //public static CharOperation Greek = new CharOperation("ΑΒΓΔΕΖΗΘΙΚΛΜΝΞΟΠΡΣΣΤΥΦΧΨΩΪΫΌΎΏ", "αβγδεζηθικλμνξοπρςστυφχψωϊϋόύώ");

        public event PropertyChangedEventHandler PropertyChanged;

        public AlphabetItem(BasicAlphabet basic, string title, bool editable)
        {
            this.Data = new AlphabetItemData();
            this.Data.Lower = new AlphabetType() { Output = basic.Small, OutputOrderType = OutputType.Lower };
            this.Data.Upper = new AlphabetType() { Output = basic.Capital, OutputOrderType = OutputType.Upper };
            OnPropertyChanged("Upper");
            OnPropertyChanged("Lower");

            this.Editable = editable;
            this.Title = title;
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

        public CrypTool.Alphabets.AlphabetItem.AlphabetType[] GetAlphabet()
        {
            return new AlphabetType[] { this.Data.Upper, this.Data.Lower, this.Data.Numeric, this.Data.Special };
        }

        public AlphabetItemData data;

        public AlphabetItemData Data
        {
            get { return data; }
            set
            {
                data = value;
                OnPropertyChanged("Editable");
                OnPropertyChanged("IsSelected");
                OnPropertyChanged("Title");
                OnPropertyChanged("Upper");
                OnPropertyChanged("Lower");
                OnPropertyChanged("Numeric");
                OnPropertyChanged("Special");
                OnPropertyChanged("Index");
            }
        }

        [Serializable]
        public class AlphabetType
        {
            private string output = string.Empty;
            public string Output
            {
                get { return output; }
                set { output = value; }
            }
            public OutputType OutputOrderType;
        }

        public bool Editable
        {
            get { return Data.Editable; }
            set { Data.Editable = value; OnPropertyChanged("Editable"); }
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

        public int Index
        {
            get { return Data.Index; }
            set { Data.Index = value; OnPropertyChanged("Index"); }
        }

        public string Upper
        {
            get { return Data.Upper.Output; }
            set { Data.Upper.Output = value; OnPropertyChanged("Upper"); }
        }

        public string Lower
        {
            get { return Data.Lower.Output; }
            set { Data.Lower.Output = value; OnPropertyChanged("Lower"); }
        }


        public string Numeric
        {
            get { return Data.Numeric.Output; }
            set { Data.Numeric.Output = value; OnPropertyChanged("Numeric"); }
        }


        public string Special
        {
            get { return Data.Special.Output; }
            set { Data.Special.Output = value; OnPropertyChanged("Special"); }
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
