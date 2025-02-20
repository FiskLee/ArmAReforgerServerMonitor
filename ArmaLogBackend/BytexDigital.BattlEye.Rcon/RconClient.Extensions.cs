using System;
using System.Threading.Tasks;
using BytexDigital.BattlEye.Rcon.Commands;

namespace BytexDigital.BattlEye.Rcon
{
    /// <summary>
    /// Partial class extends RconClient with asynchronous methods needed by RconService.
    /// </summary>
    public partial class RconClient
    {
        public Task ConnectAsync() => Task.Run(() => Connect());
        public Task DisconnectAsync() => Task.Run(() => Disconnect());
        public Task SendCommandAsync(Command command) => Task.Run(() => Send(command));

        public void Dispose()
        {
            // If your code calls Dispose, we just call Disconnect.
            Disconnect();
        }
    }
}
