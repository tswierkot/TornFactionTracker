using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;

namespace FactionHospitalTracker
{
    public static class TornAPI
    {
        /// <summary>
        /// Returns current server timestamp from Torn API.
        /// </summary>
        /// <param name="apiKey">Torn API key.</param>
        public static async Task<long> GetTimestamp(string apiKey)
        {
            ThrowInvalidAPIKey(apiKey);
            var client = new RestClient("https://api.torn.com");
            var request = new RestRequest($"/torn/?selections=timestamp&key={apiKey}");
            var response = await client.GetAsync(request);
            if (response.IsSuccessful)
            {
                if (HandleAPIError(response.Content, out int errorCode))
                {
                    Console.WriteLine(
                        $"Error while retrieving timestamp - API error code: {errorCode}. Error message: {ErrorCodeToMessage(errorCode)}");
                    return -errorCode;
                }

                var definition = new { timestamp = "" };
                var content = JsonConvert.DeserializeObject<TornAPIObjects.TimestampObject>(response.Content);
                return Convert.ToInt64(content.timestamp);
            }
            else
            {
                Console.WriteLine($"Error while retrieving timestamp - HTTP error code: {response.StatusCode}");
                return -((int)response.StatusCode);
            }
        }
        
        public static async Task<Tuple<int, string>> GetUserFaction(string apiKey)
        {
            ThrowInvalidAPIKey(apiKey);
            var client = new RestClient("https://api.torn.com");
            var request = new RestRequest($"/user/2490492?selections=&key={apiKey}");
            var response = await client.GetAsync(request);
            if (response.IsSuccessful)
            {
                if (HandleAPIError(response.Content, out int errorCode))
                {
                    Console.WriteLine(
                        $"Error while retrieving user faction - API error code: {errorCode}. Error message: {ErrorCodeToMessage(errorCode)}");
                    return null;
                }

                var definition = new { timestamp = "" };
                var user = JsonConvert.DeserializeObject<TornAPIObjects.UserObject>(response.Content);
                return new Tuple<int, string>(user.faction.faction_id, user.faction.faction_name);
            }
            else
            {
                Console.WriteLine($"Error while retrieving user faction - HTTP error code: {response.StatusCode}");
                return null;
            }
        }

        /// <summary>
        /// Returns a collection of faction members that are currently in the hospital and how long are they gonna be there.
        /// </summary>
        /// <param name="apiKey">Torn API key</param>
        /// <param name="factionID">Faction ID to check.</param>
        public static async Task<IDictionary<int, ValueTuple<string, long>>> GetFactionMembersInHospital(string apiKey,
            int factionID)
        {
            ThrowInvalidAPIKey(apiKey);
            if (factionID <= 0 || factionID >= 999999)
            {
                Console.WriteLine("Error while retriving faction members in hospital - invalid faction ID.");
            }

            var client = new RestClient("https://api.torn.com");
            var request = new RestRequest($"/faction/10820?selections=&key={apiKey}");
            var response = await client.GetAsync(request);
            if (response.IsSuccessful)
            {
                if (HandleAPIError(response.Content, out int errorCode))
                {
                    Console.WriteLine(
                        $"Error while retriving faction members in hospital - API error code: {errorCode}. Error message: {ErrorCodeToMessage(errorCode)}");
                    return null;
                }

                var definition = new { timestamp = "" };
                var content = JsonConvert.DeserializeObject<TornAPIObjects.FactionObject>(response.Content);
                var result = content.members.Where(m => m.Value.status.state == "Hospital").ToDictionary(
                    m => Convert.ToInt32(m.Key), m => new ValueTuple<string, long>(m.Value.name, m.Value.status.until));
                return result;
            }
            else
            {
                Console.WriteLine($"Error while retrieving timestamp - HTTP error code: {response.StatusCode}");
                return null;
            }
        }

        private static void ThrowInvalidAPIKey(string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey) || apiKey.Length != 16)
                throw new ArgumentException(nameof(apiKey));
        }

        private static bool HandleAPIError(string responseContent, out int errorCode)
        {
            errorCode = -1;
            if (string.IsNullOrWhiteSpace(responseContent))
            {
                return false;
            }

            if (responseContent.StartsWith("{\"error\":"))
            {
                var content = JsonConvert.DeserializeObject<TornAPIObjects.ErrorObject>(responseContent);
                if (content.error != null)
                {
                    errorCode = content.error.code;
                    return true;
                }
            }

            return false;
        }

        public static string ErrorCodeToMessage(int errorCode)
        {
            return errorCode switch
            {
                0 => "Unknown error : Unhandled error, should not occur.",
                1 => "Key is empty : Private key is empty in current request.",
                2 => "Incorrect Key : Private key is wrong/incorrect format.",
                3 => "Wrong type : Requesting an incorrect basic type.",
                4 => "Wrong fields : Requesting incorrect selection fields.",
                5 =>
                    "Too many requests : Requests are blocked for a small period of time because of too many requests per user (max 100 per minute).",
                6 => "Incorrect ID : Wrong ID value.",
                7 =>
                    "Incorrect ID-entity relation : A requested selection is private (For example, personal data of another user / faction).",
                8 => "IP block : Current IP is banned for a small period of time because of abuse.",
                9 => "API disabled : Api system is currently disabled.",
                10 => "Key owner is in federal jail : Current key can't be used because owner is in federal jail.",
                11 => "Key change error : You can only change your API key once every 60 seconds.",
                12 => "Key read error : Error reading key from Database.",
                13 =>
                    "The key is temporarily disabled due to owner inactivity : The key owner hasn't been online for more than 7 days.",
                14 =>
                    "Daily read limit reached : Too many records have been pulled today by this user from our cloud services.",
                15 =>
                    "Temporary error : An error code specifically for testing purposes that has no dedicated meaning.",
                16 =>
                    "Access level of this key is not high enough : A selection is being called of which this key does not have permission to access.",
                _ => throw new ArgumentOutOfRangeException(nameof(errorCode))
            };
        }
    }
}