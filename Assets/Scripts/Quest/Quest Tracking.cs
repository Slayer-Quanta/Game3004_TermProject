using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This file contains all player activity tracking for quests
/// and Character class extensions to integrate quest tracking with player actions
/// </summary>

// Player Activity Tracker - Tracks all player actions for quest objectives
public class QuestActivityTracker : MonoBehaviour
{
    private Character character;
    private PlayerMovement movement;
    private PlayerInput input;

    // Tracking variables
    private Vector3 lastPosition;
    private int stepsTaken = 0;
    private int blocksPlaced = 0;
    private int blocksBroken = 0;
    private int jumpsPerformed = 0;
    private int enemiesDefeated = 0;
    private HashSet<Vector3Int> visitedChunks = new HashSet<Vector3Int>();

    // Constants
    private const float STEP_DISTANCE = 2.0f; // Distance that counts as a step

    private void Start()
    {
        // Get references
        character = GetComponent<Character>();
        movement = GetComponent<PlayerMovement>();
        input = GetComponent<PlayerInput>();

        // Record initial position
        lastPosition = transform.position;

        // Hook up event listeners
        if (input != null)
        {
            input.OnMouseClick += HandleMouseClick;
            input.OnFly += HandleFlyToggle;
        }

        // Subscribe to player death event
        if (character != null)
        {
            character.OnPlayerDeath += HandlePlayerDeath;
        }

        // Record current chunk position
        World world = FindObjectOfType<World>();
        if (world != null)
        {
            RecordCurrentChunk(world);
        }

        // Patch the Character methods with our quest-tracking versions
        PatchCharacterMethods();
    }

    private void Update()
    {
        TrackMovement();
        TrackJumping();
        TrackChunks();
    }

    private void OnDestroy()
    {
        // Clean up event listeners
        if (input != null)
        {
            input.OnMouseClick -= HandleMouseClick;
            input.OnFly -= HandleFlyToggle;
        }

        if (character != null)
        {
            character.OnPlayerDeath -= HandlePlayerDeath;
        }
    }

    private void TrackMovement()
    {
        // Calculate distance traveled since last frame
        float distanceMoved = Vector3.Distance(transform.position, lastPosition);

        // Check if we've moved enough to count as a step
        if (distanceMoved >= STEP_DISTANCE)
        {
            stepsTaken++;
            lastPosition = transform.position;

            // Update movement quest
            QuestManager.Instance.UpdateObjective("TUT_MOVE", "move_steps", 1);
        }
    }

    private void TrackJumping()
    {
        // Check for jumps
        if (movement != null && input != null && movement.IsGrounded && input.IsJumping)
        {
            jumpsPerformed++;

            // Update jump quest
            QuestManager.Instance.UpdateObjective("TUT_MOVE", "jump_times", 1);
        }
    }

    private void HandleMouseClick()
    {
        // This will be called when mouse click events happen
        // The actual block placement/breaking is handled by our patched Character methods
    }

    private void HandleFlyToggle()
    {
        // Could track fly toggle for a flying tutorial quest
    }

    private void HandlePlayerDeath()
    {
        // Handle any death-related quest updates
    }

    private void TrackChunks()
    {
        // Record new chunks the player visits
        World world = FindObjectOfType<World>();
        if (world != null)
        {
            RecordCurrentChunk(world);
        }
    }

    private void RecordCurrentChunk(World world)
    {
        Vector3Int currentChunk = WorldDataHelper.ChunkPositionFromBlockCoords(world, Vector3Int.RoundToInt(transform.position));

        if (!visitedChunks.Contains(currentChunk))
        {
            visitedChunks.Add(currentChunk);

            // Update exploration quest
            QuestManager.Instance.UpdateObjective("MAIN_EXPLORE", "visit_chunks", 1);
        }
    }

    // Methods to be called from patched Character methods

    public void RecordBlockBroken()
    {
        blocksBroken++;
        QuestManager.Instance.UpdateObjective("TUT_BUILD", "break_blocks", 1);
    }

    public void RecordBlockPlaced()
    {
        blocksPlaced++;
        QuestManager.Instance.UpdateObjective("TUT_BUILD", "place_blocks", 1);
    }

    public void RecordEnemyDefeated()
    {
        enemiesDefeated++;
        QuestManager.Instance.UpdateObjective("SIDE_COMBAT", "defeat_enemies", 1);
    }

    // Method patching - replace Character methods with our quest-tracking versions
    private void PatchCharacterMethods()
    {
        // We can't actually patch methods at runtime in C#
        // This is just a conceptual placeholder - in reality, you would
        // modify the Character class directly to call our tracking methods

        // In your HandleMouseClick method in Character.cs, you would add:
        /*
            if (Physics.Raycast(playerRay, out hit, interactionRayLength, groundMask))
            {
                if (Input.GetMouseButton(0))
                {
                    // Call our tracking version instead of direct ModifyTerrain
                    ModifyTerrainWithQuest(hit);
                }
                else if (Input.GetMouseButton(1))
                {
                    // Call our tracking version instead of direct PlaceBlock
                    PlaceBlockWithQuest(hit);
                }
            }
        */
    }
}

// Enemy Quest Integration - Attach to Enemy prefab
public class EnemyQuestIntegration : MonoBehaviour
{
    private bool isDead = false;

    // Call this method when the enemy dies
    public void Die()
    {
        if (isDead) return;
        isDead = true;

        // Find player and record the enemy defeat
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            QuestActivityTracker tracker = player.GetComponent<QuestActivityTracker>();
            if (tracker != null)
            {
                tracker.RecordEnemyDefeated();
            }
        }
    }
}

// Utility class - modify Character class to use these methods
public static class CharacterQuestExtensions
{
    // Use this instead of Character.ModifyTerrain
    public static void ModifyTerrainWithQuest(this Character character, RaycastHit hit)
    {
        // Call the original method functionality
        character.PlayExplosion(hit.point);
        character.world.SetBlock(hit, BlockType.Air);

        // Track the block break for quests
        QuestActivityTracker tracker = character.GetComponent<QuestActivityTracker>();
        if (tracker != null)
        {
            tracker.RecordBlockBroken();
        }
    }

    // Use this instead of Character.PlaceBlock
    public static void PlaceBlockWithQuest(this Character character, RaycastHit hit)
    {
        // Get the original block type logic
        BlockType lookedAtBlockType = character.GetLookedAtBlockType(hit);
        BlockType blockToPlace = character.GetNextBlockType(lookedAtBlockType);

        Vector3Int targetBlockPos = new Vector3Int(
            Mathf.FloorToInt(hit.point.x - hit.normal.x * 0.5f),
            Mathf.FloorToInt(hit.point.y - hit.normal.y * 0.5f),
            Mathf.FloorToInt(hit.point.z - hit.normal.z * 0.5f)
        );
        Vector3Int placeBlockPos = targetBlockPos + Vector3Int.RoundToInt(hit.normal);

        BlockType existingBlock = character.world.GetBlockFromChunkCoordinates(
            hit.collider.GetComponent<ChunkRenderer>().ChunkData,
            placeBlockPos.x, placeBlockPos.y, placeBlockPos.z
        );

        if (existingBlock == BlockType.Air || existingBlock == BlockType.Nothing)
        {
            character.world.SetBlock(placeBlockPos, blockToPlace);

            // Track the block placement for quests
            QuestActivityTracker tracker = character.GetComponent<QuestActivityTracker>();
            if (tracker != null)
            {
                tracker.RecordBlockPlaced();
            }
        }
    }
}
