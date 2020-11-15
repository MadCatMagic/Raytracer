using UnityEngine;

public class Block
{
    public BlockType blockType;
    public Vector3 position;

    public Block(Vector3 pos, BlockType type)
    {
        blockType = type;
        position = pos;
    }

    public ushort RenderValue()
    {
        ushort output = 0;
        output += (ushort)((int)blockType * 1000);
        return output;
    }

    public enum BlockType
    {
        Air,
        Stone,
        Grass,
        Dirt
    }
}
