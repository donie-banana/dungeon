#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEditor.PackageManager;
using UnityEngine;

public class Generator : MonoBehaviour
{
    // grid coords in units of 'size'
    private HashSet<Vector3Int> usedSpaces = new HashSet<Vector3Int> { new Vector3Int(0, 0, 0) };
    private HashSet<Vector3Int> frontier = new HashSet<Vector3Int>();
    public int roomCount;
    private Vector3Int[] offsets;
    private int size = 8;

    private Func<int, int, int> random = UnityEngine.Random.Range;

    // room prefabs…
    public GameObject player, floor, dCorner, dWalls, corner, hallway, smallRoom, wall, empty;

    void Start()
    {
        offsets = new[] {
            new Vector3Int( 1, 0, 0),
            new Vector3Int( 0, 0, 1),
            new Vector3Int(-1, 0, 0),
            new Vector3Int( 0, 0,-1),
        };

        // initialize frontier around (0,0,0)
        UpdateFrontier(new Vector3Int(0, 0, 0));

        Instantiate(smallRoom, scaleVector(Vector3Int.zero), quaternion.identity);
        Instantiate(player, new Vector3(0, 1, 0), quaternion.identity);

        usedSpaces.Add(new Vector3Int(0, 0, 0));
        frontier.RemoveWhere(f => f == new Vector3Int(0, 0, 0));

        Generate();
    }

    void Generate()
    {
        const int maxTries = 20;

        int emptyAmount = random(4, 8);
        roomCount = Mathf.CeilToInt(roomCount * (1f / emptyAmount + 1f));

        for (int i = 0; i < roomCount; i++)
        {
            int chosenSize = RandomIntWithWeights(
                new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 20, 25, 35, 50, 60 },
                new[] { 96, 96, 96, 640, 560, 560, 560, 64, 48, 32, 24, 16, 8, 4, 1 }
            );

            bool placed = false;

            Vector3Int pos = (i != 0)
                ? PickFrontierCell()
                : new Vector3Int(1, 0, 0);

            for (int t = 0; t < maxTries && !placed; t++)
            {
                if (!usedSpaces.Contains(pos))
                {
                    var spaces = FindSpaces(pos, chosenSize);
                    if (spaces != null)
                    {
                        PlaceRoom(spaces);
                        placed = true;
                        break;
                    }
                }
                pos = PickFrontierCell();
            }

            if (!placed)
                Debug.LogWarning($"# {i} failed after {maxTries} tries");
        }

        // checkConnections();
        checkOverlapping();
    }

    Vector3Int PickFrontierCell()
    {
        frontier.RemoveWhere(f => usedSpaces.Contains(f));
        if (frontier.Count == 0)
            throw new InvalidOperationException("No frontier left");
        var arr = frontier.ToArray();
        return arr[random(0, arr.Length)];
    }

    // Tries to grow a connected region of 'count' cells from start;
    // returns world-grid positions if successful, null otherwise.
    List<Vector3Int>? FindSpaces(Vector3Int start, int count)
    {
        var picked = new List<Vector3Int> { start };
        var localUsed = new HashSet<Vector3Int>(usedSpaces);

        for (int k = 1; k < count; k++)
        {
            // shuffle current picks once per layer
            int n = picked.Count;
            for (int i = n - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                var tmp = picked[i]; picked[i] = picked[j]; picked[j] = tmp;
            }

            bool found = false;
            foreach (var cell in picked)
                foreach (var off in offsets)
                {
                    var cand = cell + off;
                    if (localUsed.Add(cand)) // only true if was absent
                    {
                        picked.Add(cand);
                        found = true;
                        goto NEXT;
                    }
                }
            NEXT:
            if (!found) return null; // does this work for preventing overlaps?
        }

        if (OverlapsExisting(picked)) return null;

        return picked;
    }

    void PlaceRoom(List<Vector3Int> cells, bool isEmpty = false)
    {
        // alleen écht nieuwe coords overhouden
        var newCells = cells
            .Where(c => !usedSpaces.Contains(c))
            .ToList();

        // update sets & frontier enkel voor die newCells
        foreach (var c in newCells)
        {
            usedSpaces.Add(c);
            UpdateFrontier(c);
        }

        // prune frontier
        frontier.RemoveWhere(f => usedSpaces.Contains(f));

        // als er niks nieuws is, niks instantiëren
        if (isEmpty || newCells.Count == 0)
            return;

        CreateRoom(newCells);
    }

    void CreateRoom(List<Vector3Int> room)
    {
        if (room.Count == 1)
        {
            Debug.Log("smallRoom");
            Instantiate(smallRoom, scaleVector(room[0]), quaternion.identity);
            return;
        }

        GameObject parent = Instantiate(empty, scaleVector(room[0]), quaternion.identity);
        parent.name = $"R{UnityEngine.Random.Range(10000, 100000).ToString()}";

        var col = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
        foreach (var c in room)
        {
            var worldPos = scaleVector(c);
            var tile = Instantiate(floor, worldPos, quaternion.identity);
            tile.name = $"F{UnityEngine.Random.Range(10000, 100000).ToString()}";
            tile.transform.SetParent(parent.transform);
            foreach (var r in tile.GetComponentsInChildren<Renderer>())
                r.material.color = col;
        }
    }

    void UpdateFrontier(Vector3Int added)
    {
        foreach (var off in offsets)
        {
            var n = added + off;
            if (!usedSpaces.Contains(n))
                frontier.Add(n); // HashSet ignores duplicates
        }
    }

    int RandomIntWithWeights(int[] numbers, int[] weights)
    {
        int total = 0;
        for (int i = 0; i < weights.Length; i++)
            total += weights[i];

        int r = random(0, total);
        for (int i = 0; i < weights.Length; i++)
        {
            if (r < weights[i]) return numbers[i];
            r -= weights[i];
        }
        return numbers[^1];
    }

    Vector3Int scaleVector(Vector3Int v)
    {
        return new Vector3Int(v.x * size, 0, v.z * size);
    }

    Vector3Int scaleVector(Vector3 v)
    {
        return new Vector3Int((int)(v.x * size), 0, (int)(v.z * size));
    }

    bool OverlapsExisting(IEnumerable<Vector3Int> cells)
    {
        foreach (var c in cells)
        {
            if (usedSpaces.Contains(c))
                return true;
        }
        return false;
    }

    void checkOverlapping()
    {
        var rooms = GameObject.FindGameObjectsWithTag("Room");
        var positions = new Dictionary<Vector3, Transform>();
        bool overlapFound = false;

        foreach (var room in rooms)
        {
            foreach (Transform child in room.transform)
            {
                if (positions.TryGetValue(child.position, out var otherChild))
                {
                    Debug.LogWarning($"Overlap found at {child.position} between children: {otherChild.name} (in {otherChild.parent?.name}) and {child.name} (in {child.parent?.name})");
                    overlapFound = true;
                }
                else
                {
                    positions[child.position] = child;
                }
            }
        }

        if (!overlapFound)
            Debug.Log("No overlapping child positions found.");
    }
}
