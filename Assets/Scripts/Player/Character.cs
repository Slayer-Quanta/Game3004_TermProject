using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private float holdTimeToDestroy = 3.0f; // Time required to hold for destroying blocks
    private float blockInteractionTimer = 0f;
    private bool isHoldingInteraction = false;
    private RaycastHit currentHit;
    private bool validHit = false;

    [Header("Head Movement")]
    [SerializeField] private float headTiltAmount = 5f;
    [SerializeField] private float headTiltSpeed = 3f;
    private float currentHeadTilt = 0f;

    // Event for when player health changes
    public delegate void HealthChangedHandler(float currentHealth, float maxHealth);
    public event HealthChangedHandler OnHealthChanged;

    // Event for when player dies
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

        // Initialize health
        currentHealth = maxHealth;
        lastDamageTime = -healthRegenDelay;
    }

    private void Start()
    {
        playerInput.OnMouseClick += HandleMouseClick;
        playerInput.OnFly += HandleFlyClick;

        pauseMenu = FindObjectOfType<PauseSystem>();
        playerInput.OnPause += pauseMenu.TogglePause;
        playerInput.OnInventoryToggle += ToggleInventory;

        // Initialize health UI
        UpdateHealthBar();
    }

    private void OnDestroy()
    {
        playerInput.OnMouseClick -= HandleMouseClick;
        playerInput.OnFly -= HandleFlyClick;
        playerInput.OnPause -= PauseSystem.self.TogglePause;
        playerInput.OnInventoryToggle -= ToggleInventory;
    }

    private void HandleFlyClick()
    {
        fly = !fly;
    }

    void Update()
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

        // Handle touch-based block interaction on mobile
        if (Application.isMobilePlatform)
        {
            HandleTouchBlockInteraction();
        }
    }

    private void HandleTouchBlockInteraction()
    {
        // Check for touches on the left side of the screen for block interaction
        bool foundBlockInteractionTouch = false;

        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);

            // Only use left side of screen for block interaction
            if (touch.position.x < Screen.width * 0.5f)
            {
                foundBlockInteractionTouch = true;

                // Cast ray on first touch
                if (touch.phase == TouchPhase.Began)
                {
                    Ray playerRay = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
                    validHit = Physics.Raycast(playerRay, out currentHit, interactionRayLength, groundMask);

                    if (validHit)
                    {
                        isHoldingInteraction = true;
                        blockInteractionTimer = 0f;
                        // Play click sound
                        AudioManager.instance.PlayButtonClick();
                    }
                }
                // Hold processing
                else if ((touch.phase == TouchPhase.Stationary || touch.phase == TouchPhase.Moved) && isHoldingInteraction && validHit)
                {
                    blockInteractionTimer += Time.deltaTime;

                    // Destroy block after holding for the specified time
                    if (blockInteractionTimer >= holdTimeToDestroy)
                    {
                        ModifyTerrain(currentHit);
                        isHoldingInteraction = false;
                        blockInteractionTimer = 0f;
                    }
                }
                // End of touch - place block if it was a quick tap
                else if (touch.phase == TouchPhase.Ended && isHoldingInteraction && validHit)
                {
                    // If held for less than the destroy time, place a block
                    if (blockInteractionTimer < holdTimeToDestroy)
                    {
                        PlaceBlock(currentHit);
                    }

                    isHoldingInteraction = false;
                    blockInteractionTimer = 0f;
                }

                break; // Process only one touch for block interaction
            }
        }

        // Reset if no interaction touch found
        if (!foundBlockInteractionTouch)
        {
            isHoldingInteraction = false;
            blockInteractionTimer = 0f;
        }
    }

    private void HandleHealthRegeneration()
    {
        // Check if enough time has passed since last damage
        if (Time.time - lastDamageTime > healthRegenDelay && currentHealth < maxHealth)
        {
            Heal(healthRegenRate * Time.deltaTime);
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead)
            return;

        currentHealth -= damage;
        lastDamageTime = Time.time;

        // Play hit animation/sound
        //AudioManager.instance.PlaySFX("Player_Hit");

        //// Apply visual feedback (camera shake, etc)
        StartCoroutine(DamageEffect());

        UpdateHealthBar();

        // Invoke the health changed event
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
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

        // Invoke the health changed event
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

        // Disable movement and interaction
        playerMovement.enabled = false;
        playerInput.enabled = false;

        // Play death animation
        //animator.SetTrigger("die");

        //// Play death sound
        //AudioManager.instance.PlaySFX("Player_Death");

        // Invoke the death event
        OnPlayerDeath?.Invoke();

    }

    IEnumerator DamageEffect()
    {
        // Simple camera shake
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

    private void UpdateHeadTilt()
    {
        // Apply head tilt based on movement direction and speed
        if (playerMovement.IsGrounded && playerInput.MovementInput.magnitude > 0.1f)
        {
            // Calculate target tilt based on horizontal movement
            float targetTilt = playerInput.MovementInput.x * headTiltAmount;

            // Smoothly interpolate to target tilt
            currentHeadTilt = Mathf.Lerp(currentHeadTilt, targetTilt, Time.deltaTime * headTiltSpeed);
        }
        else
        {
            // Return to neutral when not moving
            currentHeadTilt = Mathf.Lerp(currentHeadTilt, 0f, Time.deltaTime * headTiltSpeed);
        }

        // Apply the tilt rotation around the forward axis (z-axis roll)
        if (headTransform != null)
        {
            Vector3 currentRotation = headTransform.localEulerAngles;
            headTransform.localEulerAngles = new Vector3(currentRotation.x, currentRotation.y, currentHeadTilt);
        }
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

    IEnumerator ResetWaiting()
    {
        yield return new WaitForSeconds(0.1f);
        animator.ResetTrigger("jump");
        isWaiting = false;
    }

    private void HandleMouseClick()
    {
        // This is for non-mobile platforms only
        if (!Application.isMobilePlatform)
        {
            AudioManager.instance.PlayButtonClick();
            Ray playerRay = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(playerRay, out hit, interactionRayLength, groundMask))
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
            case BlockType.Grass_Dirt: return BlockType.Dirt;
            case BlockType.Dirt: return BlockType.Stone;
            case BlockType.Stone: return BlockType.TreeTrunk;
            case BlockType.TreeTrunk: return BlockType.Grass_Dirt;
            default: return BlockType.Grass_Dirt;
        }
    }

    private void ModifyTerrain(RaycastHit hit)
    {
        PlayExplosion(hit.point);
        world.SetBlock(hit, BlockType.Air);
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

    private void ToggleInventory()
    {
        AudioManager.instance.PlaySFX("Inventory Toggle");
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

    public bool IsInRangeOfEnemy(Transform enemyTransform, float attackRange)
    {
        return Vector3.Distance(transform.position, enemyTransform.position) <= attackRange;
    }
}