using Microsoft.Phone.Controls;
using System;
using System.Diagnostics;
using System.Windows;
using TripSketch.Controls;
using Xamarin.Forms.Platform.WinPhone;

namespace TripSketch.WinPhone.Controls
{
    /// <summary>
    /// Adds support for PropertyChange text binding behavior and Focus requests from EntryCellEx
    /// </summary>
    public class EntryCellPhoneTextBoxEx : EntryCellPhoneTextBox
    {
        public EntryCellPhoneTextBoxEx()
        {
            Loaded += EntryCellPhoneTextBoxEx_Loaded;
            TextChanged += EntryCellPhoneTextBoxEx_TextChanged;
        }

        private void EntryCellPhoneTextBoxEx_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Debug.Assert(sender == this);

            var binding = GetBindingExpression(PhoneTextBox.TextProperty);

            if (binding != null)
            {
                binding.UpdateSource();
            }
        }

        private void EntryCellPhoneTextBoxEx_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.Assert(sender == this);

            var entryCellEx = DataContext as EntryCellEx;

            if (entryCellEx == null)
            {
                return;
            }

            if (entryCellEx.RequestInitialFocus == true)
            {
                Focus();
                entryCellEx.RequestInitialFocus = false;
            }

            entryCellEx.FocusRequested += EntryCellEx_FocusRequested;
        }

        private void EntryCellEx_FocusRequested(object sender, EventArgs e)
        {
            Focus();
        }
    }
}
