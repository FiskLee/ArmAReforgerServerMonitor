namespace BytexDigital.BattlEye.Rcon.Requests
{
    /// <summary>
    /// A missing class that older code references in NetworkConnection.
    /// Extends NetworkRequest with a SequenceNumber property.
    /// </summary>
    public abstract class SequentialNetworkRequest : NetworkRequest
    {
        public byte? SequenceNumber { get; private set; } = null;

        public void SetSequenceNumber(byte number)
        {
            SequenceNumber = number;
        }
    }
}
