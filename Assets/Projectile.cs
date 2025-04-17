using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 15f;
    public float lifetime = 3f;
    public float damage = 100f; // Adjust as needed

    private Vector3 direction;

    public void Initialize(Vector3 shootDirection)
    {
        direction = shootDirection.normalized;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                Debug.Log("[Projectile] Hit enemy. Applying damage.");
                enemy.TakeDamage(damage);
            }

            Destroy(gameObject); // Destroy the projectile on hit
        }
    }
}

