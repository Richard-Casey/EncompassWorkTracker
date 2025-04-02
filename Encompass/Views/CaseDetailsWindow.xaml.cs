using Encompass.Models;
using Encompass.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Input;

namespace Encompass.Views
{
    public partial class CaseDetailsWindow : Window
    {
        private string filePath = "Cases.csv";
        private CaseModel currentCase;
        private string originalNotes = "";

        // Field indicating if this case is archived (read-only mode)
        private bool _isArchived = false;

        /// <summary>
        /// Constructor for active cases.
        /// </summary>
        public CaseDetailsWindow(CaseModel selectedCase) : this(selectedCase, false) { }

        /// <summary>
        /// Constructor with isArchived flag.
        /// When isArchived is true, editing is disabled and archive fields are shown.
        /// </summary>
        public CaseDetailsWindow(CaseModel selectedCase, bool isArchived)
        {
            InitializeComponent();
            currentCase = selectedCase;
            _isArchived = isArchived;

            // Populate UI controls
            UserNumberBox.Text = currentCase.UserNumber;
            FirstNameBox.Text = currentCase.FirstName;
            SurnameBox.Text = currentCase.Surname;
            PhoneNumberBox.Text = currentCase.PhoneNumber;
            EmailBox.Text = currentCase.EmailAddress;
            ContactMethodBox.Text = currentCase.PreferredContact;
            StatusDropdown.SelectedItem = currentCase.Status;

            NotesBox.Text = currentCase.Notes;
            originalNotes = currentCase.Notes;

            RefreshContactAttempts();
            BuildFullLog();

            MainTabControl.SelectionChanged += MainTabControl_SelectionChanged;

            // If this is an archived case, disable editing and show archive details.
            if (_isArchived)
            {
                DisableEditingForArchived();
                ArchiveDateBox.Text = currentCase.ArchiveDate;
                ArchiveReasonBox.Text = currentCase.ArchiveReason;
            }
        }

        /// <summary>
        /// Disables editing controls for archived cases.
        /// </summary>
        private void DisableEditingForArchived()
        {
            SaveNotesButton.IsEnabled = false;
            NotesBox.IsReadOnly = true;
            // Optionally disable other editing controls as needed.
        }

        /// <summary>
        /// Event handler for TabControl selection changes.
        /// Prompts to save notes if on the Full Log tab.
        /// </summary>
        private void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl tabControl)
            {
                TabItem selectedTab = tabControl.SelectedItem as TabItem;
                if (selectedTab != null && selectedTab.Header.ToString() == "Full Log")
                {
                    if (NotesBox.Text != originalNotes)
                    {
                        MessageBoxResult result = MessageBox.Show(
                            "The case notes have changed. Do you want to save the changes?",
                            "Save Changes",
                            MessageBoxButton.YesNoCancel,
                            MessageBoxImage.Question);

                        if (result == MessageBoxResult.Yes)
                        {
                            SaveNotes();
                            originalNotes = NotesBox.Text;
                        }
                        else if (result == MessageBoxResult.No)
                        {
                            NotesBox.Text = originalNotes;
                        }
                        else if (result == MessageBoxResult.Cancel)
                        {
                            tabControl.SelectedIndex = 0;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Saves the notes and updates the CSV.
        /// </summary>
        private void SaveNotes()
        {
            currentCase.Notes = NotesBox.Text;
            List<string> lines = File.ReadAllLines(filePath).ToList();
            for (int i = 1; i < lines.Count; i++)
            {
                var parts = lines[i].Split(',');
                if (parts[0].Trim() == currentCase.UserNumber)
                {
                    lines[i] = string.Join(",", new[]
                    {
                        currentCase.UserNumber,
                        currentCase.FirstName,
                        currentCase.Surname,
                        currentCase.SubmissionDate,
                        currentCase.SubmissionTime,
                        currentCase.PhoneNumber,
                        currentCase.EmailAddress,
                        currentCase.DOB,
                        currentCase.Age,
                        currentCase.Postcode,
                        currentCase.CurrentTown,
                        currentCase.LocalAuthority,
                        currentCase.Location,
                        currentCase.Gender,
                        currentCase.Ethnicity,
                        currentCase.PreferredContact,
                        currentCase.GestationPeriod,
                        currentCase.ServiceAccess,
                        currentCase.LivingWithPartner,
                        currentCase.LocalHub,
                        currentCase.Status,
                        currentCase.DeadlineDate,
                        currentCase.WellbeingReviewDate,
                        currentCase.WBRSentDate,
                        currentCase.FollowUpDate,
                        currentCase.Notes
                    });
                    break;
                }
            }
            File.WriteAllLines(filePath, lines);
            BuildFullLog();
        }

        private void Window_Closing(object? sender, CancelEventArgs e)
        {
            if (NotesBox.Text != originalNotes)
            {
                MessageBoxResult result = MessageBox.Show(
                    "The case notes have changed. Do you want to save the changes before closing?",
                    "Save Changes",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    SaveNotes();
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }

        /// <summary>
        /// Loads contact attempts for this case.
        /// </summary>
        private void RefreshContactAttempts()
        {
            List<ContactAttempt> attempts = ContactAttemptService.LoadContactAttempts(currentCase.UserNumber);
            AttemptsDataGrid.ItemsSource = attempts;
        }

        private void AddAttempt_Click(object sender, RoutedEventArgs e)
        {
            var attempts = ContactAttemptService.LoadContactAttempts(currentCase.UserNumber);
            int nextNumber = attempts.Count + 1;
            ContactAttemptWindow attemptWindow = new ContactAttemptWindow(currentCase.UserNumber, nextNumber);
            bool? dialogResult = attemptWindow.ShowDialog();
            if (dialogResult == true && attemptWindow.NewAttempt != null)
            {
                ContactAttemptService.SaveContactAttempt(attemptWindow.NewAttempt);
                RefreshContactAttempts();
                BuildFullLog();
            }
        }

        private void AttemptsDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (AttemptsDataGrid.SelectedItem is ContactAttempt selectedAttempt)
            {
                ContactAttemptWindow attemptWindow = new ContactAttemptWindow(selectedAttempt);
                bool? result = attemptWindow.ShowDialog();
                if (result == true && attemptWindow.NewAttempt != null)
                {
                    ContactAttemptService.UpdateAttempt(selectedAttempt.UserNumber,
                        selectedAttempt.AttemptNumber, attemptWindow.NewAttempt);
                    RefreshContactAttempts();
                    BuildFullLog();
                }
            }
        }

        private void EditAttempt_Click(object sender, RoutedEventArgs e)
        {
            var selected = AttemptsDataGrid.SelectedItem as ContactAttempt;
            if (selected == null)
            {
                MessageBox.Show("Please select an attempt to edit first.", "No selection",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ContactAttemptWindow attemptWindow = new ContactAttemptWindow(selected);
            bool? dialogResult = attemptWindow.ShowDialog();
            if (dialogResult == true && attemptWindow.NewAttempt != null)
            {
                ContactAttemptService.UpdateAttempt(selected.UserNumber,
                    selected.AttemptNumber, attemptWindow.NewAttempt);
                RefreshContactAttempts();
                BuildFullLog();
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void RebuildCombinedLog()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=== CASE INFO NOTES ===");
            sb.AppendLine(currentCase.Notes);
            sb.AppendLine();

            var attempts = ContactAttemptService.LoadContactAttempts(currentCase.UserNumber)
                          .OrderBy(a => a.AttemptNumber).ToList();

            foreach (var a in attempts)
            {
                sb.AppendLine($"--- Attempt #{a.AttemptNumber} on {a.ContactDate} ---");
                sb.AppendLine($"Method: {a.Method}");
                sb.AppendLine($"Notes: {a.Notes}");
                sb.AppendLine($"Reply: {a.Reply}");
                if (a.Reply == "Yes")
                {
                    sb.AppendLine($"Reply Method: {a.ResponseMethod}");
                    sb.AppendLine($"Reply Notes: {a.AdditionalResponseNotes}");
                }
                sb.AppendLine();
            }
            FullLogBox.Text = sb.ToString();
        }

        private void BuildFullLog()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("===================== Case Info Notes =====================");
            sb.AppendLine(currentCase.Notes);
            sb.AppendLine();

            var attempts = ContactAttemptService.LoadContactAttempts(currentCase.UserNumber)
                          .OrderBy(a => a.AttemptNumber);

            foreach (var a in attempts)
            {
                sb.AppendLine($"===================== Attempt #{a.AttemptNumber} on {a.ContactDate} =====================");
                sb.AppendLine();
                sb.AppendLine($"Method: {a.Method}");
                sb.AppendLine();
                sb.AppendLine($"Notes: {a.Notes}");
                sb.AppendLine();
                sb.AppendLine($"Reply: {a.Reply}");
                sb.AppendLine();
                if (a.Reply == "Yes")
                {
                    sb.AppendLine($"Reply Method: {a.ResponseMethod}");
                    sb.AppendLine();
                    sb.AppendLine($"Reply Notes: {a.AdditionalResponseNotes}");
                    sb.AppendLine();
                }
                sb.AppendLine();
            }
            FullLogBox.Text = sb.ToString();
        }

        private void SaveNotesButton_Click(object sender, RoutedEventArgs e)
        {
            currentCase.Notes = NotesBox.Text;
            var lines = File.ReadAllLines(filePath).ToList();
            for (int i = 1; i < lines.Count; i++)
            {
                var parts = lines[i].Split(',');
                if (parts[0].Trim() == currentCase.UserNumber)
                {
                    lines[i] = string.Join(",", new[]
                    {
                        currentCase.UserNumber,
                        currentCase.FirstName,
                        currentCase.Surname,
                        currentCase.SubmissionDate,
                        currentCase.SubmissionTime,
                        currentCase.PhoneNumber,
                        currentCase.EmailAddress,
                        currentCase.DOB,
                        currentCase.Age,
                        currentCase.Postcode,
                        currentCase.CurrentTown,
                        currentCase.LocalAuthority,
                        currentCase.Location,
                        currentCase.Gender,
                        currentCase.Ethnicity,
                        currentCase.PreferredContact,
                        currentCase.GestationPeriod,
                        currentCase.ServiceAccess,
                        currentCase.LivingWithPartner,
                        currentCase.LocalHub,
                        currentCase.Status,
                        currentCase.DeadlineDate,
                        currentCase.WellbeingReviewDate,
                        currentCase.WBRSentDate,
                        currentCase.FollowUpDate,
                        currentCase.Notes
                    });
                    break;
                }
            }
            File.WriteAllLines(filePath, lines);
            BuildFullLog();
            originalNotes = currentCase.Notes;
            MessageBox.Show("Notes updated and log refreshed.", "Save Successful", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
