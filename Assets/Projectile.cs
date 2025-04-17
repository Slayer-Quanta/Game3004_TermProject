using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 15f;
    public float lifetime = 3f;
    public int damage = 10;

    private Vector3 direction;

    public void Initialize(Vector3 shootDirection)
    {
        direction = shootDirection.normalized;
        Debug.Log($"[Projectile] Initialized with direction: {direction}");
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
        Debug.DrawRay(transform.position, direction * 0.5f, Color.red);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[Projectile] Trigger hit with: {other.name}");

        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                Debug.Log("[Projectile] Enemy detected. Dealing damage.");
                enemy.TakeDamage(damage);
            }

            Destroy(gameObject);
        }
    }
}
