﻿using System;
using System.Collections.Generic;
using UnityEngine;

public static class Chunk
{

	public static void LoopThroughTheBlocks(ChunkData chunkData, Action<int, int, int> actionToPerform)
	{
		for (int index = 0; index < chunkData.blocks.Length; index++)
		{
			var position = GetPostitionFromIndex(chunkData, index);
			actionToPerform(position.x, position.y, position.z);
		}
	}

	private static Vector3Int GetPostitionFromIndex(ChunkData chunkData, int index)
	{
		int x = index % chunkData.chunkSize;
		int y = (index / chunkData.chunkSize) % chunkData.chunkHeight;
		int z = index / (chunkData.chunkSize * chunkData.chunkHeight);
		return new Vector3Int(x, y, z);
	}

	//in chunk coordinate system
	private static bool InRange(ChunkData chunkData, int axisCoordinate)
	{
		if (axisCoordinate < 0 || axisCoordinate >= chunkData.chunkSize)
			return false;

		return true;
	}

	//in chunk coordinate system
	private static bool InRangeHeight(ChunkData chunkData, int ycoordinate)
	{
		if (ycoordinate < 0 || ycoordinate >= chunkData.chunkHeight)
			return false;

		return true;
	}

	public static BlockType GetBlockFromChunkCoordinates(ChunkData chunkData, Vector3Int chunkCoordinates)
	{
		return GetBlockFromChunkCoordinates(chunkData, chunkCoordinates.x, chunkCoordinates.y, chunkCoordinates.z);
	}

	public static BlockType GetBlockFromChunkCoordinates(ChunkData chunkData, int x, int y, int z)
	{
		if (InRange(chunkData, x) && InRangeHeight(chunkData, y) && InRange(chunkData, z))
		{
			int index = GetIndexFromPosition(chunkData, x, y, z);
			return chunkData.blocks[index];
		}

		return chunkData.worldReference.GetBlockFromChunkCoordinates(chunkData, chunkData.worldPosition.x + x, chunkData.worldPosition.y + y, chunkData.worldPosition.z + z);
	}

	public static void SetBlock(ChunkData chunkData, Vector3Int localPosition, BlockType block)
	{
		if (InRange(chunkData, localPosition.x) && InRangeHeight(chunkData, localPosition.y) && InRange(chunkData, localPosition.z))
		{
			int index = GetIndexFromPosition(chunkData, localPosition.x, localPosition.y, localPosition.z);
			chunkData.blocks[index] = block;
		}
		else
		{
			WorldDataHelper.SetBlock(chunkData.worldReference, localPosition, block);
		}
	}

	public static int GetIndexFromPosition(ChunkData chunkData, int x, int y, int z)
	{
		return x + chunkData.chunkSize * y + chunkData.chunkSize * chunkData.chunkHeight * z;
	}

	public static Vector3Int GetBlockInChunkCoordinates(ChunkData chunkData, Vector3Int pos)
	{
		return new Vector3Int
		{
			x = pos.x - chunkData.worldPosition.x,
			y = pos.y - chunkData.worldPosition.y,
			z = pos.z - chunkData.worldPosition.z
		};
	}

    public static MeshData GetChunkMeshData(ChunkData chunkData)
    {
        MeshData meshData = new MeshData(true);

        LoopThroughTheBlocks(chunkData, (x, y, z) => {
            var blockType = chunkData.blocks[GetIndexFromPosition(chunkData, x, y, z)];
            // Skip water blocks or other non-navigable blocks for NavMesh
            if (blockType != BlockType.Water && blockType != BlockType.Nothing)
            {
                meshData = BlockHelper.GetMeshData(chunkData, x, y, z, meshData, blockType);
            }
        });

        // Validate the mesh data
        ValidateMeshData(meshData);

        return meshData;
    }

    private static void ValidateMeshData(MeshData meshData)
    {
        // Remove duplicate vertices
        Dictionary<Vector3, int> uniqueVertices = new Dictionary<Vector3, int>();
        List<Vector3> newVertices = new List<Vector3>();
        List<int> vertexRemapping = new List<int>();

        for (int i = 0; i < meshData.colliderVertices.Count; i++)
        {
            Vector3 vertex = meshData.colliderVertices[i];

            // Check for NaN values
            if (float.IsNaN(vertex.x) || float.IsNaN(vertex.y) || float.IsNaN(vertex.z))
            {
                // Replace with a valid vertex to avoid errors
                vertex = Vector3.zero;
            }

            if (!uniqueVertices.TryGetValue(vertex, out int index))
            {
                // New unique vertex
                uniqueVertices[vertex] = newVertices.Count;
                vertexRemapping.Add(newVertices.Count);
                newVertices.Add(vertex);
            }
            else
            {
                // Duplicate vertex
                vertexRemapping.Add(index);
            }
        }

        // Update triangles to use new vertex indices
        List<int> newTriangles = new List<int>();
        for (int i = 0; i < meshData.colliderTriangles.Count; i++)
        {
            newTriangles.Add(vertexRemapping[meshData.colliderTriangles[i]]);
        }

        meshData.colliderVertices = newVertices;
        meshData.colliderTriangles = newTriangles;
    }

    internal static Vector3Int ChunkPositionFromBlockCoords(World world, int x, int y, int z)
	{
		Vector3Int pos = new Vector3Int
		{
			x = Mathf.FloorToInt(x / (float)world.chunkSize) * world.chunkSize,
			y = Mathf.FloorToInt(y / (float)world.chunkHeight) * world.chunkHeight,
			z = Mathf.FloorToInt(z / (float)world.chunkSize) * world.chunkSize
		};
		return pos;
	}

	internal static List<ChunkData> GetEdgeNeighbourChunk(ChunkData chunkData, Vector3Int worldPosition)
	{
		Vector3Int chunkPosition = GetBlockInChunkCoordinates(chunkData, worldPosition);
		List<ChunkData> neighboursToUpdate = new List<ChunkData>();
		if (chunkPosition.x == 0)
		{
			neighboursToUpdate.Add(WorldDataHelper.GetChunkData(chunkData.worldReference, worldPosition - Vector3Int.right));
		}
		if (chunkPosition.x == chunkData.chunkSize - 1)
		{
			neighboursToUpdate.Add(WorldDataHelper.GetChunkData(chunkData.worldReference, worldPosition + Vector3Int.right));
		}
		if (chunkPosition.y == 0)
		{
			neighboursToUpdate.Add(WorldDataHelper.GetChunkData(chunkData.worldReference, worldPosition - Vector3Int.up));
		}
		if (chunkPosition.y == chunkData.chunkHeight - 1)
		{
			neighboursToUpdate.Add(WorldDataHelper.GetChunkData(chunkData.worldReference, worldPosition + Vector3Int.up));
		}
		if (chunkPosition.z == 0)
		{
			neighboursToUpdate.Add(WorldDataHelper.GetChunkData(chunkData.worldReference, worldPosition - Vector3Int.forward));
		}
		if (chunkPosition.z == chunkData.chunkSize - 1)
		{
			neighboursToUpdate.Add(WorldDataHelper.GetChunkData(chunkData.worldReference, worldPosition + Vector3Int.forward));
		}
		return neighboursToUpdate;
	}

	internal static bool IsOnEdge(ChunkData chunkData, Vector3Int worldPosition)
	{
		Vector3Int chunkPosition = GetBlockInChunkCoordinates(chunkData, worldPosition);
		if (
			chunkPosition.x == 0 || chunkPosition.x == chunkData.chunkSize - 1 ||
			chunkPosition.y == 0 || chunkPosition.y == chunkData.chunkHeight - 1 ||
			chunkPosition.z == 0 || chunkPosition.z == chunkData.chunkSize - 1
			)
			return true;
		return false;
	}
}