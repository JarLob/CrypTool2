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

using System;
using System.Collections.Generic;
using System.Text;

namespace Primes.WpfControls.ShapeManagement.Ellipse
{
    public class EllipseConnector
    {
        public EllipseConnector()
        {
        }

        public EllipseConnector(EllipseItem end1, EllipseItem end2)
        {
            EllipseEnd1 = end1;
            EllipseEnd2 = end2;
        }

        public double X1
        {
            get { return EllipseEnd1.X + EllipseEnd1.Width / 2; }
        }

        public double X2
        {
            get { return EllipseEnd2.X + EllipseEnd2.Width / 2; }
        }

        public double Y1
        {
            get { return EllipseEnd1.Y + EllipseEnd1.Height; }
        }

        public double Y2
        {
            get { return EllipseEnd2.Y; }
        }

        private EllipseItem m_EllipseEnd1;

        public EllipseItem EllipseEnd1
        {
            get { return m_EllipseEnd1; }
            set { m_EllipseEnd1 = value; }
        }

        private EllipseItem m_EllipseEnd2;

        public EllipseItem EllipseEnd2
        {
            get { return m_EllipseEnd2; }
            set { m_EllipseEnd2 = value; }
        }

        public override bool Equals(object obj)
        {
            if (obj != null && obj.GetType() == typeof(EllipseConnector))
            {
                return
                  (obj as EllipseConnector).EllipseEnd1 == this.EllipseEnd1
                  && (obj as EllipseConnector).EllipseEnd2 == this.EllipseEnd2;
            }
            else
            {
                return base.Equals(obj);
            }
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}