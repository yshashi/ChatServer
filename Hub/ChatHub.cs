using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatService.Model;
using Microsoft.AspNetCore.SignalR;

namespace ChatService
{
    public class ChatHub : Hub 
    {
        private readonly string _botUser;
        private readonly IDictionary<string, UserConnection> _connection;
        public ChatHub(IDictionary<string, UserConnection> connection)
        {
            _botUser = "Let's Program Bot";
            _connection = connection;
        }
        public async Task JoinRoom(UserConnection userConnection)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userConnection.Room);
            _connection[Context.ConnectionId] = userConnection;
            await Clients.Group(userConnection.Room)
                .SendAsync("ReceiveMessage", _botUser, $"{userConnection.User} has joined {userConnection.Room}");
            await SendConnectedUser(userConnection.Room);
        }
        public async Task SendMessage(string message)
        {
            if(_connection.TryGetValue(Context.ConnectionId,out UserConnection userConnection))
            {
                await Clients.Group(userConnection.Room)
                    .SendAsync("ReceiveMessage", userConnection.User, message);
            }
        }
        public override Task OnDisconnectedAsync(Exception exception)
        {
            if(_connection.TryGetValue(Context.ConnectionId, out UserConnection userConnection))
            {
                _connection.Remove(Context.ConnectionId);
                Clients.Group(userConnection.Room)
                .SendAsync("ReceiveMessage", _botUser, $"{userConnection.User} has Left");
                SendConnectedUser(userConnection.Room);
            }
            return base.OnDisconnectedAsync(exception);
        }
        public Task SendConnectedUser(string room)
        {
            var users = _connection.Values
                .Where(u => u.Room == room)
                .Select(s => s.User);
            return Clients.Group(room).SendAsync("UsersInRoom", users);
        }
    }
}
