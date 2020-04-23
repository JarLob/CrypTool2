using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Threading;
using System.Windows.Threading;
using System;

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


        public void ShowData(DataSource data, bool sort, int maxNumberOfShownNGrams)
        {
            List<CollectionElement> list = data.ValueCollection.ToList();
            //here, we sort by frequency occurrence if the user wants so
            if (sort)
            {
                list.Sort(delegate(CollectionElement a, CollectionElement b) { return (a.Height > b.Height ? -1 : 1); });
            }

            //here, we remove all low frequencies until we only have maxNumberOfShownNGrams left
            List<CollectionElement> sorted_list = data.ValueCollection.ToList();
            sorted_list.Sort(delegate (CollectionElement a, CollectionElement b) { return (a.Height > b.Height ? -1 : 1); });
            for(int i = maxNumberOfShownNGrams; i < sorted_list.Count; i++)
            {
                list.Remove(sorted_list[i]);
            }                                

            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                try
                {
                    DataSource source = (DataSource)this.Resources["source"];
                    source.ValueCollection.Clear();
                    for (int i = 0; i < list.Count; i++)
                    {
                        source.ValueCollection.Add(list[i]);
                    }
                }
                catch (Exception)
                {
                    //do nothing
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
