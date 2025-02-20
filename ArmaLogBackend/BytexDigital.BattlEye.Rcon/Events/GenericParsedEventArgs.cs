
namespace BytexDigital.BattlEye.Rcon.Events
{
    /// <summary>
    /// Represents a generic parsed event argument containing any typed data (e.g., PlayerConnectedArgs).
    /// </summary>
    public class GenericParsedEventArgs : EventArgs
    {
        public object Arguments { get; }

        public GenericParsedEventArgs(EventArgs arguments)
        {
            Arguments = arguments;
        }
    }
}