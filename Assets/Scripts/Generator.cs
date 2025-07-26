#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class Generator : MonoBehaviour
{
    private HashSet<Vector3Int> usedSpaces = new HashSet<Vector3Int> { new Vector3Int(0, 0, 0) };
    private HashSet<Vector3Int> frontier = new HashSet<Vector3Int>();
    private List<List<Vector3Int>> rooms = new List<List<Vector3Int>>();
    public int roomCount;
    private Vector3Int[] offsets;
    private int size = 8;
    private Dictionary<bool[], GameObject> tileTypes;

    private Func<int, int, int> random = UnityEngine.Random.Range;

    public GameObject player, floor, dCorner, dWalls, corner, hallway, smallRoom, wall;

    void Start()
    {
        offsets = new[] {
            new Vector3Int( 1, 0, 0),
            new Vector3Int( 0, 0, 1),
            new Vector3Int(-1, 0, 0),
            new Vector3Int( 0, 0,-1),
        };

        tileTypes = new Dictionary<bool[], GameObject>(new BoolArrayComparer())
        {
            { new[] { true, true, false, true }, dCorner },
            { new[] { true, true, false, false }, corner },
            { new[] { true, false, false, false }, wall },
            { new[] { true, false, true, false }, dWalls },
        };

        UpdateFrontier(new Vector3Int(0, 0, 0));

        Instantiate(smallRoom, scaleVector(Vector3Int.zero), quaternion.identity);
        Instantiate(player, new Vector3(0, 1, 0), quaternion.identity);

        usedSpaces.Add(new Vector3Int(0, 0, 0));
        frontier.RemoveWhere(f => f == new Vector3Int(0, 0, 0));

        Generate();
        PrintRooms();
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

    List<Vector3Int>? FindSpaces(Vector3Int start, int count)
    {
        var picked = new List<Vector3Int> { start };
        var localUsed = new HashSet<Vector3Int>(usedSpaces);

        for (int k = 1; k < count; k++)
        {
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
                    if (localUsed.Add(cand))
                    {
                        picked.Add(cand);
                        found = true;
                        goto NEXT;
                    }
                }
            NEXT:
            if (!found) return null;
        }

        if (OverlapsExisting(picked)) return null;

        return picked;
    }

    void PlaceRoom(List<Vector3Int> cells, bool isEmpty = false)
    {
        var newCells = cells
            .Where(c => !usedSpaces.Contains(c))
            .ToList();

        List<Vector3Int> room = new List<Vector3Int>();

        foreach (var c in newCells)
        {
            room.Add(c);
            usedSpaces.Add(c);
            UpdateFrontier(c);
        }

        rooms.Add(room);

        frontier.RemoveWhere(f => usedSpaces.Contains(f));

        if (isEmpty || newCells.Count == 0)
            return;

        CreateRoom(new HashSet<Vector3Int>(newCells));
    }

    void CreateRoom(HashSet<Vector3Int> room)
    {
        if (room.Count == 0) return;

        Color color = UnityEngine.Random.ColorHSV(0f, 1f, 0.7f, 1f, 0.5f, 0.85f);

        GameObject parent = new GameObject();
        parent.transform.position = scaleVector(room.First());
        parent.name = random(1, 10001).ToString();
        parent.tag = "Room";

        foreach (Vector3Int space in room)
        {
            List<bool> walls = new List<bool>();

            foreach (Vector3Int off in offsets)
            {
                if (!room.Contains(off + space)) walls.Add(true);
                else walls.Add(false);
            }

            // Detect if this is a straight corridor (two opposite walls open, two closed)
            bool isCorridor = (walls.Count == 4) &&
                ((walls[0] && walls[2] && !walls[1] && !walls[3]) || (!walls[0] && !walls[2] && walls[1] && walls[3]));

            (GameObject? tile, int degrees) = CompareWalls(walls, isCorridor);

            if (tile == null)
            {
                Debug.LogError("tile = null");
                continue;
            }

            GameObject? placed = Instantiate(tile, scaleVector(space), Quaternion.Euler(0, degrees, 0));

            if (placed == null)
            {
                Debug.LogError("not placed");
                return;
            }

            placed.transform.SetParent(parent.transform);
            colorTile(placed, color);
        }
    }

    void UpdateFrontier(Vector3Int added)
    {
        foreach (var off in offsets)
        {
            var n = added + off;
            if (!usedSpaces.Contains(n))
                frontier.Add(n);
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

    void PrintRooms()
    {
        for (int i = 0; i < rooms.Count; i++)
        {
            string coords = string.Join(", ", rooms[i].Select(v => $"({v.x},{v.y},{v.z})"));
            Debug.Log($"Room {i}: {coords}");
        }
    }

    (GameObject?, int) CompareWalls(List<bool> walls, bool isCorridor = false)
    {
        GameObject? tile = null;
        int degrees = 0;
        List<bool> checker = new List<bool>();

        if (!walls.Contains(true) || !walls.Contains(false))
        {
            degrees = 0;

            if (!walls.Contains(true))
            {
                tile = floor;
            }
            else
            {
                tile = smallRoom;
            }
        }
        else
        {
            for (int i = 0; i <= 8; i++)
            {
                checker.Add(walls[i % 4]);
                if (checker.Count > 4) checker.RemoveAt(0);

                if (checker.Count == 4)
                {
                    var arr = checker.ToArray();
                    if (tileTypes.TryGetValue(arr, out var foundTile))
                    {
                        if (foundTile == dWalls && isCorridor && UnityEngine.Random.value < 0.2f)
                            tile = hallway;
                        else
                            tile = foundTile;
                        degrees = 360 - (i - 4) * 90;
                        break;
                    }
                }
            }
        }

        if (tile == null) return (null, 0);

        return (tile, degrees);
    }

    // Color each child and child of child if they have a renderer
    void colorTile(GameObject tile, Color color)
    {
        foreach (var renderer in tile.GetComponentsInChildren<Renderer>(true))
        {
            renderer.material.color = color;
        }
    }
}

public class BoolArrayComparer : IEqualityComparer<bool[]>
{
    public bool Equals(bool[]? x, bool[]? y)
    {
        if (x == null || y == null || x.Length != y.Length)
            return false;

        for (int i = 0; i < x.Length; i++)
        {
            if (x[i] != y[i])
                return false;
        }

        return true;
    }

    public int GetHashCode(bool[] obj)
    {
        int hash = 17;

        foreach (bool b in obj)
        {
            hash = hash * 31 + (b ? 1 : 0);
        }

        return hash;
    }
}
