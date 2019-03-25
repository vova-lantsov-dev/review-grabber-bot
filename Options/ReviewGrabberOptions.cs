using Newtonsoft.Json;
using ReviewGrabberBot.Models;

namespace ReviewGrabberBot.Options
{
    internal sealed class ReviewGrabberOptions
    {
        [JsonProperty(Required = Required.Always)] public NotifierData NotifierData;
        
        [JsonProperty(Required = Required.Always)] public ScriptRunnerData ScriptRunnerData;
    }
}