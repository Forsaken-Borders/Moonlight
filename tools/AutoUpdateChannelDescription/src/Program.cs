using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;

namespace Moonlight.Tools.AutoUpdateChannelDescription
{
    public sealed class Program
    {
        public static async Task Main()
        {
            string token = Environment.GetEnvironmentVariable("DISCORD_TOKEN") ?? throw new InvalidOperationException("DISCORD_TOKEN environment variable is not set.");
            string guildId = Environment.GetEnvironmentVariable("DISCORD_GUILD_ID") ?? throw new InvalidOperationException("DISCORD_GUILD_ID environment variable is not set.");
            string channelId = Environment.GetEnvironmentVariable("DISCORD_CHANNEL_ID") ?? throw new InvalidOperationException("DISCORD_CHANNEL_ID environment variable is not set.");
            string channelTopic = Environment.GetEnvironmentVariable("DISCORD_CHANNEL_TOPIC") ?? throw new InvalidOperationException("DISCORD_DESCRIPTION environment variable is not set.");
            string nugetUrl = Environment.GetEnvironmentVariable("NUGET_URL") ?? throw new InvalidOperationException("NUGET_URL environment variable is not set.");
            string githubUrl = Environment.GetEnvironmentVariable("GITHUB_URL") ?? throw new InvalidOperationException("GITHUB_URL environment variable is not set.");
            string? latestStableVersion = nugetUrl + "/" + Environment.GetEnvironmentVariable("LATEST_STABLE_VERSION");
            string? latestNightlyVersion = nugetUrl + "/" + Environment.GetEnvironmentVariable("LATEST_NIGHTLY_VERSION");

            DiscordClient client = new(new DiscordConfiguration
            {
                Token = token,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.All
            });

            client.GuildDownloadCompleted += async (client, eventArgs) =>
            {
                DiscordGuild guild = client.Guilds[ulong.Parse(guildId, NumberStyles.Number, CultureInfo.InvariantCulture)];
                DiscordChannel channel = guild.Channels[ulong.Parse(channelId, NumberStyles.Number, CultureInfo.InvariantCulture)];

                // Attempt to use the channel topic as a "cache" when the versions are not provided.
                string[] channelTopicLines = channel.Topic.Split('\n');
                if (string.IsNullOrWhiteSpace(latestStableVersion) && channelTopicLines.Any())
                {
                    latestStableVersion = channelTopicLines.FirstOrDefault(x => x.StartsWith(Formatter.Bold("Latest stable version")))?.Split(' ').Last() ?? "Unreleased.";
                }

                if (string.IsNullOrWhiteSpace(latestNightlyVersion) && channelTopicLines.Any())
                {
                    latestNightlyVersion = channelTopicLines.FirstOrDefault(x => x.StartsWith(Formatter.Bold("Latest nightly version")))?.Split(' ').Last() ?? "Unreleased.";
                }

                StringBuilder builder = new(channelTopic);
                builder.AppendLine();
                builder.AppendLine($"{Formatter.Bold("GitHub")}: {githubUrl}");
                builder.AppendLine($"{Formatter.Bold("NuGet")}: {nugetUrl}");
                builder.AppendLine($"{Formatter.Bold("Latest stable version")}: {latestStableVersion}");
                builder.AppendLine($"{Formatter.Bold("Latest nightly version")}: {latestNightlyVersion}");

                try
                {
                    await channel.ModifyAsync(channel =>
                    {
                        channel.AuditLogReason = $"Updating channel topic to match stable version {latestStableVersion} and nightly version {latestNightlyVersion}.";
                        channel.Topic = builder.ToString();
                    });
                }
                catch (DiscordException error)
                {
                    Console.WriteLine($"Error: HTTP {error.WebResponse.ResponseCode}, {error.WebResponse.Response}");
                    Environment.Exit(1);
                }

                Environment.Exit(0);
            };

            await client.ConnectAsync();

            // The program should exit ASAP after the channel description is updated.
            // However it may get caught in a ratelimit, so we'll wait for a bit.
            // The program will exit after 30 seconds no matter what.
            // This includes the time it takes to connect to the Discord gateway.
            await Task.Delay(TimeSpan.FromSeconds(30));
        }
    }
}
