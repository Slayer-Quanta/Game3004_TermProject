using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PauseSystem pauseMenu;
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private Transform headTransform;

    [Header("Interaction Settings")]
    public float interactionRayLength = 5;
    public LayerMask groundMask;
    public bool fly = false;

    [Header("Animation")]
    public Animator animator;
    private bool isWaiting = false;
    public World world;

    [Header("Block Interaction")]
    [SerializeField] private float doubleClickThreshold = 0.5f;
    private float lastClickTime = 0f;
    private Vector3Int lastClickedBlockPos = Vector3Int.zero;

    [Header("Head Movement")]
    [SerializeField] private float headTiltAmount = 5f;
    [SerializeField] private float headTiltSpeed = 3f;
    private float currentHeadTilt = 0f;

    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
        playerInput = GetComponent<PlayerInput>();
        playerMovement = GetComponent<PlayerMovement>();
        world = FindObjectOfType<World>();

        // If no specific head transform is assigned, use the camera transform
        if (headTransform == null)
            headTransform = mainCamera.transform;
    }

    private void Start()
    {
        playerInput.OnMouseClick += HandleMouseClick;
        playerInput.OnFly += HandleFlyClick;

        pauseMenu = FindObjectOfType<PauseSystem>();
        playerInput.OnPause += pauseMenu.TogglePause;
        playerInput.OnInventoryToggle += ToggleInventory;
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
        UpdateHeadTilt();

        if (fly)
        {
            HandleFlyMode();
        }
        else
        {
            HandleWalkMode();
        }
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

        // Fly using the joystick movement and input for ascend/descend
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

        // Handle Gravity and Walking
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
        AudioManager.instance.PlayButtonClick();
        Ray playerRay = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(playerRay, out hit, interactionRayLength, groundMask))
        {
            Vector3Int clickedBlockPos = new Vector3Int(
                Mathf.RoundToInt(hit.point.x),
                Mathf.RoundToInt(hit.point.y),
                Mathf.RoundToInt(hit.point.z)
            );

            if (Input.GetMouseButton(0)) // Left-click: Destroy block
            {
                if (clickedBlockPos == lastClickedBlockPos && Time.time - lastClickTime <= doubleClickThreshold)
                {
                    ModifyTerrain(hit);
                }
                else
                {
                    lastClickedBlockPos = clickedBlockPos;
                    lastClickTime = Time.time;
                }
            }
            else if (Input.GetMouseButton(1)) // Right-click: Place block
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
}