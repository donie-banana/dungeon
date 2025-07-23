using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class Player : MonoBehaviour
{
    private Rigidbody rb;
    public Camera playerCamera; 
    public GameObject bullet;
    public float thrust;
    public float maxSpeed;
    public float maxRunSpeed;
    private Keyboard input;
    private Mouse mouse;
    private const float decelFactor = 0.8f;
    private (Key key, Func<Vector3> dir)[] axes;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        input = Keyboard.current;
        mouse = Mouse.current;

        axes = new (Key, Func<Vector3>)[]
        { 
            (Key.W, () => transform.forward),
            (Key.S, () => -transform.forward),
            (Key.A, () => -transform.right),
            (Key.D, () => transform.right)
        };
    }

    void Update()
    {
        Move();
        Look();

        if (mouse.leftButton.wasPressedThisFrame)
        {
            shoot();
        }
    }

    void shoot()
    {
        var pos = playerCamera.transform.position - playerCamera.transform.up * 0.25f + playerCamera.transform.forward;
        GameObject bulletInstance = Instantiate(bullet, pos, playerCamera.transform.rotation);
        BulletController bulletController = bulletInstance.GetComponent<BulletController>();
        bulletController.PlayerVel = rb.linearVelocity; 
    }

    void Look()
    {
        var y = playerCamera.transform.eulerAngles.y;
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, y, transform.eulerAngles.z);
    }

    private bool IsKeyPressed(Key key)
    {
        if (input == null) return false;
        switch (key)
        {
            case Key.W: return input.wKey.isPressed;
            case Key.A: return input.aKey.isPressed;
            case Key.S: return input.sKey.isPressed;
            case Key.D: return input.dKey.isPressed;
            case Key.LeftShift: return input.leftShiftKey.isPressed;
            case Key.RightShift: return input.rightShiftKey.isPressed;
            default: return false;
        }
    }

    void Move()
    {
        var dir = Vector3.zero;

        foreach (var (key, getVec) in axes)
        {
            var vec = getVec();
            if (IsKeyPressed(key))
            {
                dir += vec;
            }
            else
            {
                float speed = Vector3.Dot(rb.linearVelocity, vec);
                if (speed > 0f)
                {
                    var proj = vec * speed;
                    rb.linearVelocity = rb.linearVelocity - proj + proj * decelFactor;
                }
            }
        }

        if (dir != Vector3.zero)
            rb.AddForce(dir.normalized * thrust);

        bool running = IsKeyPressed(Key.LeftShift) || IsKeyPressed(Key.RightShift);
        float limit = running ? maxRunSpeed : maxSpeed;
        if (rb.linearVelocity.magnitude > limit)
            rb.linearVelocity = rb.linearVelocity.normalized * limit;
    }
}
