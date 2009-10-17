﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cryptool.FrequencyTest
{
    public class CollectionElement
    {
        private string caption;
        private double normalizedValue;
        private double absoluteValue;

        public CollectionElement(double absoluteVal, double normalizedVal, string caption)
        {
            this.absoluteValue = absoluteVal;
            this.caption = caption;
            this.normalizedValue = normalizedVal;
        }

        /// <summary>
        /// The caption to appear under the bar
        /// </summary>
        public string Caption
        {
            get { return caption; }
            set
            {
                caption = value;
            }
        }

        /// <summary>
        /// The value to be written on top of the bar, usually the percentage value
        /// </summary>
        public double Percent
        {
            get { return normalizedValue; }
            set
            {
                normalizedValue = value;
            }
        }

        /// <summary>
        /// The absolute value, used for the absolute heigth of the bar
        /// </summary>
        public double Amount
        {
            get { return absoluteValue; }
            set
            {
                absoluteValue = value;
            }
        }
    }
}
