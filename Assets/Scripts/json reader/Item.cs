using System;
using System.Collections.Generic;
using Newtonsoft.Json;

[Serializable]
public class Item
{
    [JsonProperty("type")]
    public string type;

    [JsonProperty("stats")]
    public Dictionary<string, StatModifier> stats;
}
