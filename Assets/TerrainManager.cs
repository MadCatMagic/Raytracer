using UnityEngine;

public class TerrainManager : MonoBehaviour
{
    public Vector3 maxChunks;
    public TerrainGenerator terrainGenerator;

    private Chunk[,,] chunkList;

    private void Start()
    {
        GenerateChunks();
    }

    private void OnDisable()
    {
        DeleteChunks();
    }

    public void GenerateChunks() {
        chunkList = new Chunk[(int)maxChunks.x, (int)maxChunks.y, (int)maxChunks.z];
        for (int x = 0; x < maxChunks.x; x++)
        {
            for (int y = 0; y < maxChunks.y; y++)
            {
                for (int z = 0; z < maxChunks.z; z++)
                {
                    Vector3 position = new Vector3(x * terrainGenerator.chunkSize.x, y * terrainGenerator.chunkSize.y, z * terrainGenerator.chunkSize.z);
                    GameObject empty = new GameObject("chunk");
                    empty.transform.position = position;
                    empty.transform.parent = transform;
                    Chunk chunk = empty.AddComponent<Chunk>();
                    chunk.Initialize(terrainGenerator, true);
                    chunkList[x, y, z] = chunk;
                }
            }
        }
    }

    public void DeleteChunks()
    {
        foreach (Chunk chunk in transform.GetComponentsInChildren<Chunk>())
        {
            DestroyImmediate(chunk.gameObject);
        }
        chunkList = null;
    }

    public ushort[] GenerateChunkData()
    {
        ushort[] array = new ushort[(int)(maxChunks.x * terrainGenerator.chunkSize.x * maxChunks.y * terrainGenerator.chunkSize.y * maxChunks.z * terrainGenerator.chunkSize.z)];
        for (int x = 0; x < maxChunks.x * terrainGenerator.chunkSize.x; x++)
        {
            for (int y = 0; y < maxChunks.y * terrainGenerator.chunkSize.y; y++)
            {
                for (int z = 0; z < maxChunks.z * terrainGenerator.chunkSize.z; z++)
                {
                    Chunk chunk = chunkList[Mathf.FloorToInt(x / terrainGenerator.chunkSize.x), Mathf.FloorToInt(y / terrainGenerator.chunkSize.y), Mathf.FloorToInt(z / terrainGenerator.chunkSize.z)];
                    int i = (int)(x + y * maxChunks.y * terrainGenerator.chunkSize.y + z * maxChunks.z * terrainGenerator.chunkSize.z * maxChunks.y * terrainGenerator.chunkSize.y);
                    array[i] = chunk.BlockValue(new Vector3(x % terrainGenerator.chunkSize.x, y % terrainGenerator.chunkSize.y, z % terrainGenerator.chunkSize.z));
                }
            }
        }
        return array;
    }
}
