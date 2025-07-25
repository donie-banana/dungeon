#nullable enable

using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;


public class Generator : MonoBehaviour
{
    // grid coords in units of 'size'
    private HashSet<Vector3Int> usedSpaces = new HashSet<Vector3Int> { new Vector3Int(0,0,0) };
    private HashSet<Vector3Int> emptySpaces = new HashSet<Vector3Int> { new Vector3Int(0,0,0) };
    private List<Vector3Int> frontier = new List<Vector3Int>();
    public int roomCount;
    private Vector3Int[] offsets;
    private int size = 8;

    private Func<int,int,int> random = UnityEngine.Random.Range;

    // room prefabs…
    public GameObject player, startRoom, floor;

    void Start()
    {
        offsets = new[] {
            new Vector3Int( 1, 0, 0),
            new Vector3Int( 0, 0, 1),
            new Vector3Int(-1, 0, 0),
            new Vector3Int( 0, 0,-1),
        };

        // initialize frontier around (0,0,0)
        UpdateFrontier(new Vector3Int(0,0,0));

        Instantiate(startRoom, Vector3.zero, quaternion.identity);
        Instantiate(player, new Vector3(0,1,0), quaternion.identity);

        Generate();
    }

    void Generate()
    {
        const int maxTries = 20;

        int emptyAmount = random(4, 8);

        roomCount = Mathf.CeilToInt(roomCount * (1 / emptyAmount + 1));

        for (int i = 0; i < roomCount; i++)
        {
            int chosenSize = RandomIntWithWeights(
                new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 20, 25, 35, 50, 60 },
                new[] { 96, 96, 96, 640, 560, 560, 560, 64, 48, 32, 24, 16, 8, 4, 1 }
            );

            bool placed = false;
            bool isEmpty = i % emptyAmount == 0 ? true : false;
            Vector3Int pos = PickFrontierCell();

            for (int t = 0; t < maxTries && !placed; t++)
            {
                var spaces = FindSpaces(pos, chosenSize);
                if (spaces != null)
                {
                    PlaceRoom(spaces, isEmpty);
                    placed = true;
                    break;
                }
                pos = PickFrontierCell();
            }

            if (!placed)
                Debug.LogWarning($"# {i} failed after {maxTries} tries");
        }

        checkConnections();
    }

    void checkConnections()
    {
        while (true)
        {
            var visited = new HashSet<Vector3Int>();
            var queue = new Queue<Vector3Int>();
            queue.Enqueue(new Vector3Int(0,0,0));
            visited.Add(new Vector3Int(0,0,0));

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                foreach (var off in offsets)
                {
                    var neighbor = current + off;
                    if (usedSpaces.Contains(neighbor) && !visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }

            if (visited.Count == usedSpaces.Count)
            {
                Debug.Log("All rooms are connected.");
                break;
            }

            // Find the first unvisited room
            Vector3Int? isolated = null;
            foreach (var cell in usedSpaces)
            {
                if (!visited.Contains(cell))
                {
                    isolated = cell;
                    break;
                }
            }
            if (isolated == null)
            {
                Debug.LogWarning("No isolated room found, but not all rooms are connected?");
                break;
            }

            // Find the closest visited cell to the isolated cell
            Vector3Int closest = new Vector3Int();
            int minDist = int.MaxValue;
            foreach (var cell in visited)
            {
                int dist = Mathf.Abs(cell.x - isolated.Value.x) + Mathf.Abs(cell.y - isolated.Value.y) + Mathf.Abs(cell.z - isolated.Value.z);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = cell;
                }
            }

            // Create a path between closest and isolated
            var path = new List<Vector3Int>();
            Vector3Int from = closest;
            Vector3Int to = isolated.Value;
            while (from != to)
            {
                if (from.x != to.x) from.x += Math.Sign(to.x - from.x);
                else if (from.y != to.y) from.y += Math.Sign(to.y - from.y);
                else if (from.z != to.z) from.z += Math.Sign(to.z - from.z);
                path.Add(new Vector3Int(from.x, from.y, from.z));
            }

            PlaceRoom(path, false);
            Debug.Log("Connected isolated room.");
            // Loop again to check if more are isolated
        }
    }

    Vector3Int PickFrontierCell()
    {
        if(frontier.Count == 0)
            throw new InvalidOperationException("No frontier left");

        int idx = random(0, frontier.Count);
        return frontier[idx];
    }

    // Tries to grow a connected region of 'count' cells from start;
    // returns world-grid positions if successful, null otherwise.
    List<Vector3Int>? FindSpaces(Vector3Int start, int count)
    {
        var picked = new List<Vector3Int> { start };
        var localUsed = new HashSet<Vector3Int>(usedSpaces);
        localUsed.UnionWith(emptySpaces);

        for(int k=1; k<count; k++)
        {
            // shuffle current picks once per layer
            int n = picked.Count;
            for(int i=n-1; i>0; i--)
            {
                int j = UnityEngine.Random.Range(0, i+1);
                var tmp = picked[i]; picked[i] = picked[j]; picked[j] = tmp;
            }

            bool found = false;
            foreach(var cell in picked)
            foreach(var off  in offsets)
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
            if (!found) return null;
        }

        return picked;
    }

    void PlaceRoom(List<Vector3Int> cells, bool empty = false)
    {
        // commit new cells
        foreach (var c in cells)
        {
            if (empty) emptySpaces.Add(c);
            else usedSpaces.Add(c);
            UpdateFrontier(c);
        }

        if (empty) return;

        var col = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
        foreach(var c in cells)
        {
            var worldPos = new Vector3(c.x*size, 0, c.z*size);
            var tile = Instantiate(floor, worldPos, quaternion.identity);
            foreach(var r in tile.GetComponentsInChildren<Renderer>())
                r.material.color = col;
        }   
    }

    void UpdateFrontier(Vector3Int added)
    {
        foreach(var off in offsets)
        {
            var n = added + off;
            if (!usedSpaces.Contains(n))
                frontier.Add(n);
        }
    }

    int RandomIntWithWeights(int[] numbers, int[] weights)
    {
        int total = 0;
        for(int i=0;i<weights.Length;i++)
            total += weights[i];

        int r = random(0, total);
        for(int i=0;i<weights.Length;i++)
        {
            if (r < weights[i]) return numbers[i];
            r -= weights[i];
        }
        return numbers[^1];
    }
}
