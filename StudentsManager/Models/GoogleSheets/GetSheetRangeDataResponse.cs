namespace StudentsManager.Models.GoogleSheets
{
    public class GetSheetValuesResponse
    {
        public string Range { get; set; }
        public string MajorDimension { get; set; }
        public List<List<string>> Values { get; set; }
    }
}
