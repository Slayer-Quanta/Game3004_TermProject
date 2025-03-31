using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System.Collections;

public class Enemy : MonoBehaviour
{
    [Header("Health")]
    public float initialHealth = 100;
    public float currentHealth;
    public Image healthFill;

    [Header("AI Navigation")]
    public Transform player;
    public NavMeshAgent agent;
    public float updateRate = 1f;
    public float detectionRadius = 15f;
    public float attackRange = 1.5f;

    [Header("Attack")]
    public float attackDamage = 10f;
    public float attackCooldown = 1.5f;
    private float nextAttackTime;
    public GameObject attackEffectPrefab;
    private Character playerCharacter;
    private bool playerInRange = false;

    [Header("Animation")]
    public Animator animator;
    private static readonly int SpeedParam = Animator.StringToHash("speed");
    private static readonly int AttackParam = Animator.StringToHash("attack");
    private static readonly int DeadParam = Animator.StringToHash("dead");

    [Header("Effects")]
    public ParticleSystem hitEffect;
    public ParticleSystem deathEffect;
    public float ragdollForce = 10f;
    public bool isDead = false;

    private Collider enemyCollider;
    private Rigidbody enemyRigidbody;

    public void Init(Transform player)
    {
        this.player = player;
        playerCharacter = player.GetComponent<Character>();
        enabled = true;
        currentHealth = initialHealth;

        if (animator == null)
            animator = GetComponent<Animator>();

        enemyCollider = GetComponent<Collider>();
        enemyRigidbody = GetComponent<Rigidbody>();

        InvokeRepeating(nameof(UpdatePlayerDetection), 0, updateRate);
    }

    private void Awake()
    {
        // Set initial values
        currentHealth = initialHealth;
        nextAttackTime = 0f;
    }

    private void Update()
    {
        if (isDead)
            return;

        // Update animation based on movement
        if (animator != null && agent.enabled)
        {
            animator.SetFloat(SpeedParam, agent.velocity.magnitude / agent.speed);
        }

        // Check if player is in attack range
        if (player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            playerInRange = distanceToPlayer <= attackRange;

            if (playerInRange && Time.time >= nextAttackTime)
            {
                AttackPlayer();
            }
        }
    }

    private void UpdatePlayerDetection()
    {
        if (isDead || player == null)
            return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Only chase player if within detection radius
        if (distanceToPlayer <= detectionRadius)
        {
            // Set destination to player position
            agent.SetDestination(player.position);

            // Check if path is possible
            if (agent.pathStatus == NavMeshPathStatus.PathPartial || agent.pathStatus == NavMeshPathStatus.PathInvalid)
            {
                // Path is not valid, try to find an alternative
                NavMeshPath path = new NavMeshPath();
                agent.CalculatePath(player.position, path);
                if (path.status != NavMeshPathStatus.PathComplete)
                {
                    // No valid path to player
                    agent.ResetPath();
                }
            }
        }
        else
        {
            // Player out of detection range
            agent.ResetPath();
        }
    }

    private void AttackPlayer()
    {
        // Set next attack time
        nextAttackTime = Time.time + attackCooldown;

        // Play attack animation
        if (animator != null)
        {
            animator.SetTrigger(AttackParam);
        }

        // Damage player with slight delay to match animation
        StartCoroutine(DamagePlayerWithDelay(0.3f));
    }

    private IEnumerator DamagePlayerWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (playerCharacter != null && playerInRange && !isDead)
        {
            // Double-check if player is still in range
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (distanceToPlayer <= attackRange)
            {
                // Apply damage to player
                playerCharacter.TakeDamage(attackDamage);

                // Play attack effect
                if (attackEffectPrefab != null)
                {
                    Vector3 effectPosition = player.position;
                    effectPosition.y += 1f; // Offset to hit body
                    GameObject effect = Instantiate(attackEffectPrefab, effectPosition, Quaternion.identity);
                    Destroy(effect, 2f);
                }

                // Play attack sound
                AudioManager.instance.PlaySFX("Enemy_Attack");
            }
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead)
            return;

        currentHealth -= damage;

        // Visual feedback
        if (hitEffect != null)
        {
            hitEffect.Play();
        }

        // Audio feedback
        AudioManager.instance.PlaySFX("Enemy_Hit");

        // Update health UI
        if (healthFill != null)
        {
            healthFill.fillAmount = currentHealth / initialHealth;
        }

        if (currentHealth <= 0)
        {
            Dead();
        }
    }

    void Dead()
    {
        isDead = true;

        // Stop movement and attacks
        if (agent != null && agent.enabled)
        {
            agent.ResetPath();
            agent.enabled = false;
        }

        // Play death animation
        if (animator != null)
        {
            animator.SetBool(DeadParam, true);
        }

        // Play death effect
        if (deathEffect != null)
        {
            deathEffect.Play();
        }

        // Play death sound
        AudioManager.instance.PlaySFX("Enemy_Death");

        // Add physics-based ragdoll effect if rigidbody exists
        if (enemyRigidbody != null)
        {
            enemyRigidbody.isKinematic = false;
            Vector3 forceDirection = transform.position - player.position;
            forceDirection.y = 1f;
            forceDirection.Normalize();
            enemyRigidbody.AddForce(forceDirection * ragdollForce, ForceMode.Impulse);
        }

        // Remove collider to prevent further interactions
        if (enemyCollider != null)
        {
            enemyCollider.enabled = false;
        }

        // Destroy after delay to allow animations and effects to play
        Destroy(gameObject, 3f);
    }

    [ButtonLUFI]
    void TestTakeDamage()
    {
        TakeDamage(10);
    }

    private void OnDrawGizmosSelected()
    {
        // Draw detection radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}