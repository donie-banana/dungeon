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
}
