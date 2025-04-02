using System.Windows;

namespace Encompass.Views
{
    public partial class ArchiveReasonWindow : Window
    {
        public string? ClosureReason { get; private set; }

        public ArchiveReasonWindow()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            // Capture the typed reason
            ClosureReason = ReasonTextBox.Text.Trim();
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
