﻿/*                              
   Copyright 2011 Nils Kopal

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
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Cryptool.PluginBase.IO;

namespace WorkspaceManagerModel.Model.Tools
{
    /// <summary>
    /// This class contains helper methods for the view. Conversion from model data to view and so on
    /// </summary>
    public static class ViewHelper
    {
        /// <summary>
        /// This number is the maximum number of values of an array which should
        /// be used for the creation of the PresentationString
        /// </summary>
        private const int MaxUsedArrayValues = 200;

        /// <summary>
        /// This number is the number of characters after a linebreak is put into data representation string
        /// </summary>
        private const int MaximumCharactersToShow = 450;


        /// <summary>
        /// This number is the number of characters after a linebreak is put into data representation string
        /// </summary>
        private const int LineBreakCharacterAmount = 76;


        /// <summary>
        /// Converts a given model data value into a valid view string which
        /// can be shown to the user. Returns "null" if there was no valid data
        /// or a given array was empty
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string GetDataPresentationString(object data)
        {
            try
            {
                var str = ConvertDataPresentation(data);
                if (str.Length > MaximumCharactersToShow)
                {
                    str = str.Substring(0, MaximumCharactersToShow) + "...";
                }

                if (str.Length > LineBreakCharacterAmount)
                {
                    var output = new StringBuilder();
                    var lastBreak = 0;      //counts the number of chars the last linebreak occured
                    for (var index = 0; index < str.Length; index++)
                    {
                        //remove spaces at the beginning of a new line
                        if (!(str[index] == ' ' && lastBreak == 0))
                        {
                            output.Append(str[index]);                            
                            //we found a line break so we memorize that
                            if (str[index] == '\r' || str[index] == '\n')
                            {
                                lastBreak = 0;
                            }
                            else
                            {
                                lastBreak++;
                            }
                        }

                        //we only make a break at spaces 
                        if (index > 0 && lastBreak >= LineBreakCharacterAmount && str[index] == ' ')
                        {
                            output.Remove(output.Length-1, 1);
                            output.Append("\r\n");
                            lastBreak = 0;
                        } 
                        //or if lastBreak >= (LineBreakCharacterAmount + 5%)
                        else if (index > 0 && lastBreak >= LineBreakCharacterAmount * 1.05)
                        {
                            output.Append("\r\n");
                            lastBreak = 0;
                        } 
                    }
                    return output.ToString().Trim();
                }
                return str.Trim();
            }
            catch(Exception ex)
            {
                return string.Format("Exception during creation of data representation: {0}",ex.Message);
            }
        }

        private static string ConvertDataPresentation(object data)
        {
            if (data == null)
            {
                return "null";
            }

            if (data is string)
            {
                return data.ToString();
            }

            var value = data as byte[];
            if (value != null)
            {
                return BitConverter.ToString(value).Replace("-", " ");
            }

            var stream = data as ICryptoolStream;
            if (stream != null)
            {
                var reader = stream.CreateReader();
                if (reader.Length > 0)
                {
                    var buffer = new byte[reader.Length < MaximumCharactersToShow ? reader.Length : MaximumCharactersToShow];
                    reader.Read(buffer, 0, buffer.Length);
                    return ConvertDataPresentation(buffer);
                }
                return "null";
            }

            var array = data as Array;
            if (array != null)
            {
                switch (array.Length)
                {
                    case 0:
                        return "null";
                    case 1:
                        return array.GetValue(0).ToString();
                    default:
                        var str = "" + array.GetValue(0) + ",";
                        var counter = 0;
                        for (var i = 1; i < array.Length - 1; i++)
                        {
                            str += (array.GetValue(i) + ",");
                            counter++;
                            if (counter == MaxUsedArrayValues)
                            {
                                return str;
                            }
                        }
                        str += array.GetValue(array.Length - 1);
                        return str;
                }
            }

            var enumerable = data as System.Collections.IEnumerable;
            if (enumerable != null)
            {
                List<string> l = new List<string>();

                foreach (var obj in enumerable)
                {
                    if (l.Count >= MaxUsedArrayValues) break;
                    l.Add(obj == null ? "null" : obj.ToString());
                }

                return (l.Count == 0) ? "null" : String.Join(",", l);
            }

            return data.ToString();
        }
    }
}
