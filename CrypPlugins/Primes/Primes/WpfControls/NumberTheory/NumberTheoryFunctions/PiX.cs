/*
   Copyright 2008 Timo Eckhardt, University of Siegen

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
using System.Text;
using System.Windows.Controls;
using System.Threading;
using System.Windows;
using System.Diagnostics;
using System.Numerics;
using Primes.WpfControls.Components;
using Primes.Bignum;
using Primes.Library;
using Primes.Library.Function;
using Cryptool.PluginBase.Miscellaneous;

namespace Primes.WpfControls.NumberTheory.NumberTheoryFunctions
{
    public class PiX : BaseNTFunction
    {
        object lockobj = new object();
        public PiX() : base() { }

        #region Calculating

        protected override void DoExecute()
        {
            FireOnStart();

            FunctionPiX pix = new FunctionPiX();

            for (BigInteger x = m_From; x <= m_To; x++)
                FireOnMessage(this, x, ((int)pix.Execute((double)x)).ToString());

            FireOnStop();
        }

        public override string Description
        {
            get
            {
                return m_ResourceManager.GetString(BaseNTFunction.pix);
            }
        }

        #endregion
    }
}