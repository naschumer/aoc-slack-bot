using AocSlackBot.Clients;
using AocSlackBot.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AocSlackBot
{
    public class StarFunction
    {
        private readonly ILogger _logger;
        private readonly AocClient _aocClient;
        private readonly AzureBlobClient _azureBlobClient;
        private readonly string _leaderboardJson;
        private readonly SlackClient _slackClient;

        public StarFunction(
            ILoggerFactory loggerFactory,
            AocClient aocClient,
            AzureBlobClient azureBlobClient,
            SlackClient slackClient)
        {
            _logger = loggerFactory.CreateLogger<StarFunction>();
            _aocClient = aocClient;
            _azureBlobClient = azureBlobClient;
            _leaderboardJson = $"leaderboard{DateTime.Now.Year}.json";
            _slackClient = slackClient;
        }

        [Function("StarFunction")]
        public async Task Run([TimerTrigger("0 */15 * * 12 *", RunOnStartup = false)] TimerInfo myTimer)
        {
            try
            {
                var localFilePath = Path.Combine(Directory.GetCurrentDirectory(), _leaderboardJson);

                var newLeaderboard = await _aocClient.GetLeaderboardAsync();
                var oldLeaderboard = await _azureBlobClient.DownloadLeaderboardBlobAsync(localFilePath);

                if(oldLeaderboard == null)
                {
                    _logger.LogInformation("First time run for this year. Uploading leaderboard.");
                    await _azureBlobClient.UploadLeaderboardBlobAsync(newLeaderboard, localFilePath);
                }
                else
                {
                    var messages = GenerateStarMessages(newLeaderboard, oldLeaderboard);

                    if (messages.Any())
                    {
                        await _azureBlobClient.UploadLeaderboardBlobAsync(newLeaderboard, localFilePath);
                        var outputMessage = string.Join(Environment.NewLine, messages);
                        await _slackClient.SendToSlackAsync(outputMessage);
                        _logger.LogInformation(outputMessage);
                    }
                    else
                    {
                        _logger.LogInformation("No new stars to report.");
                    }
                }


            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.ToString());  
                _logger.LogError(ex, "An error occurred while processing the leaderboard.");
            }
        }

        private List<string> GenerateStarMessages(Leaderboard leaderboard, Leaderboard oldLeaderboard)
        {
            var messages = new List<string>();

            foreach (var memberIndex in leaderboard.Members.OrderBy(x => x.Value.Name))
            {
                var member = memberIndex.Value;
                var oldLeaderboardMember = oldLeaderboard.Members
                    .FirstOrDefault(x => x.Key == memberIndex.Key).Value;

                foreach (var day in member.CompletionDayLevel.OrderBy(x => x.Key))
                {
                    foreach (var part in new[] { "1", "2" })
                    {
                        if (day.Value.ContainsKey(part) &&
                            (oldLeaderboardMember == null || !oldLeaderboardMember.CompletionDayLevel.ContainsKey(day.Key) ||
                             !oldLeaderboardMember.CompletionDayLevel[day.Key].ContainsKey(part)))
                        {
                            var starMessage = $"*{member.Name}* received {(part == "1" ? ":star:" : ":star::star:")} for completing Day {day.Key} Part {part} :tada:";
                            messages.Add(starMessage);
                        }
                    }
                }
            }

            return messages;
        }

    }
}
