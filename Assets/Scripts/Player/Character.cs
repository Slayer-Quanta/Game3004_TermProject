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

	[SerializeField] private GameObject explosionPrefab; // Explosion effect prefab

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

		if (Physics.Raycast(playerRay, out hit, interactionRayLength, groundMask))
		{
			Vector3Int clickedBlockPos = new Vector3Int(
				Mathf.RoundToInt(hit.point.x),
				Mathf.RoundToInt(hit.point.y),
				Mathf.RoundToInt(hit.point.z)
			);

			// Check if the clicked block is the same as the last one
			if (clickedBlockPos == lastClickedBlockPos && Time.time - lastClickTime <= doubleClickThreshold)
			{
				// Double-clicked, destroy the block and play explosion
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

	private void ModifyTerrain(RaycastHit hit)
	{
		// Play explosion effect at the block's position
		PlayExplosion(hit.point);

		// Destroy the block
		world.SetBlock(hit, BlockType.Air);
	}

	private void PlayExplosion(Vector3 position)
	{
		if (explosionPrefab != null)
		{
			// Instantiate explosion effect at the block’s position
			GameObject explosion = Instantiate(explosionPrefab, position, Quaternion.identity);

			// Get the Particle System from the explosion prefab
			ParticleSystem particles = explosion.GetComponent<ParticleSystem>();

			// Play the particle system
			particles.Play();

			// Destroy explosion effect after it finishes playing
			Destroy(explosion, particles.main.duration);
		}
	}
}
