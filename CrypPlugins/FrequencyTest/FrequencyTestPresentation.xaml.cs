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
using System.Threading;
using System.Windows.Automation.Provider;
using System.Windows.Automation.Peers;
using Cryptool.FrequencyTest;
using System.Windows.Threading;
namespace Cryptool.FrequencyTest
{
    /// <summary>
    /// Interaction logic for FrequencyTestPresentation.xaml
    /// </summary>
    [Cryptool.PluginBase.Attributes.Localization("Cryptool.FrequencyTest.Properties.Resources")]
    public partial class FrequencyTestPresentation : UserControl
    {

        public  FrequencyTestPresentation()
        {
           InitializeComponent();
        }


        public void ShowData(DataSource data, bool sort)
        {
            List<CollectionElement> list = data.ValueCollection.ToList();
            //here, we sort by frequency occurrence if the user wants so
            if (sort)
            {
                list.Sort(delegate(CollectionElement a, CollectionElement b) { return (a.Amount > b.Amount ? -1 : 1); });
            }

            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                DataSource source = (DataSource)this.Resources["source"];
                source.ValueCollection.Clear();
                for (int i = 0; i < list.Count; i++)
                {
                    source.ValueCollection.Add(list[i]);
                }                
            }, null);
        }


        public void SetHeadline(string text)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                chartHeadline.Text = text;
            }, null);
        }

        public void SetScaler(double value)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                chart.LayoutTransform = new ScaleTransform(value, value);
            }, null);
        }

        public void SetBackground(Brush brush)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                chart.Background = brush;
            }, null);
        }

    }
}
