using System.Text;
using System.Text.Json;
using TinyJson;

namespace StallServer
{
    namespace DataTypes
    {
        public class Data
        {
            public string For;
            public string Payload;
            public string Type;

            public Data(string Type, string For, object Payload)
            {
                this.For = For;
                this.Payload = Payload != null ? Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(Payload))) : "";
                this.Type = Type;
            }

            public string GetDecodedPayload()
            {
                return Encoding.UTF8.GetString(Convert.FromBase64String(this.Payload));
            }
        }

        public class StallLogin
        {
            public string ImageVersion;
            public string MAC;
            public int StallNumber;
            public int StallStart;
            public string StallType;
            public int StallVersion;
            public int SystemStart;
        }

        public class SyncChanges
        {
            public string For;
            public string PruneTime;
            public List<object> Records;
            public int RemainingTraces;
            public int RequestedTraces;
            public string StallNowTime;
            public bool Success;
            public SyncInfo SyncInfo;
        }

        public class SyncInfo
        {
            public class Stream
            {
                public int Category;
                public string ID;
                public bool IsStreamActive;
                public bool IsTraceOpen;
                public object StartPayload;
                public string StartTime;
                public string Type;

                public override string ToString()
                {
                    string result = $"Stream [{Type}";

                    if (!string.IsNullOrWhiteSpace(ID)) result += $":{ID}";
                    result += "]";

                    if (StartPayload != null) result += $" - {StartPayload.ToJson()}";
                    return result;
                }
            }

            public object Marketing;
            public object Software;
            public List<Stream> Streams;
        }
    }
}