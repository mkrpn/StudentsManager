using System.ComponentModel.DataAnnotations;

namespace StudentsManager.Models
{
    public class AppSettings
    {
        public const string ConfigName = "AppSettings";

        [Required]
        public Dictionary<string, List<string>> GoogleSheetsToZoomMap { get; set; }

        [Required]
        public string GoogleSheetId { get; set; }

        [Required]
        public int StudentsPresenseRowStart { get; set; }

        [Required]
        public int StudentsPresenseRowEnd { get; set; }

        [Required]
        public string StudentsNamesColumn { get; set; }

        [Required]
        public string ZoomMeetingId { get; set; }

        [Required]
        public int AuthCodeReceivingTimeoutSeconds { get; set; }

        [Required]
        public List<string> SkipZoomCheckingNames { get; set; }

        public string PresentText { get; set; }

        public string NotPresentText { get; set; }
    }
}