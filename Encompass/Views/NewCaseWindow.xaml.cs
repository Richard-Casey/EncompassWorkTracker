using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Mail;     // For email validation
using System.Text.RegularExpressions;  // For phone/time checks
using System.Windows;
using System.Windows.Controls;
using System.Threading;
using System.Diagnostics;

namespace Encompass.Views
{
    public partial class NewCaseWindow : Window
    {
        private int currentStep = 1;
        private string computedLocalHub = "";
        private string CasesFilePath = "Cases.csv";
        private CancellationTokenSource _cts;

        public NewCaseWindow()
        {
            InitializeComponent();
            // Default Submission Date to today's date
            SubmissionDatePicker.SelectedDate = DateTime.Now.Date;

            UpdateStep();
            ValidateStep1();
            Loaded += (s, e) => ValidateStep1();
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            // If we are leaving Step 1, ensure phone/email are valid
            if (currentStep == 1)
            {
                // If you also want to block time errors in Step 1, you can do:
                // string normalizedTime = NormalizeTime(SubmissionTimeBox.Text);
                // if (normalizedTime == "Invalid") { ... block user ... }

                if (!ValidatePhoneNumber(PhoneBox.Text))
                {
                    MessageBox.Show("Please enter a valid UK phone number (mobile: 07..., landline: 0...), or leave it blank.",
                                    "Invalid Phone Number", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return; // do not advance
                }
                if (!ValidateEmail(EmailBox.Text))
                {
                    MessageBox.Show("Please enter a valid email address, or leave it blank if not applicable.",
                                    "Invalid Email", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return; // do not advance
                }
            }

            // Now we can safely go to the next step
            currentStep++;

            // When leaving Step 1 and going to Step 2, compute localHub
            if (currentStep == 2)
            {
                string postcode = PostcodeBox.Text.Trim().Replace(" ", "").ToUpper();
                computedLocalHub = DetermineLocalHub(postcode);
                Debug.WriteLine($"DEBUG: computedLocalHub set to [{computedLocalHub}]");
            }

            UpdateStep();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            currentStep--;
            UpdateStep();
        }

        private void ValidateStep1(object? sender = null, TextChangedEventArgs? e = null)
        {
            if (UserNumberBox == null || FirstNameBox == null || PostcodeBox == null || NextButton == null)
            {
                return; // Prevent errors during initialization
            }

            bool isValid = !string.IsNullOrWhiteSpace(UserNumberBox.Text) &&
                           !string.IsNullOrWhiteSpace(FirstNameBox.Text) &&
                           !string.IsNullOrWhiteSpace(PostcodeBox.Text);

            NextButton.IsEnabled = isValid;
        }

        private void ValidateStep2()
        {
            bool isValid = DOBPicker.SelectedDate != null &&
                           !string.IsNullOrWhiteSpace(AgeBox.Text) &&
                           GenderDropdown.SelectedIndex > 0 &&
                           EthnicityDropdown.SelectedIndex > 0;

            NextButton.IsEnabled = isValid;
        }

        private void DOBPicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DOBPicker.SelectedDate.HasValue)
            {
                int age = DateTime.Now.Year - DOBPicker.SelectedDate.Value.Year;
                if (DateTime.Now.DayOfYear < DOBPicker.SelectedDate.Value.DayOfYear)
                {
                    age--;
                }
                AgeBox.Text = age.ToString();
            }
        }

        private void ValidateContactMethods(object sender, RoutedEventArgs e)
        {
            bool requiresPhone = PhoneCallCheck.IsChecked == true;
            bool requiresEmail = TeamsCheck.IsChecked == true || ZoomCheck.IsChecked == true;

            if (requiresPhone && string.IsNullOrWhiteSpace(PhoneBox.Text))
            {
                _ = MessageBox.Show("Phone number is required for selected contact method.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            if (requiresEmail && string.IsNullOrWhiteSpace(EmailBox.Text))
            {
                _ = MessageBox.Show("Email is required for selected contact method.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ToggleOtherContactMethod(object sender, RoutedEventArgs e)
        {
            OtherContactBox.IsEnabled = OtherCheck.IsChecked == true;
        }

        private void CalculateAge(object sender, SelectionChangedEventArgs e)
        {
            if (DOBPicker.SelectedDate.HasValue)
            {
                DateTime dob = DOBPicker.SelectedDate.Value;
                int age = DateTime.Now.Year - dob.Year;
                if (DateTime.Now.DayOfYear < dob.DayOfYear)
                {
                    age--;
                }
                AgeBox.Text = age.ToString();
            }
        }

        private async void FetchPostcodeDetails(object sender, TextChangedEventArgs e)
        {
            string postcode = PostcodeBox.Text.Trim().Replace(" ", "").ToUpper();
            if (string.IsNullOrWhiteSpace(postcode) || postcode.Length < 5)
            {
                return;
            }

            _cts?.Cancel(); // Cancel any previous request
            _cts = new CancellationTokenSource();
            CancellationToken token = _cts.Token;

            try
            {
                await System.Threading.Tasks.Task.Delay(600, token); // Wait before triggering request

                if (token.IsCancellationRequested)
                {
                    return;
                }

                using HttpClient client = new();
                HttpResponseMessage response = await client.GetAsync($"https://api.postcodes.io/postcodes/{postcode}", token);
                string jsonResponse = await response.Content.ReadAsStringAsync(token);

                if (response.IsSuccessStatusCode)
                {
                    dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonResponse);
                    Dispatcher.Invoke(() =>
                    {
                        TownBox.Text = data.result.admin_ward ?? "Unknown";
                        LocalAuthorityBox.Text = data.result.admin_district ?? "Unknown";
                        LocationBox.Text = data.result.parish ?? "Unknown";
                    });
                }
                else
                {
                    Dispatcher.Invoke(() =>
                    {
                        TownBox.Text = "Invalid Postcode";
                        LocalAuthorityBox.Text = "Invalid Postcode";
                        LocationBox.Text = "Invalid Postcode";
                    });
                }
            }
            catch (System.Threading.Tasks.TaskCanceledException) { /* Do nothing - expected if typing resumed */ }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    TownBox.Text = "Error";
                    LocalAuthorityBox.Text = "Error";
                    LocationBox.Text = "Error";
                });
                _ = MessageBox.Show($"Failed to retrieve postcode details.\nError: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            ValidateStep1();
        }

        private void ValidateStep3()
        {
            bool isValid = (PhoneCallCheck.IsChecked == true && !string.IsNullOrWhiteSpace(PhoneBox.Text)) ||
                           ((TeamsCheck.IsChecked == true || ZoomCheck.IsChecked == true) &&
                            (!string.IsNullOrWhiteSpace(PhoneBox.Text) || !string.IsNullOrWhiteSpace(EmailBox.Text))) &&
                           GestationDropdown.SelectedIndex > 0 &&
                           ServiceAccessDropdown.SelectedIndex > 0 &&
                           LivingWithPartnerDropdown.SelectedIndex > 0;

            NextButton.IsEnabled = isValid;
        }

        private void UpdateStep()
        {
            Step1Panel.Visibility = currentStep == 1 ? Visibility.Visible : Visibility.Collapsed;
            Step2Panel.Visibility = currentStep == 2 ? Visibility.Visible : Visibility.Collapsed;
            Step3Panel.Visibility = currentStep == 3 ? Visibility.Visible : Visibility.Collapsed;

            Step1Indicator.Foreground = currentStep == 1 ? System.Windows.Media.Brushes.Blue : System.Windows.Media.Brushes.Gray;
            Step2Indicator.Foreground = currentStep == 2 ? System.Windows.Media.Brushes.Blue : System.Windows.Media.Brushes.Gray;
            Step3Indicator.Foreground = currentStep == 3 ? System.Windows.Media.Brushes.Blue : System.Windows.Media.Brushes.Gray;

            BackButton.IsEnabled = currentStep > 1;
            NextButton.Visibility = currentStep < 3 ? Visibility.Visible : System.Windows.Visibility.Collapsed;
            SubmitButton.Visibility = currentStep == 3 ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
        }

        // -----------------------------------------
        //  Additional Validations / Normalizations
        // -----------------------------------------

        private bool ValidatePhoneNumber(string phone)
        {
            // Optional field. If empty => no check
            string trimmed = phone.Replace(" ", "");
            if (string.IsNullOrWhiteSpace(trimmed)) return true;

            // Quick check for a mobile: starts with '07' and length=11
            if (trimmed.StartsWith("07") && trimmed.Length == 11)
            {
                return true;
            }
            // Possibly a landline: starts with '0' => 10 or 11 digits total
            if (trimmed.StartsWith("0") && (trimmed.Length == 10 || trimmed.Length == 11))
            {
                return true;
            }
            return false;
        }

        private bool ValidateEmail(string email)
        {
            // Optional field. If empty => no check
            if (string.IsNullOrWhiteSpace(email)) return true;
            try
            {
                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private string NormalizeTime(string input)
        {
            // e.g. "932" => "09:32", "130" => "01:30"
            string digits = input.Replace(":", "").Trim();
            if (!Regex.IsMatch(digits, @"^\d+$")) return "Invalid";

            if (digits.Length == 3)
            {
                string hStr = digits.Substring(0, 1);
                string mStr = digits.Substring(1, 2);
                if (int.TryParse(hStr, out int h) && int.TryParse(mStr, out int m))
                {
                    if (h >= 0 && h < 24 && m >= 0 && m < 60)
                    {
                        return $"{h:00}:{m:00}";
                    }
                }
                return "Invalid";
            }
            else if (digits.Length == 4)
            {
                string hStr = digits.Substring(0, 2);
                string mStr = digits.Substring(2, 2);
                if (int.TryParse(hStr, out int h) && int.TryParse(mStr, out int m))
                {
                    if (h >= 0 && h < 24 && m >= 0 && m < 60)
                    {
                        return $"{h:00}:{m:00}";
                    }
                }
                return "Invalid";
            }
            else
            {
                return "Invalid";
            }
        }

        // -----------------------------------------
        //  DetermineLocalHub logic (unchanged)
        // -----------------------------------------
        private string DetermineLocalHub(string postcode)
        {
            string pc = postcode.Replace(" ", "").ToUpper();
            Debug.WriteLine($"DEBUG: Raw postcode: [{postcode}], after cleanup: [{pc}] (length={pc.Length})");

            string[] northEast = { "CO1", "CO2", "CO3", "CO4", "CO5", "CO6", "CO7", "CO8", "CO9", "CO10", "CO11", "CO12", "CO13", "CO14", "CO15", "CO16" };
            string[] northWest = { "CM16", "CM17", "CM18", "CM19", "CM20", "CM21", "CM22", "CM23", "CM24", "CB10", "CB11", "EN9", "IG10" };
            string[] midEssex = { "CM0", "CM1", "CM2", "CM3", "CM4", "CM5", "CM6", "CM7", "CM8", "CM9" };
            string[] southWest = { "SS13", "SS14", "SS15", "SS16", "SS11", "SS12", "CM11", "CM12", "CM13", "CM14", "CM15", "RM15", "RM16", "RM17", "RM18", "RM19", "RM20", "CM4" };
            string[] southEast = { "SS0", "SS1", "SS2", "SS3", "SS4", "SS5", "SS6", "SS7", "SS8", "SS9", "SS11" };

            Debug.WriteLine("DEBUG: midEssex array: " + string.Join(",", midEssex));

            if (pc.Length >= 4)
            {
                string code4 = pc[..4];
                Debug.WriteLine($"DEBUG: Checking code4: [{code4}]");
                if (northEast.Contains(code4)) { Debug.WriteLine($"DEBUG: Matched northEast for {code4}"); return "North Essex (North East)"; }
                if (northWest.Contains(code4)) { Debug.WriteLine($"DEBUG: Matched northWest for {code4}"); return "North Essex (North West)"; }
                if (midEssex.Contains(code4)) { Debug.WriteLine($"DEBUG: Matched midEssex for {code4}"); return "Mid Essex"; }
                if (southWest.Contains(code4)) { Debug.WriteLine($"DEBUG: Matched southWest for {code4}"); return "South Essex (South West)"; }
                if (southEast.Contains(code4)) { Debug.WriteLine($"DEBUG: Matched southEast for {code4}"); return "South Essex (South East)"; }
            }

            if (pc.Length >= 3)
            {
                string code3 = pc[..3];
                Debug.WriteLine($"DEBUG: Checking code3: [{code3}]");
                if (northEast.Contains(code3)) { Debug.WriteLine($"DEBUG: Matched northEast for {code3}"); return "North Essex (North East)"; }
                if (northWest.Contains(code3)) { Debug.WriteLine($"DEBUG: Matched northWest for {code3}"); return "North Essex (North West)"; }
                if (midEssex.Contains(code3)) { Debug.WriteLine($"DEBUG: Matched midEssex for {code3}"); return "Mid Essex"; }
                if (southWest.Contains(code3)) { Debug.WriteLine($"DEBUG: Matched southWest for {code3}"); return "South Essex (South West)"; }
                if (southEast.Contains(code3)) { Debug.WriteLine($"DEBUG: Matched southEast for {code3}"); return "South Essex (South East)"; }
            }

            Debug.WriteLine("DEBUG: No match => Unknown");
            return "Unknown";
        }

        // -----------------------------------------
        //  Submit_Click
        // -----------------------------------------
        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            // 1) Validate time
            string normalizedTime = NormalizeTime(SubmissionTimeBox.Text);
            if (normalizedTime == "Invalid")
            {
                MessageBox.Show("Please enter a valid 24-hour time (e.g. 09:32, 1345 => 13:45).",
                    "Invalid Time", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2) Validate phone
            if (!ValidatePhoneNumber(PhoneBox.Text))
            {
                MessageBox.Show("Please enter a valid UK phone number (mobile: 07..., landline: 0...), or leave it blank.",
                    "Invalid Phone Number", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 3) Validate email
            if (!ValidateEmail(EmailBox.Text))
            {
                MessageBox.Show("Please enter a valid email address, or leave it blank if not applicable.",
                    "Invalid Email", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // If all validations pass, proceed to build CSV line
            string userNumber = UserNumberBox.Text.Trim();
            string firstName = FirstNameBox.Text.Trim();
            string surname = SurnameBox.Text.Trim();
            string submissionDate = SubmissionDatePicker.SelectedDate?.ToString("dd/MM/yyyy") ?? "Not Provided";
            // Use the normalized 24-hour time
            string submissionTime = normalizedTime;

            string phone = PhoneBox.Text.Trim();
            string email = EmailBox.Text.Trim();
            string dob = DOBPicker.Text;
            string age = AgeBox.Text.Trim();
            string postcode = PostcodeBox.Text.Trim().Replace(" ", "").ToUpper();
            Debug.WriteLine($"DEBUG: In Submit_Click, postcode = [{postcode}]");

            string gender = ((ComboBoxItem)GenderDropdown.SelectedItem)?.Content.ToString();
            string ethnicity = ((ComboBoxItem)EthnicityDropdown.SelectedItem)?.Content.ToString();
            string preferredContact = PreferredContactString();
            string gestation = ((ComboBoxItem)GestationDropdown.SelectedItem)?.Content.ToString();
            string serviceAccess = ((ComboBoxItem)ServiceAccessDropdown.SelectedItem)?.Content.ToString();
            string livingWithPartner = ((ComboBoxItem)LivingWithPartnerDropdown.SelectedItem)?.Content.ToString();

            // Use the computed value from Next_Click
            string localHub = computedLocalHub;
            Debug.WriteLine($"DEBUG: localHub (from computedLocalHub) = [{localHub}]");

            string csvLine = string.Join(",", new[]
            {
                userNumber,       // [0]
                firstName,        // [1]
                surname,          // [2]
                submissionDate,   // [3]
                submissionTime,   // [4]
                phone,            // [5]
                email,            // [6]
                dob,              // [7]
                age,              // [8]
                postcode,         // [9]
                TownBox.Text,     // [10] CurrentTown
                LocalAuthorityBox.Text, // [11]
                LocationBox.Text, // [12]
                gender,           // [13]
                ethnicity,        // [14]
                preferredContact, // [15]
                gestation,        // [16]
                serviceAccess,    // [17]
                livingWithPartner,// [18]
                localHub,         // [19]
                "",               // index 20 => Status blank for new
                "",               // index 21 => DeadlineDate (blank for new)
                "",               // index 22 => WellbeingReviewDate (blank)
                "",               // index 23 => WBRSentDate
                ""                // index 24 => FollowUpDate
            });
            Debug.WriteLine("DEBUG: CSV line: " + csvLine);

            using (StreamWriter writer = new StreamWriter(CasesFilePath, true))
            {
                writer.WriteLine(csvLine);
            }

            MessageBox.Show("New case added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            Close();
        }

        private string PreferredContactString()
        {
            var methods = new System.Collections.Generic.List<string>();

            if (PhoneCallCheck.IsChecked == true)
            {
                methods.Add("Phone Call");
            }
            if (TeamsCheck.IsChecked == true)
            {
                methods.Add("Microsoft Teams");
            }
            if (ZoomCheck.IsChecked == true)
            {
                methods.Add("Zoom");
            }
            if (OtherCheck.IsChecked == true && !string.IsNullOrWhiteSpace(OtherContactBox.Text))
            {
                methods.Add(OtherContactBox.Text);
            }
            return string.Join(" / ", methods);
        }
    }
}
