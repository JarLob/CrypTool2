﻿/*
   Copyright CrypTool 2 Team <ct2contact@cryptool.org>

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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;

namespace Cryptool.Plugins.VIC
{
    [Author("Adam Petro", "adam.petro.655@gmail.com", "Institute of Computer Science and Mathematics, Faculty of Electrical Engineering and Information Technology, Slovak University of Technology", "https://uim.fei.stuba.sk")]
    [PluginInfo("Cryptool.Plugins.VIC.Properties.Resources", "ComponentName", "ComponentDescription", "VIC/userdoc.xml", new[] { "VIC/Images/VIC.png" })]
    [ComponentCategory(ComponentCategory.CiphersClassic)]



    public class VIC : ICrypComponent
    {
        #region Private Variables

        private readonly VICSettings settings = new VICSettings();



        private string ALPHABET;

        private string cyrillicAlphabet = "абвгдежзиклмнопрстуфхцчшщыьэюя".ToUpper();
        private string latinAlphabet = "abcdefghijklmnopqrstuvwxyz".ToUpper();



        private string lineC;
        private string lineD;
        private string lineE;
        private string lineF;
        private string lineG;
        private string lineH;
        private string lineJ;
        private string lineK;
        private string lineL;
        private string lineM;
        private string lineN;
        private string lineP;
        private string lineS;
        private string firstPermutation;
        private string secondPermutation;

        private int[] firstTransposition;
        private int[] secondTransposition;


        string substitutionResult;
        private string onceTransposedMessage;
        private string twiceTransposedMessage;

        private int firstTableWidth;
        private int secondTableWidth;

        private char[,] matrix;

        private string[,] substitutionTable;

        private string textStartSymbol;
        private string digitLetterSymbol;
        private string repeatSymbol;

        string onceDetransposedMessage;
        string twiceDetransposedMessage;



        enum AreaColor
        {
            white,
            grey
        };

        #endregion

        #region Data Properties


        [PropertyInfo(Direction.InputData, "DateCaption", "DateTooltip")]
        public string Date
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "PasswordCaption", "PasswordTooltip")]
        public string Password
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "PhraseCaption", "PhraseTooltip")]
        public string Phrase
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "NumberCaption", "NumberTooltip")]
        public string Number
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "InitializingStringCaption", "InitializingStringTooltip")]
        public string InitializingString
        {
            get;
            set;
        }
        [PropertyInfo(Direction.InputData, "TextInputCaption", "TextInputTooltip")]
        public string Input
        {
            get;
            set;
        }
        [PropertyInfo(Direction.OutputData, "OutputCaption", "OutputTooltip")]
        public string Output
        {
            get;
            set;
        }

        void ClearLocalVariables()
        {
            lineC = null;
            lineD = null;
            lineE = null;
            lineF = null;
            lineG = null;
            lineH = null;
            lineJ = null;
            lineK = null;
            lineL = null;
            lineM = null;
            lineN = null;
            lineP = null;
            lineS = null;
            firstPermutation = null;
            secondPermutation = null;
            firstTransposition = null;
            secondTransposition = null;
            onceTransposedMessage = null;
            twiceTransposedMessage = null;
            firstTableWidth = 0;
            secondTableWidth = 0;
            matrix = null;
            substitutionTable = null;
            textStartSymbol = null;
            digitLetterSymbol = null;
            repeatSymbol = null;
            substitutionResult = null;
            onceDetransposedMessage = null;
            twiceDetransposedMessage = null;

        }

        /// <summary>
        /// This function takes global input variables and formats them to desired format
        /// </summary>
        void FormatInput()
        {
            Date = Date.ToUpper();
            Date = (Regex.Replace(Date, "[^0-9]", ""));
            if (Date.Length < 6)
            {
                throw new InvalidInputException("ShortDateError");
            }


            if ((ActionType)settings.Action == ActionType.Decrypt)
            {
                int saltInsertionIndex = int.Parse(Date.ElementAt(Date.Length - 1).ToString());
                Input = RemoveSalt(Input, saltInsertionIndex);

            }

            Password = Password.ToUpper();
            
            Password = (Regex.Replace(Password, $"[^{ALPHABET}]", ""));
            
            if (Password.Length < 7)
            {
                throw new InvalidInputException("ShortPasswordError");
            }


            Phrase = Phrase.ToUpper();
            Phrase = (Regex.Replace(Phrase, "[^A-Z0-9A-Я]", ""));
            if (Phrase.Length < 20)
            {
                throw new InvalidInputException("ShortPhraseError");
            }

            Number = (Regex.Replace(Number, "[^0-9]", ""));
            if (int.Parse(Number) > 33 || int.Parse(Number) <= 0)
            {
                throw new InvalidInputException("InvalidNumberError");
            }

            InitializingString = InitializingString.ToUpper();
            InitializingString = (Regex.Replace(InitializingString, "[^0-9]", ""));
            if (InitializingString.Length < 5)
            {
                throw new InvalidInputException("ShortInitializingStringError");
            }
            InitializingString = InitializingString.Substring(0, 5);


            Input = Input.ToUpper();
            Input = (Regex.Replace(Input, @"\s+", ""));
            
        }


        /// <summary>
        /// This Functions formats output string into groups of 5 chars. It also inserts salt to the resulting string.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        string FormatOutput(string input)
        {
            input = (Regex.Replace(input, "[^0-9]", ""));
            int iterator = 0;
            for (int i = 0; i < input.Length - 5; ++i)
            {
                if (i % 5 == 0 && i > 0)
                {
                    if (!(i + iterator >= input.Length))
                    {
                        input = input.Insert(i + iterator++, " ");
                    }
                }
            }
            int saltInsertionIndex = int.Parse(Date.ElementAt(Date.Length - 1).ToString());
            if (saltInsertionIndex != 0)
            {
                input = InsertSalt(input, saltInsertionIndex, InitializingString);
            }
            return input;
        }





        /// <summary>
        /// Mod also working for negative numbers.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="m"></param>
        /// <returns></returns>
        private int Mod(int x, int m)
        {
            return (x % m + m) % m;
        }


        /// <summary>
        /// Enumerates string according to alphabetical order.
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private int[] EnumerateString(string input)
        {
            char[] inputAr = input.ToArray();

            char[] sorted = inputAr.ToArray();
            Array.Sort(sorted);
            int[] output = new int[input.Length];


            for (int i = 1; i <= sorted.Length; ++i)
            {
                int index = Array.IndexOf(inputAr, (sorted.ElementAt(i - 1)));
                inputAr.SetValue('\0', index);
                output.SetValue(Mod(i, 10), index);

            }
            return output;
        }

        /// <summary>
        /// Enumerates string of digits
        /// </summary>
        /// <param name="phrase"></param>
        /// <returns></returns>
        private int[] EnumeratePermutation(string phrase, bool modulate)
        {
            int[] phraseAr = new int[phrase.Length];

            for (int i = 0; i < phrase.Length; ++i)
            {
                if (phrase.ElementAt(i).Equals('0'))
                {
                    phraseAr.SetValue(10, i);
                }
                else
                {
                    phraseAr.SetValue(Int32.Parse(phrase.ElementAt(i).ToString()), i);
                }
            }

            int[] sorted = phraseAr.ToArray();
            Array.Sort(sorted);
            int[] output = new int[phrase.Length];


            for (int i = 1; i <= sorted.Length; ++i)
            {
                int index = Array.IndexOf(phraseAr, (sorted.ElementAt(i - 1)));
                phraseAr.SetValue('\0', index);
                if (modulate)
                {
                    output.SetValue(Mod(i, 10), index);
                }
                else
                {
                    output.SetValue(i, index);
                }

            }
            return output;
        }

        /// <summary>
        /// Adds two strings of numbers together number by number and modulates the result.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private string AddModulo(string a, string b)
        {
            
            string output = "";
            int aa, bb, cc;
            for (int i = 0; i < a.Length; ++i)
            {
                aa = (int)a.ElementAt(i) - '0';
                bb = (int)b.ElementAt(i) - '0';
                cc = aa + bb;
                cc = Mod(cc, 10);
                output += cc.ToString();
            }
            return output;
        }


        /// <summary>
        /// Substracts one string of numbers from another number by number and modulates the result.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private string SubstractModulo(string a, string b)
        {
            
            string output = "";
            int aa, bb, cc;
            for (int i = 0; i < a.Length; ++i)
            {
                aa = (int)a.ElementAt(i) - '0';
                bb = (int)b.ElementAt(i) - '0';
                cc = aa - bb;
                cc = Mod(cc, 10);
                output += cc.ToString();
            }
            return output;
        }

        /// <summary>
        /// Extends a string to 10 digits by chain adding
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private string ExtendToTenDigits(string input)
        {
            int firstAddend;
            int secondAddend;
            int stopCondition = 5;
            for (int i = 0; i < stopCondition; ++i)
            {
                firstAddend = (int)input.ElementAt(i) - '0';
                secondAddend = (int)input.ElementAt(i + 1) - '0';
                input += (Mod(firstAddend + secondAddend, 10)).ToString();
            }
            return input;
        }


        /// <summary>
        /// Extends string of 10 to string of 20 using chain addition.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private string ChainAddition(string input)
        {
            String output = "";
            int firstAddend, secondAddend;
            for (int i = 0; i < 9; ++i)
            {
                firstAddend = (int)input.ElementAt(i) - '0';
                secondAddend = (int)input.ElementAt(i + 1) - '0';
                output += ((Mod(firstAddend + secondAddend, 10)).ToString());
            }

            firstAddend = (int)input.Last() - '0';
            secondAddend = (int)output.ElementAt(0) - '0';
            output += ((Mod(firstAddend + secondAddend, 10)).ToString());

            return output;
        }


        /// <summary>
        /// Finds last two dissimilar digits in a string of numbers and returns them as items of tuple.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private Tuple<int, int> FindLastTwoDissimilarDigits(string input)
        {
            int firstDigit;
            int secondDigit;
            firstDigit = (int)input.Last() - '0';
            int index = input.Length - 2;
            secondDigit = (int)input.ElementAt(index) - '0';
            while (firstDigit == secondDigit)
            {
                secondDigit = (int)input.ElementAt(--index) - '0';
            }
            return new Tuple<int, int>(firstDigit, secondDigit);
        }

        /// <summary>
        /// Takes array of strings and converts it to matrix of chars, where each string is one row of matrix.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private char[,] ConvertToCharMatrix(string[] input)
        {
            char[,] output = new char[input.Length, input.ElementAt(0).Length];

            for (int i = 0; i < input.Length; ++i)
            {
                for (int j = 0; j < input.ElementAt(i).Length; ++j)
                {
                    output.SetValue(input.ElementAt(i).ElementAt(j), i, j);
                }
            }
            return output;
        }

        /// <summary>
        /// Find each letter of string a in string b and return their indices
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private int[] LocateEachLetterInAnotherString(string a, string b)
        {
            int[] output = new int[a.Length];
            for (int i = 0; i < a.Length; ++i)
            {
                output.SetValue(b.IndexOf(a.ElementAt(i)), i);
            }
            return output;
        }

        /// <summary>
        /// Reads elements from matrix by column.
        /// </summary>
        /// <param name="matrix"> Matrix to read from. </param>
        /// <param name="firstLength"> Width of the first permutation. This determines number of elements read from the matrix and stored in the first item of returning Tuple.</param>
        /// <param name="secondLength">Width of the second permutation. This determines number of elements read from the matrix and stored in the second item of returning Tuple.</param>
        /// <param name="indicationString">String indicating the order of columns reading from.</param>
        /// <returns></returns>
        private Tuple<String, String> ReadColsFromCharMatrix(char[,] matrix, int firstLength, int secondLength, string indicationString)
        {

            string firstPermutation = "";
            string secondPermutation = "";
            int row = 0; int col = 0;
            char[] sortedIndicationString = indicationString.ToArray();
            Array.Sort(sortedIndicationString);
            int iterator = 1;
            string output = "";
            while (output.Length < firstLength + secondLength)
            {
                row = 0;
                while (row < matrix.GetLength(0))
                {
                    col = indicationString.IndexOf(sortedIndicationString.ElementAt(iterator));
                    output += (matrix.GetValue(row++, col));
                }
                iterator++;
            }
            for (int i = 0; i < firstLength; ++i)
            {
                firstPermutation += (output.ElementAt(i));
            }

            for (int i = 0; i < secondLength; ++i)
            {
                secondPermutation += (output.ElementAt(i + firstLength));
            }
            return new Tuple<string, string>(firstPermutation, secondPermutation);
        }

        /// <summary>
        /// Injects static cyrillic letters to fixed positions in substitution table.
        /// </summary>
        /// <param name="substitutionTable"></param>
        /// <returns></returns>
        string[,] InjectCyrillicLetters(string[,] substitutionTable)
        {

            substitutionTable[2, 3] = ".";
            substitutionTable[3, 3] = ",";
            substitutionTable[4, 3] = "Л/П";

            substitutionTable[2, 5] = "№";
            substitutionTable[3, 5] = "Н/Ц";
            substitutionTable[4, 5] = "НТ";

            substitutionTable[4, 10] = "пвт";

            return substitutionTable;
        }

        /// <summary>
        /// Injects static latin letters to fixed positions in substitution table.
        /// </summary>
        /// <param name="substitutionTable"></param>
        /// <returns></returns>
        string[,] InjectLatinLetters(string[,] substitutionTable)
        {
            substitutionTable[2, 3] = ".";
            substitutionTable[3, 3] = ",";
            substitutionTable[4, 3] = "P/L";

            substitutionTable[2, 5] = "№";
            substitutionTable[3, 5] = "C/D";
            substitutionTable[4, 5] = "TS";

            substitutionTable[3, 9] = "!";
            substitutionTable[4, 9] = "?";

            substitutionTable[2, 10] = ":";
            substitutionTable[3, 10] = ";";
            substitutionTable[4, 10] = "RPT";

            return substitutionTable;
        }

        /// <summary>
        /// Constructs the substitution table.
        /// </summary>
        /// <param name="coordinates"> Permutation makes the x and y coordinates of the table.</param>
        /// <param name="password"> Password makes the first row of the table</param>
        /// <param name="alphabet"> What alphabet (Cyrillic, Latin) should the table be constructed of</param>
        /// <returns></returns>
        private string[,] ConstructSubstitutionTable(string coordinates, string password, string alphabet)
        {

            string[,] substitutionTable = new string[5, 11];
            for (int i = 0; i < 5; ++i)
            {
                for (int j = 0; j < 11; ++j)
                {
                    substitutionTable[i, j] = "*";
                }
            }

            //<First Line of Table>
            for (int i = 0; i < coordinates.Length; ++i)
            {
                substitutionTable[0, i + 1] = coordinates.ElementAt(i).ToString();
            }

            //</First Line of Table>

            //<First Col of Table>
            substitutionTable[1, 0] = "-";

            int iterator = 2;
            for (int i = coordinates.Length - 3; i < coordinates.Length; ++i)
            {
                substitutionTable[iterator++, 0] = coordinates.ElementAt(i).ToString();
            }
            //</First Col of Table>



            //<Second Line of Table>            
            char[] injectedLetters = new char[7];
            iterator = 0;
            
            for (int i = 1; i <= 7; ++i)
            {
                while (injectedLetters.Contains(password.ElementAt(iterator)))
                {
                    ++iterator;
                }
                injectedLetters[i - 1] = password.ElementAt(iterator);
                substitutionTable[1, i] = password.ElementAt(iterator++).ToString().ToUpper();
            }
            string temp = "";
            foreach (var element in injectedLetters)
            {
                temp += element;
            }
            
            //</Second Line of Table>





            //<Remove password letters from alphabet>
            for (int i = 0; i < alphabet.Length; ++i)
            {
                if (injectedLetters.Contains(alphabet.ElementAt(i)))
                {
                    

                    alphabet = alphabet.Remove(i, 1);
                    --i;
                }
                else
                {
                    
                }
            }
            
            //</Remove password letters from alphabet>






            //<Inject the rest of alphabet into the table>
            iterator = 0;
            for (int i = 1; i <= 10; ++i)
            {
                for (int j = 2; j <= 4; ++j)
                {
                    if (iterator >= alphabet.Length) break;
                    if (i != 3 && i != 5)
                    {
                        substitutionTable[j, i] = alphabet.ElementAt(iterator++).ToString().ToUpper();
                    }
                }
            }
            
            if (settings.Alphabet.Equals((int)AlphabetType.Cyrillic))
            {
                substitutionTable = InjectCyrillicLetters(substitutionTable);
            }
            else if (settings.Alphabet.Equals((int)AlphabetType.Latin))
            {
                substitutionTable = InjectLatinLetters(substitutionTable);
            }



            //</Inject the rest of alphabet into the table>

            return substitutionTable;
        }



        /// <summary>
        /// Prepends text start symbol to the input text.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private string PrependTextStartSymbol(string message)
        {
            message = message.Insert(0, textStartSymbol);
            return message;
        }

        /// <summary>
        /// Substitutes the input according to the substitution table.
        /// </summary>
        /// <param name="substitutionTable"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private string PerformSubstitution(string[,] substitutionTable, string message)
        {
            message = PrependTextStartSymbol(message);
            int iterationsToSkip = 0;
            string resultNumbers = "";
            string firstOutput = "";
            string secondOutput = "";

            //Generate splitting point for the string to be split into two parts
            Random random = new Random();
            int lowerBoundary = message.Length / 10;
            int upperBoundary = 9 * (message.Length / 10);
            int splittingPoint = random.Next(lowerBoundary, upperBoundary);

            for (int i = 0; i < message.Length; ++i)
            {
                if (iterationsToSkip > 0)
                {
                    iterationsToSkip--;
                    continue;
                }
                char character = message.ElementAt(i);
                if (Char.IsDigit(character))
                {
                    resultNumbers = (PerformDigitSubstitution(character, substitutionTable));
                }
                else if (!PerformSpecialCharSubstitution(message, i, ref resultNumbers, ref iterationsToSkip, substitutionTable))
                {
                    resultNumbers = LocateLetterInMatrix(substitutionTable, character);
                }
                if (i <= splittingPoint)
                {
                    firstOutput += resultNumbers;
                }
                else if (i > splittingPoint)
                {
                    secondOutput += resultNumbers;
                }
            }
            
            
            return secondOutput + firstOutput;


        }

        /// <summary>
        /// Decides what alphabet is used and performs substitution accordingly
        /// </summary>
        /// <param name="input"></param>
        /// <param name="index"></param>
        /// <param name="outputMessage"></param>
        /// <param name="iterationsToSkip"></param>
        /// <param name="substitutionTable"></param>
        /// <returns></returns>
        private bool PerformSpecialCharSubstitution(string input, int index, ref string outputMessage, ref int iterationsToSkip, string[,] substitutionTable)
        {
            if ((AlphabetType)settings.Alphabet == AlphabetType.Cyrillic)
            {
                return PerformSpecialCyrillicCharSubstitution(input, index, ref outputMessage, ref iterationsToSkip, substitutionTable);
            }
            else
            {
                return PerformSpecialLatinCharSubstitution(input, index, ref outputMessage, ref iterationsToSkip, substitutionTable);
            }
        }



        /// <summary>
        /// Performs special char substitution and returns true if a char was substituted.
        /// </summary>
        /// <param name="input"> Message being substituted </param>
        /// <param name="index"> Index of character in message </param>
        /// <param name="outputMessage"> Reference to string where the output is to be stored. </param>
        /// <param name="iterationsToSkip"> Reference to an int where the number of substituted chars is to be stored </param>
        /// <returns></returns>
        private bool PerformSpecialCyrillicCharSubstitution(string input, int index, ref string outputMessage, ref int iterationsToSkip, string[,] substitutionTable)
        {
            char character = input.ElementAt(index);

            if (index + 1 > input.Length - 1)
            {
                return false;
            }
            char nextCharacter = input.ElementAt(index + 1);

            if (character.Equals('Н'))
            {
                if (nextCharacter.Equals('/') && index + 2 < input.Length - 1)
                {
                    if (input.ElementAt(index + 2).Equals('Ц'))
                    {
                        outputMessage += LocateStringInMatrix(substitutionTable, "Н/Ц");
                        iterationsToSkip = 2;
                        return true;
                    }
                }
                else if (nextCharacter.Equals('Т'))
                {
                    outputMessage += LocateStringInMatrix(substitutionTable, "НТ");
                    iterationsToSkip = 1;
                    return true;
                }
            }
            else if (character.Equals('Л'))
            {
                if (nextCharacter.Equals('/') && index + 2 < input.Length - 1)
                {
                    if (input.ElementAt(index + 2).Equals('П'))
                    {
                        outputMessage += LocateStringInMatrix(substitutionTable, "Л/П");
                        iterationsToSkip = 1;
                        return true;
                    }
                }
            }
            return false;

        }

        /// <summary>
        /// Performs special char substitution and returns true if a char was substituted.
        /// </summary>
        /// <param name="message"> Message being substituted </param>
        /// <param name="index"> Index of character in message </param>
        /// <param name="outputMessage"> Reference to string where the output is to be stored. </param>
        /// <param name="iterationsToSkip"> Reference to an int where the number of substituted chars is to be stored </param>
        /// <returns></returns>
        private bool PerformSpecialLatinCharSubstitution(string message, int index, ref string outputMessage, ref int iterationsToSkip, string[,] substitutionTable)
        {
            char character = message.ElementAt(index);

            if (index + 1 > message.Length - 1)
            {
                return false;
            }
            char nextCharacter = message.ElementAt(index + 1);

            if (character.Equals('T'))
            {
                if (nextCharacter.Equals('/') && index + 2 < message.Length - 1)
                {
                    if (message.ElementAt(index + 2).Equals('N'))
                    {
                        outputMessage += LocateStringInMatrix(substitutionTable, "T/N");
                        iterationsToSkip = 2;
                        return true;
                    }
                }
                else if (nextCharacter.Equals('S'))
                {
                    outputMessage += LocateStringInMatrix(substitutionTable, "TS");
                    iterationsToSkip = 1;
                    return true;
                }
            }
            else if (character.Equals('P'))
            {
                if (nextCharacter.Equals('/') && index + 2 < message.Length - 1)
                {
                    if (message.ElementAt(index + 2).Equals('L'))
                    {
                        outputMessage += LocateStringInMatrix(substitutionTable, "P/L");
                        iterationsToSkip = 1;
                        return true;
                    }
                }
            }
            return false;

        }

        /// <summary>
        /// Splits message to two parts at random splitting point.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        string SplitMessageRandomly(string input)
        {
            
            Random random = new Random();
            int lowerBoundary = input.Length / 10;
            int upperBoundary = 9 * (input.Length / 10);
            int splittingPoint = random.Next(lowerBoundary, upperBoundary);

            string secondString = input.Substring(0, splittingPoint);
            string firstString = input.Substring(splittingPoint, input.Length - splittingPoint);
            

            return firstString + secondString;

        }

        /// <summary>
        /// Adds random single digit numbers at the end of a string until the length of string mod(5) is 0; 
        /// </summary>
        /// <param name="input"> input string to append the numbers to</param>
        /// <returns></returns>
        string AddZeros(string input)
        {
            
            if (input.Length % 5 == 0)
            {
                return input;
            }
            int nOfZeros = 5 - input.Length % 5;
            Random random = new Random();
            string output = input;
            for (int i = 0; i < nOfZeros; ++i)
            {
                char newNumber = random.Next(0, 9).ToString().ElementAt(0);
                
                output += newNumber;
            }

            
            return output;
        }

        /// <summary>
        /// Performs digit substitution
        /// </summary>
        /// <param name="digit"></param>
        /// <returns></returns>
        string PerformDigitSubstitution(char digit, string[,] substitutionTable)
        {
            
            
            return (LocateStringInMatrix(substitutionTable, digitLetterSymbol) + digit.ToString() + digit.ToString() + digit.ToString() + LocateStringInMatrix(substitutionTable, digitLetterSymbol));
        }

        /// <summary>
        /// Locates string in matrix and returns the coordinates as a string
        /// </summary>
        /// <param name="Matrix"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        string LocateStringInMatrix(string[,] Matrix, string input)
        {
            for (int i = 1; i <= 4; ++i)
            {
                for (int j = 1; j <= 10; ++j)
                {
                    if (Matrix.GetValue(i, j).ToString().ToUpper().Equals(input.ToUpper()))
                    {
                        if (Matrix.GetValue(i, 0).Equals("-"))
                        {
                            return Matrix.GetValue(0, j).ToString();
                        }
                        else
                        {
                            return Matrix.GetValue(i, 0).ToString() + Matrix.GetValue(0, j).ToString();
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Locates letter in matrix and returns the coordinates as a string
        /// </summary>
        /// <param name="Matrix"></param>
        /// <param name="letter"></param>
        /// <returns></returns>
        string LocateLetterInMatrix(string[,] Matrix, char letter)
        {
            for (int i = 1; i <= 4; ++i)
            {
                for (int j = 1; j <= 10; ++j)
                {
                    if (Matrix.GetValue(i, j).ToString().ToUpper().Equals(letter.ToString().ToUpper()))
                    {
                        if (Matrix.GetValue(i, 0).Equals("-"))
                        {
                            return Matrix.GetValue(0, j).ToString();
                        }
                        else
                        {
                            return Matrix.GetValue(i, 0).ToString() + Matrix.GetValue(0, j).ToString();
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Performs the first of 2 transpositions of vic cipher. This is a classic columnar transposition.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="permutation"> Order of columns.</param>
        /// <returns></returns>
        private string PerformFirstTransposition(string message, int[] permutation)
        {
            int iterator = 0;
            int transpositionMatrixHeight = (int)(Math.Ceiling((double)message.Length / permutation.Length));
            char[,] transpositionMatrix = new char[transpositionMatrixHeight, permutation.Length];
            for (int i = 0; i < transpositionMatrixHeight; ++i)
            {
                for (int j = 0; j < permutation.Length; ++j)
                {

                    if (iterator < message.Length)
                    {
                        transpositionMatrix.SetValue(message.ElementAt(iterator++), i, j);
                    }
                    else
                    {
                        transpositionMatrix.SetValue('*', i, j);
                    }
                }
            }
            return Transpose(permutation, transpositionMatrix);
        }

        /// <summary>
        /// Transposes columns of matrix according to a permutation.
        /// </summary>
        /// <param name="permutation"></param>
        /// <param name="transpositionMatrix"></param>
        /// <returns></returns>
        private string Transpose(int[] permutation, char[,] transpositionMatrix)
        {
            string output = "";
            int column = 0;
            for (int i = 0; i < transpositionMatrix.GetLength(1); ++i)
            {
                column = Array.IndexOf(permutation, i + 1);
                for (int j = 0; j < transpositionMatrix.GetLength(0); ++j)
                {
                    if (!transpositionMatrix.GetValue(j, column).Equals('*'))
                    {
                        output += transpositionMatrix.GetValue(j, column);
                    }
                }
            }
            output = output.Replace("\0", "");
            return output;

        }

        /// <summary>
        /// Finds index of string element in an integer array. 
        /// </summary>
        /// <param name="element"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        int IndexOf(string element, int[] input)
        {
            for (int i = 0; i < input.Length; ++i)
            {
                if (input.ElementAt(i).Equals(element.ToString()))
                    return i;
            }
            return -1;
        }


        /// <summary>
        /// Performs second transposition of VIC cipher, this is a special graphical transposition.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="permutation"></param>
        /// <returns></returns>
        string PerformSecondTransposition(string message, int[] permutation)
        {
            AreaColor[,] secondTranspositionTableColors = ConstructSecondTranspositionTableColors(permutation, message.Length);
            char[,] secondTranspositionTable = FillSecondTranspositionTable(message, secondTranspositionTableColors);
            return Transpose(permutation, secondTranspositionTable);

        }


        /// <summary>
        /// Fills The second transposition table according to the color of areas.
        /// </summary>
        /// <param name="message">Message to transpose.</param>
        /// <param name="secondTranspositionTableColors"> Matrix with the same dimensions as the transposition matrix indicating the colors of the areas of transposition matrix.</param>
        /// <returns></returns>
        char[,] FillSecondTranspositionTable(string message, AreaColor[,] secondTranspositionTableColors)
        {
            int iterator = 0;
            char[,] secondTranspositionTable = new char[secondTranspositionTableColors.GetLength(0), secondTranspositionTableColors.GetLength(1)];

            //Fill out white parts
            for (int i = 0; i < secondTranspositionTable.GetLength(0); ++i)
            {
                for (int j = 0; j < secondTranspositionTable.GetLength(1); ++j)
                {
                    if (secondTranspositionTableColors.GetValue(i, j).Equals(AreaColor.white))
                    {
                        secondTranspositionTable.SetValue(message.ElementAt(iterator), i, j);
                        iterator++;
                    }
                }
            }

            //Fill out grey parts
            for (int i = 0; i < secondTranspositionTable.GetLength(0); ++i)
            {
                for (int j = 0; j < secondTranspositionTable.GetLength(1); ++j)
                {
                    if (secondTranspositionTableColors.GetValue(i, j).Equals(AreaColor.grey))
                    {
                        secondTranspositionTable.SetValue(message.ElementAt(iterator), i, j);
                        iterator++;
                    }
                    if (iterator >= message.Length)
                    {
                        break;
                    }
                }
            }

            string logMessage = "";
            for (int i = 0; i < secondTranspositionTable.GetLength(0); i++)
            {
                for (int j = 0; j < secondTranspositionTable.GetLength(1); j++)
                {
                    logMessage += secondTranspositionTable.GetValue(i, j) + " ";
                }
                logMessage += "\n";
            }
            

            return secondTranspositionTable;
        }

        /// <summary>
        /// Constructs matrix with the same dimensions as the transposition matrix indicating the color of the areas.
        /// </summary>
        /// <param name="permutation"></param>
        /// <param name="messageLength"></param>
        /// <returns></returns>
        private AreaColor[,] ConstructSecondTranspositionTableColors(int[] permutation, int messageLength)
        {
            int tableWidth = permutation.Length;
            int tableHeight = (int)(Math.Ceiling((double)messageLength / tableWidth));


            AreaColor[,] secondTranspositionTableColors = new AreaColor[tableHeight, tableWidth];

            bool greyFlag = false;
            bool entireWhiteLine = false;
            bool greyInProgress = false;
            int greyFieldStarter = 1;
            for (int i = 0; i < tableHeight; ++i)
            {
                greyFlag = false;
                for (int j = 0; j < tableWidth; ++j)
                {
                    if (!entireWhiteLine)
                    {
                        if (i > 0 && j > 0)
                        {
                            if (secondTranspositionTableColors.GetValue(i - 1, j - 1).Equals(AreaColor.grey))
                            {
                                greyFlag = true;
                                greyInProgress = true;
                            }
                        }
                        if ((permutation.ElementAt(j).Equals(greyFieldStarter) && !greyInProgress))
                        {
                            greyFlag = true;
                            greyInProgress = true;
                            greyFieldStarter++;
                        }
                        if (greyFlag)
                        {
                            secondTranspositionTableColors.SetValue(AreaColor.grey, i, j);
                        }
                        else if (!greyFlag)
                        {
                            secondTranspositionTableColors.SetValue(AreaColor.white, i, j);
                        }
                    }
                }
                entireWhiteLine = false;
                if ((secondTranspositionTableColors.GetValue(i, tableWidth - 1).Equals(AreaColor.grey)) && (secondTranspositionTableColors.GetValue(i, tableWidth - 2).Equals(AreaColor.white)))
                {
                    entireWhiteLine = true;
                    greyInProgress = false;
                }

            }
            string temp = "";
            for (int l = 0; l < secondTranspositionTableColors.GetLength(0); ++l)
            {
                temp += "\n";
                for (int k = 0; k < secondTranspositionTableColors.GetLength(1); ++k)
                {
                    if (secondTranspositionTableColors.GetValue(l, k).Equals(AreaColor.grey))
                    {
                        temp += "grey  ";
                    }
                    else if (secondTranspositionTableColors.GetValue(l, k).Equals(AreaColor.white))
                    {
                        temp += "white ";
                    }

                }

            }
            
            return secondTranspositionTableColors;
        }

        /// <summary>
        /// Performs the opposite operation to substitution with the matrix provided.
        /// </summary>
        /// <param name="message">Message to desubstitute</param>
        /// <param name="matrix">Substitution Matrix</param>
        /// <returns></returns>
        private string Desubstitute(string message, string[,] matrix)
        {
            int iterationsToSkip = 0;
            char currentChar;
            char nextChar;
            string desubResult = "";
            string output = "";

            for (int i = 0; i < message.Length; ++i)
            {
                if (iterationsToSkip == 0)
                {
                    currentChar = message.ElementAt(i);
                    if (i < message.Length - 1)
                    {
                        nextChar = message.ElementAt(i + 1);
                        desubResult = DesubstituteMultipleChars(currentChar.ToString() + nextChar.ToString(), matrix);
                        if (desubResult.Length == 0)
                        {
                            output += DesubstituteSingleChar(currentChar.ToString(), matrix);
                            
                        }
                        else if (desubResult.Equals(digitLetterSymbol))
                        {
                            
                            output += message.ElementAt(i + 2);
                            iterationsToSkip = 6;
                        }
                        else
                        {
                            
                            output += desubResult;
                            iterationsToSkip = 1;
                        }

                    }
                    else
                    {
                        output += DesubstituteSingleChar(currentChar.ToString(), matrix);
                        
                    }
                }
                else
                {
                    iterationsToSkip--;
                }
            }
            return output;
        }

        /// <summary>
        /// Desubstitutes multiple characters, such as RPT, that are common in VIC Cipher
        /// </summary>
        /// <param name="input"></param>
        /// <param name="matrix"></param>
        /// <returns></returns>
        private string DesubstituteMultipleChars(string input, string[,] matrix)
        {
            int row = -1;
            int col = -1;


            for (int i = 0; i < matrix.GetLength(0); ++i)
            {
                if (matrix.GetValue(i, 0).ToString().Equals(input.ElementAt(0).ToString()))
                {
                    row = i;
                }
            }

            for (int i = 0; i < matrix.GetLength(1); ++i)
            {
                if (matrix.GetValue(0, i).ToString().Equals(input.ElementAt(1).ToString()))
                {
                    col = i;
                }
            }

            if (row.Equals(-1) || col.Equals(-1))
            {
                return "";
            }
            else
            {
                return matrix.GetValue(row, col).ToString();
            }

        }

        /// <summary>
        /// Desubstitutes just single characters
        /// </summary>
        /// <param name="input"></param>
        /// <param name="matrix"></param>
        /// <returns></returns>
        private string DesubstituteSingleChar(string input, string[,] matrix)
        {
            int row = -1;
            int col = -1;
            for (int i = 0; i < matrix.GetLength(1); ++i)
            {
                if (matrix.GetValue(0, i).ToString().Equals(input.ElementAt(0).ToString()))
                {
                    col = i;
                }
            }

            if (row.Equals(-1) && !col.Equals(-1))
            {
                return matrix.GetValue(1, col).ToString();
            }
            else
            {
                return "";
            }
        }


        /// <summary>
        /// Makes the opposite operation to first transpostition, so that it writes to the table by col and reads by row.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="transposition"></param>
        /// <returns></returns>
        private string DeTransposeFirstTransposition(string message, int[] transposition)
        {
            string output = "";
            int transpositionMatrixHeight = (int)(Math.Ceiling((double)message.Length / transposition.Length));


            char[,] transpositionMatrix = new char[transpositionMatrixHeight, transposition.Length];


            transpositionMatrix = FillFirstTranspositionTableByCol(transpositionMatrix, transposition, message);
            string temp = "\n";
            for (int i = 0; i < transpositionMatrix.GetLength(0); ++i)
            {
                for (int j = 0; j < transpositionMatrix.GetLength(1); ++j)
                {
                    temp += (transpositionMatrix.GetValue(i, j));
                }
                temp += "\n";
            }
            
            //read the rest of the table by rows;
            for (int i = 0; i < transpositionMatrix.GetLength(0); ++i)
            {
                for (int j = 0; j < transpositionMatrix.GetLength(1); ++j)
                {
                    output += transpositionMatrix.GetValue(i, j);
                }
            }

            return output.Replace("*", " ");

        }


        /// <summary>
        /// Fills transpostion table by column. This is a help operation for the function DeTransposeFirstTransposition
        /// </summary>
        /// <param name="transpositionMatrix"></param>
        /// <param name="transposition"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private char[,] FillFirstTranspositionTableByCol(char[,] transpositionMatrix, int[] transposition, string message)
        {


            int numberOfEmptyCells = (transpositionMatrix.GetLength(0) * transpositionMatrix.GetLength(1)) % message.Length;
            //Tag the emptycells
            for (int i = numberOfEmptyCells; i > 0; --i)
            {
                transpositionMatrix.SetValue('*', transpositionMatrix.GetLength(0) - 1, transpositionMatrix.GetLength(1) - i);
            }

            //fill the rest of the table by cols
            int column = 0;
            int iterator = 0;
            for (int i = 0; i < transposition.Length; ++i)
            {
                column = Array.IndexOf(transposition, i + 1);
                
                for (int j = 0; j < transpositionMatrix.GetLength(0); ++j)
                {
                    if (iterator == message.Length)
                    {
                        break;
                    }
                    if (!transpositionMatrix.GetValue(j, column).Equals('*'))
                    {
                        transpositionMatrix.SetValue(message.ElementAt(iterator++), j, column);
                    }
                }
            }
            return transpositionMatrix;
        }

        /// <summary>
        /// Detransposes the second transposition.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="permutation"></param>
        /// <returns></returns>
        private string DeTransposeSecondTransposition(string message, int[] permutation)
        {
            AreaColor[,] secondTranspositionTableColors = ConstructSecondTranspositionTableColors(permutation, message.Length);

            char[,] secondTranspositionTable = new char[secondTranspositionTableColors.GetLength(0), secondTranspositionTableColors.GetLength(1)];
            secondTranspositionTable = FillSecondTranspositionTableByCol(secondTranspositionTable, permutation, message, secondTranspositionTableColors);
            string output = "";

            //read by rows, white first
            for (int i = 0; i < secondTranspositionTable.GetLength(0); ++i)
            {
                for (int j = 0; j < secondTranspositionTable.GetLength(1); ++j)
                {
                    if (secondTranspositionTableColors.GetValue(i, j).Equals(AreaColor.white))
                    {
                        output += secondTranspositionTable.GetValue(i, j);
                    }
                }
            }

            for (int i = 0; i < secondTranspositionTable.GetLength(0); ++i)
            {
                for (int j = 0; j < secondTranspositionTable.GetLength(1); ++j)
                {
                    if (secondTranspositionTableColors.GetValue(i, j).Equals(AreaColor.grey))
                    {
                        output += secondTranspositionTable.GetValue(i, j);
                    }
                }
            }

            // output = output.Substring(0, output.IndexOf('*'));
            output = output.Replace("*", "");


            return output;
        }

        /// <summary>
        /// Fills second transposition table by column.
        /// </summary>
        /// <param name="transpositionMatrix"></param>
        /// <param name="transposition"></param>
        /// <param name="message"></param>
        /// <param name="secondTranspositionTableColors"></param>
        /// <returns></returns>
        static char[,] FillSecondTranspositionTableByCol(char[,] transpositionMatrix, int[] transposition, string message, AreaColor[,] secondTranspositionTableColors)
        {
            int numberOfEmptyCells = (transpositionMatrix.GetLength(0) * transpositionMatrix.GetLength(1)) % message.Length;
            //Tag the emptycells
            for (int i = transpositionMatrix.GetLength(0) - 1; i > 0; --i)
            {
                for (int j = transpositionMatrix.GetLength(1) - 1; j > 0; --j)
                {
                    if (numberOfEmptyCells == 0)
                    {
                        break;
                    }
                    if (secondTranspositionTableColors.GetValue(i, j).Equals(AreaColor.grey))
                    {
                        transpositionMatrix.SetValue('*', i, j);
                        numberOfEmptyCells--;
                    }
                }
            }

            //fill the rest of the table by cols
            int column = 0;
            int iterator = 0;
            for (int i = 0; i < transposition.Length; ++i)
            {
                column = Array.IndexOf(transposition, i + 1);
                for (int j = 0; j < transpositionMatrix.GetLength(0); ++j)
                {
                    if (iterator == message.Length)
                    {
                        break;
                    }
                    if (!transpositionMatrix.GetValue(j, column).Equals('*'))
                    {
                        transpositionMatrix.SetValue(message.ElementAt(iterator++), j, column);
                    }
                }
            }
            return transpositionMatrix;
        }

        string DetermineTextStart(string input)
        {
            string output;
            if (input.Contains(textStartSymbol))
            {
                string[] stringSeparators = new string[] { textStartSymbol };
                string[] splitStrings = input.Split(stringSeparators, StringSplitOptions.None);
                output = splitStrings.ElementAt(1) + splitStrings.ElementAt(0);
            }
            else
            {
                output = input;
            }
            return output;
        }

        /// <summary>
        /// Inserts initializing string to a certain position in the output string.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="pos"></param>
        /// <param name="salt"></param>
        /// <returns></returns>
        string InsertSalt(string input, int pos, string salt)
        {
            string[] inputAr = input.Split(' ');
            ArrayList inputAl = new ArrayList(inputAr);
            int index = inputAl.Count - pos;
            inputAl.Insert(index, salt);
            return String.Join(" ", inputAl.ToArray());
        }

        /// <summary>
        /// Removes initializing string from a certain position in the input string.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        string RemoveSalt(string input, int pos)
        {
            string[] inputAr = input.Split(' ');
            ArrayList inputAl = new ArrayList(inputAr);
            int index = inputAl.Count - (pos + 1);
            inputAl.RemoveAt(index);
            return String.Join(" ", inputAl.ToArray());
        }

        #endregion

        #region IPlugin Members

        /// <summary>
        /// Provide plugin-related parameters (per instance) or return null.
        /// </summary>
        public ISettings Settings
        {
            get { return settings; }
        }

        /// <summary>
        /// Provide custom presentation to visualize the execution or return null.
        /// </summary>
        public UserControl Presentation
        {
            get { return null; }
        }

        /// <summary>
        /// Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {
            ClearLocalVariables();
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            ProgressChanged(0, 1);
            if ((AlphabetType)settings.Alphabet == AlphabetType.Cyrillic)
            {
                ALPHABET = cyrillicAlphabet;
                textStartSymbol = "НТ";
                digitLetterSymbol = "Н/Ц";
                repeatSymbol = "Л/П";
            }
            else if ((AlphabetType)settings.Alphabet == AlphabetType.Latin)
            {
                ALPHABET = latinAlphabet;
                textStartSymbol = "TS";
                digitLetterSymbol = "C/D";
                repeatSymbol = "RPT";
            }
            try
            {
                FormatInput();
                //1. Take first five digits from date and substract them from the random number
                lineC = SubstractModulo(InitializingString, Date);
                
                ProgressChanged(1, 16);
                //2.Extend to ten digits by adding together pairs
                lineC = ExtendToTenDigits(lineC);
                
                ProgressChanged(2, 16);


                //3.Append '1234567890'
                lineF = lineC + "1234567890";

                
                ProgressChanged(3, 16);


                //4.Take first 20 letters of passphrase
                lineD = Phrase.Substring(0, 20);

                
                ProgressChanged(4, 16);


                //5.Enumerate each 10 letters
                lineE = string.Join("", EnumerateString(lineD.Substring(0, 10)));
                lineE += string.Join("", EnumerateString(lineD.Substring(10, 10)));

                
                ProgressChanged(5, 16);


                //6.Add first 10 letters together with line F
                lineG = AddModulo(lineE.Substring(0, 10), lineF.Substring(0, 10));

                
                ProgressChanged(6, 16);


                //7. Find each letter of line G in line F and replace it by the letter on the same index in line E
                foreach (var item in LocateEachLetterInAnotherString(lineG, lineF.Substring(10, 10)))
                {
                    lineH += (lineE.Substring(10, 10)[item]);
                }
                ProgressChanged(7, 16);

                

                //8.Enumerate it and obtain line J.
                lineJ = string.Join("", EnumerateString(lineH));

                
                ProgressChanged(8, 16);

                //9. chain addition to obtain lines K,L,M,N,P
                lineK = ChainAddition(lineH);
                lineL = ChainAddition(lineK);
                lineM = ChainAddition(lineL);
                lineN = ChainAddition(lineM);
                lineP = ChainAddition(lineN);

                
                
                
                
                
                ProgressChanged(9, 16);

                //10. Get the first and second transposition table width
                firstTableWidth = FindLastTwoDissimilarDigits(lineP).Item2 + int.Parse(Number);

                secondTableWidth = FindLastTwoDissimilarDigits(lineP).Item1 + int.Parse(Number);

                
                

                ProgressChanged(10, 16);

                //11.Form a matrix from the lines K-P
                matrix = ConvertToCharMatrix(new string[] { lineK, lineL, lineM, lineN, lineP });
                
                ProgressChanged(11, 16);

                //12. Get the first and second transposition.
                firstPermutation = ReadColsFromCharMatrix(matrix, firstTableWidth, secondTableWidth, lineJ).Item1;

                
                ProgressChanged(12, 16);

                secondPermutation = ReadColsFromCharMatrix(matrix, firstTableWidth, secondTableWidth, lineJ).Item2;

                


                //13. Enumerate line P to obtain line s
                lineS = string.Join("", EnumeratePermutation(lineP, true));

                





                firstTransposition = EnumeratePermutation(firstPermutation, false);
                string temp = "";
                foreach (var element in firstTransposition)
                {
                    temp += " & " + element;
                }
                


                secondTransposition = EnumeratePermutation(secondPermutation, false);
                temp = "";
                foreach (var element in secondTransposition)
                {
                    temp += " & " + element;
                }
                


                ProgressChanged(13, 16);


                if ((ActionType)settings.Action == ActionType.Encrypt)
                {

                    //14. Perform the first substitution
                    substitutionTable = ConstructSubstitutionTable(lineS, Password, ALPHABET);

                    substitutionResult = (PerformSubstitution(substitutionTable, Input));
                    substitutionResult = AddZeros(substitutionResult);

                    ProgressChanged(14, 16);

                    // 15. Perform the first transposition
                    onceTransposedMessage = PerformFirstTransposition(substitutionResult, firstTransposition);

                    


                    
                    ProgressChanged(15, 16);

                    //16.Perform the first transposition
                    twiceTransposedMessage = PerformSecondTransposition(onceTransposedMessage, secondTransposition);
                    



                    Output = FormatOutput(twiceTransposedMessage);
                    
                    OnPropertyChanged("Output");
                }
                else if ((ActionType)settings.Action == ActionType.Decrypt)
                {
                    // 14. Detranspose second transposition
                    onceDetransposedMessage = DeTransposeSecondTransposition(Input, secondTransposition);

                    

                    ProgressChanged(15, 16);

                    // 15. Detranspose first transposition
                    temp = "";
                    foreach (var element in firstTransposition)
                    {
                        temp += element + ",";
                    }
                    
                    twiceDetransposedMessage = DeTransposeFirstTransposition(onceDetransposedMessage, firstTransposition);

                    


                    // 16. Desubstitute substitution
                    substitutionTable = ConstructSubstitutionTable(lineS, Password, ALPHABET);
                    string logMessage = "";
                    
                    Output = Desubstitute(twiceDetransposedMessage, substitutionTable);
                    Output = DetermineTextStart(Output);
                    


                    OnPropertyChanged("Output");
                }
                ProgressChanged(16, 16);


            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format(ex.Message + "\n" + ex.StackTrace), NotificationLevel.Error);
            }

            ProgressChanged(1, 1);
        }

        /// <summary>
        /// Called once after workflow execution has stopped.
        /// </summary>
        public void PostExecution()
        {
        }

        /// <summary>
        /// Triggered time when user clicks stop button.
        /// Shall abort long-running execution.
        /// </summary>
        public void Stop()
        {
        }

        /// <summary>
        /// Called once when plugin is loaded into editor workspace.
        /// </summary>
        public void Initialize()
        {
        }

        /// <summary>
        /// Called once when plugin is removed from editor workspace.
        /// </summary>
        public void Dispose()
        {
        }

        #endregion

        #region Event Handling

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        #endregion
    }

    class InvalidInputException : Exception
    {
        public InvalidInputException()
        {
        }
        public InvalidInputException(string message) : base(message)
        {
        }
        public InvalidInputException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
