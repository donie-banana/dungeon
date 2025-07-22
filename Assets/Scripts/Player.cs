using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class Player : MonoBehaviour
{
    private Rigidbody rb;
    public Camera camera;
    private CameraController cameraController;
    public GameObject bullet;
    public float thrust;
    public float maxSpeed;
    public float maxRunSpeed;
    public Keyboard input;
    private const float decelFactor = 0.8f;
    private (Key key, Func<Vector3> dir)[] axes;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        cameraController = camera.GetComponent<CameraController>();
        input = Keyboard.current;

        axes = new (Key, Func<Vector3>)[]
        {
            (Key.W, () => -transform.forward),
            (Key.S, () =>  transform.forward),
            (Key.A, () =>  transform.right),
            (Key.D, () => -transform.right)
        };
    }

    void Update()
    {
        Move();
        Look();
    }

    void Look()
    {
        float mx = cameraController.mouseX;
        if (mx != 0f)
            transform.Rotate(0f, mx * cameraController.sens / 10f, 0f);
    }

    void Move()
    {
        var dir = Vector3.zero;

        foreach (var (key, getVec) in axes)
        {
            var vec = getVec();
            if (input[key].isPressed)
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

        bool running = input[Key.LeftShift].isPressed || input[Key.RightShift].isPressed;
        float limit = running ? maxRunSpeed : maxSpeed;
        if (rb.linearVelocity.magnitude > limit)
            rb.linearVelocity = rb.linearVelocity.normalized * limit;
    }
}
