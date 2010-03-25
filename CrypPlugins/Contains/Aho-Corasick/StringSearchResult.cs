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
using System.Runtime.InteropServices;

namespace Contains.Aho_Corasick
{
  [StructLayout(LayoutKind.Sequential)]
  public struct StringSearchResult
  {
    private int _index;
    private string _keyword;
    public StringSearchResult(int index, string keyword)
    {
      this._index = index;
      this._keyword = keyword;
    }

    public int Index
    {
      get
      {
        return this._index;
      }
    }
    public string Keyword
    {
      get
      {
        return this._keyword;
      }
    }
    public static StringSearchResult Empty
    {
      get
      {
        return new StringSearchResult(-1, "");
      }
    }
  }
}
