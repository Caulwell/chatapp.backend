using Microsoft.AspNetCore.SignalR;

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
    }
}
