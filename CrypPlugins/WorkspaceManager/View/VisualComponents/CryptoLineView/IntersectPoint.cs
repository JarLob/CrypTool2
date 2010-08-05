﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace WorkspaceManager.View.VisualComponents
{
    public enum IntersectPointMode
    {
        InnerIntersect,
        NormalIntersect
    };

    public class IntersectPoint
    {
        public Point Point { get; private set; }
        public IntersectPointMode Mode { get; private set; }

        public IntersectPoint(Point point, IntersectPointMode mode)
        {
            this.Mode = mode;
            this.Point = point;
        }
    }
}
