﻿using System.Collections.Generic;
using Cryptool.PluginBase.Control;
using KeySearcher.Properties;

namespace KeySearcher.Helper
{
    class KeyQualityHelper
    {
        private readonly IControlCost costFunction;

        public KeyQualityHelper(IControlCost costFunction)
        {
            this.costFunction = costFunction;
        }

        public double WorstValue()
        {
            if (costFunction.GetRelationOperator() == RelationOperator.LargerThan)
            {
                return double.MinValue;
            }
            else
            {
                return double.MaxValue;
            }
        }

        public void FillListWithDummies(int maxInList, LinkedList<KeySearcher.ValueKey> costList)
        {
            KeySearcher.ValueKey valueKey = new KeySearcher.ValueKey();
            valueKey.value = WorstValue();
            valueKey.key = Resources.dummykey;
            valueKey.decryption = new byte[0];
            LinkedListNode<KeySearcher.ValueKey> node = costList.AddFirst(valueKey);
            for (int i = 1; i < maxInList; i++)
            {
                node = costList.AddAfter(node, valueKey);
            }
        }

        public bool IsBetter(double value, double value2)
        {
            if (costFunction.GetRelationOperator() == RelationOperator.LargerThan)
            {
                if (value > value2)
                {
                    return true;
                }
            }
            else
            {
                if (value < value2)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
