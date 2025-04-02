using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Encompass.Views
{
    public partial class RegisterWindow : Window
    {
        private static readonly string UserDataFile = "Users.csv";

        public RegisterWindow()
        {
            InitializeComponent();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string firstName = FirstNameInput.Text.Trim();
            string surname = SurnameInput.Text.Trim();
            string email = RegisterEmailInput.Text.Trim().ToLower();
            string role = (RoleDropdown.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "User";

            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(surname) || !email.Contains("@"))
            {
                RegisterMessage.Text = "Please enter valid details.";
                return;
            }

            if (!File.Exists(UserDataFile))
            {
                File.WriteAllText(UserDataFile, "First Name,Surname,Email,Role\n"); // Add header
            }

            // Check for duplicate email
            string[] lines = File.ReadAllLines(UserDataFile);
            foreach (string? line in lines.Skip(1)) // Skip header
            {
                string[] parts = line.Split(',');
                if (parts.Length > 2 && parts[2].Trim().Equals(email, StringComparison.OrdinalIgnoreCase))
                {
                    RegisterMessage.Text = "This email is already registered.";
                    return;
                }
            }

            // Append new user data
            File.AppendAllText(UserDataFile, $"{firstName},{surname},{email},{role}{Environment.NewLine}");

            _ = MessageBox.Show("Registration successful! You can now log in.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            Close();
        }
    }
}
