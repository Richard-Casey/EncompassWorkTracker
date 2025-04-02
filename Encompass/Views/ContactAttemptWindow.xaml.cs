using Encompass.Models;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Encompass.Views
{
    public partial class ContactAttemptWindow : Window
    {
        private bool isEditMode = false;
        public ContactAttempt? NewAttempt { get; private set; }

        // Constructor for "Add Attempt"
        public ContactAttemptWindow(string userNumber, int attemptNumber)
        {
            InitializeComponent();
            // Create a new attempt
            NewAttempt = new ContactAttempt
            {
                UserNumber = userNumber,
                AttemptNumber = attemptNumber,
                ContactDate = DateTime.Now.ToString("dd/MM/yyyy"),
                Method = "",
                Notes = "",
                Reply = "No"
            };
            isEditMode = false;
            PopulateFields();
        }

        // Constructor for "Edit Attempt"
        public ContactAttemptWindow(ContactAttempt existingAttempt)
        {
            InitializeComponent();
            // Copy the existing attempt into a new object
            NewAttempt = new ContactAttempt
            {
                UserNumber = existingAttempt.UserNumber,
                AttemptNumber = existingAttempt.AttemptNumber,
                ContactDate = existingAttempt.ContactDate,
                Method = existingAttempt.Method,
                Notes = existingAttempt.Notes,
                Reply = existingAttempt.Reply,
                ResponseMethod = existingAttempt.ResponseMethod,
                AdditionalResponseNotes = existingAttempt.AdditionalResponseNotes
            };
            isEditMode = true;
            PopulateFields();
        }

        private void PopulateFields()
        {
            if (NewAttempt == null) return;

            // Method
            foreach (var item in MethodDropdown.Items)
            {
                if (item is ComboBoxItem cbi && cbi.Content.ToString() == NewAttempt.Method)
                {
                    MethodDropdown.SelectedItem = cbi;
                    break;
                }
            }

            AttemptNotesTextBox.Text = NewAttempt.Notes;
            if (NewAttempt.Reply == "Yes") ReplyYesRadio.IsChecked = true;
            else ReplyNoRadio.IsChecked = true;

            // ResponseMethod
            foreach (var item in ResponseMethodDropdown.Items)
            {
                if (item is ComboBoxItem cbi && cbi.Content.ToString() == NewAttempt.ResponseMethod)
                {
                    ResponseMethodDropdown.SelectedItem = cbi;
                    break;
                }
            }
            AdditionalNotesTextBox.Text = NewAttempt.AdditionalResponseNotes;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (NewAttempt == null) return;

            // Existing fields
            string method = (MethodDropdown.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Unknown";
            NewAttempt.Method = method;
            NewAttempt.Notes = AttemptNotesTextBox.Text.Trim();
            NewAttempt.Reply = (ReplyYesRadio.IsChecked == true) ? "Yes" : "No";

            // Only fill in these fields if user replied "Yes"
            if (ReplyYesRadio.IsChecked == true)
            {
                string respMethod = (ResponseMethodDropdown.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
                NewAttempt.ResponseMethod = respMethod;
                NewAttempt.AdditionalResponseNotes = AdditionalNotesTextBox.Text.Trim();
            }
            else
            {
                NewAttempt.ResponseMethod = "";
                NewAttempt.AdditionalResponseNotes = "";
            }

            DialogResult = true;
            Close();
        }


        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ReplyRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (ReplyYesRadio.IsChecked == true)
            {
                // If "Yes" is selected, show additional fields.
                AdditionalResponsePanel.Visibility = Visibility.Visible;
            }
            else
            {
                AdditionalResponsePanel.Visibility = Visibility.Collapsed;
            }
        }

    }
}
