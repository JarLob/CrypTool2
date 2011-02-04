﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cryptool.PluginBase.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TabColorAttribute : Attribute
    {
        public string Brush { get; private set; }

        public TabColorAttribute(string brush)
        {
            Brush = brush;
        }
    }
}
