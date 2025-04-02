using System.IO;
using System.Windows;
using System.Windows.Input;

namespace Encompass.Views
{
    public partial class LoginWindow : Window
    {
        private static readonly string UserDataFile = "Users.csv";// User data file

        private static readonly string LastLoginFile = "LastLogin.txt";// Store last login

        private Dictionary<string, string> users = [];

        public LoginWindow()
        {
            InitializeComponent();
            EnsureUserFileExists();
            LoadUserData();
            LoadLastLogin(); // Load the last login on startup
        }

        private void EmailInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                LoginButton_Click(sender, e); // Call the login function when Enter is pressed
            }
        }

        private void LoadUserData()
        {
            users.Clear();
            if (File.Exists(UserDataFile))
            {
                IEnumerable<string> lines = File.ReadAllLines(UserDataFile).Skip(1); // Skip header
                foreach (string? line in lines)
                {
                    string[] parts = line.Split(',');
                    if (parts.Length == 4)
                    {
                        string firstName = parts[0].Trim();
                        string email = parts[2].Trim().ToLower(); // Email is now in position 2
                        string role = parts[3].Trim();

                        users[email] = firstName + "," + role;
                    }
                }
            }
        }

        private void LoadLastLogin()
        {
            if (File.Exists(LastLoginFile))
            {
                string lastEmail = File.ReadAllText(LastLoginFile).Trim();
                if (!string.IsNullOrEmpty(lastEmail))
                {
                    EmailInput.Text = lastEmail;
                    EmailInput.SelectAll(); // Highlights text so user can overwrite easily
                }
            }
        }

        private void SaveLastLogin(string email)
        {
            File.WriteAllText(LastLoginFile, email);
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailInput.Text.Trim().ToLower();

            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
            {
                ErrorMessage.Text = "Please enter a valid work email.";
                return;
            }

            if (users.TryGetValue(email, out string userData))
            {
                string[] parts = userData.Split(',');
                string firstName = parts[0];
                string role = parts[1];

                _ = MessageBox.Show($"Login Successful! Role: {role}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                // Save the last login email
                SaveLastLogin(email);

                // Open the main dashboard with first name and role
                MainWindow mainWindow = new(firstName, role);
                mainWindow.Show();
                Close();
            }
            else
            {
                ErrorMessage.Text = "Email not recognized. Please register.";
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            RegisterWindow registerWindow = new();
            _ = registerWindow.ShowDialog(); // Open registration window
        }

        private void EnsureUserFileExists()
        {
            if (!File.Exists(UserDataFile))
            {
                using StreamWriter writer = new(UserDataFile);
                writer.WriteLine("FirstName,Surname,Email,Role"); // CSV Header
                writer.WriteLine("Master,User,master@company.com,Master");
                writer.WriteLine("Audrey,Haggis,audrey@company.com,Admin");
                writer.WriteLine("Richard,Casey,richard@company.com,User");
            }
        }
    }
}
