using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BytexDigital.BattlEye.Rcon.Events;
using BytexDigital.BattlEye.Rcon.Requests;
using BytexDigital.BattlEye.Rcon.Responses;

namespace BytexDigital.BattlEye.Rcon
{
    public class NetworkMessageHandler
    {
        private readonly StringEncoder _stringEncoder;
        private readonly NetworkConnection _networkConnection;
        private readonly List<NetworkRequest> _networkRequests;

        public NetworkMessageHandler(NetworkConnection networkConnection)
        {
            _networkRequests = new List<NetworkRequest>();
            _stringEncoder = new StringEncoder();
            _networkConnection = networkConnection;
        }

        public void Track(NetworkRequest networkRequest)
        {
            Cleanup();
            lock (_networkRequests)
            {
                _networkRequests.Add(networkRequest);
            }
        }

        public void Handle(IEnumerable<byte> data)
        {
            var payload = data.Skip(7);
            var type = payload.First();

            if (type == 0x00) HandleLoginResponse(payload.Skip(1));
            else if (type == 0x01) HandleCommandResponse(payload.Skip(1));
            else if (type == 0x02) HandleMessagePacket(payload.Skip(1));
        }

        public void Cleanup()
        {
            lock (_networkRequests)
            {
                var toRemoves = _networkRequests.Where(
                        x => !x.Acknowledged && DateTime.UtcNow - x.SentTimeUtc > TimeSpan.FromSeconds(5))
                    .ToArray();
                foreach (var tr in toRemoves)
                    RemoveRequest(tr);
            }
        }

        private void HandleLoginResponse(IEnumerable<byte> data)
        {
            var success = data.First() == 0x01;

            lock (_networkRequests)
            {
                foreach (var loginReq in _networkRequests.Where(x => x is LoginNetworkRequest).ToArray())
                {
                    RemoveRequest(loginReq);

                    loginReq.SetResponse(new LoginNetworkResponse(success));
                    loginReq.MarkResponseReceived();
                }
            }
        }

        private void HandleMessagePacket(IEnumerable<byte> data)
        {
            var sequenceNumber = data.First();
            var payload = data.Skip(1);
            var content = _stringEncoder.GetString(payload.ToArray());

            _networkConnection.Send(new Requests.AcknowledgeRequest(sequenceNumber));

            try
            {
                _networkConnection.FireMessageReceived(content);
            }
            catch
            {
            }

            // parse content with regex for player connect/disconnect, etc.
            // ...
        }

        private void HandleCommandResponse(IEnumerable<byte> data)
        {
            var sequenceNumber = data.First();
            var request = GetRequest(sequenceNumber);
            if (request == null) return;

            var payload = data.Skip(1);
            if (payload.Any())
            {
                var header = payload.First();
                if (header == 0x00)
                {
                    // multi-part message
                    if (request.Response == null)
                        request.SetResponse(new CommandNetworkResponse(""));

                    var expectedAmount = data.Skip(2).First();
                    var currentIndex = data.Skip(3).First();
                    var partData = data.Skip(4);

                    var isLastPart = expectedAmount == currentIndex + 1;
                    (request.Response as CommandNetworkResponse)!.AppendContentBytes(partData.ToArray());

                    if (isLastPart)
                    {
                        (request.Response as CommandNetworkResponse)!.ConvertCollectedBytesToContentString(
                            bytes => _stringEncoder.GetString(bytes.ToArray()));
                        RemoveRequest(request);
                        request.MarkResponseReceived();
                    }
                }
                else
                {
                    RemoveRequest(request);
                    var result = _stringEncoder.GetString(data.Skip(1).ToArray());
                    request.SetResponse(new CommandNetworkResponse(result));
                    request.MarkResponseReceived();
                }
            }
            else
            {
                // empty payload => ack
                if (!request.Acknowledged) request.MarkAcknowledged();
                RemoveRequest(request);
            }
        }

        private SequentialNetworkRequest? GetRequest(byte sequenceNumber)
        {
            lock (_networkRequests)
            {
                return _networkRequests
                    .OfType<SequentialNetworkRequest>()
                    .FirstOrDefault(x => x.SequenceNumber == sequenceNumber);
            }
        }

        private void RemoveRequest(NetworkRequest netReq)
        {
            lock (_networkRequests)
            {
                _networkRequests.Remove(netReq);
            }
        }
    }
}
