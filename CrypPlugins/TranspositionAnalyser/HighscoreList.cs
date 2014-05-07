﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TranspositionAnalyser
{
    public class HighscoreList : List<ValueKey>
    {
        private ValueKeyComparer comparer;

        public HighscoreList(ValueKeyComparer comparer, int Capacity)
        {
            this.comparer = comparer;
            this.Capacity = Capacity;
        }

        public bool isBetter(ValueKey v)
        {
            if (Count == 0) return true;
            return comparer.Compare(v, this[0]) > 0;
        }

        public bool isPresent(ValueKey v, out int i)
        {
            if (Count == 0 || comparer.Compare(v, this[Count - 1]) < 0) { i = Count; return false; }

            for (i = 0; i < Count; i++)
            {
                int cmp = comparer.Compare(v, this[i]);
                if (cmp > 0) return false;
                if (cmp == 0) return true;
            }

            return false;
        }

        new public bool Add(ValueKey v)
        {
            int i;
            if (isPresent(v, out i)) return false;
            return Add(v, i);
        }

        public bool Add(ValueKey v, int i)
        {
            if (i >= Capacity) return false;
            if (Count >= Capacity) RemoveAt(Capacity - 1);
            Insert(i, (ValueKey)v.Clone());
            return true;
        }

        public void Merge(HighscoreList list)
        {
            foreach (var v in list)
                if (!Add(v)) return;
        }
    }
}