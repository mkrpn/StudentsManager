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

        public async Task<bool> Init()
        {
            consoleService.WriteLine("Waiting for Google Sheets login ...");
            consoleService.WriteLine();
            await googleSheetsService.Oauth2Login();

            consoleService.WriteLine("Waiting for Zoom login ...");
            consoleService.WriteLine();
            await zoomService.Oauth2Login();

            return await ValidateStudentsMap();
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
            consoleService.WriteLine();
            var range = consoleService.EnterData("Enter sheet column to fill presense:", 
                googleSheetsService.IsColumnNameValid);

            await UpdatePresenseInGoogleSheets(range, presenseStatuses);

            consoleService.WriteLine();
            consoleService.WriteLine("Google Sheets updated!");
        }

        async Task<bool> ValidateStudentsMap()
        {
            var studentNamesFromConfig = config.GoogleSheetsToZoomMap.Select(m => m.Key).ToArray();
            var studentNamesFromGoogleSheet = await googleSheetsService.GetRangeValues(config.GoogleSheetId, GetStudentsRange());
            var notFoundInMapStudents = studentNamesFromGoogleSheet.Where(n => !config.GoogleSheetsToZoomMap.ContainsKey(n));
            var areMissingStudentsInMap = notFoundInMapStudents.Any();

            if (areMissingStudentsInMap)
            {
                consoleService.WriteErrorline("Please, update 'GoogleSheetsToZoomMap' for:");

                foreach (var studentName in notFoundInMapStudents)
                {
                    consoleService.WriteErrorline(studentName);
                }
            }

            return !areMissingStudentsInMap;
        }

        public async Task<Dictionary<string, bool>> GetLastMeetingPresence()
        {
            var lastMeetingParticipants = await zoomService.GetMeetingParticipants(config.ZoomMeetingId);

            return config.GoogleSheetsToZoomMap.ToDictionary(k => k.Key, v => lastMeetingParticipants.Contains(v.Value));
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
