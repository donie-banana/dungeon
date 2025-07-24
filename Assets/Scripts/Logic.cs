using UnityEngine;

public class Logic : MonoBehaviour
{
    public GameObject player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Instantiate(player, new Vector3(0, 1, 0), Quaternion.identity);        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
