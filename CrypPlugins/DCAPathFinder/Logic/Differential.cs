using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCAPathFinder.Logic
{
    public class Differential : ICloneable
    {
        public ushort InputDifferential;
        public ushort OutputDifferential;
        public ushort Count = 0;
        public double Probability;

        /// <summary>
        /// Constructor
        /// </summary>
        public Differential()
        {
            InputDifferential = 0;
            OutputDifferential = 0;
            Count = 0;
        }

        /// <summary>
        /// Implementation of ICloneable
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            var clone = new Differential
            {
                InputDifferential = this.InputDifferential,
                OutputDifferential = this.OutputDifferential,
                Count = this.Count,
                Probability = this.Probability
            };

            return clone;
        }
    }
}