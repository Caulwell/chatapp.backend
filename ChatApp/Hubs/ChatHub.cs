using Microsoft.AspNetCore.SignalR;
using System.Linq;

namespace ChatApp.Hubs
{
    public class ChatHub : Hub
    {
        private readonly string _botUser;
        private readonly IDictionary<string, UserConnection> _connections;

        public ChatHub(IDictionary<string, UserConnection> connections)
        {
            _botUser = "MyChat Bot";
            _connections = connections; 
        }
        public async Task JoinRoom(UserConnection userConnection)
        {
            // add connection id to group (room)
            await Groups.AddToGroupAsync(Context.ConnectionId, userConnection.Room);

            // save connection to singleton dictionary data store
            _connections[Context.ConnectionId] = userConnection;

            // send message to all clients in particular group (room)
            await Clients.Group(userConnection.Room).SendAsync("ReceiveMessage", _botUser, $"{userConnection.User} has joined {userConnection.Room}");

            // send updated user list to client
            await SendConnectedUsers(userConnection.Room);
        }

        public async Task SendMessage(string message)
        {
            // if data store holds connection, set userConnection to found and return true
            if(_connections.TryGetValue(Context.ConnectionId, out UserConnection userConnection))
            {
                // send message to all in group (room)
                await Clients.Group(userConnection.Room)
                    .SendAsync("ReceiveMessage", userConnection.User, message);
            }
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            // if connection
            if(_connections.TryGetValue(Context.ConnectionId, out UserConnection userConnection))
            {
                //remove client
                _connections.Remove(Context.ConnectionId);
                //send message to other clients in group
                Clients.Group(userConnection.Room)
                    .SendAsync("ReceiveMessage", _botUser, $"{userConnection.User} has left");
                //send updated user list to room
                SendConnectedUsers(userConnection.Room);
            }
            return base.OnDisconnectedAsync(exception);
        }

        public Task SendConnectedUsers(string room)
        {
            // filter connections by room, select only users
            var users = _connections.Values
                .Where(c => c.Room == room)
                .Select(c => c.User);
            return Clients.Group(room).SendAsync("UsersInRoom", users);
        }
    }
}
