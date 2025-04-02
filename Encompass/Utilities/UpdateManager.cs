// UpdateManager.cs
using Encompass.Models;
using Encompass.Views;
using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace Encompass.Services
{
    public static class UpdateManager
    {
        // You can maintain a list of open windows or global data collections if needed.
        public static Action GlobalUpdateAction;  // This can be set to a function that refreshes UI

        public static void UpdateAll(List<CaseModel> cases)
        {
            foreach (CaseModel c in cases)
            {
                bool hasContactAttempt = ContactAttemptService.LoadContactAttempts(c.UserNumber).Count > 0;
                var computed = CaseStatusService.CalculateStatus(
                    c.SubmissionDate,
                    c.SubmissionTime,
                    hasContactAttempt,
                    DateTime.Now
                );
                c.Status = computed.StatusText;
                c.StatusBrush = new SolidColorBrush(computed.StatusColor);
                c.ForegroundBrush = new SolidColorBrush(computed.ForegroundColor);
                c.FontWeight = computed.FontWeight;
                // If your CaseModel implements INotifyPropertyChanged,
                // updating these properties will automatically refresh the UI.
            }
            // If you have a global update action registered (e.g., refresh a DataGrid),
            // call it here.
            GlobalUpdateAction?.Invoke();
        }
    }
}
