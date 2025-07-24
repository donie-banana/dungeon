using UnityEngine;
using Newtonsoft.Json;
using System.IO;

public class JsonLoader : MonoBehaviour
{
    void Start()
    {
        var path = Path.Combine(Application.streamingAssetsPath, "testdata.json");
        var json = File.ReadAllText(path);
        Root data = JsonConvert.DeserializeObject<Root>(json);
        
        // example:
        Debug.Log("Item 3 type: " + data.items["3"].type);
        Debug.Log("Rare color: " + data.colors.type["rare"]);
        Debug.Log("Damage color: " + data.colors.stat["gray"]);
    }
}
