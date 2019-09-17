using System;
using System.Windows;
using System.Windows.Controls;

namespace Cryptool.CrypAnalysisViewControl
{
    public class SectionViewLabelsControl : ItemsControl
    {
        static SectionViewLabelsControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SectionViewLabelsControl), new FrameworkPropertyMetadata(typeof(SectionViewLabelsControl)));
        }
    }
}
