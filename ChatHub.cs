using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
namespace Hitchhikers
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        // public Task SendMessageToCaller(string message)
        // {
        //     return Clients.Caller.SendAsync("ReceiveMessage", message);
        // }

        // public Task SendMessageToGroups(string user, string message)
        // {
        //     List<string> groups = new List<string>() { "SignalR Users" };
        //     return Clients.Groups(groups).SendAsync("ReceiveMessage", message);
        // }

        // public override async Task OnConnectedAsync()
        // {
        //     await Groups.AddAsync(Context.User.Identity.Name, "SignalR Users");
        //     await base.OnConnectedAsync();
        // }
    }
}