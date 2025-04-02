using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace Encompass.Services
{
    /// <summary>
    /// Represents status information for a case, including text, background color,
    /// foreground color, font weight, and an urgency ranking.
    /// </summary>
    public class CaseStatus
    {
        public string StatusText { get; set; }
        public Color StatusColor { get; set; }
        public Color ForegroundColor { get; set; }
        public FontWeight FontWeight { get; set; }
        public int UrgencyRanking { get; set; }
    }

    /// <summary>
    /// Provides methods for calculating the status of a case based on submission date/time
    /// and whether any contact attempts have been made.
    /// </summary>
    public static class CaseStatusService
    {
        public static bool IsWorkingDay(DateTime date)
        {
            return date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday;
        }

        public static DateTime NextWorkingDay(DateTime date)
        {
            DateTime next = date.AddDays(1);
            while (!IsWorkingDay(next))
            {
                next = next.AddDays(1);
            }
            return next;
        }

        public static DateTime AddWorkingDays(DateTime date, int workingDays)
        {
            DateTime result = date;
            while (workingDays > 0)
            {
                result = result.AddDays(1);
                if (IsWorkingDay(result))
                {
                    workingDays--;
                }
            }
            return result;
        }

        /// <summary>
        /// Calculates the case status based on submission date/time and contact attempts.
        /// </summary>
        public static CaseStatus CalculateStatus(
            string submissionDateStr,
            string submissionTimeStr,
            bool hasContactAttempt,
            DateTime currentDateTime)
        {
            // If any contact attempt exists, return a "Contact Attempt Made" status.
            if (hasContactAttempt)
            {
                return new CaseStatus
                {
                    StatusText = "Contact Attempt Made",
                    StatusColor = Colors.LightGreen,
                    ForegroundColor = Colors.Black,
                    FontWeight = FontWeights.Normal,
                    UrgencyRanking = 0
                };
            }

            // Try to parse the submission date/time; if it fails, use the current date/time.
            if (!DateTime.TryParseExact(
                    submissionDateStr + " " + submissionTimeStr,
                    "dd/MM/yyyy HH:mm",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime submissionDateTime))
            {
                submissionDateTime = currentDateTime;
            }

            // If submission time is past 17:00, treat it as the next working day.
            if (submissionDateTime.TimeOfDay > new TimeSpan(17, 0, 0))
            {
                submissionDateTime = NextWorkingDay(submissionDateTime);
            }
            else
            {
                submissionDateTime = submissionDateTime.Date;
            }

            // Calculate the due date: 5 working days after the effective submission date.
            DateTime dueDate = AddWorkingDays(submissionDateTime, 5);
            int daysRemaining = (dueDate - currentDateTime.Date).Days;

            // Overdue
            if (currentDateTime.Date > dueDate)
            {
                return new CaseStatus
                {
                    StatusText = "Awaiting Initial Contact - OVERDUE",
                    StatusColor = Colors.Red,
                    ForegroundColor = Colors.White,
                    FontWeight = FontWeights.Bold,
                    UrgencyRanking = 1
                };
            }
            // Due Today
            else if (daysRemaining == 0)
            {
                return new CaseStatus
                {
                    StatusText = "Awaiting Initial Contact - Due Today",
                    StatusColor = Colors.Red,
                    ForegroundColor = Colors.White,
                    FontWeight = FontWeights.Bold,
                    UrgencyRanking = 1
                };
            }
            // Otherwise, use a gradient for days remaining.
            else
            {
                Color chosenColor;
                Color chosenForeground = Colors.Black;
                FontWeight chosenFontWeight = FontWeights.Normal;

                switch (daysRemaining)
                {
                    case 5:
                        chosenColor = Colors.Yellow;
                        break;
                    case 4:
                        chosenColor = Colors.Gold;
                        break;
                    case 3:
                        chosenColor = Colors.Orange;
                        break;
                    case 2:
                        chosenColor = Colors.DarkOrange;
                        break;
                    case 1:
                        chosenColor = Colors.Red;
                        chosenForeground = Colors.White;
                        chosenFontWeight = FontWeights.Bold;
                        break;
                    default:
                        chosenColor = Colors.Yellow;
                        break;
                }

                return new CaseStatus
                {
                    StatusText = "Awaiting Initial Contact",
                    StatusColor = chosenColor,
                    ForegroundColor = chosenForeground,
                    FontWeight = chosenFontWeight,
                    UrgencyRanking = daysRemaining + 1
                };
            }
        }
    }
}
