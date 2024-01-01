using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR;

[CreateAssetMenu(fileName = "BiomeAttributes", menuName = "Minecraft/Biome Attribute")]
public class Biome : ScriptableObject
{
    //높이는 2d noise
    //땅속 블록은 3d noise
    //tree 는 2d
    //cave 는 3d
    [Header("가장 겉면(윗면)을 무슨블록이 채울건지")]
    public _BLOCK surfaceType;
    [Header("가장 겉면(윗면)이 몇칸 차지할건지")]
    public int surfaceDepth;
    [Header("청크를 채울 기본적인 블록")]
    public _BLOCK nomalType;
    //높이를 정하는 기준이 2개인 이유는 noise를 이용한 값으론 표현이 불가능함
    [Header("기본적인 높이 (이 높이를 기준으로 noise + - 어느정도 들어감)")]
    public int nomalHeight;
    [Header("기본적인 높이 nomal을 적용 후 이만큼 더해서 높이가 정해짐")]
    public int surfaceHeight;
    [Header("Height Noise")]
    public float offsetHeight;
    //[MinValue(0.1f)]
    public float scaleHeight = 0.5f;

    [Header("동굴이 있는 지형인지 (아직 미구현)")]
    public bool cave;

    [Header("나무에 대한 정보들 (위에 있을수록 우선적 {겹칠경우 위에있는게 생성됨})")]
    public TreePlacement[] treePlacements;

    [Header("땅속에 확률적으로 있는 블록들 (위에 있을 수록 우선적 {겹칠경우 위에있는게 생성됨} )")]
    public Underground[] undergrounds;

    public int Height(int x, int z)
    {
        return (int)(Noise.Get2DPerlin(new Vector2Int(x, z), offsetHeight, scaleHeight) * nomalHeight + surfaceHeight);
    }

    //기본적인 지형을 만들기
    public void CreateBaseMap(Chunk chunk, ref byte[,,] map)
    {
        for (int x = 0; x < BlockInfo.ChunkWidth; x++)
        {
            for (int z = 0; z < BlockInfo.ChunkWidth; z++)
            {
                int yHeight = Height(chunk.Position.x + x, chunk.Position.z + z);
                for (int y = 0; y < BlockInfo.ChunkHeight; y++)
                {
                    //air 의 경우 넣어주지 않더라도 0으로 들어가있어서 괜찮음
                    if (y > yHeight)
                        break;
                    if (y < 1)
                        map[x, y, z] = (byte)_BLOCK.BEDROCK; //배드락으로 바꿔야함
                    else if (y >= yHeight - surfaceDepth)
                        map[x, y, z] = (byte)surfaceType;
                    else
                        map[x, y, z] = (byte)nomalType;
                }
            }
        }
    }

    //나무 심기
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
    [Header("무슨 확률로 존재할지")]
    [Range(0.1f, 0.9f)]
    public float probability;
    [Header("Tree Noise")]
    //최소 8이상이여야 함 아니면 겹침 (treeZone을 쓰지 않는다면)
    public float scaleTree = 8;
    public float offsetTree;
    [Header("나무의 블록이 무엇으로 이루어 졌는지")]
    public _BLOCK woodType;
    //나무가 심어질 수 있는 지역을 먼저 정하고 그 지역안에서 확률적으로 나무를 심는 방법
    //public float treeZoneScale = 1.3f;
    //[Range(0.1f, 0.9f)]
    //public float treeZoneThreshold = 0.6f;

    [Header("나뭇잎이 존재하는지")]
    public bool leaves;
    [Header("나무잎의 블록이 무엇으로 이루어 졌는지")]
    public _BLOCK leavesType;
    [Header("나무가 심어지는 땅은 무슨 블록인지")]
    public _BLOCK groundType;
    [Header("나무의 크기")]
    public int minHeight;
    public int maxHeight;

    /// <summary>
    /// 월드포지션
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
        //나무가설치될 땅은 흙이다
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


//땅속 지형 
[System.Serializable]
public class Underground
{
    [Header("무슨 블록이")]
    public _BLOCK type;
    [Header("무슨 확률로 존재할지")]
    [Range(0f, 1f)]
    public float probability;
    [Header("어느 깊이에 있을지")]
    public float depth;
    [Header("Noise")]
    public float offset;
    public float scale;
}

public class BlockOrder
{
    //이름이 world인 이유는 월드포지션을 줘야한다고 기억할려고
    public Vector3Int world;
    //무슨 블록인지 
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

//잘 모르겠네
//[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
//sealed class MinValueAttribute : Attribute
//{
//    // 최소값 속성
//    public float MinValue { get; }

//    // 생성자를 통해 최소값을 받음
//    public MinValueAttribute(float minValue)
//    {
//        MinValue = minValue;
//    }
//}
