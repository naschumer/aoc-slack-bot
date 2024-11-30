using AocSlackBot.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace AocSlackBot.Clients
{
    public class AocClient
    {
        private readonly ILogger<AocClient> _logger;
        private readonly HttpClient _httpClient;
        private readonly string _aocSession;
        private readonly string _leaderboardId;
        private readonly string _year;
        private const string BaseUrl = "https://adventofcode.com";

        public AocClient(ILogger<AocClient> logger, HttpClient httpClient, IConfiguration configuration)
        {
            _logger = logger;
            _httpClient = httpClient;
            _aocSession = configuration["AocSession"];
            _leaderboardId = configuration["LeaderboardId"];
            _year = DateTime.Today.Year.ToString();
        }

        public async Task<Leaderboard> GetLeaderboardAsync()
        {
            try
            {
                var url = $"{BaseUrl}/{_year}/leaderboard/private/view/{_leaderboardId}.json";
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Cookie", $"session={_aocSession}");

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var jsonString = await response.Content.ReadAsStringAsync();
                var leaderboard = JsonConvert.DeserializeObject<Leaderboard>(jsonString);

                _logger.LogInformation("Successfully fetched the leaderboard.");
                return leaderboard;
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError($"HTTP error fetching leaderboard: {httpEx.Message}. StatusCode: {httpEx.StatusCode}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching leaderboard: {ex.Message}");
                return null;
            }
        }
    }
}
