using Newtonsoft.Json;

namespace ReviewGrabberBot.Models
{
    public class ScriptRunnerData
    {
        [JsonProperty(Required = Required.Always)] public string WorkingDirectory;
        
        [JsonProperty(Required = Required.Always)] public string Arguments;
        
        [JsonProperty(Required = Required.Always)] public string FileName;
    }
}