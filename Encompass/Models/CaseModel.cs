using System.Windows;
using System.Windows.Media;

namespace Encompass.Models
{
    public class CaseModel
    {
        public required string UserNumber { get; set; }
        public required string FirstName { get; set; }
        public required string Surname { get; set; }
        public string FullName => $"{FirstName} {Surname}";
        public required string SubmissionDate { get; set; }
        public required string SubmissionTime { get; set; }
        public required string PhoneNumber { get; set; }
        public required string EmailAddress { get; set; }
        public required string DOB { get; set; }
        public required string Age { get; set; }
        public required string Postcode { get; set; }
        public required string CurrentTown { get; set; }
        public required string LocalAuthority { get; set; }
        public required string Location { get; set; }
        public required string Gender { get; set; }
        public required string Ethnicity { get; set; }
        public required string PreferredContact { get; set; }
        public required string GestationPeriod { get; set; }
        public required string ServiceAccess { get; set; }
        public required string LivingWithPartner { get; set; }
        public required string LocalHub { get; set; }
        public required string Status { get; set; }
        public Brush StatusBrush { get; set; } = Brushes.White;
        public Brush ForegroundBrush { get; set; }
        public FontWeight FontWeight { get; set; }

        // New fields for the workflow:
        public string DeadlineDate { get; set; } = "";          // e.g. "dd/MM/yyyy"
        public string WellbeingReviewDate { get; set; } = "";
        public string WBRSentDate { get; set; } = "";
        public string FollowUpDate { get; set; } = "";

        public string Notes { get; set; } = "";
        public string ArchiveDate { get; set; } = "";
        public string ArchiveReason { get; set; } = "";

    }
}
