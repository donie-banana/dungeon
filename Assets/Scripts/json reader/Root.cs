using System;
using System.Collections.Generic;
using Newtonsoft.Json;

[Serializable]
public class Root
{
    [JsonProperty("colors")]
    public ColorSets colors;

    [JsonProperty("rarities")]
    public Dictionary<string, string> rarities;

    [JsonProperty("items")]
    public Dictionary<string, Item> items;
}
