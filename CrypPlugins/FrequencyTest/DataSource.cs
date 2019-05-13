﻿using System.Collections.ObjectModel;

namespace Cryptool.FrequencyTest
{
    public class DataSource
    {
        private ObservableCollection<CollectionElement> valueCollection;

        public ObservableCollection<CollectionElement> ValueCollection
        {
            get { return valueCollection; }
            set { valueCollection = value; }
        }

        public DataSource()
        {
            valueCollection = new ObservableCollection<CollectionElement>();
        }
    }
}
