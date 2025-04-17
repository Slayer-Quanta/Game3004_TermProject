using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.UI;

public class Character : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PauseSystem pauseMenu;
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private Transform headTransform;

    [Header("Health System")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] public float currentHealth;
    [SerializeField] private Image healthBar;
    [SerializeField] private float healthRegenRate = 5f;
    [SerializeField] private float healthRegenDelay = 3f;
    private float lastDamageTime;
    private bool isDead = false;

    [Header("Interaction Settings")]
    public float interactionRayLength = 5;
    public LayerMask groundMask;
    public bool fly = false;

    [Header("Animation")]
    public Animator animator;
    private bool isWaiting = false;
    public World world;

    [Header("Block Interaction")]
    [SerializeField] private float holdTimeToDestroy = 3.0f;
    private float blockInteractionTimer = 0f;
    private bool isHoldingInteraction = false;
    private RaycastHit currentHit;
    private bool validHit = false;

    [Header("Head Movement")]
    [SerializeField] private float headTiltAmount = 5f;
    [SerializeField] private float headTiltSpeed = 3f;
    private float currentHeadTilt = 0f;

    [Header("Shooting")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float projectileSpeed = 15f;

    // ✅ Events
    public delegate void HealthChangedHandler(float currentHealth, float maxHealth);
    public event HealthChangedHandler OnHealthChanged;

    public delegate void PlayerDeathHandler();
    public event PlayerDeathHandler OnPlayerDeath;

    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
        playerInput = GetComponent<PlayerInput>();
        playerMovement = GetComponent<PlayerMovement>();
        world = FindObjectOfType<World>();

        if (headTransform == null)
            headTransform = mainCamera.transform;

        currentHealth = maxHealth;
        lastDamageTime = -healthRegenDelay;
    }

    private void Start()
    {
        playerInput.OnMouseClick += HandleMouseClick;
        playerInput.OnFly += HandleFlyClick;
        playerInput.OnPause += pauseMenu.TogglePause;
        playerInput.OnInventoryToggle += ToggleInventory;

        UpdateHealthBar();
    }

    private void OnDestroy()
    {
        playerInput.OnMouseClick -= HandleMouseClick;
        playerInput.OnFly -= HandleFlyClick;
        playerInput.OnPause -= PauseSystem.self.TogglePause;
        playerInput.OnInventoryToggle -= ToggleInventory;
    }

    private void Update()
    {
        if (isDead)
            return;

        UpdateHeadTilt();

        if (fly)
        {
            HandleFlyMode();
        }
        else
        {
            HandleWalkMode();
        }

        HandleHealthRegeneration();

        if (!Application.isMobilePlatform && Input.GetKeyDown(KeyCode.Space))
        {
            Shoot();
        }

        HandleTouchBlockInteraction();
    }

    private void HandleTouchBlockInteraction()
    {
        bool foundBlockInteractionTouch = false;

        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);

            if (touch.position.x < Screen.width * 0.5f)
            {
                foundBlockInteractionTouch = true;

                if (touch.phase == TouchPhase.Began)
                {
                    Ray playerRay = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
                    validHit = Physics.Raycast(playerRay, out currentHit, interactionRayLength, groundMask);

                    if (validHit)
                    {
                        isHoldingInteraction = true;
                        blockInteractionTimer = 0f;
                        AudioManager.instance.PlayButtonClick();
                    }
                }
                else if ((touch.phase == TouchPhase.Stationary || touch.phase == TouchPhase.Moved) && isHoldingInteraction && validHit)
                {
                    blockInteractionTimer += Time.deltaTime;

                    if (blockInteractionTimer >= holdTimeToDestroy)
                    {
                        ModifyTerrain(currentHit);
                        isHoldingInteraction = false;
                        blockInteractionTimer = 0f;
                    }
                }
                else if (touch.phase == TouchPhase.Ended && isHoldingInteraction && validHit)
                {
                    if (blockInteractionTimer < holdTimeToDestroy)
                    {
                        PlaceBlock(currentHit);
                    }

                    isHoldingInteraction = false;
                    blockInteractionTimer = 0f;
                }

                break;
            }
        }

        if (!foundBlockInteractionTouch)
        {
            isHoldingInteraction = false;
            blockInteractionTimer = 0f;
        }
    }

    private void HandleHealthRegeneration()
    {
        if (Time.time - lastDamageTime > healthRegenDelay && currentHealth < maxHealth)
        {
            currentHealth += healthRegenRate * Time.deltaTime;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
            UpdateHealthBar();
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead)
            return;

        currentHealth -= damage;
        lastDamageTime = Time.time;

        StartCoroutine(DamageEffect());
        UpdateHealthBar();
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Debug.Log("[Player] Health has reached 0. Player is dead.");
            Die();
        }
    }

    public void Heal(float healAmount)
    {
        if (isDead)
            return;

        currentHealth += healAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthBar();
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.fillAmount = currentHealth / maxHealth;
        }
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("[Player] Died.");
        playerMovement.enabled = false;
        playerInput.enabled = false;
        OnPlayerDeath?.Invoke();
    }

    private void HandleFlyClick()
    {
        fly = !fly;
    }

    private void HandleWalkMode()
    {
        animator.SetBool("isGrounded", playerMovement.IsGrounded);

        if (playerMovement.IsGrounded && playerInput.IsJumping && !isWaiting)
        {
            animator.SetTrigger("jump");
            AudioManager.instance.PlayJumpSound();
            isWaiting = true;
            StopAllCoroutines();
            StartCoroutine(ResetWaiting());
        }

        animator.SetFloat("speed", playerInput.MovementInput.magnitude);

        if (playerMovement.IsGrounded && playerInput.MovementInput.magnitude > 0)
        {
            if (!AudioManager.instance.sfxSource.isPlaying)
                AudioManager.instance.PlayWalkSound();
        }

        playerMovement.HandleGravity(playerInput.IsJumping);
        playerMovement.Walk(playerInput.RunningPressed);
    }

    private void HandleFlyMode()
    {
        animator.SetFloat("speed", 0);
        animator.SetBool("isGrounded", false);
        animator.ResetTrigger("jump");

        bool ascendInput = playerInput.IsJumping;
        bool descendInput = playerInput.RunningPressed;
        playerMovement.Fly(ascendInput, descendInput);
    }

    IEnumerator ResetWaiting()
    {
        yield return new WaitForSeconds(0.1f);
        animator.ResetTrigger("jump");
        isWaiting = false;
    }

    private void UpdateHeadTilt()
    {
        if (playerMovement.IsGrounded && playerInput.MovementInput.magnitude > 0.1f)
        {
            float targetTilt = playerInput.MovementInput.x * headTiltAmount;
            currentHeadTilt = Mathf.Lerp(currentHeadTilt, targetTilt, Time.deltaTime * headTiltSpeed);
        }
        else
        {
            currentHeadTilt = Mathf.Lerp(currentHeadTilt, 0f, Time.deltaTime * headTiltSpeed);
        }

        if (headTransform != null)
        {
            Vector3 currentRotation = headTransform.localEulerAngles;
            headTransform.localEulerAngles = new Vector3(currentRotation.x, currentRotation.y, currentHeadTilt);
        }
    }

    private void HandleMouseClick()
    {
        if (!Application.isMobilePlatform)
        {
            AudioManager.instance.PlayButtonClick();
            Ray playerRay = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
            if (Physics.Raycast(playerRay, out RaycastHit hit, interactionRayLength, groundMask))
            {
                if (Input.GetMouseButton(0))
                {
                    ModifyTerrain(hit);
                }
                else if (Input.GetMouseButton(1))
                {
                    PlaceBlock(hit);
                }
            }
        }
    }

    private void ModifyTerrain(RaycastHit hit)
    {
        PlayExplosion(hit.point);
        world.SetBlock(hit, BlockType.Air);
    }

    private void PlaceBlock(RaycastHit hit)
    {
        BlockType lookedAtBlockType = GetLookedAtBlockType(hit);
        BlockType blockToPlace = GetNextBlockType(lookedAtBlockType);

        Vector3Int targetBlockPos = new Vector3Int(
            Mathf.FloorToInt(hit.point.x - hit.normal.x * 0.5f),
            Mathf.FloorToInt(hit.point.y - hit.normal.y * 0.5f),
            Mathf.FloorToInt(hit.point.z - hit.normal.z * 0.5f)
        );
        Vector3Int placeBlockPos = targetBlockPos + Vector3Int.RoundToInt(hit.normal);

        BlockType existingBlock = world.GetBlockFromChunkCoordinates(
            hit.collider.GetComponent<ChunkRenderer>().ChunkData,
            placeBlockPos.x, placeBlockPos.y, placeBlockPos.z
        );

        if (existingBlock == BlockType.Air || existingBlock == BlockType.Nothing)
        {
            world.SetBlock(placeBlockPos, blockToPlace);
        }
    }

    private BlockType GetNextBlockType(BlockType currentBlock)
    {
        switch (currentBlock)
        {
            case BlockType.Grass_Dirt:
                return BlockType.Dirt;
            case BlockType.Dirt:
                return BlockType.Stone;
            case BlockType.Stone:
                return BlockType.TreeTrunk;
            case BlockType.TreeTrunk:
                return BlockType.Grass_Dirt;
            default:
                return BlockType.Grass_Dirt;
        }
    }

    private BlockType GetLookedAtBlockType(RaycastHit hit)
    {
        Vector3Int blockPos = new Vector3Int(
            Mathf.RoundToInt(hit.point.x - hit.normal.x / 2),
            Mathf.RoundToInt(hit.point.y - hit.normal.y / 2),
            Mathf.RoundToInt(hit.point.z - hit.normal.z / 2)
        );

        return world.GetBlockFromChunkCoordinates(
            hit.collider.GetComponent<ChunkRenderer>().ChunkData,
            blockPos.x, blockPos.y, blockPos.z
        );
    }

    private void ToggleInventory()
    {
        AudioManager.instance.PlaySFX("Inventory Toggle");
    }

    private void PlayExplosion(Vector3 position)
    {
        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, position, Quaternion.identity);
            ParticleSystem particles = explosion.GetComponent<ParticleSystem>();
            particles.Play();
            Destroy(explosion, particles.main.duration);
        }
    }

    public bool IsInRangeOfEnemy(Transform enemyTransform, float attackRange)
    {
        return Vector3.Distance(transform.position, enemyTransform.position) <= attackRange;
    }

    public bool IsDead() => isDead;

    public void Shoot()
    {
        if (projectilePrefab == null || firePoint == null)
        {
            Debug.LogWarning("[Character] Projectile prefab or firePoint not assigned!");
            return;
        }

        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        if (projectile.TryGetComponent(out Projectile projectileScript))
        {
            Vector3 shootDirection = firePoint.forward;
            projectileScript.speed = projectileSpeed;
            projectileScript.Initialize(shootDirection);
            Debug.Log("[Character] Projectile shot.");
        }
        else
        {
            Debug.LogError("[Character] Projectile prefab missing Projectile script!");
        }
    }

    private IEnumerator DamageEffect()
    {
        Vector3 originalPosition = mainCamera.transform.localPosition;
        float elapsed = 0f;
        float duration = 0.15f;
        float magnitude = 0.1f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            mainCamera.transform.localPosition = new Vector3(
                originalPosition.x + x,
                originalPosition.y + y,
                originalPosition.z
            );

            elapsed += Time.deltaTime;
            yield return null;
        }

        mainCamera.transform.localPosition = originalPosition;
    }

}
