using System;
using System.Threading.Tasks;
using BytexDigital.BattlEye.Rcon;
using BytexDigital.BattlEye.Rcon.Commands;

namespace ArmaLogBackend.Services
{
    /// <summary>
    /// RconService wraps RconClient (with a 3-arg constructor) and calls 
    /// ConnectAsync, DisconnectAsync, SendCommandAsync, etc.
    /// </summary>
    public class RconService : IDisposable
    {
        private readonly RconClient _client;

        public RconService(string host, int port, string password)
        {
            // RconClient now has a 3-arg constructor
            _client = new RconClient(host, port, password);
        }

        public async Task ConnectAsync()
        {
            await _client.ConnectAsync();
        }

        public async Task DisconnectAsync()
        {
            await _client.DisconnectAsync();
        }

        public async Task KickPlayerAsync(int playerId, string reason = "Kicked by admin")
        {
            var command = new KickCommand(playerId, reason);
            await _client.SendCommandAsync(command);
        }

        // Additional methods => BanOnlinePlayerAsync, BanPlayerAsync, etc.
        // For demonstration, just Kick is shown
        // (You can add the rest of your RCON commands as needed)

        public void Dispose()
        {
            // If we do 'using var rcon = new RconService(...)', 
            // we call _client.Dispose() => calls Disconnect
            _client.Dispose();
        }
    }
}
