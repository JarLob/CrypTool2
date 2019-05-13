using System;
using System.Numerics;

namespace DiscreteLogarithm
{    
    class NotInvertibleException : Exception
    {
        public BigInteger NotInvertibleNumber
        {
            get;
            private set;
        }

        public NotInvertibleException(BigInteger notInvertible)
        {
            NotInvertibleNumber = notInvertible;
        }
    }
}
