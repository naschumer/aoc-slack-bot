using Microsoft.Extensions.Logging;
using SlackNet.WebApi;
using SlackNet;
using Microsoft.Extensions.Configuration;

namespace AocSlackBot.Clients
{
    public class SlackClient
    {
        private readonly ILogger<SlackClient> _logger;
        private readonly string _slackOathToken;
        private readonly string _slackWebhookUrl;
        private readonly string _slackChannelName;

        public SlackClient(
            ILogger<SlackClient> logger,
            IConfiguration configuration)
        {
            _logger = logger;
            _slackOathToken = configuration["SlackOathToken"];
            _slackWebhookUrl = configuration["SlackWebhookUrl"];
            _slackChannelName = configuration["SlackChannelName"];
        }

        public async Task SendToSlackAsync(string message)
        {
            try
            {
                var api = new SlackServiceBuilder()
                    .UseApiToken(_slackOathToken)
                    .GetApiClient();

                var slackMessage = new Message
                {
                    Text = message,
                    Channel = _slackChannelName
                };

                await api.PostToWebhook(_slackWebhookUrl, slackMessage);
                _logger.LogInformation("Message successfully sent to Slack.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send message to Slack: {ex.Message}");
            }
        }
    }
}
