using Encompass.Models;
using Encompass.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Encompass.Views
{
    public partial class CasesWindow : Window
    {
        private string CasesFilePath = "Cases.csv";
        private bool IsAdmin => true;

        private List<CaseModel> AllCases = new List<CaseModel>();
        private List<CaseModel> FilteredCases = new List<CaseModel>();
        private List<CaseModel> ArchivedCases = new List<CaseModel>();

        public CasesWindow()
        {
            InitializeComponent();
            LoadCases();
            UpdateCaseCount();

            // Hook up event handlers for the DataGrids and TabControl.
            LeftDataGrid.SelectionChanged += Grid_SelectionChanged;
            MainDataGrid.SelectionChanged += Grid_SelectionChanged;
            StatusDataGrid.SelectionChanged += Grid_SelectionChanged;
            DataTabControl.SelectionChanged += DataTabControl_SelectionChanged;

            // Synchronize vertical scrolling between grids.
            ScrollViewer leftScroll = GetScrollViewer(LeftDataGrid);
            ScrollViewer mainScroll = GetScrollViewer(MainDataGrid);
            ScrollViewer statusScroll = GetScrollViewer(StatusDataGrid);
            if (leftScroll != null && mainScroll != null && statusScroll != null)
            {
                leftScroll.ScrollChanged += (s, e) =>
                {
                    mainScroll.ScrollToVerticalOffset(e.VerticalOffset);
                    statusScroll.ScrollToVerticalOffset(e.VerticalOffset);
                };
                mainScroll.ScrollChanged += (s, e) =>
                {
                    leftScroll.ScrollToVerticalOffset(e.VerticalOffset);
                    statusScroll.ScrollToVerticalOffset(e.VerticalOffset);
                };
                statusScroll.ScrollChanged += (s, e) =>
                {
                    leftScroll.ScrollToVerticalOffset(e.VerticalOffset);
                    mainScroll.ScrollToVerticalOffset(e.VerticalOffset);
                };
            }
        }

        // Called when the TabControl selection changes.
        private void DataTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl)
            {
                TabItem selectedTab = (sender as TabControl)?.SelectedItem as TabItem;
                if (selectedTab != null && selectedTab.Header.ToString() == "Archived Cases")
                {
                    LoadArchivedCases();
                }
            }
        }

        // Loads active cases from Cases.csv.
        private void LoadCases()
        {
            if (!File.Exists(CasesFilePath))
            {
                File.WriteAllText(CasesFilePath,
                    "UserNumber,FirstName,Surname,SubmissionDate,SubmissionTime,PhoneNumber,EmailAddress,DOB,Age,Postcode,CurrentTown,LocalAuthority,Location,Gender,Ethnicity,PreferredContact,GestationPeriod,ServiceAccess,LivingWithPartner,LocalHub,Status,DeadlineDate,WellbeingReviewDate,WBRSentDate,FollowUpDate,Notes\n");
            }

            IEnumerable<string> lines = File.ReadAllLines(CasesFilePath).Skip(1);
            AllCases = lines.Select(line =>
            {
                string[] parts = line.Split(',');
                while (parts.Length < 26)
                    parts = parts.Append("").ToArray();

                return new CaseModel
                {
                    UserNumber = parts[0].Trim(),
                    FirstName = parts[1].Trim(),
                    Surname = parts[2].Trim(),
                    SubmissionDate = parts[3].Trim(),
                    SubmissionTime = parts[4].Trim(),
                    PhoneNumber = parts[5].Trim(),
                    EmailAddress = parts[6].Trim(),
                    DOB = parts[7].Trim(),
                    Age = parts[8].Trim(),
                    Postcode = parts[9].Trim(),
                    CurrentTown = parts[10].Trim(),
                    LocalAuthority = parts[11].Trim(),
                    Location = parts[12].Trim(),
                    Gender = parts[13].Trim(),
                    Ethnicity = parts[14].Trim(),
                    PreferredContact = parts[15].Trim(),
                    GestationPeriod = parts[16].Trim(),
                    ServiceAccess = parts[17].Trim(),
                    LivingWithPartner = parts[18].Trim(),
                    LocalHub = parts[19].Trim(),
                    Status = parts[20].Trim(),
                    DeadlineDate = parts[21].Trim(),
                    WellbeingReviewDate = parts[22].Trim(),
                    WBRSentDate = parts[23].Trim(),
                    FollowUpDate = parts[24].Trim(),
                    Notes = parts[25].Trim()
                };
            }).ToList();

            foreach (CaseModel c in AllCases)
            {
                var attempts = ContactAttemptService.LoadContactAttempts(c.UserNumber);
                bool hasContactAttempt = attempts.Any();
                var computed = CaseStatusService.CalculateStatus(c.SubmissionDate, c.SubmissionTime, hasContactAttempt, DateTime.Now);
                c.Status = computed.StatusText;
                c.StatusBrush = new SolidColorBrush(computed.StatusColor);
                c.ForegroundBrush = new SolidColorBrush(computed.ForegroundColor);
                c.FontWeight = computed.FontWeight;
            }

            FilteredCases = AllCases.ToList();

            LeftDataGrid.ItemsSource = FilteredCases;
            MainDataGrid.ItemsSource = FilteredCases;
            StatusDataGrid.ItemsSource = FilteredCases;

            UpdateCaseCount();
        }

        // Loads archived cases from ArchivedCases.csv.
        private void LoadArchivedCases()
        {
            string archivedFile = "ArchivedCases.csv";
            if (!File.Exists(archivedFile))
            {
                File.WriteAllText(archivedFile,
                  "UserNumber,FirstName,Surname,SubmissionDate,SubmissionTime,PhoneNumber,EmailAddress,DOB,Age,Postcode,CurrentTown,LocalAuthority,Location,Gender,Ethnicity,PreferredContact,GestationPeriod,ServiceAccess,LivingWithPartner,LocalHub,Status,DeadlineDate,WellbeingReviewDate,WBRSentDate,FollowUpDate,Notes,ArchiveDate,ArchiveReason\n");
            }

            IEnumerable<string> lines = File.ReadAllLines(archivedFile).Skip(1);
            ArchivedCases = lines.Select(line =>
            {
                string[] parts = line.Split(',');
                while (parts.Length < 28)
                    parts = parts.Append("").ToArray();

                var model = new CaseModel
                {
                    UserNumber = parts[0].Trim(),
                    FirstName = parts[1].Trim(),
                    Surname = parts[2].Trim(),
                    SubmissionDate = parts[3].Trim(),
                    SubmissionTime = parts[4].Trim(),
                    PhoneNumber = parts[5].Trim(),
                    EmailAddress = parts[6].Trim(),
                    DOB = parts[7].Trim(),
                    Age = parts[8].Trim(),
                    Postcode = parts[9].Trim(),
                    CurrentTown = parts[10].Trim(),
                    LocalAuthority = parts[11].Trim(),
                    Location = parts[12].Trim(),
                    Gender = parts[13].Trim(),
                    Ethnicity = parts[14].Trim(),
                    PreferredContact = parts[15].Trim(),
                    GestationPeriod = parts[16].Trim(),
                    ServiceAccess = parts[17].Trim(),
                    LivingWithPartner = parts[18].Trim(),
                    LocalHub = parts[19].Trim(),
                    Status = parts[20].Trim(),
                    DeadlineDate = parts[21].Trim(),
                    WellbeingReviewDate = parts[22].Trim(),
                    WBRSentDate = parts[23].Trim(),
                    FollowUpDate = parts[24].Trim(),
                    Notes = parts[25].Trim(),
                    ArchiveDate = parts[26].Trim(),
                    ArchiveReason = parts[27].Trim()
                };

                // Color-code archived statuses.
                if (model.Status == "Complete")
                {
                    model.StatusBrush = new SolidColorBrush(Colors.LightGreen);
                    model.ForegroundBrush = new SolidColorBrush(Colors.Black);
                    model.FontWeight = FontWeights.Bold;
                }
                else if (model.Status == "Closed")
                {
                    model.StatusBrush = new SolidColorBrush(Colors.Red);
                    model.ForegroundBrush = new SolidColorBrush(Colors.Black);
                    model.FontWeight = FontWeights.Bold;
                }
                else
                {
                    model.StatusBrush = new SolidColorBrush(Colors.White);
                    model.ForegroundBrush = new SolidColorBrush(Colors.Black);
                    model.FontWeight = FontWeights.Normal;
                }

                return model;
            }).ToList();

            ArchivedLeftDataGrid.ItemsSource = ArchivedCases;
            ArchivedMainDataGrid.ItemsSource = ArchivedCases;
            ArchivedStatusDataGrid.ItemsSource = ArchivedCases;
        }

        // Updates the case count label.
        private void UpdateCaseCount()
        {
            int displayedCount = FilteredCases.Count;
            int totalCount = AllCases.Count;
            CaseCountLabel.Text = $"Showing {displayedCount} cases. Total cases: {totalCount}";
        }

        // Event handler for double-clicking an active case.
        private void MainDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (MainDataGrid.SelectedItem is CaseModel selectedCase)
            {
                OpenCaseDetails(selectedCase, false);
            }
        }

        // Event handler for double-clicking an archived case.
        private void ArchivedMainDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ArchivedMainDataGrid.SelectedItem is CaseModel archivedCase)
            {
                OpenCaseDetails(archivedCase, true);
            }
        }

        // Handles left DataGrid mouse down to clear selection if nothing is hit.
        private void LeftDataGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            HitTestResult hitResult = VisualTreeHelper.HitTest(LeftDataGrid, e.GetPosition(LeftDataGrid));
            if (hitResult == null)
                return;

            DependencyObject current = hitResult.VisualHit;
            while (current != null && !(current is DataGridRow))
            {
                current = VisualTreeHelper.GetParent(current);
            }
            if (current == null)
                LeftDataGrid.SelectedItem = null;
        }

        // Handles main DataGrid mouse down to clear selection if nothing is hit.
        private void MainDataGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            HitTestResult hitResult = VisualTreeHelper.HitTest(MainDataGrid, e.GetPosition(MainDataGrid));
            if (hitResult == null)
                return;

            DependencyObject current = hitResult.VisualHit;
            while (current != null && !(current is DataGridRow))
            {
                current = VisualTreeHelper.GetParent(current);
            }
            if (current == null)
                MainDataGrid.SelectedItem = null;
        }

        // Handles status DataGrid mouse down to clear selection if nothing is hit.
        private void StatusDataGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            HitTestResult hitResult = VisualTreeHelper.HitTest(StatusDataGrid, e.GetPosition(StatusDataGrid));
            if (hitResult == null)
                return;

            DependencyObject current = hitResult.VisualHit;
            while (current != null && !(current is DataGridRow))
            {
                current = VisualTreeHelper.GetParent(current);
            }
            if (current == null)
                StatusDataGrid.SelectedItem = null;
        }

        // Handles selection changed for the DataGrids.
        private void Grid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Ensure all three grids show the same selection.
            object selectedItem = (sender as DataGrid)?.SelectedItem;
            LeftDataGrid.SelectedItem = selectedItem;
            MainDataGrid.SelectedItem = selectedItem;
            StatusDataGrid.SelectedItem = selectedItem;
        }


        // Opens the details window for a case. The isArchived flag indicates which mode to use.
        private void OpenCaseDetails(CaseModel selectedCase, bool isArchived)
        {
            CaseDetailsWindow detailsWindow = new CaseDetailsWindow(selectedCase, isArchived);
            detailsWindow.ShowDialog();

            if (!isArchived)
                LoadCases();
            else
                LoadArchivedCases();
        }

        // Helper to get the ScrollViewer from a dependency object.
        private ScrollViewer? GetScrollViewer(DependencyObject obj)
        {
            if (obj is ScrollViewer viewer)
                return viewer;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                ScrollViewer result = GetScrollViewer(child);
                if (result != null)
                    return result;
            }
            return null;
        }

        // Event handler for when the SearchBox gains focus.
        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchBox.Text == "Search cases...")
            {
                SearchBox.Text = "";
                SearchBox.Foreground = Brushes.Black;
            }
        }

        // Event handler for when the SearchBox loses focus.
        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                SearchBox.Text = "Search cases...";
                SearchBox.Foreground = Brushes.Gray;
            }
        }

        // Event handler for the Search button click.
        private void Search_Click(object sender, RoutedEventArgs e)
        {
            string query = SearchBox.Text.ToLower();
            FilteredCases = AllCases.Where(c =>
                c.FirstName.ToLower().Contains(query) ||
                c.Surname.ToLower().Contains(query) ||
                c.UserNumber.ToLower().Contains(query)).ToList();

            LeftDataGrid.ItemsSource = FilteredCases;
            MainDataGrid.ItemsSource = FilteredCases;
            StatusDataGrid.ItemsSource = FilteredCases;
            UpdateCaseCount();
        }

        // Event handler for the New Case button click.
        private void NewCase_Click(object sender, RoutedEventArgs e)
        {
            NewCaseWindow newCaseWindow = new NewCaseWindow();
            newCaseWindow.ShowDialog();
            LoadCases();
        }

        // Event handler for the Edit Case button click.
        private void EditCase_Click(object sender, RoutedEventArgs e)
        {
            if (MainDataGrid.SelectedItem is CaseModel selectedCase)
            {
                OpenCaseDetails(selectedCase, false);
            }
        }

        // Event handler for the Delete Case button click.
        private void DeleteCase_Click(object sender, RoutedEventArgs e)
        {
            if (MainDataGrid.SelectedItem is CaseModel selectedCase)
            {
                MessageBoxResult result = MessageBox.Show(
                    $"Are you sure you want to delete case for {selectedCase.FullName}?",
                    "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    AllCases.Remove(selectedCase);
                    FilteredCases.Remove(selectedCase);

                    List<string> lines = new List<string>
                    {
                        "UserNumber,FirstName,Surname,SubmissionDate,SubmissionTime,PhoneNumber,Email,DOB,Age,Postcode,CurrentTown,LocalAuthority,Location,Gender,Ethnicity,PreferredContact,Gestation,ServiceAccess,LivingWithPartner,LocalHub,Status"
                    };

                    foreach (CaseModel c in AllCases)
                    {
                        lines.Add(string.Join(",", new[]
                        {
                            c.UserNumber,
                            c.FirstName,
                            c.Surname,
                            c.SubmissionDate,
                            c.SubmissionTime,
                            $"\"{c.PhoneNumber}\"",
                            c.EmailAddress,
                            c.DOB,
                            c.Age,
                            c.Postcode,
                            c.CurrentTown,
                            c.LocalAuthority,
                            c.Location,
                            c.Gender,
                            c.Ethnicity,
                            c.PreferredContact,
                            c.GestationPeriod,
                            c.ServiceAccess,
                            c.LivingWithPartner,
                            c.LocalHub,
                            c.Status
                        }));
                    }

                    File.WriteAllLines(CasesFilePath, lines);
                    LeftDataGrid.ItemsSource = FilteredCases;
                    MainDataGrid.ItemsSource = FilteredCases;
                    StatusDataGrid.ItemsSource = FilteredCases;
                    UpdateCaseCount();
                }
            }
        }

        // Event handler for the Archive Case button click.
        private void ArchiveCase_Click(object sender, RoutedEventArgs e)
        {
            if (MainDataGrid.SelectedItem is CaseModel selectedCase)
            {
                MessageBoxResult result = MessageBox.Show(
                    $"Are you sure you want to archive case {selectedCase.FullName}?",
                    "Archive Case", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    ArchiveReasonWindow reasonWindow = new ArchiveReasonWindow();
                    bool? dialogResult = reasonWindow.ShowDialog();
                    if (dialogResult == true && !string.IsNullOrWhiteSpace(reasonWindow.ClosureReason))
                    {
                        ArchiveCase(selectedCase, reasonWindow.ClosureReason);
                    }
                    else
                    {
                        MessageBox.Show("Archiving canceled or no reason provided.", "No reason",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a case to archive.", "No Selection",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // Archives a case by writing it to ArchivedCases.csv and removing it from active cases.
        private void ArchiveCase(CaseModel caseToArchive, string closureReason)
        {
            EnsureArchivedCasesFileExists();

            caseToArchive.Status = "Closed";
            caseToArchive.ArchiveDate = DateTime.Now.ToString("dd/MM/yyyy");
            caseToArchive.ArchiveReason = closureReason;

            string archivedLine = string.Join(",", new[]
            {
                caseToArchive.UserNumber,
                caseToArchive.FirstName,
                caseToArchive.Surname,
                caseToArchive.SubmissionDate,
                caseToArchive.SubmissionTime,
                $"\"{caseToArchive.PhoneNumber}\"",
                caseToArchive.EmailAddress,
                caseToArchive.DOB,
                caseToArchive.Age,
                caseToArchive.Postcode,
                caseToArchive.CurrentTown,
                caseToArchive.LocalAuthority,
                caseToArchive.Location,
                caseToArchive.Gender,
                caseToArchive.Ethnicity,
                caseToArchive.PreferredContact,
                caseToArchive.GestationPeriod,
                caseToArchive.ServiceAccess,
                caseToArchive.LivingWithPartner,
                caseToArchive.LocalHub,
                caseToArchive.Status,
                caseToArchive.DeadlineDate,
                caseToArchive.WellbeingReviewDate,
                caseToArchive.WBRSentDate,
                caseToArchive.FollowUpDate,
                caseToArchive.Notes,
                caseToArchive.ArchiveDate,
                caseToArchive.ArchiveReason
            });
            File.AppendAllText("ArchivedCases.csv", archivedLine + "\n");

            RemoveCaseFromActiveCsv(caseToArchive.UserNumber);
            LoadCases();
        }

        // Removes a case from the active CSV.
        private void RemoveCaseFromActiveCsv(string userNumber)
        {
            List<string> lines = File.ReadAllLines(CasesFilePath).ToList();
            for (int i = 1; i < lines.Count; i++)
            {
                string[] parts = lines[i].Split(',');
                if (parts[0].Trim() == userNumber)
                {
                    lines.RemoveAt(i);
                    break;
                }
            }
            File.WriteAllLines(CasesFilePath, lines);
        }

        // Ensures that ArchivedCases.csv exists.
        private void EnsureArchivedCasesFileExists()
        {
            string path = "ArchivedCases.csv";
            if (!File.Exists(path))
            {
                File.WriteAllText(path,
                    "UserNumber,FirstName,Surname,SubmissionDate,SubmissionTime,PhoneNumber,EmailAddress,DOB,Age,Postcode,CurrentTown,LocalAuthority,Location,Gender,Ethnicity,PreferredContact,GestationPeriod,ServiceAccess,LivingWithPartner,LocalHub,Status,DeadlineDate,WellbeingReviewDate,WBRSentDate,FollowUpDate,Notes,ArchiveDate,ArchiveReason\n");
            }
        }
    }
}
