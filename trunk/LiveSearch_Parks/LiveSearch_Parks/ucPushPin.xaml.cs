using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace LiveSearch_Parks
{
    public partial class ucPushPin : UserControl
    {
        bool _enteredPopup = false;

        public ucPushPin()
        {
            InitializeComponent();

            // we want to initially hide the popup
            cnvPopup.Visibility = Visibility.Collapsed;
        }

        private void Group_1_MouseEnter(object sender, MouseEventArgs e)
        {
            
            // we want to force all other children in the canvas to the back
            foreach (ucPushPin pin in (this.Parent as Canvas).Children)
            {
                pin.SetValue(Canvas.ZIndexProperty, 0);
                pin.cnvPopup.Visibility = Visibility.Collapsed;
            }

            cnvPopup.Visibility = Visibility.Visible;
            this.SetValue(Canvas.ZIndexProperty, 1000);
        }

        private void cnvPopup_MouseEnter(object sender, MouseEventArgs e)
        {
            _enteredPopup = true;
        }

        private void Group_1_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!_enteredPopup)
                cnvPopup.Visibility = Visibility.Collapsed;
        }

        private void cnvPopup_MouseLeave(object sender, MouseEventArgs e)
        {
            cnvPopup.Visibility = Visibility.Collapsed;
            _enteredPopup = false;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            cnvPopup.Visibility = Visibility.Collapsed;
            _enteredPopup = false;
        }

    }
}
