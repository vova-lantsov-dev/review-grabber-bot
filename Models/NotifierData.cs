using System.Collections.Generic;
using Newtonsoft.Json;

namespace ReviewGrabberBot.Models
{
    internal sealed class NotifierData
    {
        [JsonProperty(Required = Required.Always)] public List<Restaurant> Restaurants;
        
        [JsonProperty(Required = Required.Always)] public Dictionary<string, int> MaxValuesOfRating;

        [JsonProperty(Required = Required.Always)] public List<string> PreferAvatarOverProfileLinkFor;
    }
}