using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using BytexDigital.BattlEye.Rcon.Requests;   // IMPORTANT for SequentialNetworkRequest

namespace BytexDigital.BattlEye.Rcon
{
    public class NetworkConnection
    {
        public event EventHandler<string>? MessageReceived;
        public event EventHandler? Disconnected;

        // We can remove references to GenericParsedEventArgs if not used or
        // keep it if you also have a ProtocolEvent in your code:
        public event EventHandler<Events.GenericParsedEventArgs>? ProtocolEvent;

        private readonly NetworkMessageHandler _handler;
        private readonly IPEndPoint _remoteEndpoint;
        private readonly SequenceCounter _sequenceCounter;
        private readonly UdpClient _udpClient;
        private CancellationToken _cancellationToken;
        private DateTime _lastSent;

        public NetworkConnection(IPEndPoint remoteEndpoint, CancellationToken cancellationToken)
        {
            _remoteEndpoint = remoteEndpoint;
            _cancellationToken = cancellationToken;
            _udpClient = new UdpClient(remoteEndpoint.AddressFamily);
            _udpClient.Connect(_remoteEndpoint);
            _handler = new NetworkMessageHandler(this);
            _sequenceCounter = new SequenceCounter();
        }

        public void BeginReceiving()
        {
            _ = Task.Run(Receive, _cancellationToken);
        }

        public void BeginHeartbeat()
        {
            _ = Task.Run(Heartbeat, _cancellationToken);
        }

        public void Send(NetworkMessage networkMessage)
        {
            _handler.Cleanup();

            // If it’s a SequentialNetworkRequest => set sequence number
            if (networkMessage is SequentialNetworkRequest seqReq)
                seqReq.SetSequenceNumber(_sequenceCounter.Next());

            if (networkMessage is Requests.NetworkRequest netReq)
                _handler.Track(netReq);

            var data = networkMessage.ToBytes();
            _udpClient.Send(data, data.Length);

            networkMessage.MarkSent();
            _lastSent = DateTime.UtcNow;
        }

        internal void FireMessageReceived(string message)
        {
            MessageReceived?.Invoke(this, message);
        }

        internal void FireProtocolEvent(Events.GenericParsedEventArgs args)
        {
            ProtocolEvent?.Invoke(this, args);
        }

        private async void Receive()
        {
            try
            {
                var closeTask = Task.Delay(-1, _cancellationToken);
                while (!_cancellationToken.IsCancellationRequested)
                {
                    var receiveTask = _udpClient.ReceiveAsync();
                    var task = await Task.WhenAny(receiveTask, closeTask).ConfigureAwait(false);

                    if (task == closeTask) break;
                    if (receiveTask.IsFaulted) continue;

                    var result = receiveTask.Result;
                    try
                    {
                        _handler.Handle(result.Buffer);
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }
            catch (SocketException)
            {
                // ignored
            }
        }

        private async void Heartbeat()
        {
            while (!_cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(3000, _cancellationToken);
                if (_cancellationToken.IsCancellationRequested) return;

                var keepAlivePacket = new Requests.CommandNetworkRequest("");
                Send(keepAlivePacket);
                keepAlivePacket.WaitUntilAcknowledged(5000);

                if (keepAlivePacket.Acknowledged) continue;

                try
                {
                    Disconnected?.Invoke(this, EventArgs.Empty);
                }
                catch
                {
                    // ignored
                }
            }
        }
    }
}
