namespace Encompass.Models
{
    public class ContactAttempt
    {
        public required string UserNumber { get; set; }
        public required int AttemptNumber { get; set; }
        public required string ContactDate { get; set; } // "dd/MM/yyyy"
        public required string Method { get; set; }      // e.g. "Email"
        public string Notes { get; set; } = "";
        public string Reply { get; set; } = "";

        // Make sure these names match the CSV columns
        public string ResponseMethod { get; set; } = "";
        public string AdditionalResponseNotes { get; set; } = "";
    }
}
