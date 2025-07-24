using UnityEngine;
using Newtonsoft.Json;

public class ItemController : MonoBehaviour
{
    public Stats stats;
    public string type;

    void Start()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("items");
        Root data = JsonConvert.DeserializeObject<Root>(jsonFile.text);

        // not using rarities yet
        int itemNR = UnityEngine.Random.Range(1, 7);
        Item item = data.items[itemNR.ToString()];

        Debug.Log(item.type);

        foreach (var stat in item.stats)
        {
            Debug.Log($"item stat: {stat.Key}, item stat type: {stat.Value.type}, item stat value: {stat.Value.value}");
            float statValue = float.Parse(stat.Value.value);
            stats.addEntry(stat.Key, statValue, stat.Value.type);
        }

        // Debug.Log("Item 3 type: " + data.items["3"].type);
        // Debug.Log("Rare color: " + data.colors.type["rare"]);
        // Debug.Log("Damage color: " + data.colors.stat["damage"]);
        // Debug.Log("Chance for epic: " + data.rarities["epic"]);

        stats = GetComponent<Stats>();
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
