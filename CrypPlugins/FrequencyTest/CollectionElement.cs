using System.Windows;
using System.Windows.Media;

namespace Cryptool.FrequencyTest
{
    public class CollectionElement
    {
        private string caption;
        private int absoluteValue;
        private double percentageValue;
        private double height;        
        private bool showAbsoluteValues = false;
        private Visibility visibility;
        private Color colorA = Colors.Turquoise;
        private Color colorB = Colors.DarkBlue;

        public CollectionElement(double height, int absoluteValue, double percentageValue, string caption, bool showAbsoluteValues, Visibility visibility = Visibility.Visible)
        {
            this.height = height;
            this.caption = caption;
            this.absoluteValue = absoluteValue;
            this.percentageValue = percentageValue;
            this.showAbsoluteValues = showAbsoluteValues;
            this.visibility = visibility;
        }

        /// <summary>
        /// The caption to appear under the bar
        /// </summary>
        public string Caption
        {
            get
            {
                return caption;
            }
            set
            {
                caption = value;
            }
        }

        /// <summary>
        /// The value to be written on top of the bar, usually the percentage value
        /// </summary>
        public string BarHeadValue
        {
            get
            {
                if (showAbsoluteValues)
                {
                    if(AbsoluteValue == 0)
                    {
                        return string.Empty;
                    }
                    return AbsoluteValue.ToString();
                }
                else
                {
                    if (PercentageValue == 0)
                    {
                        return string.Empty;
                    }
                    return PercentageValue.ToString();
                }
            }
        }

        /// <summary>
        /// The absolute value
        /// </summary>
        public int AbsoluteValue
        {
            get
            {
                return absoluteValue;
            }
            set
            {
                absoluteValue = value;
            }
        }

        /// <summary>
        /// The percentage value
        /// </summary>
        public double PercentageValue
        {
            get
            {
                return percentageValue;
            }
            set
            {
                percentageValue = value;
            }
        }

        /// <summary>
        /// Height of the bar
        /// </summary>
        public double Height
        {
            get
            {
                return height;
            }
            set
            {
                height = value;
            }
        }

        public Visibility Visibility
        {
            get
            {
                return visibility;
            }
            set
            {
                visibility = value;
            }
        }

        public Color ColorA
        {
            get
            {
                return colorA;
            }
            set
            {
                colorA = value;
            }
        }

        public Color ColorB
        {
            get
            {
                return colorB;
            }
            set
            {
                colorB = value;
            }
        }
    }
}
