using System.Collections.Generic;
using Newtonsoft.Json;

namespace ReviewGrabberBot.Models
{
    internal sealed class NotifierOptionsData
    {
        [JsonProperty(Required = Required.Always)] public List<Restaurant> Restaurants;
        [JsonProperty(Required = Required.Always)] public Dictionary<string, int> MaxValuesOfRating;
    }
}