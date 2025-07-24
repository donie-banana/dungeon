using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Generator : MonoBehaviour
{
    private List<Vector3> usedSpaces = new List<Vector3> { new Vector3(0, 0, 0) };
    public int roomCount;
    private Vector3[] neighbours;

    // room building stuff
    public GameObject player;
    public GameObject item;
    public GameObject floor;
    public GameObject wall;
    public GameObject corner;
    public GameObject dCorner;
    public GameObject smallRoom;
    public GameObject startRoom;
    public GameObject hallway;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        neighbours = new Vector3[] {
            new Vector3(5, 0, 0),
            new Vector3(0, 0, 5),
            new Vector3(-5, 0, 0),
            new Vector3(0, 0, -5),
        };

        Instantiate(startRoom, new Vector3(0, 0, 0), quaternion.identity);
        Instantiate(player, new Vector3(0, 1, 0), quaternion.identity);

        generate(new Vector3(5, 0, 0));
    }

    void generate(Vector3 startCoord)
    {
        if (usedSpaces == null)
        {
            usedSpaces = new List<Vector3>();
        }

        // loop:
        //   choose random size of room, with 4 being most likely and 10 being wayy less, and 1 being in the middle of those 2
        //   place walls, corners, floors etc. make sure it fits. if it doesnt, just fill in some space
        //   put the coords of all placed spaces into usedSpaces
        //   decide next place for a room randomly next to an already existing room.
        //   check if the new start point overlaps with a usespace
        //     if it does, dont place anything and rechoose a place for the new room randomly
        //     if it doesn't, start new room there

        for (int i = 0; i < roomCount; i++)
        {
            int chosenAmount = RandomIntWithWeights(
                new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 },
                new int[] { 3, 3, 3, 75, 2, 2, 2, 2, 2, 1 }
            );

            Vector3 chosenPosition = Vector3.zero;

            if (i == 0)
            {
                chosenPosition = startCoord;
            }
            else
            {
                bool foundPosition = false;
                
                while (!foundPosition)
                {
                    Vector3 neighbour = usedSpaces[UnityEngine.Random.Range(0, usedSpaces.Count)];
                    foreach (Vector3 x in neighbours)
                    {
                        Vector3 candidate = neighbour + x;
                        if (!usedSpaces.Contains(candidate))
                        {
                            chosenPosition = candidate;
                            foundPosition = true;
                        }
                    }
                }
            }

            usedSpaces.Add(chosenPosition);

            Instantiate(floor, chosenPosition, quaternion.identity);

            Debug.Log($"chosenAmount: {chosenAmount}, chosenPosition: {chosenPosition}");
        }

        
    }

    int RandomIntWithWeights(int[] roomSizes, int[] weights)
    {
        if (roomSizes == null || weights == null)
            throw new ArgumentNullException("roomSizes and weights cannot be null");
        if (roomSizes.Length != weights.Length)
            throw new ArgumentException("roomSizes.Length must equal weights.Length");

        int totalWeight = 0;
        for (int i = 0; i < weights.Length; i++)
        {
            if (weights[i] < 0)
                throw new ArgumentException("All weights must be non-negative", nameof(weights));
            totalWeight += weights[i];
        }
        if (totalWeight == 0)
            throw new InvalidOperationException("Sum of weights must be greater than zero");

        int rand = UnityEngine.Random.Range(0, totalWeight);
        for (int i = 0; i < weights.Length; i++)
        {
            if (rand < weights[i])
                return roomSizes[i];
            rand -= weights[i];
        }

        return roomSizes[roomSizes.Length - 1];
    }
}
