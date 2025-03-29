using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
	[SerializeField] private Camera mainCamera;
	[SerializeField] private PlayerInput playerInput;
	[SerializeField] private PlayerMovement playerMovement;

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
				isWaiting = true;
				StopAllCoroutines();
				StartCoroutine(ResetWaiting());
			}
			animator.SetFloat("speed", playerInput.MovementInput.magnitude);
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
		Ray playerRay = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
		RaycastHit hit;

<<<<<<< Updated upstream
		if (Physics.Raycast(playerRay, out hit, interactionRayLength, groundMask))
		{
			Vector3Int clickedBlockPos = new Vector3Int(Mathf.RoundToInt(hit.point.x), Mathf.RoundToInt(hit.point.y), Mathf.RoundToInt(hit.point.z));
=======
            if (playerMovement.IsGrounded && playerInput.IsJumping && !isWaiting)
            {
                animator.SetTrigger("jump");
                //AudioManager.instance.PlayJumpSound();
                isWaiting = true;
                StopAllCoroutines();
                StartCoroutine(ResetWaiting());
            }
>>>>>>> Stashed changes

			// Check if the clicked block is the same as the last one
			if (clickedBlockPos == lastClickedBlockPos && Time.time - lastClickTime <= doubleClickThreshold)
			{
				// Double-clicked, destroy the block
				ModifyTerrain(hit);
			}
			else
			{
				// First click, record the time and block position
				lastClickedBlockPos = clickedBlockPos;
				lastClickTime = Time.time;
			}
		}
	}

<<<<<<< Updated upstream
	private void ModifyTerrain(RaycastHit hit)
	{
		world.SetBlock(hit, BlockType.Air);
	}
=======
            if (playerMovement.IsGrounded && playerInput.MovementInput.magnitude > 0)
            {
                //if (!AudioManager.instance.sfxSource.isPlaying)
                //    AudioManager.instance.PlayWalkSound();
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
        //AudioManager.instance.PlayButtonClick();
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
>>>>>>> Stashed changes
}
