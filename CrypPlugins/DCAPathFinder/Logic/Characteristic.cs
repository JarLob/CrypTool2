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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCAPathFinder.Logic
{
    public abstract class Characteristic : ICloneable
    {
        public UInt16[] InputDifferentials;
        public UInt16[] OutputDifferentials;
        public double Probability = -1;

        /// <summary>
        /// to be implemented in subclasses
        /// </summary>
        /// <returns></returns>
        public abstract object Clone();

        /// <summary>
        /// to be implemented in subclasses
        /// </summary>
        /// <returns></returns>
        public abstract string ToString();
    }
}
