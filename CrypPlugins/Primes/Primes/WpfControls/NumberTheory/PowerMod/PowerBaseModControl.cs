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
using Primes.Resources.lang.Numbertheory;
using Primes.WpfControls.Components;
using System.Collections.Generic;

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

        protected override PrimesBigInteger DefaultExp => 5;
        protected override PrimesBigInteger DefaultBase => 24;
        protected override PrimesBigInteger DefaultMod => 12;

        protected override PrimesBigInteger DoIterationStep(PrimesBigInteger lastResult, PrimesBigInteger iteration)
        {
            var result = iteration.ModPow(Exp, Mod);
            AddIterationLogEntry(iteration.IntValue,
                $"{iteration}^{{{Exp}}} \\text{{ mod }} {Mod} = {result}",
                string.Format(Numbertheory.powermod_execution, iteration, iteration, Exp, Mod, result));
            return result;
        }

        private bool ContainsDuplicateValues()
        {
            var i = IterationStart;
            var previousValues = new HashSet<int>();
            while (i < Mod)
            {
                var value = i.ModPow(Exp, Mod);
                if (previousValues.Contains(value.IntValue))
                {
                    return true;
                }
                previousValues.Add(value.IntValue);
                i += 1;
            }
            return false;
        }

        public override void SetCycleInfo()
        {
            CycleInfo1.Text = string.Format(Numbertheory.powermod_cycle_length, Mod);
            CycleInfo2.Text = null;
            if (ContainsDuplicateValues())
            {
                CycleInfo2.Text = Numbertheory.powermod_cycle_duplicate_values;
            }
        }

        public override void SetFormula()
        {
            Formula.Formula = $"i^{{{Exp}}} \\text{{ mod }} {Mod} \\text{{          }} i = 0,\\ldots,{Base}";
        }

        protected override PrimesBigInteger MaxIteration => Base;

        protected override PrimesBigInteger IterationStart => 0;
    }
}
