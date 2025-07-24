using System;
using System.Collections.Generic;
using UnityEngine;

// 1. Define all your stat types here
public enum StatType
{
    WalkSpeed,
    RunSpeed,
    Accel,
    BulletSpeed,
    ReloadSpeed,
    BulletRange,
    BulletDamage,
    MeleeDamage,
    BodyDamage,
    MaxHealth,
    Health,
    HealthRegen,
    MaxShield,
    Shield,
    ShieldRegen,
}

// 2. A serializable pair so Unity can display it
[Serializable]
public class StatEntry
{
    public StatType stat;
    public float value;
    public char type; // only for items
}

// 3. The Stats container
public class Stats : MonoBehaviour
{
    [SerializeField]
    private List<StatEntry> entries = new List<StatEntry>();

    private float[] values;

    private void Awake()
    {
        values = new float[Enum.GetValues(typeof(StatType)).Length];
        foreach (var e in entries)
            values[(int)e.stat] = e.value;
    }

    // read
    public float Get(StatType type)
    {
        return values[(int)type];
    }

    // write
    public void Set(StatType type, float val)
    {
        values[(int)type] = val;
        // also update the serialized list so inspector stays in sync
        var e = entries.Find(x => x.stat == type);
        if (e != null) e.value = val;
        else entries.Add(new StatEntry { stat = type, value = val });
    }

    // increment
    public void Add(StatType type, float delta) => Set(type, Get(type) + delta);

    // Indexer for enum
    public float this[StatType type]
    {
        get => values[(int)type];
        set => Set(type, value);
    }

    // Indexer for string
    public float this[string typeName]
    {
        get
        {
            if (Enum.TryParse(typeName, true, out StatType type))
                return this[type];
            throw new ArgumentException($"StatType '{typeName}' not found.");
        }
        set
        {
            if (Enum.TryParse(typeName, true, out StatType type))
                this[type] = value;
            else
                throw new ArgumentException($"StatType '{typeName}' not found.");
        }
    }

    public char GetType(StatType type)
    {
        var entry = entries.Find(e => e.stat == type);
        if (entry != null)
            return entry.type;
        throw new ArgumentException($"StatType '{type}' not found in entries.");
    }

    public bool HasStat(StatType type)
    {
        return entries.Exists(e => e.stat == type);
    }

    // public void addEntry(StatType stat, float value, char type)
    // {
    //     var entry = entries.Find(e => e.stat == stat);
    //     if (entry != null)
    //     {
    //         entry.value = value;
    //         entry.type = type;
    //     }
    //     else
    //     {
    //         entries.Add(new StatEntry { stat = stat, value = value, type = type });
    //     }
    //     values[(int)stat] = value;
    // }

    public void addEntry(string statName, float value, char type)
    {
        if (!Enum.TryParse(statName, true, out StatType stat))
            throw new ArgumentException($"StatType '{statName}' not found.");

        var entry = entries.Find(e => e.stat == stat);
        if (entry != null)
        {
            entry.value = value;
            entry.type = type;
        }
        else
        {
            entries.Add(new StatEntry { stat = stat, value = value, type = type });
        }
        values[(int)stat] = value;
    }
}
