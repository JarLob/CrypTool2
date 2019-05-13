using System;

namespace DiscreteLogarithm
{
    class LinearDependentException : Exception
    {
        public int[] RowsToDelete
        {
            get;
            private set;
        }

        public LinearDependentException(int[] rowsToDelete)
        {
            this.RowsToDelete = rowsToDelete;
        }
    }
}
