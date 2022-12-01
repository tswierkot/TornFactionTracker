using System.Collections.Generic;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace FactionHospitalTracker
{
    public static class TornAPIObjects
    {
        public class ErrorObject
        {
            public class Error
            {
                public int code;
                public string error;
            }

            public Error error;
        }

        public class TimestampObject
        {
            public long timestamp;
        }

        public class UserObject
        {
            public class Life
            {
                public int ucrrent;
                public int maximum;
                public int increment;
                public int interval;
                public int ticktime;
                public int fulltime;
            }

            public class Status
            {
                public string description;
                public string details;
                public string state;
                public string color;
                public long until;
            }

            public class Job
            {
                public string position;
                public int company_id;
                public string company_name;
                public int company_type;
            }

            public class Faction
            {
                public string position;
                public int faction_id;
                public int days_in_faction;
                public string faction_name;
                public string faction_tag;
            }

            public class Married
            {
                public int spouse_id;
                public string spouse_name;
                public int duration;
            }

            public class States
            {
                public long hospital_timestamp;
                public long jail_timestamp;
            }

            public class LastAction
            {
                public string status;
                public long timestamp;
                public string relative;
            }

            public string rank;
            public int level;
            public string gender;
            public string property;
            public string signup;
            public int awards;
            public int friends;
            public int enemies;
            public int forum_posts;
            public int karma;
            public int age;
            public string role;
            public int donator;
            public int player_id;
            public string name;
            public int property_id;
            public int revivable;
            public Life life;
            public Status status;
            public Job job;
            public Faction faction;
            public Married married;
            public States states;
            public LastAction last_action;
        }

        public class FactionObject
        {
            public class Rank
            {
                public int level;
                public string name;
                public int divison;
                public int position;
                public int wins;
            }

            public class Member
            {
                public class LastAction
                {
                    public string status;
                    public long timestamp;
                    public string relative;
                }

                public class Status
                {
                    public string description;
                    public string details;
                    public string state;
                    public string color;
                    public long until;
                }

                public string name;
                public int level;
                public int days_in_faction;
                public LastAction last_action;
                public Status status;
                public string position;
            }

            public int ID;
            public string name;
            public string tag;
            public string tag_image;
            public int leader;
            [JsonProperty("co-leader")] public int coleader;
            public int respect;
            public int age;
            public int capacity;
            public int best_chain;
            public Rank rank;
            public Dictionary<string, Member> members;
        }
    }
}