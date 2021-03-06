using System.Collections.Generic;
using Newtonsoft.Json;

namespace ReviewGrabberBot.Models
{
    internal sealed class Restaurant
    {
        [JsonProperty(Required = Required.Always)] public string Name;

        [JsonProperty(Required = Required.Always)] public long ChatId;

        [JsonProperty(Required = Required.Always)] public List<int> AdminIds;

        [JsonProperty(Required = Required.Always)] public Dictionary<string, string> Urls;
    }
}