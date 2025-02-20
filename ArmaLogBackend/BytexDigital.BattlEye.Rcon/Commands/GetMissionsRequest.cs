using System.Collections.Generic;
using System.Text.RegularExpressions;
using BytexDigital.BattlEye.Rcon.Domain;

namespace BytexDigital.BattlEye.Rcon.Commands
{
    /// <summary>
    /// Requests the currently available missions. 
    /// We fix the non-nullable property 'Missions' must contain a non-null value by init with empty.
    /// </summary>
    public class GetMissionsRequest : Command, IHandlesResponse, IProvidesResponse<IEnumerable<Mission>>
    {
        public IEnumerable<Mission> Missions { get; private set; } = new List<Mission>();

        public GetMissionsRequest() : base("missions")
        {
        }

        public void Handle(string content)
        {
            var matches = Regex.Matches(content, @"(.+)\.(.+)\.pbo");
            var missions = new List<Mission>();

            foreach (Match match in matches)
            {
                try
                {
                    missions.Add(new Mission(match.Groups[1].Value, match.Groups[2].Value));
                }
                catch
                {
                }
            }

            Missions = missions;
        }

        public IEnumerable<Mission> GetResponse()
        {
            return Missions;
        }
    }
}
