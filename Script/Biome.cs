using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR;

[CreateAssetMenu(fileName = "BiomeAttributes", menuName = "Minecraft/Biome Attribute")]
public class Biome : ScriptableObject
{
    //���̴� 2d noise
    //���� ����� 3d noise
    //tree �� 2d
    //cave �� 3d
    [Header("���� �Ѹ�(����)�� ��������� ä�����")]
    public _BLOCK surfaceType;
    [Header("���� �Ѹ�(����)�� ��ĭ �����Ұ���")]
    public int surfaceDepth;
    [Header("ûũ�� ä�� �⺻���� ���")]
    public _BLOCK nomalType;
    //���̸� ���ϴ� ������ 2���� ������ noise�� �̿��� ������ ǥ���� �Ұ�����
    [Header("�⺻���� ���� (�� ���̸� �������� noise + - ������� ��)")]
    public int nomalHeight;
    [Header("�⺻���� ���� nomal�� ���� �� �̸�ŭ ���ؼ� ���̰� ������")]
    public int surfaceHeight;
    [Header("Height Noise")]
    public float offsetHeight;
    //[MinValue(0.1f)]
    public float scaleHeight = 0.5f;

    [Header("������ �ִ� �������� (���� �̱���)")]
    public bool cave;

    [Header("������ ���� ������ (���� �������� �켱�� {��ĥ��� �����ִ°� ������})")]
    public TreePlacement[] treePlacements;

    [Header("���ӿ� Ȯ�������� �ִ� ��ϵ� (���� ���� ���� �켱�� {��ĥ��� �����ִ°� ������} )")]
    public Underground[] undergrounds;

    public int Height(int x, int z)
    {
        return (int)(Noise.Get2DPerlin(new Vector2Int(x, z), offsetHeight, scaleHeight) * nomalHeight + surfaceHeight);
    }

    //�⺻���� ������ �����
    public void CreateBaseMap(Chunk chunk, ref byte[,,] map)
    {
        for (int x = 0; x < BlockInfo.ChunkWidth; x++)
        {
            for (int z = 0; z < BlockInfo.ChunkWidth; z++)
            {
                int yHeight = Height(chunk.Position.x + x, chunk.Position.z + z);
                for (int y = 0; y < BlockInfo.ChunkHeight; y++)
                {
                    //air �� ��� �־����� �ʴ��� 0���� ���־ ������
                    if (y > yHeight)
                        break;
                    if (y < 1)
                        map[x, y, z] = (byte)_BLOCK.BEDROCK; //�������� �ٲ����
                    else if (y >= yHeight - surfaceDepth)
                        map[x, y, z] = (byte)surfaceType;
                    else
                        map[x, y, z] = (byte)nomalType;
                }
            }
        }
    }

    //���� �ɱ�
    public void CreateTreeMap(Chunk chunk)
    {
        for (int x = 0; x < BlockInfo.ChunkWidth; x++)
        {
            for (int z = 0; z < BlockInfo.ChunkWidth; z++)
            {
                int yHeight = Height(chunk.Position.x + x, chunk.Position.z + z);
                for(int i = treePlacements.Length - 1; i >= 0 ; i--)
                {
                    if (treePlacements[i].MakeTree(x + chunk.Position.x, z + chunk.Position.z))
                    {
                        chunk.world.EditBlock(treePlacements[i].CreateTree(new Vector3Int(chunk.Position.x + x, yHeight, chunk.Position.z + z)));
                    }
                }
            }
        }
    }
}


[System.Serializable]
public class TreePlacement
{
    [Header("���� Ȯ���� ��������")]
    [Range(0.1f, 0.9f)]
    public float probability;
    [Header("Tree Noise")]
    //�ּ� 8�̻��̿��� �� �ƴϸ� ��ħ (treeZone�� ���� �ʴ´ٸ�)
    public float scaleTree = 8;
    public float offsetTree;
    [Header("������ ����� �������� �̷�� ������")]
    public _BLOCK woodType;
    //������ �ɾ��� �� �ִ� ������ ���� ���ϰ� �� �����ȿ��� Ȯ�������� ������ �ɴ� ���
    //public float treeZoneScale = 1.3f;
    //[Range(0.1f, 0.9f)]
    //public float treeZoneThreshold = 0.6f;

    [Header("�������� �����ϴ���")]
    public bool leaves;
    [Header("�������� ����� �������� �̷�� ������")]
    public _BLOCK leavesType;
    [Header("������ �ɾ����� ���� ���� �������")]
    public _BLOCK groundType;
    [Header("������ ũ��")]
    public int minHeight;
    public int maxHeight;

    /// <summary>
    /// ����������
    /// </summary>
    /// <param name="x"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    public bool MakeTree(int x, int z)
    {
        //if (Noise.Get2DPerlin(new Vector2Int(x, z), 0, treeZoneScale) < treeZoneThreshold)
        //{
        if (Noise.Get2DPerlin(new Vector2Int(x, z), offsetTree, scaleTree) < probability)
        {
            return true;
            //}
        }
        return false;
    }

    public List<BlockOrder> CreateTree(Vector3Int world)
    {
        List<BlockOrder> list = new List<BlockOrder>();
        int height = Random.Range(minHeight, maxHeight);
        //��������ġ�� ���� ���̴�
        list.Add(new BlockOrder(world.x, world.y, world.z, groundType));

        for (int i = 1; i < height; i++)
        {
            list.Add(new BlockOrder(world.x, world.y + i, world.z, woodType));
        }
        if (!leaves)
            return list;

        height -= 2;

        for (int y = 0; y < 2; y++)
        {
            for (int x = -2; x <= 2; x++)
            {
                for (int z = -2; z <= 2; z++)
                {
                    if (x == 0 && z == 0)
                        continue;
                    list.Add(new BlockOrder(world.x + x, world.y + y + height, world.z + z, leavesType));
                }
            }
        }

        height += 2;

        for (int x = -1; x <= 1; x++)
        {
            for (int z = -1; z <= 1; z++)
            {
                if (x == 0 && z == 0)
                    continue;
                list.Add(new BlockOrder(world.x + x, world.y + height, world.z + z, leavesType));
            }
        }
        height += 1;
        list.Add(new BlockOrder(world.x + 0, world.y + height, world.z + 1, leavesType));
        list.Add(new BlockOrder(world.x + 1, world.y + height, world.z + 0, leavesType));
        list.Add(new BlockOrder(world.x + 0, world.y + height, world.z + 0, leavesType));
        list.Add(new BlockOrder(world.x + -1, world.y + height, world.z + 0, leavesType));
        list.Add(new BlockOrder(world.x + 0, world.y + height, world.z + -1, leavesType));

        return list;
    }
}


//���� ���� 
[System.Serializable]
public class Underground
{
    [Header("���� �����")]
    public _BLOCK type;
    [Header("���� Ȯ���� ��������")]
    [Range(0f, 1f)]
    public float probability;
    [Header("��� ���̿� ������")]
    public float depth;
    [Header("Noise")]
    public float offset;
    public float scale;
}

public class BlockOrder
{
    //�̸��� world�� ������ ������������ ����Ѵٰ� ����ҷ���
    public Vector3Int world;
    //���� ������� 
    public _BLOCK type;
    public BlockOrder(int x, int y, int z, _BLOCK type)
    {
        world = new Vector3Int(x, y, z);
        this.type = type;
    }
    public BlockOrder(Vector3Int pos, _BLOCK type)
    {
        world = pos;
        this.type = type;
    }
}

//�� �𸣰ڳ�
//[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
//sealed class MinValueAttribute : Attribute
//{
//    // �ּҰ� �Ӽ�
//    public float MinValue { get; }

//    // �����ڸ� ���� �ּҰ��� ����
//    public MinValueAttribute(float minValue)
//    {
//        MinValue = minValue;
//    }
//}
