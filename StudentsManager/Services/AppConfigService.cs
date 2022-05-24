namespace StudentsManager.Services
{
    public class AppConfigService
    {
        public void AddOrUpdateAppSetting<T>(string key, T value)
        {
            var filePath = Path.Combine(AppContext.BaseDirectory, "appSettings.json");
            string json = File.ReadAllText(filePath);
            dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);

            var sectionPath = ToLowerFirstChar(key.Split(":")[0]);

            if (!string.IsNullOrEmpty(sectionPath))
            {
                var keyPath = ToLowerFirstChar(key.Split(":")[1]);
                jsonObj[sectionPath][keyPath] = value;
            }
            else
            {
                jsonObj[sectionPath] = value;
            }

            string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(filePath, output);
        }

        string ToLowerFirstChar(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return char.ToLower(input[0]) + input.Substring(1);
        }
    }
}
