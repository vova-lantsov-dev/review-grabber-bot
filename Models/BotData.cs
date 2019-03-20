using System.Collections.Generic;
using Newtonsoft.Json;

namespace ReviewGrabberBot.Models
{
    internal sealed class BotData
    {
        [JsonProperty(Required = Required.Always)] public long ChatId;

        [JsonProperty(Required = Required.Always)] public List<int> AdminIds;
    }
}