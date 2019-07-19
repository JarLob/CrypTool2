using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DCAPathFinder.UI.Controls;

namespace DCAPathFinder.UI.Tutorial2
{
    /// <summary>
    /// Interaktionslogik für AttackKeyRound3.xaml
    /// </summary>
    [Cryptool.PluginBase.Attributes.Localization("DCAPathFinder.Properties.Resources")]
    public partial class AttackKeyRound3 : UserControl
    {
        public event EventHandler<EventArgs> SelectionChanged;

        /// <summary>
        /// Constructor
        /// </summary>
        public AttackKeyRound3()
        {
            InitializeComponent();

            //round3
            SBox4Round3.SelectionChanged += SBoxSelectionChanged;
            SBox3Round3.SelectionChanged += SBoxSelectionChanged;
            SBox2Round3.SelectionChanged += SBoxSelectionChanged;
            SBox1Round3.SelectionChanged += SBoxSelectionChanged;

            //round2
            SBox4Round2.AlreadyAttacked = false;
            SBox4Round2.IsClickable = false;

            SBox3Round2.AlreadyAttacked = false;
            SBox3Round2.IsClickable = false;

            SBox2Round2.AlreadyAttacked = false;
            SBox2Round2.IsClickable = false;

            SBox1Round2.AlreadyAttacked = false;
            SBox1Round2.IsClickable = false;

            //round1
            SBox4Round1.AlreadyAttacked = false;
            SBox4Round1.IsClickable = false;

            SBox3Round1.AlreadyAttacked = false;
            SBox3Round1.IsClickable = false;

            SBox2Round1.AlreadyAttacked = false;
            SBox2Round1.IsClickable = false;

            SBox1Round1.AlreadyAttacked = false;
            SBox1Round1.IsClickable = false;
        }

        /// <summary>
        /// Listener to handle change of selection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SBoxSelectionChanged(object sender, EventArgs e)
        {
            SelectionChanged.Invoke(sender, e);
        }
    }
}
