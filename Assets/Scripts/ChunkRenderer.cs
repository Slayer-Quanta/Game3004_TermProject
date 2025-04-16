using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class ChunkRenderer : MonoBehaviour
{
    MeshFilter meshFilter;
    MeshCollider meshCollider;
    Mesh mesh;
    public bool showGizmo = false;

    public ChunkData ChunkData { get; private set; }

    public bool ModifiedByThePlayer
    {
        get
        {
            return ChunkData.modifiedByThePlayer;
        }
        set
        {
            ChunkData.modifiedByThePlayer = value;
        }
    }

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();
        mesh = meshFilter.mesh;
    }

    public void InitializeChunk(ChunkData data)
    {
        this.ChunkData = data;
    }

    private void RenderMesh(MeshData meshData)
    {
        mesh.Clear();

        mesh.subMeshCount = 2;
        mesh.vertices = meshData.vertices.Concat(meshData.waterMesh.vertices).ToArray();

        mesh.SetTriangles(meshData.triangles.ToArray(), 0);
        mesh.SetTriangles(meshData.waterMesh.triangles.Select(val => val + meshData.vertices.Count).ToArray(), 1);

        mesh.uv = meshData.uv.Concat(meshData.waterMesh.uv).ToArray();
        mesh.RecalculateNormals();

        meshCollider.sharedMesh = null;
        Mesh collisionMesh = new Mesh();
        collisionMesh.vertices = meshData.colliderVertices.ToArray();
        collisionMesh.triangles = meshData.colliderTriangles.ToArray();
        collisionMesh.RecalculateNormals();

        meshCollider.sharedMesh = collisionMesh;
    }

    public void UpdateChunk()
    {
        RenderMesh(Chunk.GetChunkMeshData(ChunkData));
    }

    public void UpdateChunk(MeshData data)
    {
        RenderMesh(data);
    }

    public void BuildNavMeshForChunk()
    {
        // Create a simplified mesh for navigation
        Mesh navMesh = new Mesh();

        // Get only the solid, walkable blocks
        List<Vector3> navVertices = new List<Vector3>();
        List<int> navTriangles = new List<int>();

        // For each block that's solid and has a block above that's air
        Chunk.LoopThroughTheBlocks(ChunkData, (x, y, z) => {
            BlockType blockType = ChunkData.blocks[Chunk.GetIndexFromPosition(ChunkData, x, y, z)];
            BlockType blockAbove = Chunk.GetBlockFromChunkCoordinates(ChunkData, x, y + 1, z);

            if (BlockHelper.IsSolid(blockType) && blockAbove == BlockType.Nothing)
            {
                // Add a simple quad for the top face
                int baseIndex = navVertices.Count;

                // Positions need to be in world space
                Vector3 worldPos = new Vector3(
                    ChunkData.worldPosition.x + x,
                    ChunkData.worldPosition.y + y + 1, // Top of the block
                    ChunkData.worldPosition.z + z
                );

                // Add the four corners of the top face
                navVertices.Add(worldPos + new Vector3(0, 0, 0));
                navVertices.Add(worldPos + new Vector3(1, 0, 0));
                navVertices.Add(worldPos + new Vector3(1, 0, 1));
                navVertices.Add(worldPos + new Vector3(0, 0, 1));

                // Add two triangles for the quad
                navTriangles.Add(baseIndex);
                navTriangles.Add(baseIndex + 1);
                navTriangles.Add(baseIndex + 2);

                navTriangles.Add(baseIndex);
                navTriangles.Add(baseIndex + 2);
                navTriangles.Add(baseIndex + 3);
            }
        });

        navMesh.vertices = navVertices.ToArray();
        navMesh.triangles = navTriangles.ToArray();
        navMesh.RecalculateNormals();

        // Create a new GameObject for the NavMesh
        GameObject navMeshObject = new GameObject("ChunkNavMesh");
        navMeshObject.transform.parent = this.transform;
        navMeshObject.transform.localPosition = Vector3.zero;
        navMeshObject.AddComponent<MeshFilter>().mesh = navMesh;
        navMeshObject.AddComponent<MeshRenderer>().enabled = false; // Make it invisible

        // You can now build your NavMesh using this object
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (showGizmo)
        {
            if (Application.isPlaying && ChunkData != null)
            {
                if (Selection.activeObject == gameObject)
                    Gizmos.color = new Color(0, 1, 0, 0.4f);
                else
                    Gizmos.color = new Color(1, 0, 1, 0.4f);

                Gizmos.DrawCube(transform.position + new Vector3(ChunkData.chunkSize / 2f, ChunkData.chunkHeight / 2f, ChunkData.chunkSize / 2f), new Vector3(ChunkData.chunkSize, ChunkData.chunkHeight, ChunkData.chunkSize));
            }
        }
    }
#endif
}