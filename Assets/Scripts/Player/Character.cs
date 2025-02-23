using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
	[SerializeField] private Camera mainCamera;
	[SerializeField] private PlayerInput playerInput;
	[SerializeField] private PlayerMovement playerMovement;

    [SerializeField] private PauseSystem pauseMenu;
    public float interactionRayLength = 5;
    public LayerMask groundMask;
	public bool fly = false;
	public Animator animator;
	bool isWaiting = false;
	public World world;

	private float lastClickTime = 0f;
	private float doubleClickThreshold = 0.5f; // Time window for double-click in seconds
	private Vector3Int lastClickedBlockPos = Vector3Int.zero;

	private void Awake()
	{
		if (mainCamera == null)
			mainCamera = Camera.main;
		playerInput = GetComponent<PlayerInput>();
		playerMovement = GetComponent<PlayerMovement>();
		world = FindObjectOfType<World>();
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
        if (fly)
        {
            animator.SetFloat("speed", 0);
            animator.SetBool("isGrounded", false);
            animator.ResetTrigger("jump");
            playerMovement.Fly(playerInput.MovementInput, playerInput.IsJumping, playerInput.RunningPressed);
        }
        else
        {
            animator.SetBool("isGrounded", playerMovement.IsGrounded);

            if (playerMovement.IsGrounded && playerInput.IsJumping && isWaiting == false)
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
            playerMovement.Walk(playerInput.MovementInput, playerInput.RunningPressed);
        }
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
            // Get the block type at the clicked position
            BlockType lookedAtBlockType = GetLookedAtBlockType(hit);

            // Determine the block type to place based on what you're looking at
            BlockType blockToPlace;
            switch (lookedAtBlockType)
            {
                case BlockType.Grass_Dirt:
                    blockToPlace = BlockType.Dirt;
                    break;
                case BlockType.Dirt:
                    blockToPlace = BlockType.Stone;
                    break;
                case BlockType.Stone:
                    blockToPlace = BlockType.TreeTrunk;
                    break;
                case BlockType.TreeTrunk:
                    blockToPlace = BlockType.Grass_Dirt;
                    break;
                default:
                    blockToPlace = BlockType.Grass_Dirt;
                    break;
            }

            // Calculate the block position slightly away from the hit point
            Vector3Int placeBlockPos = new Vector3Int(
                Mathf.RoundToInt(hit.point.x - hit.normal.x / 2),
                Mathf.RoundToInt(hit.point.y - hit.normal.y / 2),
                Mathf.RoundToInt(hit.point.z - hit.normal.z / 2)
            );

                world.SetBlock(hit, blockToPlace);

            }
        }
}

    private void ToggleInventory()
    {
        AudioManager.instance.PlaySFX("Inventory Toggle");
    }

    private void ModifyTerrain(RaycastHit hit)
	{
		world.SetBlock(hit, BlockType.Air);
	}
    private BlockType GetLookedAtBlockType(RaycastHit hit)
    {
        Vector3Int blockPos = new Vector3Int(
            Mathf.RoundToInt(hit.point.x - hit.normal.x / 2),
            Mathf.RoundToInt(hit.point.y - hit.normal.y / 2),
            Mathf.RoundToInt(hit.point.z - hit.normal.z / 2)
        );

        // Get the block type at the clicked position
        return world.GetBlockFromChunkCoordinates(
            hit.collider.GetComponent<ChunkRenderer>().ChunkData,
            blockPos.x, blockPos.y, blockPos.z
        );
    }

}
