using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Cryptool.CrypAnalysisViewControl
{
    public class CrypAnalysisViewControl : ContentControl
    {
        public string ResultHeaderCaption
        {
            get { return (string)GetValue(ResultHeaderCaptionProperty); }
            set { SetValue(ResultHeaderCaptionProperty, value); }
        }

        public static readonly DependencyProperty ResultHeaderCaptionProperty = DependencyProperty.Register(
          "ResultHeaderCaption", typeof(string), typeof(CrypAnalysisViewControl), new PropertyMetadata(null));

        public string ResultListCaption
        {
            get { return (string)GetValue(ResultListCaptionProperty); }
            set { SetValue(ResultListCaptionProperty, value); }
        }

        public static readonly DependencyProperty ResultListCaptionProperty = DependencyProperty.Register(
          "ResultListCaption", typeof(string), typeof(CrypAnalysisViewControl), new PropertyMetadata(null));

        public string ResultProgressCaption
        {
            get { return (string)GetValue(ResultProgressCaptionProperty); }
            set { SetValue(ResultProgressCaptionProperty, value); }
        }

        public static readonly DependencyProperty ResultProgressCaptionProperty = DependencyProperty.Register(
          "ResultProgressCaption", typeof(string), typeof(CrypAnalysisViewControl), new PropertyMetadata(null));

        public List<ViewLabel> ResultHeaderLabels { get; } = new List<ViewLabel>();

        public List<ViewLabel> ResultProgressLabels { get; } = new List<ViewLabel>();

        public List<SectionControl> AdditionalSections { get; } = new List<SectionControl>();

        static CrypAnalysisViewControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CrypAnalysisViewControl), new FrameworkPropertyMetadata(typeof(CrypAnalysisViewControl)));
        }
    }
}
