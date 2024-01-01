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
    //������ �κ��� �� ������� (�������ִ� ����� �׷��� �ϴ��� �Ǵ��ؾ� �ϱ⿡)
    public bool transparent;
    //����� �ܴ����� (������ �� �ִ���)
    public bool isSolid;
    //����� �ܴ��� ����
    public Hardness hardness;
    //�� ����� �ı������� ���� �������� ����� ���ΰ�
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
