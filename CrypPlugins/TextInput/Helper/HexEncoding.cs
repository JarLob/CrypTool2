/*
   Copyright 2008 Thomas Schmid, University of Siegen

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
using System.Text;

namespace Cryptool.TextInput.Helper
{
  static class HexStringConverter
  {

    public static byte[] ToByteArray(String HexString)
    {

      int NumberChars = HexString.Length;

      byte[] bytes = new byte[NumberChars / 2];
      for (int i = 0; i < NumberChars; i += 2)
      {
        bytes[i / 2] = Convert.ToByte(HexString.Substring(i, 2), 16);
      }
      return bytes;

    }

  }

}
