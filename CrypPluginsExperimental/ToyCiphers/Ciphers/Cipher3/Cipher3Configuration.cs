﻿/*
   Copyright 2019 Christian Bender christian1.bender@student.uni-siegen.de

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

namespace ToyCiphers.Ciphers.Cipher3
{
    public static class Cipher3Configuration
    {
        public static readonly int BITWIDTHCIPHERFOUR = 4;
        public static readonly int SBOXNUM = 4;
        public static readonly int KEYNUM = 6;
        public static readonly int[] SBOX = { 6, 4, 12, 5, 0, 7, 2, 14, 1, 15, 3, 13, 8, 10, 9, 11 };
        public static readonly int[] PBOX = { 0, 4, 8, 12, 1, 5, 9, 13, 2, 6, 10, 14, 3, 7, 11, 15 };
    }
}
