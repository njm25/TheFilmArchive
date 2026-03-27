using Infrastructure.Clients;
using Microsoft.Extensions.Hosting;

namespace Api.Services;

public class DiscordBotHostedService : IHostedService
{
    private readonly DiscordBot _discordBot;

    public DiscordBotHostedService(DiscordBot discordBot)
    {
        _discordBot = discordBot;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _discordBot.StartAsync();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _discordBot.StopAsync();
    }
}