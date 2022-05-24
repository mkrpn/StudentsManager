using StudentsManager.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StudentsManager.Models.Zoom;
using StudentsManager.Models.GoogleSheets;

namespace StudentsManager.Services
{
    public static class AppSetupService
    {
        public static ServiceProvider ConfigureServices()
        {
            var configBuilder = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

            var services = new ServiceCollection();

            services
                .AddSingleton<StudentsService>()
                .AddSingleton<ConsoleService>()
                .AddSingleton<HttpService>()
                .AddSingleton<GoogleSheetsService>()
                .AddSingleton<ZoomService>()
                .AddTransient<Oauth2TokensService>()
                .AddSingleton<AppConfigService>();

            services.AddOptions<ZoomConfig>().Bind(configBuilder.GetRequiredSection(ZoomConfig.configName)).ValidateDataAnnotations();
            services.AddOptions<GoogleSheetsConfig>().Bind(configBuilder.GetRequiredSection(GoogleSheetsConfig.configName)).ValidateDataAnnotations();
            services.AddOptions<AppSettings>().Bind(configBuilder.GetRequiredSection(AppSettings.ConfigName)).ValidateDataAnnotations();

            return services.BuildServiceProvider();
        }
    }
}
