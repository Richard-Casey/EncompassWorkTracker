using Encompass.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Encompass.Services
{
    public static class ContactAttemptService
    {
        private static readonly string FilePath = "ContactAttempts.csv";

        public static void EnsureFileExists()
        {
            if (!File.Exists(FilePath))
            {
                // 8 columns total now
                File.WriteAllText(FilePath,
                 "UserNumber,AttemptNumber,ContactDate,Method,Notes,Reply,ResponseMethod,AdditionalResponseNotes\n");
            }
        }

        public static void SaveContactAttempt(ContactAttempt attempt)
        {
            EnsureFileExists();
            // 8 columns in the CSV line
            string line = $"{attempt.UserNumber},{attempt.AttemptNumber},{attempt.ContactDate}," +
                          $"{attempt.Method},{attempt.Notes},{attempt.Reply}," +
                          $"{attempt.ResponseMethod},{attempt.AdditionalResponseNotes}";
            File.AppendAllText(FilePath, line + "\n");
        }

        public static List<ContactAttempt> LoadContactAttempts(string userNumber)
        {
            EnsureFileExists();
            return File.ReadAllLines(FilePath)
                       .Skip(1)
                       .Select(line =>
                       {
                           var parts = line.Split(',');
                           // Pad any short lines
                           while (parts.Length < 8) parts = parts.Append("").ToArray();

                           return new ContactAttempt
                           {
                               UserNumber = parts[0].Trim(),
                               AttemptNumber = int.TryParse(parts[1], out int num) ? num : 0,
                               ContactDate = parts[2].Trim(),
                               Method = parts[3].Trim(),
                               Notes = parts[4].Trim(),
                               Reply = parts[5].Trim(),
                               ResponseMethod = parts[6].Trim(),
                               AdditionalResponseNotes = parts[7].Trim()
                           };
                       })
                       .Where(a => a.UserNumber == userNumber)
                       .ToList();
        }

        public static void UpdateAttempt(string userNumber, int attemptNumber, ContactAttempt updated)
        {
            EnsureFileExists();
            var lines = File.ReadAllLines(FilePath).ToList();

            for (int i = 1; i < lines.Count; i++)
            {
                var parts = lines[i].Split(',');
                while (parts.Length < 8) parts = parts.Append("").ToArray();

                if (parts[0].Trim() == userNumber &&
                    int.TryParse(parts[1], out int oldNum) &&
                    oldNum == attemptNumber)
                {
                    // Rebuild the line
                    lines[i] = $"{updated.UserNumber},{updated.AttemptNumber},{updated.ContactDate}," +
                               $"{updated.Method},{updated.Notes},{updated.Reply}," +
                               $"{updated.ResponseMethod},{updated.AdditionalResponseNotes}";
                    break;
                }
            }

            File.WriteAllLines(FilePath, lines);
        }
    }
}
