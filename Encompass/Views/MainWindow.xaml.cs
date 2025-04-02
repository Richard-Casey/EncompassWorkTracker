using Encompass.Views;
using System.Windows;

namespace Encompass
{
    public partial class MainWindow : Window
    {
        private string firstName;

        private string role;

        public MainWindow(string firstName, string role)
        {
            InitializeComponent();  // Ensure this is present
            this.firstName = firstName;
            this.role = role;
            WelcomeMessage.Text = $"Welcome, {firstName}! Your role: {role}";
        }

        private void Home_Click(object sender, RoutedEventArgs e)
        {
            _ = MessageBox.Show("Home clicked! (Feature coming soon)", "Navigation");
        }

        private void Cases_Click(object sender, RoutedEventArgs e)
        {
            CasesWindow casesWindow = new();
            casesWindow.Show();
        }

        private void Reports_Click(object sender, RoutedEventArgs e)
        {
            _ = MessageBox.Show("Reports clicked! (Feature coming soon)", "Navigation");
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            _ = MessageBox.Show("Settings clicked! (Feature coming soon)", "Navigation");
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new();
            loginWindow.Show();
            Close();
        }
    }
}
