using Newtonsoft.Json;

namespace StudentsManager.Models.Zoom
{
    public class ParticipantsResponse
    {
        [JsonProperty("Id")]
        public string Id { get; set; }

        [JsonProperty("page_count")]
        public int PageCount { get; set; }

        [JsonProperty("page_ize")]
        public int PageSize { get; set; }

        [JsonProperty("total_records")]
        public int TotalRecords { get; set; }

        [JsonProperty("next_page_token")]
        public string NextPageToken { get; set; }

        [JsonProperty("participants")]
        public List<Participant> Participants { get; set; }
    }

    public class Participant
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("user_email")]
        public string Email { get; set; }
    }
}

