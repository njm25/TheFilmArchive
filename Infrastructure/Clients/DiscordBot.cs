using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Clients;

public class DiscordBot
{
    private readonly DiscordSocketClient _client;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly string _token;
    private readonly ulong _ownerUserId;

    public DiscordBot(
        string token,
        ulong ownerUserId,
        IServiceScopeFactory scopeFactory
    )
    {
        _token = token;
        _ownerUserId = ownerUserId;
        _scopeFactory = scopeFactory;

        _client = new DiscordSocketClient(new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.DirectMessages 
        });

        _client.Ready += OnReady;
    }

    public async Task StartAsync()
    {
        await _client.LoginAsync(TokenType.Bot, _token);
        await _client.StartAsync();
    }

    public async Task StopAsync()
    {
        await _client.StopAsync();
        await _client.LogoutAsync();
    }

    private Task OnReady()
    {
        Console.WriteLine($"Discord bot connected as {_client.CurrentUser}");
        return Task.CompletedTask;
    }

}