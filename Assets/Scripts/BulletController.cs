using UnityEngine;

public class BulletController : MonoBehaviour
{
    public float thrust;
    public float lifeTime;
    public Vector3 PlayerVel;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        float forwardComponent = Vector3.Dot(PlayerVel, transform.forward);
        float rightComponent = Vector3.Dot(PlayerVel, transform.right);

        Vector3 bulletVelocity = transform.forward * thrust
            + transform.forward * forwardComponent * 0.2f
            + transform.right * rightComponent * 0.8f;

        rb.linearVelocity = bulletVelocity;
        Destroy(gameObject, lifeTime);
    }

    void OnCollisionEnter(Collision collision)
    {
        Destroy(gameObject);
    }
}