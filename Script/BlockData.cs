using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum _BLOCK
{
    AIR = 0,
    STONE,
    GRASS,
    BEDROCK,
    DIRT,
    WOOD,
    LEAVES,
    PLANK,

}

[System.Serializable]
public class BlockData
{
    public string blockName;
    //투명한 부분이 들어간 블록인지 (주위에있는 블록을 그려야 하는지 판단해야 하기에)
    public bool transparent;
    //블록이 단단한지 (지나갈 수 있는지)
    public bool isSolid;
    //블록의 단단한 정도
    public Hardness hardness;
    //이 블록을 파괴했을때 무슨 아이템을 드랍할 것인가
    public int itemID;

    [Header("Texture Values")]
    public int backFaceTexture;
    public int frontFaceTexture;
    public int topFaceTexture;
    public int bottomFaceTexture;
    public int leftFaceTexture;
    public int rightFaceTexture;

    public int GetTextureID(int faceIndex)
    {
        switch (faceIndex)
        {
            case 0:
                return backFaceTexture;
            case 1:
                return frontFaceTexture;
            case 2:
                return topFaceTexture;
            case 3:
                return bottomFaceTexture;
            case 4:
                return leftFaceTexture;
            case 5:
                return rightFaceTexture;
            default:
                Debug.Log("Error in GetTextureID; invalid face index");
                return 0;
        }
    }
}
