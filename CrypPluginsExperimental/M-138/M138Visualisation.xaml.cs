using Cryptool.PluginBase.Miscellaneous;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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

namespace M_138
{
    /// <summary>
    /// Interaktionslogik für M138Visualisation.xaml
    /// </summary>
    public partial class M138Visualisation : UserControl
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private string[,] stripesFilled;
        private int rows;
        private int columns;
        private string[,] toVisualize;
        private int offset;
        private List<List<string>> list = new List<List<string>>();
        DataTable shapedResultsTable = new DataTable();
        private DataTable _stockResultsTable;

        public M138Visualisation()
        {
            InitializeComponent();
            //_dataGrid.ItemsSource = list;
            //_dataGrid.ItemsSource = toVisualize;
        }

        public void setStripes(string[,] s)
        {
            stripesFilled = s;
        }
        public void setOffset(int o)
        {
            offset = o;
        }
        public void fillArray(int r, int c, int[] stripes)
        {
            rows = r;
            columns = c;
            toVisualize = new string[r + 1, c + 2];
            for (int i = 0; i < c ; i++) // Fill first Row
            {
                toVisualize[0, i] = i.ToString();
            }
            for (int i = 0; i < r + 1; i++) // Fill last column
            {
                toVisualize[i, c + 1] = i.ToString();
            }
            // Fill last Row
            for (int i = 1; i < r + 1; i++)
            {
                toVisualize[i, 0] = (stripes[i - 1] + 1).ToString();
            }
            // Fill rest of Array
            for (int i = 1; i < r+1; i++)
            {
                for (int j = 1; j < c+1; j++)
                {
                    toVisualize[i, j] = stripesFilled[i - 1, j - 1];
                }
            }
            toVisualize[0, 0] = "Stripnumber";
            toVisualize[0, c + 1] = "Row";
            printArray(toVisualize, r + 1, c + 2);
            //List
            for (int i = 0; i < c + 2; i++)
            {
                List<string> l = new List<string>();
                for (int j = 0; j < r + 1; j++)
                {
                    l.Add(toVisualize[j, i]);
                }
                list.Add(l);
            }
            //Shaped Results Table
            for (int i = 0; i < c+2; i++)
            {
                shapedResultsTable.Columns.Add(toVisualize[0, i], typeof(string));
            }
            for (int i = 1; i < r + 1; i++)
            {
                List<string> l = new List<string>();
                for(int j=0; j<c+2; j++) {
                    l.Add(toVisualize[i,j]);
                }
                shapedResultsTable.Rows.Add(l);
            }
            StockResultsTable = shapedResultsTable;
        }

        private void printArray(string[,] a, int r, int c)
        {
            for (int i = 0; i < r; i++)
            {
                for (int j = 0; j < c; j++)
                {
                    Console.Write(a[i,j]+"\t");
                }
                Console.Write("\n");
            }
        }

        public DataTable StockResultsTable
        {
            get { return _stockResultsTable; }
            set
            {
                _stockResultsTable = value;
                OnPropertyChanged("StockResultsTable"); // <--- defo' need this one.
            }
        }

        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }
    }
}
