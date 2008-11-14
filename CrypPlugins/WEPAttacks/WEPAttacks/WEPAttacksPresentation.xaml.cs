﻿using System;
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
using System.Collections.ObjectModel;
using System.Threading;
using System.Globalization;

namespace Cryptool.WEPAttacks
{
    /// <summary>
    /// Interaktionslogik für WEPAttacksPresentation.xaml
    /// </summary>
    public partial class WEPAttacksPresentation : UserControl
    {
        public WEPAttacksPresentation()
        {
            InitializeComponent();
            this.Height = double.NaN;
            this.Width = double.NaN;
        }

        #region Public methods for text settings
        /// <summary>
        /// Sets text within label "attack". Indicates which attack is currently running.
        /// </summary>
        /// <param name="attack">The number for the attack. 0 = no attack, 1 = FMS, 2 = KoreK, 3 = PTW.</param>
        public void setKindOfAttack(int attackNumber)
        {
            switch (attackNumber)
            {
                case 0:
                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                        (SendOrPostCallback)delegate
                        {
                            labelAttack.Content = "No attack running.";
                        }, attackNumber);
                    break;
                case 1:
                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                        (SendOrPostCallback)delegate
                    {
                        labelAttack.Content = "FMS attack running...";
                    }, attackNumber);
                    break;
                case 2:
                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                        (SendOrPostCallback)delegate
                    {
                        labelAttack.Content = "KoreK attack running...";
                    }, attackNumber);
                    break;
                case 3:
                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                        (SendOrPostCallback)delegate
                    {
                        labelAttack.Content = "PTW attack running...";
                    }, attackNumber);
                    break;
                default:
                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                        (SendOrPostCallback)delegate
                    {
                        labelAttack.Content = "No attack running.";
                    }, attackNumber);
                    break;
            }
        }

        /// <summary>
        /// Sets the text within label "collectedPackets". Indicates how many packets have been sniffed up to now.
        /// </summary>
        /// <param name="counter">The number of sniffed packets.</param>
        public void setNumberOfSniffedPackages(int counter)
        {
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                (SendOrPostCallback)delegate
                {
                    labelCollectedPackets.Content = "Sniffed packets: " + counter.ToString("#,#", CultureInfo.InstalledUICulture);
                }, counter);
        }

        /// <summary>
        /// Sets the text within label "usedIVs". Indicates how many packets has been used for crypto analysis.
        /// </summary>
        /// <param name="usedIVs">The up to now used packets.</param>
        public void setUsedIVs(int usedIVs)
        {
            if (usedIVs == int.MaxValue)
            {
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                    (SendOrPostCallback)delegate
                    {
                        labelUsedIVs.Content = "Used packets: ";
                    }, usedIVs);
            }
            else
            {
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                    (SendOrPostCallback)delegate
                    {
                        labelUsedIVs.Content = "Used packets: " + usedIVs.ToString("#,#", CultureInfo.InstalledUICulture);
                    }, usedIVs);
            }
        }

        /// <summary>
        /// Clears the text box.
        /// </summary>
        public void resetTextBox(String text)
        {
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                (SendOrPostCallback)delegate
                {
                    textBox.Text = text;
                }, text);
        }

        /// <summary>
        /// Sets the text within the text box. Key, key ranking and so on.
        /// </summary>
        /// <param name="votes">The votes table.</param>
        /// <param name="success">Indicates if the attack was successful.</param>
        /// <param name="keySize">Indicates the key size (40 bit or 104 bit).</param>
        /// <param name="counter">Indicates number of used packets.</param>
        /// <param name="duration">The total timespan for the attack.</param>
        /// <param name="inputMode">"file" or "plugin".</param>
        /// <param name="kindOfAttack">"FMS" or "KoreK".</param>
        /// <param name="stop">Stop button from the outer world.</param>
        public void setTextBox(int[,] votes, bool success, int keySize, int counter, TimeSpan duration, string inputMode, string kindOfAttack, bool stop)
        {
            int firstKeyByteMaxVoted = indexOfMaxVoted(votes, 0);
            int firstKeyByteMaxVotedVotes = votes[0, firstKeyByteMaxVoted];
            int firstKeyByteSecondMostVoted = indexOfSpecificVoted(votes, 0, firstKeyByteMaxVotedVotes, firstKeyByteMaxVoted);
            int firstKeyByteSecondMostVotedVotes = votes[0, firstKeyByteSecondMostVoted];
            int firstKeyByteThirdMostVoted = indexOfSpecificVoted(votes, 0, firstKeyByteSecondMostVotedVotes, firstKeyByteSecondMostVoted);
            int firstKeyByteThirdMostVotedVotes = votes[0, firstKeyByteThirdMostVoted];

            int secondKeyByteMaxVoted = indexOfMaxVoted(votes, 1);
            int secondKeyByteMaxVotedVotes = votes[1, secondKeyByteMaxVoted];
            int secondKeyByteSecondMostVoted = indexOfSpecificVoted(votes, 1, secondKeyByteMaxVotedVotes, secondKeyByteMaxVoted);
            int secondKeyByteSecondMostVotedVotes = votes[1, secondKeyByteSecondMostVoted];
            int secondKeyByteThirdMostVoted = indexOfSpecificVoted(votes, 1, secondKeyByteSecondMostVotedVotes, secondKeyByteSecondMostVoted);
            int secondKeyByteThirdMostVotedVotes = votes[1, secondKeyByteThirdMostVoted];

            int thirdKeyByteMaxVoted = indexOfMaxVoted(votes, 2);
            int thirdKeyByteMaxVotedVotes = votes[2, thirdKeyByteMaxVoted];
            int thirdKeyByteSecondMostVoted = indexOfSpecificVoted(votes, 2, thirdKeyByteMaxVotedVotes, thirdKeyByteMaxVoted);
            int thirdKeyByteSecondMostVotedVotes = votes[2, thirdKeyByteSecondMostVoted];
            int thirdKeyByteThirdMostVoted = indexOfSpecificVoted(votes, 2, thirdKeyByteSecondMostVotedVotes, thirdKeyByteSecondMostVoted);
            int thirdKeyByteThirdMostVotedVotes = votes[2, thirdKeyByteThirdMostVoted];

            int fourthKeyByteMaxVoted = indexOfMaxVoted(votes, 3);
            int fourthKeyByteMaxVotedVotes = votes[3, fourthKeyByteMaxVoted];
            int fourthKeyByteSecondMostVoted = indexOfSpecificVoted(votes, 3, fourthKeyByteMaxVotedVotes, fourthKeyByteMaxVoted);
            int fourthKeyByteSecondMostVotedVotes = votes[3, fourthKeyByteSecondMostVoted];
            int fourthKeyByteThirdMostVoted = indexOfSpecificVoted(votes, 3, fourthKeyByteSecondMostVotedVotes, fourthKeyByteSecondMostVoted);
            int fourthKeyByteThirdMostVotedVotes = votes[3, fourthKeyByteThirdMostVoted];

            int fifthKeyByteMaxVoted = indexOfMaxVoted(votes, 4);
            int fifthKeyByteMaxVotedVotes = votes[4, fifthKeyByteMaxVoted];
            int fifthKeyByteSecondMostVoted = indexOfSpecificVoted(votes, 4, fifthKeyByteMaxVotedVotes, fifthKeyByteMaxVoted);
            int fifthKeyByteSecondMostVotedVotes = votes[4, fifthKeyByteSecondMostVoted];
            int fifthKeyByteThirdMostVoted = indexOfSpecificVoted(votes, 4, fifthKeyByteSecondMostVotedVotes, fifthKeyByteSecondMostVoted);
            int fifthKeyByteThirdMostVotedVotes = votes[4, fifthKeyByteThirdMostVoted];

            int sixthKeyByteMaxVoted = indexOfMaxVoted(votes, 5);
            int sixthKeyByteMaxVotedVotes = votes[5, sixthKeyByteMaxVoted];
            int sixthKeyByteSecondMostVoted = indexOfSpecificVoted(votes, 5, sixthKeyByteMaxVotedVotes, sixthKeyByteMaxVoted);
            int sixthKeyByteSecondMostVotedVotes = votes[5, sixthKeyByteSecondMostVoted];
            int sixthKeyByteThirdMostVoted = indexOfSpecificVoted(votes, 5, sixthKeyByteSecondMostVotedVotes, sixthKeyByteSecondMostVoted);
            int sixthKeyByteThirdMostVotedVotes = votes[5, sixthKeyByteThirdMostVoted];

            int seventhKeyByteMaxVoted = indexOfMaxVoted(votes, 6);
            int seventhKeyByteMaxVotedVotes = votes[6, seventhKeyByteMaxVoted];
            int seventhKeyByteSecondMostVoted = indexOfSpecificVoted(votes, 6, seventhKeyByteMaxVotedVotes, seventhKeyByteMaxVoted);
            int seventhKeyByteSecondMostVotedVotes = votes[6, seventhKeyByteSecondMostVoted];
            int seventhKeyByteThirdMostVoted = indexOfSpecificVoted(votes, 6, seventhKeyByteSecondMostVotedVotes, seventhKeyByteSecondMostVoted);
            int seventhKeyByteThirdMostVotedVotes = votes[6, seventhKeyByteThirdMostVoted];

            int eighthKeyByteMaxVoted = indexOfMaxVoted(votes, 7);
            int eighthKeyByteMaxVotedVotes = votes[7, eighthKeyByteMaxVoted];
            int eighthKeyByteSecondMostVoted = indexOfSpecificVoted(votes, 7, eighthKeyByteMaxVotedVotes, eighthKeyByteMaxVoted);
            int eighthKeyByteSecondMostVotedVotes = votes[7, eighthKeyByteSecondMostVoted];
            int eighthKeyByteThirdMostVoted = indexOfSpecificVoted(votes, 7, eighthKeyByteSecondMostVotedVotes, eighthKeyByteSecondMostVoted);
            int eighthKeyByteThirdMostVotedVotes = votes[7, eighthKeyByteThirdMostVoted];

            int ninthKeyByteMaxVoted = indexOfMaxVoted(votes, 8);
            int ninthKeyByteMaxVotedVotes = votes[8, ninthKeyByteMaxVoted];
            int ninthKeyByteSecondMostVoted = indexOfSpecificVoted(votes, 8, ninthKeyByteMaxVotedVotes, ninthKeyByteMaxVoted);
            int ninthKeyByteSecondMostVotedVotes = votes[8, ninthKeyByteSecondMostVoted];
            int ninthKeyByteThirdMostVoted = indexOfSpecificVoted(votes, 8, ninthKeyByteSecondMostVotedVotes, ninthKeyByteSecondMostVoted);
            int ninthKeyByteThirdMostVotedVotes = votes[8, ninthKeyByteThirdMostVoted];

            int tenthKeyByteMaxVoted = indexOfMaxVoted(votes, 9);
            int tenthKeyByteMaxVotedVotes = votes[9, tenthKeyByteMaxVoted];
            int tenthKeyByteSecondMostVoted = indexOfSpecificVoted(votes, 9, tenthKeyByteMaxVotedVotes, tenthKeyByteMaxVoted);
            int tenthKeyByteSecondMostVotedVotes = votes[9, tenthKeyByteSecondMostVoted];
            int tenthKeyByteThirdMostVoted = indexOfSpecificVoted(votes, 9, tenthKeyByteSecondMostVotedVotes, tenthKeyByteSecondMostVoted);
            int tenthKeyByteThirdMostVotedVotes = votes[9, tenthKeyByteThirdMostVoted];

            int eleventhKeyByteMaxVoted = indexOfMaxVoted(votes, 10);
            int eleventhKeyByteMaxVotedVotes = votes[10, eleventhKeyByteMaxVoted];
            int eleventhKeyByteSecondMostVoted = indexOfSpecificVoted(votes, 10, eleventhKeyByteMaxVotedVotes, eleventhKeyByteMaxVoted);
            int eleventhKeyByteSecondMostVotedVotes = votes[10, eleventhKeyByteSecondMostVoted];
            int eleventhKeyByteThirdMostVoted = indexOfSpecificVoted(votes, 10, eleventhKeyByteSecondMostVotedVotes, eleventhKeyByteSecondMostVoted);
            int eleventhKeyByteThirdMostVotedVotes = votes[10, eleventhKeyByteThirdMostVoted];

            int twelfthKeyByteMaxVoted = indexOfMaxVoted(votes, 11);
            int twelfthKeyByteMaxVotedVotes = votes[11, twelfthKeyByteMaxVoted];
            int twelfthKeyByteSecondMostVoted = indexOfSpecificVoted(votes, 11, twelfthKeyByteMaxVotedVotes, twelfthKeyByteMaxVoted);
            int twelfthKeyByteSecondMostVotedVotes = votes[11, twelfthKeyByteSecondMostVoted];
            int twelfthKeyByteThirdMostVoted = indexOfSpecificVoted(votes, 11, twelfthKeyByteSecondMostVotedVotes, twelfthKeyByteSecondMostVoted);
            int twelfthKeyByteThirdMostVotedVotes = votes[11, twelfthKeyByteThirdMostVoted];

            int thirteenthKeyByteMaxVoted = indexOfMaxVoted(votes, 12);
            int thirteenthKeyByteMaxVotedVotes = votes[12, thirteenthKeyByteMaxVoted];
            int thirteenthKeyByteSecondMostVoted = indexOfSpecificVoted(votes, 12, thirteenthKeyByteMaxVotedVotes, thirteenthKeyByteMaxVoted);
            int thirteenthKeyByteSecondMostVotedVotes = votes[12, thirteenthKeyByteSecondMostVoted];
            int thirteenthKeyByteThirdMostVoted = indexOfSpecificVoted(votes, 12, thirteenthKeyByteSecondMostVotedVotes, thirteenthKeyByteSecondMostVoted);
            int thirteenthKeyByteThirdMostVotedVotes = votes[12, thirteenthKeyByteThirdMostVoted];

            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                (SendOrPostCallback)delegate
                {
                    textBox.Text = 
                        "KB\tbyte(vote)\n"
                        +"00\t"+String.Format("{0:X2}", firstKeyByteMaxVoted)+"("+String.Format("{0:D4}", firstKeyByteMaxVotedVotes)+") "
                        + String.Format("{0:X2}", firstKeyByteSecondMostVoted)+"("+String.Format("{0:D4}", firstKeyByteSecondMostVotedVotes)+") "
                        + String.Format("{0:X2}", firstKeyByteThirdMostVoted)+"("+String.Format("{0:D4}", firstKeyByteThirdMostVotedVotes)+")\n"

                        + "01\t" + String.Format("{0:X2}", secondKeyByteMaxVoted) + "(" + String.Format("{0:D4}", secondKeyByteMaxVotedVotes) + ") "
                        + String.Format("{0:X2}", secondKeyByteSecondMostVoted) + "(" + String.Format("{0:D4}", secondKeyByteSecondMostVotedVotes) + ") "
                        + String.Format("{0:X2}", secondKeyByteThirdMostVoted) + "(" + String.Format("{0:D4}", secondKeyByteThirdMostVotedVotes) + ")\n"

                        + "02\t" + String.Format("{0:X2}", thirdKeyByteMaxVoted) + "(" + String.Format("{0:D4}", thirdKeyByteMaxVotedVotes) + ") "
                        + String.Format("{0:X2}", thirdKeyByteSecondMostVoted) + "(" + String.Format("{0:D4}", thirdKeyByteSecondMostVotedVotes) + ") "
                        + String.Format("{0:X2}", thirdKeyByteThirdMostVoted) + "(" + String.Format("{0:D4}", thirdKeyByteThirdMostVotedVotes) + ")\n"

                        +"03\t" + String.Format("{0:X2}", fourthKeyByteMaxVoted) + "(" + String.Format("{0:D4}", fourthKeyByteMaxVotedVotes) + ") "
                        + String.Format("{0:X2}", fourthKeyByteSecondMostVoted) + "(" + String.Format("{0:D4}", fourthKeyByteSecondMostVotedVotes) + ") "
                        + String.Format("{0:X2}", fourthKeyByteThirdMostVoted) + "(" + String.Format("{0:D4}", fourthKeyByteThirdMostVotedVotes) + ")\n"

                        + "04\t" + String.Format("{0:X2}", fifthKeyByteMaxVoted) + "(" + String.Format("{0:D4}", fifthKeyByteMaxVotedVotes) + ") "
                        + String.Format("{0:X2}", fifthKeyByteSecondMostVoted) + "(" + String.Format("{0:D4}", fifthKeyByteSecondMostVotedVotes) + ") "
                        + String.Format("{0:X2}", fifthKeyByteThirdMostVoted) + "(" + String.Format("{0:D4}", fifthKeyByteThirdMostVotedVotes) + ")\n"

                        + "05\t" + String.Format("{0:X2}", sixthKeyByteMaxVoted) + "(" + String.Format("{0:D4}", sixthKeyByteMaxVotedVotes) + ") "
                        + String.Format("{0:X2}", sixthKeyByteSecondMostVoted) + "(" + String.Format("{0:D4}", sixthKeyByteSecondMostVotedVotes) + ") "
                        + String.Format("{0:X2}", sixthKeyByteThirdMostVoted) + "(" + String.Format("{0:D4}", sixthKeyByteThirdMostVotedVotes) + ")\n"

                        + "06\t" + String.Format("{0:X2}", seventhKeyByteMaxVoted) + "(" + String.Format("{0:D4}", seventhKeyByteMaxVotedVotes) + ") "
                        + String.Format("{0:X2}", seventhKeyByteSecondMostVoted) + "(" + String.Format("{0:D4}", seventhKeyByteSecondMostVotedVotes) + ") "
                        + String.Format("{0:X2}", seventhKeyByteThirdMostVoted) + "(" + String.Format("{0:D4}", seventhKeyByteThirdMostVotedVotes) + ")\n"

                        + "07\t" + String.Format("{0:X2}", eighthKeyByteMaxVoted) + "(" + String.Format("{0:D4}", eighthKeyByteMaxVotedVotes) + ") "
                        + String.Format("{0:X2}", eighthKeyByteSecondMostVoted) + "(" + String.Format("{0:D4}", eighthKeyByteSecondMostVotedVotes) + ") "
                        + String.Format("{0:X2}", eighthKeyByteThirdMostVoted) + "(" + String.Format("{0:D4}", eighthKeyByteThirdMostVotedVotes) + ")\n"

                        + "08\t" + String.Format("{0:X2}", ninthKeyByteMaxVoted) + "(" + String.Format("{0:D4}", ninthKeyByteMaxVotedVotes) + ") "
                        + String.Format("{0:X2}", ninthKeyByteSecondMostVoted) + "(" + String.Format("{0:D4}", ninthKeyByteSecondMostVotedVotes) + ") "
                        + String.Format("{0:X2}", ninthKeyByteThirdMostVoted) + "(" + String.Format("{0:D4}", ninthKeyByteThirdMostVotedVotes) + ")\n"

                        + "09\t" + String.Format("{0:X2}", tenthKeyByteMaxVoted) + "(" + String.Format("{0:D4}", tenthKeyByteMaxVotedVotes) + ") "
                        + String.Format("{0:X2}", tenthKeyByteSecondMostVoted) + "(" + String.Format("{0:D4}", tenthKeyByteSecondMostVotedVotes) + ") "
                        + String.Format("{0:X2}", tenthKeyByteThirdMostVoted) + "(" + String.Format("{0:D4}", tenthKeyByteThirdMostVotedVotes) + ")\n"

                        + "10\t" + String.Format("{0:X2}", eleventhKeyByteMaxVoted) + "(" + String.Format("{0:D4}", eleventhKeyByteMaxVotedVotes) + ") "
                        + String.Format("{0:X2}", eleventhKeyByteSecondMostVoted) + "(" + String.Format("{0:D4}", eleventhKeyByteSecondMostVotedVotes) + ") "
                        + String.Format("{0:X2}", eleventhKeyByteThirdMostVoted) + "(" + String.Format("{0:D4}", eleventhKeyByteThirdMostVotedVotes) + ")\n"

                        + "11\t" + String.Format("{0:X2}", twelfthKeyByteMaxVoted) + "(" + String.Format("{0:D4}", twelfthKeyByteMaxVotedVotes) + ") "
                        + String.Format("{0:X2}", twelfthKeyByteSecondMostVoted) + "(" + String.Format("{0:D4}", twelfthKeyByteSecondMostVotedVotes) + ") "
                        + String.Format("{0:X2}", twelfthKeyByteThirdMostVoted) + "(" + String.Format("{0:D4}", twelfthKeyByteThirdMostVotedVotes) + ")\n"

                        + "12\t" + String.Format("{0:X2}", thirteenthKeyByteMaxVoted) + "(" + String.Format("{0:D4}", thirteenthKeyByteMaxVotedVotes) + ") "
                        + String.Format("{0:X2}", thirteenthKeyByteSecondMostVoted) + "(" + String.Format("{0:D4}", thirteenthKeyByteSecondMostVotedVotes) + ") "
                        + String.Format("{0:X2}", thirteenthKeyByteThirdMostVoted) + "(" + String.Format("{0:D4}", thirteenthKeyByteThirdMostVotedVotes) + ")\n"

                        ;
                }, votes);
            if (success && (keySize == 40))
            {
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                (SendOrPostCallback)delegate
                {
                    textBox.Text =
                        "KB\tbyte(vote)\n"
                        + "00\t" + String.Format("{0:X2}", firstKeyByteMaxVoted) + "(" + String.Format("{0:D4}", firstKeyByteMaxVotedVotes) + ") "
                        + String.Format("{0:X2}", firstKeyByteSecondMostVoted) + "(" + String.Format("{0:D4}", firstKeyByteSecondMostVotedVotes) + ") "
                        + String.Format("{0:X2}", firstKeyByteThirdMostVoted) + "(" + String.Format("{0:D4}", firstKeyByteThirdMostVotedVotes) + ")\n"

                        + "01\t" + String.Format("{0:X2}", secondKeyByteMaxVoted) + "(" + String.Format("{0:D4}", secondKeyByteMaxVotedVotes) + ") "
                        + String.Format("{0:X2}", secondKeyByteSecondMostVoted) + "(" + String.Format("{0:D4}", secondKeyByteSecondMostVotedVotes) + ") "
                        + String.Format("{0:X2}", secondKeyByteThirdMostVoted) + "(" + String.Format("{0:D4}", secondKeyByteThirdMostVotedVotes) + ")\n"

                        + "02\t" + String.Format("{0:X2}", thirdKeyByteMaxVoted) + "(" + String.Format("{0:D4}", thirdKeyByteMaxVotedVotes) + ") "
                        + String.Format("{0:X2}", thirdKeyByteSecondMostVoted) + "(" + String.Format("{0:D4}", thirdKeyByteSecondMostVotedVotes) + ") "
                        + String.Format("{0:X2}", thirdKeyByteThirdMostVoted) + "(" + String.Format("{0:D4}", thirdKeyByteThirdMostVotedVotes) + ")\n"

                        + "03\t" + String.Format("{0:X2}", fourthKeyByteMaxVoted) + "(" + String.Format("{0:D4}", fourthKeyByteMaxVotedVotes) + ") "
                        + String.Format("{0:X2}", fourthKeyByteSecondMostVoted) + "(" + String.Format("{0:D4}", fourthKeyByteSecondMostVotedVotes) + ") "
                        + String.Format("{0:X2}", fourthKeyByteThirdMostVoted) + "(" + String.Format("{0:D4}", fourthKeyByteThirdMostVotedVotes) + ")\n"

                        + "04\t" + String.Format("{0:X2}", fifthKeyByteMaxVoted) + "(" + String.Format("{0:D4}", fifthKeyByteMaxVotedVotes) + ") "
                        + String.Format("{0:X2}", fifthKeyByteSecondMostVoted) + "(" + String.Format("{0:D4}", fifthKeyByteSecondMostVotedVotes) + ") "
                        + String.Format("{0:X2}", fifthKeyByteThirdMostVoted) + "(" + String.Format("{0:D4}", fifthKeyByteThirdMostVotedVotes) + ")\n"

                        + "\n\n"

                        + "Possible key found after using " + counter.ToString("#,#", CultureInfo.InstalledUICulture) + " packets!\n["
                        + String.Format("{0:X2}", firstKeyByteMaxVoted) + ":"
                        + String.Format("{0:X2}", secondKeyByteMaxVoted) + ":"
                        + String.Format("{0:X2}", thirdKeyByteMaxVoted) + ":"
                        + String.Format("{0:X2}", fourthKeyByteMaxVoted) + ":"
                        + String.Format("{0:X2}", fifthKeyByteMaxVoted) + "]"

                        + "\n(ASCII: "
                        + (char)firstKeyByteMaxVoted
                        + (char)secondKeyByteMaxVoted
                        + (char)thirdKeyByteMaxVoted
                        + (char)fourthKeyByteMaxVoted
                        + (char)fifthKeyByteMaxVoted
                        + ")\n\n"


                        + "Time used [h:min:sec]: " + duration + "."
                        ;
                }, votes);
            }

            if (success && (keySize == 104))
            {
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                (SendOrPostCallback)delegate
                {
                    textBox.Text =
                        "KB\tbyte(vote)\n"
                        + "00\t" + String.Format("{0:X2}", firstKeyByteMaxVoted) + "(" + String.Format("{0:D4}", firstKeyByteMaxVotedVotes) + ") "
                        + String.Format("{0:X2}", firstKeyByteSecondMostVoted) + "(" + String.Format("{0:D4}", firstKeyByteSecondMostVotedVotes) + ") "
                        + String.Format("{0:X2}", firstKeyByteThirdMostVoted) + "(" + String.Format("{0:D4}", firstKeyByteThirdMostVotedVotes) + ")\n"

                        + "01\t" + String.Format("{0:X2}", secondKeyByteMaxVoted) + "(" + String.Format("{0:D4}", secondKeyByteMaxVotedVotes) + ") "
                        + String.Format("{0:X2}", secondKeyByteSecondMostVoted) + "(" + String.Format("{0:D4}", secondKeyByteSecondMostVotedVotes) + ") "
                        + String.Format("{0:X2}", secondKeyByteThirdMostVoted) + "(" + String.Format("{0:D4}", secondKeyByteThirdMostVotedVotes) + ")\n"

                        + "02\t" + String.Format("{0:X2}", thirdKeyByteMaxVoted) + "(" + String.Format("{0:D4}", thirdKeyByteMaxVotedVotes) + ") "
                        + String.Format("{0:X2}", thirdKeyByteSecondMostVoted) + "(" + String.Format("{0:D4}", thirdKeyByteSecondMostVotedVotes) + ") "
                        + String.Format("{0:X2}", thirdKeyByteThirdMostVoted) + "(" + String.Format("{0:D4}", thirdKeyByteThirdMostVotedVotes) + ")\n"

                        + "03\t" + String.Format("{0:X2}", fourthKeyByteMaxVoted) + "(" + String.Format("{0:D4}", fourthKeyByteMaxVotedVotes) + ") "
                        + String.Format("{0:X2}", fourthKeyByteSecondMostVoted) + "(" + String.Format("{0:D4}", fourthKeyByteSecondMostVotedVotes) + ") "
                        + String.Format("{0:X2}", fourthKeyByteThirdMostVoted) + "(" + String.Format("{0:D4}", fourthKeyByteThirdMostVotedVotes) + ")\n"

                        + "04\t" + String.Format("{0:X2}", fifthKeyByteMaxVoted) + "(" + String.Format("{0:D4}", fifthKeyByteMaxVotedVotes) + ") "
                        + String.Format("{0:X2}", fifthKeyByteSecondMostVoted) + "(" + String.Format("{0:D4}", fifthKeyByteSecondMostVotedVotes) + ") "
                        + String.Format("{0:X2}", fifthKeyByteThirdMostVoted) + "(" + String.Format("{0:D4}", fifthKeyByteThirdMostVotedVotes) + ")\n"

                        + "05\t" + String.Format("{0:X2}", sixthKeyByteMaxVoted) + "(" + String.Format("{0:D4}", sixthKeyByteMaxVotedVotes) + ") "
                        + String.Format("{0:X2}", sixthKeyByteSecondMostVoted) + "(" + String.Format("{0:D4}", sixthKeyByteSecondMostVotedVotes) + ") "
                        + String.Format("{0:X2}", sixthKeyByteThirdMostVoted) + "(" + String.Format("{0:D4}", sixthKeyByteThirdMostVotedVotes) + ")\n"

                        + "06\t" + String.Format("{0:X2}", seventhKeyByteMaxVoted) + "(" + String.Format("{0:D4}", seventhKeyByteMaxVotedVotes) + ") "
                        + String.Format("{0:X2}", seventhKeyByteSecondMostVoted) + "(" + String.Format("{0:D4}", seventhKeyByteSecondMostVotedVotes) + ") "
                        + String.Format("{0:X2}", seventhKeyByteThirdMostVoted) + "(" + String.Format("{0:D4}", seventhKeyByteThirdMostVotedVotes) + ")\n"

                        + "07\t" + String.Format("{0:X2}", eighthKeyByteMaxVoted) + "(" + String.Format("{0:D4}", eighthKeyByteMaxVotedVotes) + ") "
                        + String.Format("{0:X2}", eighthKeyByteSecondMostVoted) + "(" + String.Format("{0:D4}", eighthKeyByteSecondMostVotedVotes) + ") "
                        + String.Format("{0:X2}", eighthKeyByteThirdMostVoted) + "(" + String.Format("{0:D4}", eighthKeyByteThirdMostVotedVotes) + ")\n"

                        + "08\t" + String.Format("{0:X2}", ninthKeyByteMaxVoted) + "(" + String.Format("{0:D4}", ninthKeyByteMaxVotedVotes) + ") "
                        + String.Format("{0:X2}", ninthKeyByteSecondMostVoted) + "(" + String.Format("{0:D4}", ninthKeyByteSecondMostVotedVotes) + ") "
                        + String.Format("{0:X2}", ninthKeyByteThirdMostVoted) + "(" + String.Format("{0:D4}", ninthKeyByteThirdMostVotedVotes) + ")\n"

                        + "09\t" + String.Format("{0:X2}", tenthKeyByteMaxVoted) + "(" + String.Format("{0:D4}", tenthKeyByteMaxVotedVotes) + ") "
                        + String.Format("{0:X2}", tenthKeyByteSecondMostVoted) + "(" + String.Format("{0:D4}", tenthKeyByteSecondMostVotedVotes) + ") "
                        + String.Format("{0:X2}", tenthKeyByteThirdMostVoted) + "(" + String.Format("{0:D4}", tenthKeyByteThirdMostVotedVotes) + ")\n"

                        + "10\t" + String.Format("{0:X2}", eleventhKeyByteMaxVoted) + "(" + String.Format("{0:D4}", eleventhKeyByteMaxVotedVotes) + ") "
                        + String.Format("{0:X2}", eleventhKeyByteSecondMostVoted) + "(" + String.Format("{0:D4}", eleventhKeyByteSecondMostVotedVotes) + ") "
                        + String.Format("{0:X2}", eleventhKeyByteThirdMostVoted) + "(" + String.Format("{0:D4}", eleventhKeyByteThirdMostVotedVotes) + ")\n"

                        + "11\t" + String.Format("{0:X2}", twelfthKeyByteMaxVoted) + "(" + String.Format("{0:D4}", twelfthKeyByteMaxVotedVotes) + ") "
                        + String.Format("{0:X2}", twelfthKeyByteSecondMostVoted) + "(" + String.Format("{0:D4}", twelfthKeyByteSecondMostVotedVotes) + ") "
                        + String.Format("{0:X2}", twelfthKeyByteThirdMostVoted) + "(" + String.Format("{0:D4}", twelfthKeyByteThirdMostVotedVotes) + ")\n"

                        + "12\t" + String.Format("{0:X2}", thirteenthKeyByteMaxVoted) + "(" + String.Format("{0:D4}", thirteenthKeyByteMaxVotedVotes) + ") "
                        + String.Format("{0:X2}", thirteenthKeyByteSecondMostVoted) + "(" + String.Format("{0:D4}", thirteenthKeyByteSecondMostVotedVotes) + ") "
                        + String.Format("{0:X2}", thirteenthKeyByteThirdMostVoted) + "(" + String.Format("{0:D4}", thirteenthKeyByteThirdMostVotedVotes) + ")\n"


                        + "\n\n"

                        + "Possible key found after using " + counter.ToString("#,#", CultureInfo.InstalledUICulture) + " packets!\n["
                        + String.Format("{0:X2}", firstKeyByteMaxVoted) + ":"
                        + String.Format("{0:X2}", secondKeyByteMaxVoted) + ":"
                        + String.Format("{0:X2}", thirdKeyByteMaxVoted) + ":"
                        + String.Format("{0:X2}", fourthKeyByteMaxVoted) + ":"
                        + String.Format("{0:X2}", sixthKeyByteMaxVoted) + ":"
                        + String.Format("{0:X2}", seventhKeyByteMaxVoted) + ":"
                        + String.Format("{0:X2}", eighthKeyByteMaxVoted) + ":"
                        + String.Format("{0:X2}", ninthKeyByteMaxVoted) + ":"
                        + String.Format("{0:X2}", tenthKeyByteMaxVoted) + ":"
                        + String.Format("{0:X2}", eleventhKeyByteMaxVoted) + ":"
                        + String.Format("{0:X2}", twelfthKeyByteMaxVoted) + ":"
                        + String.Format("{0:X2}", thirteenthKeyByteMaxVoted) + "]"

                        + "\n(ASCII: "
                        + (char)firstKeyByteMaxVoted
                        + (char)secondKeyByteMaxVoted
                        + (char)thirdKeyByteMaxVoted
                        + (char)fourthKeyByteMaxVoted
                        + (char)fifthKeyByteMaxVoted
                        + (char)sixthKeyByteMaxVoted
                        + (char)seventhKeyByteMaxVoted
                        + (char)eighthKeyByteMaxVoted
                        + (char)ninthKeyByteMaxVoted
                        + (char)tenthKeyByteMaxVoted
                        + (char)eleventhKeyByteMaxVoted
                        + (char)twelfthKeyByteMaxVoted
                        + (char)thirteenthKeyByteMaxVoted
                        + ")\n\n"

                        + "Time used [h:min:sec]: " + duration + "."
                        ;
                }, votes);
            }

            if (stop)
            {
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                (SendOrPostCallback)delegate
                {
                    textBox.Text =
                        "KB\tbyte(vote)\n"
                        + "00\t" + String.Format("{0:X2}", firstKeyByteMaxVoted) + "(" + String.Format("{0:D4}", firstKeyByteMaxVotedVotes) + ") "
                        + String.Format("{0:X2}", firstKeyByteSecondMostVoted) + "(" + String.Format("{0:D4}", firstKeyByteSecondMostVotedVotes) + ") "
                        + String.Format("{0:X2}", firstKeyByteThirdMostVoted) + "(" + String.Format("{0:D4}", firstKeyByteThirdMostVotedVotes) + ")\n"

                        + "01\t" + String.Format("{0:X2}", secondKeyByteMaxVoted) + "(" + String.Format("{0:D4}", secondKeyByteMaxVotedVotes) + ") "
                        + String.Format("{0:X2}", secondKeyByteSecondMostVoted) + "(" + String.Format("{0:D4}", secondKeyByteSecondMostVotedVotes) + ") "
                        + String.Format("{0:X2}", secondKeyByteThirdMostVoted) + "(" + String.Format("{0:D4}", secondKeyByteThirdMostVotedVotes) + ")\n"

                        + "02\t" + String.Format("{0:X2}", thirdKeyByteMaxVoted) + "(" + String.Format("{0:D4}", thirdKeyByteMaxVotedVotes) + ") "
                        + String.Format("{0:X2}", thirdKeyByteSecondMostVoted) + "(" + String.Format("{0:D4}", thirdKeyByteSecondMostVotedVotes) + ") "
                        + String.Format("{0:X2}", thirdKeyByteThirdMostVoted) + "(" + String.Format("{0:D4}", thirdKeyByteThirdMostVotedVotes) + ")\n"

                        + "03\t" + String.Format("{0:X2}", fourthKeyByteMaxVoted) + "(" + String.Format("{0:D4}", fourthKeyByteMaxVotedVotes) + ") "
                        + String.Format("{0:X2}", fourthKeyByteSecondMostVoted) + "(" + String.Format("{0:D4}", fourthKeyByteSecondMostVotedVotes) + ") "
                        + String.Format("{0:X2}", fourthKeyByteThirdMostVoted) + "(" + String.Format("{0:D4}", fourthKeyByteThirdMostVotedVotes) + ")\n"

                        + "04\t" + String.Format("{0:X2}", fifthKeyByteMaxVoted) + "(" + String.Format("{0:D4}", fifthKeyByteMaxVotedVotes) + ") "
                        + String.Format("{0:X2}", fifthKeyByteSecondMostVoted) + "(" + String.Format("{0:D4}", fifthKeyByteSecondMostVotedVotes) + ") "
                        + String.Format("{0:X2}", fifthKeyByteThirdMostVoted) + "(" + String.Format("{0:D4}", fifthKeyByteThirdMostVotedVotes) + ")\n"

                        + "05\t" + String.Format("{0:X2}", sixthKeyByteMaxVoted) + "(" + String.Format("{0:D4}", sixthKeyByteMaxVotedVotes) + ") "
                        + String.Format("{0:X2}", sixthKeyByteSecondMostVoted) + "(" + String.Format("{0:D4}", sixthKeyByteSecondMostVotedVotes) + ") "
                        + String.Format("{0:X2}", sixthKeyByteThirdMostVoted) + "(" + String.Format("{0:D4}", sixthKeyByteThirdMostVotedVotes) + ")\n"

                        + "06\t" + String.Format("{0:X2}", seventhKeyByteMaxVoted) + "(" + String.Format("{0:D4}", seventhKeyByteMaxVotedVotes) + ") "
                        + String.Format("{0:X2}", seventhKeyByteSecondMostVoted) + "(" + String.Format("{0:D4}", seventhKeyByteSecondMostVotedVotes) + ") "
                        + String.Format("{0:X2}", seventhKeyByteThirdMostVoted) + "(" + String.Format("{0:D4}", seventhKeyByteThirdMostVotedVotes) + ")\n"

                        + "07\t" + String.Format("{0:X2}", eighthKeyByteMaxVoted) + "(" + String.Format("{0:D4}", eighthKeyByteMaxVotedVotes) + ") "
                        + String.Format("{0:X2}", eighthKeyByteSecondMostVoted) + "(" + String.Format("{0:D4}", eighthKeyByteSecondMostVotedVotes) + ") "
                        + String.Format("{0:X2}", eighthKeyByteThirdMostVoted) + "(" + String.Format("{0:D4}", eighthKeyByteThirdMostVotedVotes) + ")\n"

                        + "08\t" + String.Format("{0:X2}", ninthKeyByteMaxVoted) + "(" + String.Format("{0:D4}", ninthKeyByteMaxVotedVotes) + ") "
                        + String.Format("{0:X2}", ninthKeyByteSecondMostVoted) + "(" + String.Format("{0:D4}", ninthKeyByteSecondMostVotedVotes) + ") "
                        + String.Format("{0:X2}", ninthKeyByteThirdMostVoted) + "(" + String.Format("{0:D4}", ninthKeyByteThirdMostVotedVotes) + ")\n"

                        + "09\t" + String.Format("{0:X2}", tenthKeyByteMaxVoted) + "(" + String.Format("{0:D4}", tenthKeyByteMaxVotedVotes) + ") "
                        + String.Format("{0:X2}", tenthKeyByteSecondMostVoted) + "(" + String.Format("{0:D4}", tenthKeyByteSecondMostVotedVotes) + ") "
                        + String.Format("{0:X2}", tenthKeyByteThirdMostVoted) + "(" + String.Format("{0:D4}", tenthKeyByteThirdMostVotedVotes) + ")\n"

                        + "10\t" + String.Format("{0:X2}", eleventhKeyByteMaxVoted) + "(" + String.Format("{0:D4}", eleventhKeyByteMaxVotedVotes) + ") "
                        + String.Format("{0:X2}", eleventhKeyByteSecondMostVoted) + "(" + String.Format("{0:D4}", eleventhKeyByteSecondMostVotedVotes) + ") "
                        + String.Format("{0:X2}", eleventhKeyByteThirdMostVoted) + "(" + String.Format("{0:D4}", eleventhKeyByteThirdMostVotedVotes) + ")\n"

                        + "11\t" + String.Format("{0:X2}", twelfthKeyByteMaxVoted) + "(" + String.Format("{0:D4}", twelfthKeyByteMaxVotedVotes) + ") "
                        + String.Format("{0:X2}", twelfthKeyByteSecondMostVoted) + "(" + String.Format("{0:D4}", twelfthKeyByteSecondMostVotedVotes) + ") "
                        + String.Format("{0:X2}", twelfthKeyByteThirdMostVoted) + "(" + String.Format("{0:D4}", twelfthKeyByteThirdMostVotedVotes) + ")\n"

                        + "12\t" + String.Format("{0:X2}", thirteenthKeyByteMaxVoted) + "(" + String.Format("{0:D4}", thirteenthKeyByteMaxVotedVotes) + ") "
                        + String.Format("{0:X2}", thirteenthKeyByteSecondMostVoted) + "(" + String.Format("{0:D4}", thirteenthKeyByteSecondMostVotedVotes) + ") "
                        + String.Format("{0:X2}", thirteenthKeyByteThirdMostVoted) + "(" + String.Format("{0:D4}", thirteenthKeyByteThirdMostVotedVotes) + ")\n"


                        + "\n\n"

                        + "Aborted after [h:min:sec]" + duration + ".";
                }, votes);
            }

            if (!success && inputMode.Equals("file"))
            {
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                (SendOrPostCallback)delegate
                {
                    textBox.Text =
                        "KB\tbyte(vote)\n"
                        + "00\t" + String.Format("{0:X2}", firstKeyByteMaxVoted) + "(" + String.Format("{0:D4}", firstKeyByteMaxVotedVotes) + ") "
                        + String.Format("{0:X2}", firstKeyByteSecondMostVoted) + "(" + String.Format("{0:D4}", firstKeyByteSecondMostVotedVotes) + ") "
                        + String.Format("{0:X2}", firstKeyByteThirdMostVoted) + "(" + String.Format("{0:D4}", firstKeyByteThirdMostVotedVotes) + ")\n"

                        + "01\t" + String.Format("{0:X2}", secondKeyByteMaxVoted) + "(" + String.Format("{0:D4}", secondKeyByteMaxVotedVotes) + ") "
                        + String.Format("{0:X2}", secondKeyByteSecondMostVoted) + "(" + String.Format("{0:D4}", secondKeyByteSecondMostVotedVotes) + ") "
                        + String.Format("{0:X2}", secondKeyByteThirdMostVoted) + "(" + String.Format("{0:D4}", secondKeyByteThirdMostVotedVotes) + ")\n"

                        + "02\t" + String.Format("{0:X2}", thirdKeyByteMaxVoted) + "(" + String.Format("{0:D4}", thirdKeyByteMaxVotedVotes) + ") "
                        + String.Format("{0:X2}", thirdKeyByteSecondMostVoted) + "(" + String.Format("{0:D4}", thirdKeyByteSecondMostVotedVotes) + ") "
                        + String.Format("{0:X2}", thirdKeyByteThirdMostVoted) + "(" + String.Format("{0:D4}", thirdKeyByteThirdMostVotedVotes) + ")\n"

                        + "03\t" + String.Format("{0:X2}", fourthKeyByteMaxVoted) + "(" + String.Format("{0:D4}", fourthKeyByteMaxVotedVotes) + ") "
                        + String.Format("{0:X2}", fourthKeyByteSecondMostVoted) + "(" + String.Format("{0:D4}", fourthKeyByteSecondMostVotedVotes) + ") "
                        + String.Format("{0:X2}", fourthKeyByteThirdMostVoted) + "(" + String.Format("{0:D4}", fourthKeyByteThirdMostVotedVotes) + ")\n"

                        + "04\t" + String.Format("{0:X2}", fifthKeyByteMaxVoted) + "(" + String.Format("{0:D4}", fifthKeyByteMaxVotedVotes) + ") "
                        + String.Format("{0:X2}", fifthKeyByteSecondMostVoted) + "(" + String.Format("{0:D4}", fifthKeyByteSecondMostVotedVotes) + ") "
                        + String.Format("{0:X2}", fifthKeyByteThirdMostVoted) + "(" + String.Format("{0:D4}", fifthKeyByteThirdMostVotedVotes) + ")\n"

                        + "05\t" + String.Format("{0:X2}", sixthKeyByteMaxVoted) + "(" + String.Format("{0:D4}", sixthKeyByteMaxVotedVotes) + ") "
                        + String.Format("{0:X2}", sixthKeyByteSecondMostVoted) + "(" + String.Format("{0:D4}", sixthKeyByteSecondMostVotedVotes) + ") "
                        + String.Format("{0:X2}", sixthKeyByteThirdMostVoted) + "(" + String.Format("{0:D4}", sixthKeyByteThirdMostVotedVotes) + ")\n"

                        + "06\t" + String.Format("{0:X2}", seventhKeyByteMaxVoted) + "(" + String.Format("{0:D4}", seventhKeyByteMaxVotedVotes) + ") "
                        + String.Format("{0:X2}", seventhKeyByteSecondMostVoted) + "(" + String.Format("{0:D4}", seventhKeyByteSecondMostVotedVotes) + ") "
                        + String.Format("{0:X2}", seventhKeyByteThirdMostVoted) + "(" + String.Format("{0:D4}", seventhKeyByteThirdMostVotedVotes) + ")\n"

                        + "07\t" + String.Format("{0:X2}", eighthKeyByteMaxVoted) + "(" + String.Format("{0:D4}", eighthKeyByteMaxVotedVotes) + ") "
                        + String.Format("{0:X2}", eighthKeyByteSecondMostVoted) + "(" + String.Format("{0:D4}", eighthKeyByteSecondMostVotedVotes) + ") "
                        + String.Format("{0:X2}", eighthKeyByteThirdMostVoted) + "(" + String.Format("{0:D4}", eighthKeyByteThirdMostVotedVotes) + ")\n"

                        + "08\t" + String.Format("{0:X2}", ninthKeyByteMaxVoted) + "(" + String.Format("{0:D4}", ninthKeyByteMaxVotedVotes) + ") "
                        + String.Format("{0:X2}", ninthKeyByteSecondMostVoted) + "(" + String.Format("{0:D4}", ninthKeyByteSecondMostVotedVotes) + ") "
                        + String.Format("{0:X2}", ninthKeyByteThirdMostVoted) + "(" + String.Format("{0:D4}", ninthKeyByteThirdMostVotedVotes) + ")\n"

                        + "09\t" + String.Format("{0:X2}", tenthKeyByteMaxVoted) + "(" + String.Format("{0:D4}", tenthKeyByteMaxVotedVotes) + ") "
                        + String.Format("{0:X2}", tenthKeyByteSecondMostVoted) + "(" + String.Format("{0:D4}", tenthKeyByteSecondMostVotedVotes) + ") "
                        + String.Format("{0:X2}", tenthKeyByteThirdMostVoted) + "(" + String.Format("{0:D4}", tenthKeyByteThirdMostVotedVotes) + ")\n"

                        + "10\t" + String.Format("{0:X2}", eleventhKeyByteMaxVoted) + "(" + String.Format("{0:D4}", eleventhKeyByteMaxVotedVotes) + ") "
                        + String.Format("{0:X2}", eleventhKeyByteSecondMostVoted) + "(" + String.Format("{0:D4}", eleventhKeyByteSecondMostVotedVotes) + ") "
                        + String.Format("{0:X2}", eleventhKeyByteThirdMostVoted) + "(" + String.Format("{0:D4}", eleventhKeyByteThirdMostVotedVotes) + ")\n"

                        + "11\t" + String.Format("{0:X2}", twelfthKeyByteMaxVoted) + "(" + String.Format("{0:D4}", twelfthKeyByteMaxVotedVotes) + ") "
                        + String.Format("{0:X2}", twelfthKeyByteSecondMostVoted) + "(" + String.Format("{0:D4}", twelfthKeyByteSecondMostVotedVotes) + ") "
                        + String.Format("{0:X2}", twelfthKeyByteThirdMostVoted) + "(" + String.Format("{0:D4}", twelfthKeyByteThirdMostVotedVotes) + ")\n"

                        + "12\t" + String.Format("{0:X2}", thirteenthKeyByteMaxVoted) + "(" + String.Format("{0:D4}", thirteenthKeyByteMaxVotedVotes) + ") "
                        + String.Format("{0:X2}", thirteenthKeyByteSecondMostVoted) + "(" + String.Format("{0:D4}", thirteenthKeyByteSecondMostVotedVotes) + ") "
                        + String.Format("{0:X2}", thirteenthKeyByteThirdMostVoted) + "(" + String.Format("{0:D4}", thirteenthKeyByteThirdMostVotedVotes) + ")\n"


                        + "\n\n"

                        + "Could not recover key.\n"
                        + "May be you need more packets for a\n"
                        + "successful attack.\n";

                        if (kindOfAttack.Equals("FMS"))
                        {
                            textBox.AppendText("For a 104 bit key and a FMS attack\n"
                            + "you need usually 3 - 4 mio. encrypted packets.");
                        }
                        if (kindOfAttack.Equals("KoreK"))
                        {
                            textBox.AppendText("For a KoreK attack\n"
                            + "you need usually 500.000 - 1 mio. encrypted packets.");
                        }

                    textBox.AppendText("\n\n"
                        + "Time used [h:min:sec]: " + duration + ".");
                }, votes);
            }
        }


        /// <summary>
        /// Sets the text within the text box. Key, key ranking and so on.
        /// </summary>
        /// <param name="votes">The votes table.</param>
        /// <param name="success">Indicates if the attack was successful.</param>
        /// <param name="keySize">Indicates the key size (40 bit or 104 bit).</param>
        /// <param name="counter">Indicates number of used packets.</param>
        public void setTextBoxPTW(int[,] votes, bool success, int keySize, int counter, TimeSpan duration, string inputMode, bool stop)
        {
            int firstKeyByteMaxVoted = indexOfMaxVoted(votes, 0);
            int secondKeyByteMaxVoted = (indexOfMaxVoted(votes, 1) - indexOfMaxVoted(votes, 0)) & 0xFF;
            int thirdKeyByteMaxVoted = (indexOfMaxVoted(votes, 2) - indexOfMaxVoted(votes, 1)) & 0xFF;
            int fourthKeyByteMaxVoted = (indexOfMaxVoted(votes, 3) - indexOfMaxVoted(votes, 2)) & 0xFF;
            int fifthKeyByteMaxVoted = (indexOfMaxVoted(votes, 4) - indexOfMaxVoted(votes, 3)) & 0xFF;
            int sixthKeyByteMaxVoted = (indexOfMaxVoted(votes, 5) - indexOfMaxVoted(votes, 4)) & 0xFF;
            int seventhKeyByteMaxVoted = (indexOfMaxVoted(votes, 6) - indexOfMaxVoted(votes, 5)) & 0xFF;
            int eighthKeyByteMaxVoted = (indexOfMaxVoted(votes, 7) - indexOfMaxVoted(votes, 6)) & 0xFF;
            int ninthKeyByteMaxVoted = (indexOfMaxVoted(votes, 8) - indexOfMaxVoted(votes, 7)) & 0xFF;
            int tenthKeyByteMaxVoted = (indexOfMaxVoted(votes, 9) - indexOfMaxVoted(votes, 8)) & 0xFF;
            int eleventhKeyByteMaxVoted = (indexOfMaxVoted(votes, 10) - indexOfMaxVoted(votes, 9)) & 0xFF;
            int twelfthKeyByteMaxVoted = (indexOfMaxVoted(votes, 11) - indexOfMaxVoted(votes, 10)) & 0xFF;
            int thirteenthKeyByteMaxVoted = (indexOfMaxVoted(votes, 12) - indexOfMaxVoted(votes, 11)) & 0xFF;
            
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                (SendOrPostCallback)delegate
                {
                    textBox.Text =
                        "Possible key:\n"
                        + String.Format("{0:X2}", firstKeyByteMaxVoted) + ":"
                        + String.Format("{0:X2}", secondKeyByteMaxVoted) + ":"
                        + String.Format("{0:X2}", thirdKeyByteMaxVoted) + ":"
                        + String.Format("{0:X2}", fourthKeyByteMaxVoted) + ":"
                        + String.Format("{0:X2}", fifthKeyByteMaxVoted) + ":"
                        + String.Format("{0:X2}", sixthKeyByteMaxVoted) + ":"
                        + String.Format("{0:X2}", seventhKeyByteMaxVoted) + ":"
                        + String.Format("{0:X2}", eighthKeyByteMaxVoted) + ":"
                        + String.Format("{0:X2}", ninthKeyByteMaxVoted) + ":"
                        + String.Format("{0:X2}", tenthKeyByteMaxVoted) + ":"
                        + String.Format("{0:X2}", eleventhKeyByteMaxVoted) + ":"
                        + String.Format("{0:X2}", twelfthKeyByteMaxVoted) + ":"
                        + String.Format("{0:X2}", thirteenthKeyByteMaxVoted);
                }, votes);
            if (success && (keySize == 40))
            {
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                (SendOrPostCallback)delegate
                {
                    textBox.Text =
                        "Possible key found after using " + counter.ToString("#,#", CultureInfo.InstalledUICulture) + " packets!\n["
                        + String.Format("{0:X2}", firstKeyByteMaxVoted) + ":"
                        + String.Format("{0:X2}", secondKeyByteMaxVoted) + ":"
                        + String.Format("{0:X2}", thirdKeyByteMaxVoted) + ":"
                        + String.Format("{0:X2}", fourthKeyByteMaxVoted) + ":"
                        + String.Format("{0:X2}", fifthKeyByteMaxVoted) + "]"
                        + "\n\n"

                        + "(ASCII: "
                        + (char)firstKeyByteMaxVoted
                        + (char)secondKeyByteMaxVoted
                        + (char)thirdKeyByteMaxVoted
                        + (char)fourthKeyByteMaxVoted
                        + (char)fifthKeyByteMaxVoted
                        + ")\n\n"

                        + "Time used [h:min:sec]: " + duration + "."
                        ;
                }, votes);
            }

            if (success && (keySize == 104))
            {
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                (SendOrPostCallback)delegate
                {
                    textBox.Text =
                        "Possible key found after using " + counter.ToString("#,#", CultureInfo.InstalledUICulture) + " packets!\n["
                        + String.Format("{0:X2}", firstKeyByteMaxVoted) + ":"
                        + String.Format("{0:X2}", secondKeyByteMaxVoted) + ":"
                        + String.Format("{0:X2}", thirdKeyByteMaxVoted) + ":"
                        + String.Format("{0:X2}", fourthKeyByteMaxVoted) + ":"
                        + String.Format("{0:X2}", fifthKeyByteMaxVoted) + ":"
                        + String.Format("{0:X2}", sixthKeyByteMaxVoted) + ":"
                        + String.Format("{0:X2}", eighthKeyByteMaxVoted) + ":"
                        + String.Format("{0:X2}", ninthKeyByteMaxVoted) + ":"
                        + String.Format("{0:X2}", tenthKeyByteMaxVoted) + ":" 
                        + String.Format("{0:X2}", eleventhKeyByteMaxVoted) + ":" 
                        + String.Format("{0:X2}", twelfthKeyByteMaxVoted) + ":"
                        + String.Format("{0:X2}", thirteenthKeyByteMaxVoted) + "]"
                        
                        + "\n\n"

                        + "(ASCII: "
                        + (char)firstKeyByteMaxVoted
                        + (char)secondKeyByteMaxVoted
                        + (char)thirdKeyByteMaxVoted
                        + (char)fourthKeyByteMaxVoted
                        + (char)fifthKeyByteMaxVoted
                        + (char)sixthKeyByteMaxVoted
                        + (char)seventhKeyByteMaxVoted
                        + (char)eighthKeyByteMaxVoted
                        + (char)ninthKeyByteMaxVoted
                        + (char)tenthKeyByteMaxVoted
                        + (char)eleventhKeyByteMaxVoted
                        + (char)twelfthKeyByteMaxVoted
                        + (char)thirteenthKeyByteMaxVoted
                        + ")\n\n"

                        + "Time used [h:min:sec]: " + duration + "."

                        ;
                }, votes);
            }

            if ((!success) && (keySize == 900))
            {
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                (SendOrPostCallback)delegate
                {
                    textBox.Text =
                        "Possible key:\n"
                        + String.Format("{0:X2}", firstKeyByteMaxVoted) + ":"
                        + String.Format("{0:X2}", secondKeyByteMaxVoted) + ":"
                        + String.Format("{0:X2}", thirdKeyByteMaxVoted) + ":"
                        + String.Format("{0:X2}", fourthKeyByteMaxVoted) + ":"
                        + String.Format("{0:X2}", fifthKeyByteMaxVoted) + ":"
                        + String.Format("{0:X2}", sixthKeyByteMaxVoted) + ":"
                        + String.Format("{0:X2}", eighthKeyByteMaxVoted) + ":"
                        + String.Format("{0:X2}", ninthKeyByteMaxVoted) + ":"
                        + String.Format("{0:X2}", tenthKeyByteMaxVoted) + ":"
                        + String.Format("{0:X2}", eleventhKeyByteMaxVoted) + ":"
                        + String.Format("{0:X2}", twelfthKeyByteMaxVoted) + ":"
                        + String.Format("{0:X2}", thirteenthKeyByteMaxVoted)

                        + "\n\n"

                        + "Could not recover key.\nPropably it is a strong key."

                        + "\n\n"

                        + "Time used [h:min:sec]: " + duration + ".";
                }, votes);
            }

            if ((!success) && (inputMode.Equals("file")))
            {
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                (SendOrPostCallback)delegate
                {
                    textBox.Text =
                        "Possible key:\n"
                        + String.Format("{0:X2}", firstKeyByteMaxVoted) + ":"
                        + String.Format("{0:X2}", secondKeyByteMaxVoted) + ":"
                        + String.Format("{0:X2}", thirdKeyByteMaxVoted) + ":"
                        + String.Format("{0:X2}", fourthKeyByteMaxVoted) + ":"
                        + String.Format("{0:X2}", fifthKeyByteMaxVoted) + ":"
                        + String.Format("{0:X2}", sixthKeyByteMaxVoted) + ":"
                        + String.Format("{0:X2}", eighthKeyByteMaxVoted) + ":"
                        + String.Format("{0:X2}", ninthKeyByteMaxVoted) + ":"
                        + String.Format("{0:X2}", tenthKeyByteMaxVoted) + ":"
                        + String.Format("{0:X2}", eleventhKeyByteMaxVoted) + ":"
                        + String.Format("{0:X2}", twelfthKeyByteMaxVoted) + ":"
                        + String.Format("{0:X2}", thirteenthKeyByteMaxVoted)

                        + "\n\n"

                        + "Could not recover key.\n"
                        + "May be you need more packets.\n"
                        + "For a 104 bit key you usually\n"
                        + "need 100.000 ARP packets or\n"
                        + "a few more IP packets."

                        + "\n\n"

                        + "Time used [h:min:sec]: " + duration + ".";
                }, votes);
            }
        }
        #endregion

        #region Private help methods

        /// <summary>
        /// Finds the index of the highest value in a two dimensional array in the given dimension.
        /// </summary>
        /// <param name="votes">The two dimensional array with the key byte votes in it.</param>
        /// <param name="dimenstion">Dimension, in which the max are searched.</param>
        /// <returns>The position of the hightes value and in this way the most voted key byte.</returns>
        public int indexOfMaxVoted(int[,] votes, int dimension)
        {
            int temp = 0;
            int index = 0;
            for (int i = 0; i < 256; i++)
            {
                if (votes[dimension, i] > temp)
                {
                    temp = votes[dimension, i];
                    index = i;
                }
            }
            return index;
        }

        /// <summary>
        /// Searches for a specific voted key byte. Returns NOT the most voted, it returns the most voted
        /// up to the given limit.
        /// Example: Most voted key byte is 0x47. Then this method searches for the scond most voted key byte
        /// under 0x47.
        /// </summary>
        /// <param name="votes">The two dimensional array with the key byte votes in it.</param>
        /// <param name="dimension">Dimension in which the key byte is searched.</param>
        /// <param name="limit">The upper limit of votes.</param>
        /// <param name="indexOfGreaterKeyByte">Index of keybyte value, which is in actual context more voted.
        /// Needed if two different key bytes have same voting.</param>
        /// <returns>The most voted key byte under the given limit.</returns>
        private int indexOfSpecificVoted(int[,] votes, int dimension, int limit, int indexOfGreaterKeyByte)
        {
            int index = 0;
            int temp = 0;
            for (int i = 0; i < 256; i++)
            {
                if (votes[dimension,i] > temp)
                {
                    temp = votes[dimension, i];
                    
                    if ((temp <= limit) && (i != indexOfGreaterKeyByte))
                    {
                        index = i;
                    }
                }
            }
            return index;
        }

        #endregion
    }
}
