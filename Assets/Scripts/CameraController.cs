using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class CameraController : MonoBehaviour
{
    public Player player;
    public float mouseX;
    private float mouseY;
    public float sens;

    private float pitch = 0f; 
    public float maxPitch = 80f;

    private IEnumerator Start()
    {
        yield return null;
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
    }

    void Update()
    {
        transform.position = player.transform.position + new Vector3(0f, 1f, 0f);

        mouseX = Mouse.current != null ? Mouse.current.delta.x.ReadValue() : 0f;
        mouseY = Mouse.current != null ? Mouse.current.delta.y.ReadValue() : 0f;

        pitch -= mouseY * sens / 10;
        pitch = Mathf.Clamp(pitch, -maxPitch, maxPitch);

        transform.rotation = Quaternion.Euler(
            pitch,
            transform.eulerAngles.y + mouseX * sens / 10,
            0f
        );
    }
}
