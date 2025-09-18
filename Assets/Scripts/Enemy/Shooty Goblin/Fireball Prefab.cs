using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(Rigidbody))]
public class SimpleFireball : MonoBehaviour
{
    [SerializeField] float lifetime = 5f;

    Rigidbody rb;
    int damage;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        var col = GetComponent<SphereCollider>();
        col.isTrigger = true;
    }

    public void Launch(Vector3 velocity, int dmg)
    {
        damage = dmg;
        rb.linearVelocity = velocity;
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter(Collider other)
    {
        // Damage the player if present
        var hp = other.GetComponent<PlayerHealth>();
        if (hp != null)
        {
            hp.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // Hit any solid world geometry
        if (!other.isTrigger)
        {
            Destroy(gameObject);
        }
    }
}
