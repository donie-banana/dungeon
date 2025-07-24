using System;
using Newtonsoft.Json;

[Serializable]
public class StatModifier
{
    [JsonProperty("type")]
    public char type;

    [JsonProperty("value")]
    public string value;
}
