using System;
using System.Collections.Generic;
using Newtonsoft.Json;

[Serializable]
public class ColorSets
{
    [JsonProperty("type")]
    public Dictionary<string, string> type;

    [JsonProperty("stat")]
    public Dictionary<string, string> stat;
}
