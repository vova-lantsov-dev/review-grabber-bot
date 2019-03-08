using Newtonsoft.Json;

namespace ReviewGrabberBot.Models
{
    internal sealed class Restaurant
    {
        [JsonProperty(Required = Required.Always)] public string Name;
        [JsonProperty(Required = Required.Always)] public string GoogleUrl;
        [JsonProperty(Required = Required.Always)] public string FoursquareUrl;
        [JsonProperty(Required = Required.Always)] public string RestoclubUrl;
        [JsonProperty(Required = Required.Always)] public string RestoratingUrl;
        [JsonProperty(Required = Required.Always)] public string TripadvisorUrl;
        [JsonProperty(Required = Required.Always)] public string YellUrl;
    }
}