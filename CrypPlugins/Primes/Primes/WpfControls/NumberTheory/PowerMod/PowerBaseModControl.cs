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

using Primes.Bignum;
using Primes.WpfControls.Components;

namespace Primes.WpfControls.NumberTheory.PowerMod
{
    /// <summary>
    /// Power Mod tutorial which iterates the base.
    /// </summary>
    public class PowerBaseModControl : PowerModControl
    {
        protected override bool SameArrowMarksCycle => false;

        protected override InputSingleControl ActiveExpControl => iscExp;
        protected override InputSingleControl ActiveBaseControl => iscMaxBase;

        protected override PrimesBigInteger DefaultExp => 28;
        protected override PrimesBigInteger DefaultBase => 20;

        protected override PrimesBigInteger DoIterationStep(PrimesBigInteger lastResult, PrimesBigInteger iteration)
        {
            var result = iteration.ModPow(Exp, Mod);
            log.Info(string.Format(Primes.Resources.lang.Numbertheory.Numbertheory.powermod_execution, iteration, iteration, Exp, Mod, result));
            return result;
        }

        public override void SetFormula()
        {
            Formula.Formula = $"b^{{{Exp}}} \\text{{ mod }} {Mod} \\text{{          }} b = 0,\\ldots,{Base}";
        }

        protected override PrimesBigInteger MaxIteration => Base;

        protected override PrimesBigInteger IterationStart => 0;
    }
}
