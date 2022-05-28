using StudentsManager.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

try
{
    var serviceProvider = AppSetupService.ConfigureServices();
    var studentsService = serviceProvider.GetRequiredService<StudentsService>();
    var consoleService = serviceProvider.GetRequiredService<ConsoleService>();

    consoleService.WriteLine("Welcome to Students manager!");
    consoleService.WriteLine();

    await studentsService.Init();

    var lastMeetingPresense = await studentsService.GetLastMeetingPresence();

    studentsService.ShowPresence(lastMeetingPresense);

    consoleService.WriteLine();

    await studentsService.MarkPresenseInGoogleSheets(lastMeetingPresense);

    consoleService.WriteLine();
    consoleService.WriteLine("Google Sheets updated!");
}
catch(OptionsValidationException ex)
{
    Console.WriteLine("Please, setup appSettings.json properly.");
    Console.WriteLine();
    Console.WriteLine("Error: " + ex.Message.ToString());
}
catch (Exception ex)
{
    Console.WriteLine(ex.ToString());
}
finally
{
    Console.WriteLine();
    Console.WriteLine("Press any key to exit ...");
    Console.ReadLine();
}