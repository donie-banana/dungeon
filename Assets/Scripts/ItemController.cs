using UnityEngine;
using Newtonsoft.Json;
using UnityEditor.ShaderGraph;
using System.Collections.Generic;

public class ItemController : MonoBehaviour
{
    public Stats stats;
    public string type;
    public List<string> adjustedStats = new List<string>();
    private Root data;

    void Start()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("items");
        data = JsonConvert.DeserializeObject<Root>(jsonFile.text);

        // not using rarities yet
        int itemNR = UnityEngine.Random.Range(1, 7);
        Item item = data.items[itemNR.ToString()];

        Debug.Log(item.type);

        foreach (var stat in item.stats)
        {
            Debug.Log($"item stat: {stat.Key}, item stat type: {stat.Value.type}, item stat value: {stat.Value.value}");
            float statValue = float.Parse(stat.Value.value);
            stats.addEntry(stat.Key, statValue, stat.Value.type);
            adjustedStats.Add(stat.Key);
            
        }

        generateTexture();

        // Debug.Log("Item 3 type: " + data.items["3"].type);
        // Debug.Log("Rare color: " + data.colors.type["rare"]);
        // Debug.Log("Damage color: " + data.colors.stat["damage"]);
        // Debug.Log("Chance for epic: " + data.rarities["epic"]);

        stats = GetComponent<Stats>();
    }

    void generateTexture()
    {
        // Generate a horizontal gradient texture
        int width = 128;
        int height = 1;
        Texture2D gradientTexture = new Texture2D(width, height);

        Gradient gradient = new Gradient();
        List<GradientColorKey> colorKeys = new List<GradientColorKey>();
        int count = adjustedStats.Count;
        for (int i = 0; i < count; i++)
        {
            string Astat = adjustedStats[i];
            Color color;
            string statCategory = data.statCategories[Astat];
            if (ColorUtility.TryParseHtmlString(data.colors.stat[statCategory], out color))
            {
                float position = (count == 1) ? 0f : (float)i / (count - 1); // Avoid division by zero
                colorKeys.Add(new GradientColorKey(color, position));
            }
            else
            {
                Debug.LogWarning($"Invalid hex color: {data.colors.stat[Astat]}");
            }
        }

        gradient.colorKeys = colorKeys.ToArray();

        for (int x = 0; x < width; x++)
        {
            float t = (float)x / (width - 1);
            Color color = gradient.Evaluate(t);
            gradientTexture.SetPixel(x, 0, color);
        }
        gradientTexture.Apply();

        GetComponent<Renderer>().material.mainTexture = gradientTexture;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Player player = collision.gameObject.GetComponent<Player>();

            foreach (StatType stat in System.Enum.GetValues(typeof(StatType)))
            {
                if (stats.HasStat(stat))
                {
                    char operation = stats.GetType(stat);

                    if (operation == '*')
                        player.stats[stat] *= stats[stat];  
                    else if (operation == '-')
                        player.stats[stat] -= stats[stat];
                    else if (operation == '/')
                        player.stats[stat] /= stats[stat];
                    else
                        player.stats[stat] += stats[stat];
                }
            }

            Destroy(gameObject);
        }
    }
}

