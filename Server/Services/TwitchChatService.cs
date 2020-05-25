using System;
using System.Threading;
using System.Threading.Tasks;
using Janet.Server.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Server.Configuration;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace Server.Services
{
    public class TwitchChatService : IHostedService
    {
        
        private TwitchConfiguration _twitchConfig;
        private readonly TwitchClient _client;
        private readonly IServiceProvider _services;

        public TwitchChatService(IOptionsMonitor<TwitchConfiguration> options, IServiceProvider services)
        {
            _services = services;
            _twitchConfig = options.CurrentValue;
            options.OnChange(config =>
            {
                _twitchConfig = config;
            });
            _client = new TwitchClient();
            _client.OnChatCommandReceived += _client_OnChatCommandReceived;
        }

        private void _client_OnChatCommandReceived(object sender, TwitchLib.Client.Events.OnChatCommandReceivedArgs e)
        {
            var context = _services.GetRequiredService<IHubContext<CommandHub>>();
            context.Clients.All.SendAsync("Play", e.Command.CommandText);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var credentials = new ConnectionCredentials(_twitchConfig.Username, _twitchConfig.AuthToken);
            _client.Initialize(credentials);
            _client.OnConnected += (sender, args) =>
            {
                _client.JoinChannel(_twitchConfig.Channel);
            };

            _client.Connect();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _client.Disconnect();
            return Task.CompletedTask;
        }
    }
}