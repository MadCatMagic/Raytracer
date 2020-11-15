using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public Vector3 chunkSize;

    public Block[,,] GenerateChunk(Vector3 chunkPosition)
    {
        Block[,,] blocks = new Block[(int)chunkSize.x, (int)chunkSize.y, (int)chunkSize.z];
        for (int x = 0; x < chunkSize.x; x++)
        {
            for (int y = 0; y < chunkSize.y; y++)
            {
                for (int z = 0; z < chunkSize.z; z++)
                {
                    Vector3 position = chunkPosition + new Vector3(x, y, z);
                    blocks[x, y, z] = GenerateBlock(position);
                }
            }
        }
        return blocks;
    }

    private Block GenerateBlock(Vector3 worldPosition)
    {
        Block block = new Block(worldPosition, Random.value < 0.95f ? Block.BlockType.Air : Block.BlockType.Dirt);
        return block;
    }
}
