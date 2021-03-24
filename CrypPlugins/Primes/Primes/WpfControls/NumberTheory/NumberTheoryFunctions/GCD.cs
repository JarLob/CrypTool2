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

using System.Numerics;
using CrypTool.PluginBase.Miscellaneous;

namespace Primes.WpfControls.NumberTheory.NumberTheoryFunctions
{
    public class GCD : BaseNTFunction
    {
        object lockobj = new object();
        public GCD() : base() { }

        #region Calculating

        protected override void DoExecute()
        {
            FireOnStart();

            for (BigInteger x = m_From; x <= m_To; x++)
                FireOnMessage(this, x, x.GCD(m_SecondParameter).ToString());

            FireOnStop();
        }

        public override string Description
        {
            get
            {
                return m_ResourceManager.GetString(BaseNTFunction.gcd);
            }
        }

        public override bool NeedsSecondParameter
        {
            get
            {
                return true;
            }
        }

        #endregion
    }
}