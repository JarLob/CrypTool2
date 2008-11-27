﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Analysis;
using System.ComponentModel;
using Cryptool.KasiskiTest;

using System.Windows.Controls;

namespace Cryptool.KasiskiTest
{
    [PluginInfo(false,
      "Kasiski's Test",
      "Calculates possible keylenghts of a polyalphabetic substitusion cipher",
      "URL",
      "KasiskiTest/icon.png")]
    public class KasiskiTest : IStatistic
    {
        
        

        #region Private Variables
        private int integerValue;
        private string stringOutput;
        private string stringInput;
        #endregion
        public static DataSource Data = new DataSource();
        #region Properties (Inputs/Outputs)

        [PropertyInfo(Direction.Input, "The string to be analyzed", "blablah", "", true, true, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public string StringInput
        {
            get
            {
                return stringInput;
            }
            set { stringInput = value; OnPropertyChanged("StringInput"); }
        }
       
        [PropertyInfo(Direction.Output, "Integer value.", "The last generated ineteger value.", "",false , false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
    public int IntegerValue
    {
        get { return integerValue; }
        set
        {
            if (value != integerValue)
            {
                integerValue = value;
                OnPropertyChanged("IntegerValue");
            }
        }
    }
     
    
        [PropertyInfo(Direction.Output, "The string to be outputed", "blablah", "", true, true, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public string StringOutput
        {
            get
            {
                return stringOutput;
            }
            set 
            { 
                stringOutput = value; OnPropertyChanged("StringOutput"); 
            }

        }




        #endregion

        #region IPlugin Members

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        private KasiskiTestSettings settings;
        public ISettings Settings
        {
            get { return settings; }
            set { settings = (KasiskiTestSettings)value; }
        }



        public KasiskiTestPresentation presentation;
        public KasiskiTest()
        {
            settings = new KasiskiTestSettings();
            presentation = new KasiskiTestPresentation(this);
            Presentation = presentation;
        }
       public UserControl Presentation { get; private set; }


       public UserControl QuickWatchPresentation
       {
           get { return null; }
       }

        public void PreExecution()
        {
            //return null;
        }

        public void Execute()
        {
            if (stringInput != null)
            {
                
                // BEGIN PREWORK TO INPUT STRING DEPENDING ON USER CHOICE IN SETTINGS PANEL
                
                string workString = stringInput; //Copy the input string to a work string in order to make changes to it if neccessary
                string workstring2 = "";
                
                  //UNKNOWN SYMBOL HANDLING
                                
                
                if (settings.unknownSymbolHandling == 1) //Determine (via setting in Settings Panel) if unknown symbols (e.g. , : \ /) should be discarded from the text which is to be analysed
                {
                    char[] validchars = new char[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' }; //Sets alphabet of valid characters
                    string strValidChars = new string(validchars);
                    StringBuilder workstring1 = new StringBuilder();
                    foreach (char c in workString.ToCharArray())
                    {
                        if (strValidChars.IndexOf(c) >= 0) //If a char from the workString is valid, it is  appended to a newly built workstring1
                        {
                            workstring1.Append(c);
                        }
                    }

                    workstring2 = workstring1.ToString(); // Now copy workstring1 to workstring2. Needed because workstring1 can not be altered once its built
                }
                else 
                {
                    workstring2 = workString; // In case unknown symbols are to be ignored copy workString to workstring2, and proceed with analysis of workstring2
                }

                   //CASE SENSITIVITY

                if (settings.caseSensitivity == 0) { workstring2=workstring2.ToUpper(); }                         
                
                
               
                
                //int grammLength = 3; - Setting for maximum gramm length now avaliable in Settings Panel
                ArrayList checkedGramms = new ArrayList();
                ArrayList distances = new ArrayList();

                int g = 0;
                string grammToSearch;

                 
                              

                //TODO "add functionality that allows the user to choose grammLength - Done" , "add another for loop which starts from 3 until gramLength is reached" - array size of checkedGramms[],factors[] and distances[] needs tweaking, otherwise done

                
                //BEGIN TO SEARCH FOR EQUAL SUBSTRINGS AND TO COUNT DISTANCES BETWEEN THEM

                
                for (int d = 3; d <= settings.grammLength; d++)
                {
                    for (int i = 0; i <= workstring2.Length - settings.grammLength; i++)   //go through string 
                    {
                        grammToSearch = workstring2.Substring(i, d);   //  get every gramm(substring) with gramLength from Settings


                        //if (g <= checkedGramms.Length - 1)    // 
                        //{

                            
                            for (int n = i + settings.grammLength; n <= workstring2.Length - settings.grammLength; n++)  //go through workString starting after the end of the taken grammToSearch
                            {
                                if (grammToSearch == workstring2.Substring(n, d)) //if grammToSearch in workString
                                {
                                    distances.Add(n-i);               //save the distance in index 'g' of distances exactly where gramToSearch in checkedGramms is placed  
                                    checkedGramms.Add(grammToSearch);                     //put in unused space of checkedGramms
                                    
break;
                                }


                            }
                        //}
                       
                    }
                }

                //BEGIN TO FACTORIZE THE DISTANCES


                

                //TODO: SPEED !@#$%^& !!!

                             
                
                
                //int[] copyOfDistances = distances.GetType(); //Copy distances to find largest member
                //Array.Copy(distances, copyOfDistances, distances.Length);
                //Array.Sort(copyOfDistances);
                //int sqrtOfLargestDist = Convert.ToInt32(Math.Sqrt(copyOfDistances[copyOfDistances.Length - 1])); //sqrtOf largest distance will give the maximum number of prime factors of the distance for example if the distance is a power of 2 which is the worst case
                int x=0;


                int[,] factors = new int[distances.Count, settings.factorSize /*sqrtOfLargestDist */];    //Rectangular array factors     

                int[] factorCounter = new int[(settings.factorSize+1) /*sqrtOfLargestDist */];                //Vector array each factor is the index of the array, the element at this index illustrates how many times the factor is met 
                
                for (int z = 0; z <= distances.Count - 1; z++)       //for each distance
                {
                    int numberToFactorize = Convert.ToInt32( distances[z]);             //assign variable
                    x = 0;

                    for (int y = 2; y <= settings.factorSize/*Math.Sqrt(numberToFactorize)*/; y++) // for each number starting from 2 until sqrt(number to factorize)
                    {
                        if (numberToFactorize == 0) { break;}
                        if (numberToFactorize % y == 0 /*& numberToFactorize!=0*/)     // if devisibe without rest 
                        {
                            factors[z, x] = y;
                            x++;
                            factorCounter[y]++;
                            
                           // while (numberToFactorize % y == 0)  //if a factor is found numberToFactorize is divided by it until division without rest is not possible anymore
                           // {
                           //     numberToFactorize = numberToFactorize / y;
                           // }
                        }
                    }
                    //if (factors[z, 0] == 0) { factors[z, 0] = numberToFactorize; } //numberToFactorize is prime and is put on first place

                }
                Data.ValueCollection.Clear();
                double bigestheight = factorCounter[2];
                for (int z = 3; z <= factorCounter.Count()-1; z++)
                {
                    if (bigestheight < (double)factorCounter[z])
                    {
                        bigestheight = (double)factorCounter[z];
                    }
                }
                 
                for (int n = 2; n <= factorCounter.Count()-1; n++)
                {
                    
                    CollectionElement row = new CollectionElement(n, factorCounter[n], (factorCounter[n]*(180/bigestheight)) );
                    Data.ValueCollection.Add(row);
                }
                

                // OUTPUT
                StringOutput = "";
                for(int i=2; i<=factorCounter.Count()-1; i++)
                
                {
                    StringOutput +=i+":"+Convert.ToString(factorCounter[i])+Environment.NewLine;
                }
                presentation.OpenPresentationFile();

             //   for (int k = 0; k <= checkedGramms.Count - 1; k++)
             //   {
             //       if (Convert.ToInt32(distances[k] )!= 0)
             //       { StringOutput += checkedGramms[k] + "/" + Convert.ToString(distances[k]);
             //         for (int l = 0; l <= settings.factorSize /*sqrtOfLargestDist */ - 1; l++)
             //           {
             //               if (factors[k, l] != 0) { StringOutput += "/" + Convert.ToString(factors[k, l]); }
///
          //              }
          //              StringOutput += '\n';
          //          }
          //          
           //     }   
           //     
                //find the most frequent factor
              //  int biggestSoFar=0;
              //  int biggestSoFarIndex=0;
             //  
             //   for (int j = 0; j <= factorCounter.Length - 1; j++)
              //  {
              //      if (factorCounter[j] > biggestSoFar)
               //     {
                //        
                //        biggestSoFar = factorCounter[j];
                //        biggestSoFarIndex = j;
                //    }
               //     
               // }
               // if (biggestSoFarIndex % 2 == 0)
               // {
               //     StringOutput += '\n' + "The probable keylength is a multiple of: " + biggestSoFarIndex ;
               /// }
               // else
               // {
           //
              //      StringOutput += '\n' + "most frequent factor is:" + biggestSoFarIndex + '\n' + "with: " + biggestSoFar + " hits";
             //   }
             //   integerValue = biggestSoFarIndex;
            } 

        }

         //P.S. The arrays used in the solution are:
        //checkedGramms -the array contains all checked gramms :)  
        //distances - The same size as checkedGramms, if the gramm doesn't repeat itself the value of distances[k] corresponding to checkedGramms[k] equals 0
        //factors - same size as checkedGramms and distances on one side ,and 20 (for now)on the other side for the factors , if the corresponding distance is 0 all the factors are also 0
        
        
        
        
        public void PostExecution()
        {
            //throw new NotImplementedException();
        }

        public void Pause()
        {
            //throw new NotImplementedException();
        }

        public void Stop()
        {
           // throw new NotImplementedException();
        }

        public void Initialize()
        {
            //throw new NotImplementedException();
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion
    }
}
