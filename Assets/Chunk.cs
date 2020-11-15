using UnityEngine;

public class Chunk : MonoBehaviour
{
    private Block[,,] blocks;
    private TerrainGenerator terrainGenerator;
    private bool drawGizmos;
    // initialize replaces chunk to let it take parameters
    public void Initialize(TerrainGenerator t, bool drawGizmos_)
    {
        terrainGenerator = t;
        blocks = terrainGenerator.GenerateChunk(transform.position);
        drawGizmos = drawGizmos_;
    }

    public ushort BlockValue(Vector3 block)
    {
        return blocks[(int)block.x, (int)block.y, (int)block.z].RenderValue();
    }

    private void OnDrawGizmos()
    {
        if (Application.isEditor && drawGizmos)
        {
            Gizmos.color = Color.red;
            foreach (Block block in blocks)
            {
                if (block.blockType == Block.BlockType.Dirt)
                {
                    Gizmos.DrawCube(block.position + Vector3.one / 2f, Vector3.one);
                }
            }
        }
    }
}
