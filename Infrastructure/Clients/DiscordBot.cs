using Discord;
using Discord.WebSocket;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
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
        _client.MessageReceived += OnMessageReceived;
        _client.Log += OnLog;
    }

    public async Task StartAsync()
    {
        Console.WriteLine("DiscordBot.StartAsync entered");

        await _client.LoginAsync(TokenType.Bot, _token);
        Console.WriteLine("Discord bot login succeeded");

        await _client.StartAsync();
        Console.WriteLine("Discord bot socket start called");
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
    private Task OnLog(LogMessage message)
    {
        Console.WriteLine($"[DISCORD] {message}");
        return Task.CompletedTask;
    }

    private async Task OnMessageReceived(SocketMessage message)
    {
        if (message.Author.IsBot)
            return;

        if (message.Author.Id != _ownerUserId)
        {
            await message.Channel.SendMessageAsync("I don't know you");
            return;
        }

        if (!message.Content.StartsWith("!create "))
        {
            await message.Channel.SendMessageAsync("Usage: !create username password");
            return;
        }

        var parts = message.Content.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length < 3)
        {
            await message.Channel.SendMessageAsync("Usage: !create username password");
            return;
        }

        var username = parts[1];
        var password = parts[2];

        using var scope = _scopeFactory.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var existingUser = await userManager.FindByNameAsync(username);
        if (existingUser != null)
        {
            await message.Channel.SendMessageAsync($"User already exists: {username}");
            return;
        }

        var user = new ApplicationUser
        {
            UserName = username
        };

        var result = await userManager.CreateAsync(user, password);

        if (result.Succeeded)
        {
            await message.Channel.SendMessageAsync($"User created: {username}");
        }
        else
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            await message.Channel.SendMessageAsync($"Failed: {errors}");
        }
    }
}