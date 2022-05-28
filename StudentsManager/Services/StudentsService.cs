using StudentsManager.Models;
using Microsoft.Extensions.Options;

namespace StudentsManager.Services
{
    public class StudentsService
    {
        private readonly AppSettings config;
        private readonly GoogleSheetsService googleSheetsService;
        private readonly ZoomService zoomService;
        private readonly ConsoleService consoleService;

        public StudentsService(IOptions<AppSettings> options, GoogleSheetsService googleSheetsService, ZoomService zoomService,
            ConsoleService consoleService)
        {
            this.config = options.Value;
            this.googleSheetsService = googleSheetsService;
            this.zoomService = zoomService;
            this.consoleService = consoleService;
        }

        public async Task Init()
        {
            consoleService.WriteLine("Waiting for Google Sheets login ...");
            consoleService.WriteLine();
            await googleSheetsService.Oauth2Login();

            consoleService.WriteLine("Waiting for Zoom login ...");
            consoleService.WriteLine();
            await zoomService.Oauth2Login();

            await ValidateGoogleSheetsStudentsMap();

            var extraParticipants = await GetZoomExtraParticipants();

            if (extraParticipants.Any())
            {
                consoleService.WriteError($"Found unexpected Zoom participants: {string.Join(", ", extraParticipants)}");
                consoleService.WriteLine();
            }
        }

        private async Task<IEnumerable<string>> GetZoomExtraParticipants()
        {
            var lastMeetingParticipants = await zoomService.GetMeetingParticipants(config.ZoomMeetingId);

            return lastMeetingParticipants.Where(zoomPatricipant =>
                !config.SkipZoomCheckingNames.Contains(zoomPatricipant) &&
                !config.GoogleSheetsToZoomMap.Any(googleParticipants => googleParticipants.Value.Contains(zoomPatricipant)));
        }

        public void ShowPresence(Dictionary<string, bool> presenseStatuses)
        {
            consoleService.WriteLine("Last meeting students presence:");
            consoleService.WriteLine();

            foreach (var item in presenseStatuses)
            {
                if (item.Value)
                {
                    consoleService.WriteSuccessline($"\t{item.Key} was present");
                }
                else
                {
                    consoleService.WriteErrorline($"\t{item.Key} was not present");
                }
            }

            consoleService.WriteLine();
            consoleService.Write($"<<Summary>>: Total: {presenseStatuses.Count()}. ");
            consoleService.WriteSuccess($"Present: {presenseStatuses.Count(p => p.Value)}. ");
            consoleService.WriteError($"Absent: {presenseStatuses.Count(p => !p.Value)}.");
            consoleService.WriteLine();
        }

        public async Task MarkPresenseInGoogleSheets(Dictionary<string, bool> presenseStatuses)
        {
            var range = consoleService.EnterData("Enter sheet column to fill presense:", 
                googleSheetsService.IsColumnNameValid);

            await UpdatePresenseInGoogleSheets(range, presenseStatuses);
        }

        async Task ValidateGoogleSheetsStudentsMap()
        {
            var studentNamesFromConfig = config.GoogleSheetsToZoomMap.Select(m => m.Key).ToArray();
            var studentNamesFromGoogleSheet = await googleSheetsService.GetRangeValues(config.GoogleSheetId, GetStudentsRange());
            var notFoundInMapStudents = studentNamesFromGoogleSheet.Where(n => !config.GoogleSheetsToZoomMap.ContainsKey(n));

            if (notFoundInMapStudents.Any())
            {
                throw new Exception($"Please, update 'GoogleSheetsToZoomMap' for: {string.Join(", ", notFoundInMapStudents)}");
            }
        }

        public async Task<Dictionary<string, bool>> GetLastMeetingPresence()
        {
            var lastMeetingParticipants = await zoomService.GetMeetingParticipants(config.ZoomMeetingId);

            return config.GoogleSheetsToZoomMap.ToDictionary(k => k.Key, 
                v => v.Value.Any(mapParticipant => lastMeetingParticipants.Contains(mapParticipant)));
        }

        async Task UpdatePresenseInGoogleSheets(string columnNameToUpdate, Dictionary<string, bool> presenceDictionary)
        {
            var studentNamesFromGoogleSheet = await googleSheetsService.GetRangeValues(config.GoogleSheetId, GetStudentsRange());
            var values = studentNamesFromGoogleSheet.Select(name => presenceDictionary[name] ? 
                this.config.PresentText : this.config.NotPresentText);
            var rangeToUpdate = $"{columnNameToUpdate}{config.StudentsPresenseRowStart}:{columnNameToUpdate}{config.StudentsPresenseRowEnd}";
            
            await googleSheetsService.UpdateRange(config.GoogleSheetId, rangeToUpdate, values);
        }

        string GetStudentsRange() => 
            $"{config.StudentsNamesColumn}{config.StudentsPresenseRowStart}:{config.StudentsNamesColumn}{config.StudentsPresenseRowEnd}";
    }
}
