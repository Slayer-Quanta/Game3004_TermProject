using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    [Header("Health")]
    public float initialHealth = 100;
    public float currentHealth;
    public Image healthFill;

    [Space(20)]
    public Transform player;
    public NavMeshAgent agent;
    public float updateRate = 1f;


    public void Init(Transform player)
    {
        this.player = player;
        enabled = true;
        initialHealth = currentHealth;

        InvokeRepeating(nameof(UpdatePlayerDetection), 0, updateRate);
    }

    private void UpdatePlayerDetection()
    {
        agent.SetDestination(player.position);
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        healthFill.fillAmount = currentHealth / initialHealth;
        if (currentHealth <= 0)
        {
            Dead();
        }
    }

    void Dead()
    {
        Destroy(gameObject);
    }


    [ButtonLUFI]
    void TestTakeDamage()
    {
        TakeDamage(10);
    }
}
