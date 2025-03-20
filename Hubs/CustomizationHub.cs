using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace PosBackend.Hubs
{
    public class CustomizationHub : Hub
    {
        // When a client connects, add them to a group based on their user identifier.
        public override async Task OnConnectedAsync()
        {
            // If you are using authentication, ensure that the UserIdentifier is set
            if (!string.IsNullOrEmpty(Context.UserIdentifier))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, Context.UserIdentifier);
            }
            await base.OnConnectedAsync();
        }

        // Allow clients to explicitly join a group (e.g., for non-authenticated scenarios)
        public async Task JoinUserGroup(string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
        }

        // Allow clients to leave a group when needed
        public async Task LeaveUserGroup(string userId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
        }

        // Optionally, allow clients to send a customization update through the hub
        public async Task SendCustomizationUpdate(string userId, object customization)
        {
            await Clients.Group(userId).SendAsync("ReceiveCustomizationUpdate", customization);
        }
    }
}
