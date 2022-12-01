using System;
using System.Linq;
using System.Threading.Tasks;
using CommandLine;
using RestSharp;

namespace FactionHospitalTracker
{
    internal class Options
    {
        [Option('k', "key", Required = true, HelpText = "Torn City API key.")]
        public string ApiKey { get; set; }
        
        [Option('f', "factionid", Required = false, HelpText = "Torn faction ID", Group = "ID")]
        public string FactionID { get; set; }
        
        [Option('u', "userid", Required = false, HelpText = "Torn user ID (to extract faction ID from API)", Group = "ID")]
        public string UserID { get; set; }
    }

    class Program
    {
        public static async Task<int> Main(String[] args)
        {
            int initCode = default;
            string ApiKey = default;
            int userID = default;
            int factionID = default;
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(o =>
                {
                    if (string.IsNullOrWhiteSpace(o.ApiKey) | o.ApiKey.Length != 16)
                    {
                        Console.WriteLine("Invalid Torn City API key - exiting.");
                        initCode = -1;
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(o.UserID) && string.IsNullOrWhiteSpace(o.FactionID))
                    {
                        Console.WriteLine("Provide either a faction ID or user ID.");
                        initCode = -1;
                        return;
                    }

                    if (!string.IsNullOrWhiteSpace(o.FactionID) && Convert.ToInt32(o.FactionID) <= 0)
                    {
                        Console.WriteLine("Invalid faction ID.");
                        initCode = -1;
                        return;
                    }

                    if (!string.IsNullOrWhiteSpace(o.UserID) && Convert.ToInt32(o.UserID) <= 0)
                    {
                        Console.WriteLine("Invalid user ID.");
                        initCode = -1;
                        return;
                    }

                    ApiKey = o.ApiKey;
                    factionID = Convert.ToInt32(o.FactionID);
                    userID = Convert.ToInt32(o.UserID);
                }).WithNotParsed(o => initCode = -1);

            if (initCode != 0)
            {
                Console.WriteLine("Initialization failed, exiting.");

                return initCode;
            }

            Console.WriteLine($"Testing API key: {ApiKey}");
            var timestamp = await TornAPI.GetTimestamp(ApiKey);
            if (timestamp > 0)
            {
                Console.WriteLine("API key correct.");
            }
            else
            {
                Console.WriteLine("Failed to retrive timestamp with supplied API key - exiting.");
                return (int)timestamp;
            }

            if (factionID == 0 && userID > 0)
            {
                Console.WriteLine($"Retrieving faction ID for user ID {userID}");
                var result = await TornAPI.GetUserFaction(ApiKey);
                if (result != null)
                    factionID = result.Item1;

                if (factionID > 0)
                    Console.WriteLine($"User {userID} is in faction {result.Item2} [{result.Item1}]");
            }

            if (factionID <= 0)
            {
                Console.WriteLine("No faction ID - exiting");
                return 0;
            }
            Console.WriteLine($"Checking how many members of faction ID {factionID} are in the hospital.");
            var hospedMembers = await TornAPI.GetFactionMembersInHospital(ApiKey, factionID);
            if (hospedMembers == null)
            {
                Console.WriteLine("Fail");
            }
            else
            {
                Console.WriteLine($"{hospedMembers.Count} members in the hospital.");
                var now = DateTimeOffset.UtcNow;
                foreach (var member in hospedMembers.OrderBy(m => m.Value.Item2))
                {
                    var hospedUntil = DateTimeOffset.FromUnixTimeSeconds(member.Value.Item2);
                    TimeSpan hospTime = hospedUntil - now;
                    var timeString = hospTime.TotalHours >= 1 ? @"{0:h\:mm\:ss}" : @"{0:mm\:ss}";
                    Console.WriteLine($"{member.Value.Item1} [{member.Key}] \tHosped for {string.Format(timeString, hospTime)}");
                }
            }

            return 0;
        }
    }
}