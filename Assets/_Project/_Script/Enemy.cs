using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System.Collections;

public class Enemy : MonoBehaviour
{
    [Header("Health")]
    public float initialHealth = 100f;
    public float currentHealth;
    public Image healthFill;

    [Header("AI Navigation")]
    public Transform player;
    public NavMeshAgent agent;
    public float updateRate = 1f;
    public float detectionRadius = 15f;
    public float attackRange = 1.5f;

    private float sqrDetectionRadius;
    private float sqrAttackRange;
    private NavMeshPath cachedPath;
    private float nextPathUpdateTime;

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

    [Header("Touch Damage Over Time")]
    public float contactDamage = 5f;                      // ✅ Separate contact damage
    public float contactDamageCooldown = 1.5f;
    private float nextTouchDamageTime = 0f;

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

        sqrDetectionRadius = detectionRadius * detectionRadius;
        sqrAttackRange = attackRange * attackRange;
        cachedPath = new NavMeshPath();
        nextPathUpdateTime = 0f;
    }

    private void Awake()
    {
        currentHealth = initialHealth;
        nextAttackTime = 0f;
    }

    private void Update()
    {
        if (isDead)
            return;

        if (animator != null && agent.enabled)
            animator.SetFloat(SpeedParam, agent.velocity.magnitude / agent.speed);

        if (player != null)
        {
            Vector3 directionToPlayer = player.position - transform.position;
            float sqrDistanceToPlayer = directionToPlayer.sqrMagnitude;
            playerInRange = sqrDistanceToPlayer <= sqrAttackRange;

            if (playerInRange && Time.time >= nextAttackTime)
            {
                AttackPlayer();
            }

            if (Time.time >= nextPathUpdateTime)
            {
                UpdatePlayerDetection(sqrDistanceToPlayer);
                nextPathUpdateTime = Time.time + updateRate;
            }
        }
    }

    private void UpdatePlayerDetection(float sqrDistanceToPlayer)
    {
        if (isDead || player == null)
            return;

        if (sqrDistanceToPlayer <= sqrDetectionRadius)
        {
            agent.CalculatePath(player.position, cachedPath);

            if (cachedPath.status == NavMeshPathStatus.PathComplete)
                agent.SetPath(cachedPath);
            else
                agent.ResetPath();
        }
        else
        {
            agent.ResetPath();
        }
    }

    private void AttackPlayer()
    {
        nextAttackTime = Time.time + attackCooldown;

        if (animator != null)
            animator.SetTrigger(AttackParam);

        StartCoroutine(DamagePlayerWithDelay(0.3f));
    }

    private IEnumerator DamagePlayerWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (playerCharacter != null && playerInRange && !isDead)
        {
            playerCharacter.TakeDamage(attackDamage);

            if (attackEffectPrefab != null)
            {
                Vector3 effectPosition = player.position + Vector3.up;
                Instantiate(attackEffectPrefab, effectPosition, Quaternion.identity);
            }

            AudioManager.instance.PlaySFX("Enemy_Attack");
        }
    }

    // ✅ Touch-based damage (separate from attack animation)
    private void OnTriggerStay(Collider other)
    {
        if (isDead || Time.time < nextTouchDamageTime)
            return;

        if (other.CompareTag("Player"))
        {
            Character player = other.GetComponent<Character>();
            if (player != null && !player.IsDead())
            {
                Debug.Log("[Enemy] Dealing contact damage to player.");
                player.TakeDamage(contactDamage);
                nextTouchDamageTime = Time.time + contactDamageCooldown;

                if (attackEffectPrefab != null)
                {
                    Instantiate(attackEffectPrefab, transform.position + Vector3.up, Quaternion.identity);
                }

                AudioManager.instance.PlaySFX("Enemy_Attack");
            }
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead)
            return;

        currentHealth -= damage;
        Debug.Log($"[Enemy] Took {damage} damage.");

        if (hitEffect != null)
            hitEffect.Play();
        if (healthFill != null)
            healthFill.fillAmount = currentHealth / initialHealth;

        if (currentHealth <= 0)
        {
            Dead();
        }
    }

    void Dead()
    {
        isDead = true;
        if (agent != null && agent.enabled)
            agent.enabled = false;
        if (animator != null)
            animator.SetBool(DeadParam, true);
        if (deathEffect != null)
            deathEffect.Play();
        AudioManager.instance.PlaySFX("Enemy_Death");

        if (enemyRigidbody != null)
        {
            enemyRigidbody.isKinematic = false;
            Vector3 forceDirection = transform.position - player.position;
            forceDirection.y = 1f;
            enemyRigidbody.AddForce(forceDirection.normalized * ragdollForce, ForceMode.Impulse);
        }

        if (enemyCollider != null)
            enemyCollider.enabled = false;

        Destroy(gameObject, 3f);
    }
}
