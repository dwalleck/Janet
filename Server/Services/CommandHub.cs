using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Janet.Server.Services
{
    public class CommandHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            await Clients.All.SendAsync("TestMessage", "Stuff");
        }
    }
}
