using UnityEngine;
using UnityEngine.InputSystem;
using System;
using ST = StatType;
using JetBrains.Annotations;

public class Player : MonoBehaviour
{
    private Rigidbody rb;
    public Camera playerCamera; 
    public GameObject bullet;
    private Keyboard input;
    private Mouse mouse;
    private const float decelFactor = 0.8f;
    private (Key key, Func<Vector3> dir)[] axes;
    private (GameObject item, float order, float amount)[] inventory;
    public Stats stats;
    private float timer = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        input = Keyboard.current;
        mouse = Mouse.current;
        stats = GetComponent<Stats>();
        playerCamera = Camera.main;

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

        timer += Time.deltaTime;

        if (mouse.leftButton.wasPressedThisFrame && timer >= stats["reloadSpeed "])
        {
            shoot();
            timer = 0;
        }
    }

    void shoot()
    {
        var pos = playerCamera.transform.position - playerCamera.transform.up * 0.25f + playerCamera.transform.forward;
        GameObject bulletInstance = Instantiate(bullet, pos, playerCamera.transform.rotation);
        BulletController Bctrl = bulletInstance.GetComponent<BulletController>();
        Bctrl.PlayerVel = rb.linearVelocity;
        Bctrl.stats = stats;
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
            rb.AddForce(dir.normalized * stats["bulletSpeed"]);

        bool running = IsKeyPressed(Key.LeftShift) || IsKeyPressed(Key.RightShift);
        float limit = running ? stats["runSpeed"] : stats["walkSpeed"];
        if (rb.linearVelocity.magnitude > limit)
            rb.linearVelocity = rb.linearVelocity.normalized * limit;
    }
}
