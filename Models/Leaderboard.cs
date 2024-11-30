using Newtonsoft.Json;

namespace AocSlackBot.Models
{
    public class Leaderboard
    {
        [JsonProperty("owner_id")]
        public long OwnerId { get; set; }

        [JsonProperty("members")]
        public Dictionary<long, Member> Members { get; set; }
    }

    public class Member
    {
        [JsonProperty("local_score")]
        public int LocalScore { get; set; }

        [JsonProperty("stars")]
        public int Stars { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("last_star_ts")]
        public long LastStarTs { get; set; }

        [JsonProperty("completion_day_level")]
        public Dictionary<string, Dictionary<string, CompletionDetails>> CompletionDayLevel { get; set; }

        [JsonProperty("global_score")]
        public int GlobalScore { get; set; }
    }

    public class CompletionDetails
    {
        [JsonProperty("star_index")]
        public long StarIndex { get; set; }

        [JsonProperty("get_star_ts")]
        public long GetStarTs { get; set; }
    }
}
