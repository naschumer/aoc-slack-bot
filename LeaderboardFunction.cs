using AocSlackBot.Clients;
using ConsoleTables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AocSlackBot
{
    public class LeaderboardFunction
    {
        private readonly ILogger<LeaderboardFunction> _logger;
        private readonly AocClient _aocClient;
        private readonly SlackClient _slackClient;

        public LeaderboardFunction(
            ILoggerFactory loggerFactory, 
            AocClient aocClient, 
            SlackClient slackClient)
        {
            _logger = loggerFactory.CreateLogger<LeaderboardFunction>();
            _aocClient = aocClient;
            _slackClient = slackClient;
        }

        [Function("LeaderboardFunction")]
        public async Task RunAsync([TimerTrigger("0 0 14 * 12 *", RunOnStartup = false)] TimerInfo myTimer)
        {
            try
            {
                var leaderboard = await _aocClient.GetLeaderboardAsync();
                if (leaderboard == null || leaderboard.Members == null || !leaderboard.Members.Any())
                {
                    _logger.LogWarning("No leaderboard data found.");
                    return; 
                }

                var sortedMembers = leaderboard.Members
                    .OrderByDescending(x => x.Value.LocalScore)
                    .Where(x => x.Value.Stars > 0)
                    .Select((x, i) => new string[] { (i + 1).ToString(), x.Value.Name, x.Value.Stars.ToString(), x.Value.LocalScore.ToString() })
                    .ToList();

                if (!sortedMembers.Any())
                {
                    _logger.LogWarning("No members with stars to display.");
                    return;
                }

                var leaderboardTable = GenerateLeaderboardTable(sortedMembers);
                var slackMessage = FormatSlackMessage(leaderboardTable);

                _logger.LogInformation("Leaderboard successfully generated.");
                await _slackClient.SendToSlackAsync(slackMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while processing the leaderboard: {ex.Message}");
            }
        }

        private string GenerateLeaderboardTable(List<string[]> sortedMembers)
        {
            var table = new ConsoleTable("", "User", "Stars", "Score");
            foreach (var member in sortedMembers)
            {
                table.AddRow(member);
            }

            return table.ToMarkDownString();
        }

        private string FormatSlackMessage(string leaderboardTable)
        {
            return ":trophy: *Current Leaderboard* :trophy:" + $"```\n{leaderboardTable}```";
        }
    }
}
