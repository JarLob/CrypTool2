using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace CrypCloud.Manager.Controller
{
    public abstract class Controller<T> where T : UserControl
    {
        protected T View;
        protected CrypCloudManager Root;

        protected Controller(T view, CrypCloudManager root)
        {
            View = view;
            Root = root;
        }

        protected void ShowView()
        {
            View.Visibility = System.Windows.Visibility.Visible;
        }

        protected void HideView()
        {
            View.Visibility = System.Windows.Visibility.Collapsed;
        }

        public virtual void Activate()
        {
            ShowView();
        }

        public virtual void Deactivate()
        {
            HideView();
        }





    }
}
