using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class World : MonoBehaviour
{
    public Material blockMaterial;
    private Chunk[,] chunks;
    public BlockData[] blockDatas;
    public Biome[] biomes; 
    //�׷��� �ϴ� �߰����� ��ϵ�

    // Start is called before the first frame update
    void Start()
    {
        //ûũ �����
        chunks = new Chunk[BlockInfo.startChunkSize, BlockInfo.startChunkSize];
        for (int x = 0; x < BlockInfo.startChunkSize; x++)
        {
            for (int z = 0; z < BlockInfo.startChunkSize; z++)
            {
                chunks[x, z] = new GameObject("Chunk " + x + " " + z).AddComponent<Chunk>();
                chunks[x, z].transform.SetParent(transform);
                //���⿡ �⺻���� ���� ����� �ڵ尡 �������
                chunks[x, z].Init(this, new Vector3Int(x * BlockInfo.ChunkWidth, 0, z * BlockInfo.ChunkWidth), biomes[0]);
            }
        }

        //���� ������ �ֱ�
        for (int x = 0; x < BlockInfo.startChunkSize; x++)
        {
            for (int z = 0; z < BlockInfo.startChunkSize; z++)
            {
                chunks[x, z].biome.CreateTreeMap(chunks[x, z]);
            }
        }

        //������ �� �־����� �׸���
        for (int x = 0; x < BlockInfo.startChunkSize; x++)
        {
            for (int z = 0; z < BlockInfo.startChunkSize; z++)
            {
                chunks[x, z].Draw();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //����� ��ġ�ϸ鼭 �ٽ� draw�ϸ鼭 ���� ûũ���� �ٽ� draw��
    public void InstallBlock(BlockOrder order)
    {
        //���� ûũ�� draw�ؾ����� �־�δ� ����Ʈ
        List<Chunk> Site = new List<Chunk>();
        //��� ����� �ٲ��� (��ġ����)
        Chunk c = WorldPositionToChunk(order.world);
        //����� �� ûũ��� 
        if(c != null)
        {
            //��� �ٲٰ�
            c.EditMap(order.world - c.Position, order.type);
            //draw�ؾ��ϴ� ����Ʈ�� �ֱ�
            Site.Add(c);
        }

        //������ �ٸ� ûũ���� �ٸ� ûũ��� �� ûũ���� ���� draw�ؾ���
        c = WorldPositionToChunk(order.world + new Vector3Int(1, 0, 0));
        if(c != null && !Site.Contains(c))
            Site.Add(c);
        c = WorldPositionToChunk(order.world + new Vector3Int(0, 0, 1));
        if (c != null && !Site.Contains(c))
            Site.Add(c);
        c = WorldPositionToChunk(order.world + new Vector3Int(-1, 0, 0));
        if (c != null && !Site.Contains(c))
            Site.Add(c);
        c = WorldPositionToChunk(order.world + new Vector3Int(0, 0, -1));
        if (c != null && !Site.Contains(c))
            Site.Add(c);

        //����Ʈ�� �ִ� ûũ�� ���� draw
        for(int i = 0; i < Site.Count; i++)
        {
            Site[i].Draw();
        }
    }


    //����� ��ġ������ �ٽ� Draw���� ���� (���� ó�� �����Ҷ� ȣ���� {tree�� ���鶧 ȣ����})
    public void EditBlock(List<BlockOrder> order)
    {
        for(int i = 0; i < order.Count; i++)
        {
            if (WorldBlockPositon(order[i].world))
            {
                Vector2Int index = new Vector2Int(order[i].world.x / BlockInfo.ChunkWidth, order[i].world.z / BlockInfo.ChunkWidth);
                chunks[index.x, index.y].EditMap(order[i].world - chunks[index.x, index.y].Position, order[i].type);
            }
        }
    }

    //���� ��ġ�� �ִ� ����� �����͸� ��������
    public BlockData Vector3IntWorldBlockToBlockData(Vector3Int position)
    {
        //��ġ�� ���忡 ���� ������ ��ġ�ΰ�
        if (WorldBlockPositon(position))
        {
            //��� ûũ����
            Vector2Int index = new Vector2Int(position.x / BlockInfo.ChunkWidth, position.z / BlockInfo.ChunkWidth);
            //ûũ�ȿ� ��ġ�� �ִ� ����� ���� ��������
            return blockDatas[chunks[index.x, index.y].WorldPositionBlock(position)];
        }
        return null;
    }

    //�� ��ġ�� ����� �ִ���
    public bool Vector3WorldBlock(Vector3 position)
    {
        //��ġ�� ã��
        Vector3Int v = new Vector3Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y), Mathf.RoundToInt(position.z));
        //�� ��ġ�� ���� �������� Ȯ���ϰ�
        if(WorldBlockPositon(v))
        {
            //��� ûũ���� ã��
            Vector2Int index = new Vector2Int(v.x / BlockInfo.ChunkWidth, v.z / BlockInfo.ChunkWidth);
            //�װ��� ���id�� 0�� �ƴ϶�� (0�� air�� ���� ��� ����̴ϱ�)
            if (chunks[index.x,index.y].WorldPositionBlock(v) > 0)
            {
                return true;
            }
        }
        return false;
    }

    //�� ��ġ�� ûũ�� ���������
    public Chunk WorldPositionToChunk(Vector3Int worldPosition)
    {
        //0���� �۴ٸ� ���°���
        if (worldPosition.x < 0 || worldPosition.z < 0)
            return null;
        //ûũ�� �ε����� ã��
        Vector2Int index = new Vector2Int(worldPosition.x / BlockInfo.ChunkWidth, worldPosition.z / BlockInfo.ChunkWidth);
        //������
        return chunks[index.x, index.y];
    }

    //�� ��ġ�� ����� ��������
    public bool WorldBlockPositionTransparent(Vector3Int worldPosition)
    {
        //-1 / 8 = 0, 1 / 8 = 0 �̹Ƿ� ����ó���� ����߰ڴ�
        //�̰� true�� -����� ���� (���� ���� ó�� �ٱ� �κ��� �׸��ٴ� ��)
        if(worldPosition.x < 0 || worldPosition.z < 0)
            return false;

        //ûũ�� �ε���
        Vector2Int index = new Vector2Int(worldPosition.x / BlockInfo.ChunkWidth, worldPosition.z / BlockInfo.ChunkWidth);

        //�̰� true��� ���� �� �� �κ��� �׸� (���� ���κ� �ٱ����� �׸��ٴ� �� �� �Ʒ��� ������ �׸�)
        if (index.x < 0 || index.x >= chunks.GetLength(0) || index.y < 0 || index.y >= chunks.GetLength(1))
            return false;

        return blockDatas[chunks[index.x, index.y].WorldPositionBlock(worldPosition)].transparent;
    }

    //�� ��ġ�� ���� ���� ���� ��������
    public bool WorldBlockPositon(Vector3Int worldPosition)
    {
        //0���� �������� ���忡 ����
        if (worldPosition.x < 0 || worldPosition.z < 0)
            return false;
        Vector2Int index = new Vector2Int(worldPosition.x / BlockInfo.ChunkWidth, worldPosition.z / BlockInfo.ChunkWidth);
        //ûũ ũ�⸦ ����ٴ� �� index.y�� z���� �ǹ���
        if (index.x < 0 || index.x >= chunks.GetLength(0) || index.y < 0 || index.y >= chunks.GetLength(1))
            return false;
        return true;
    }
    //�� ��ġ�� ûũ ���� ���� ��������
    public bool ChunkBlockPosition(Vector3Int localPosition)
    {
        //0���� �۰ų� ûũ ũ�⸦ ��� localposition�� ���� �Ұ�����
        if(localPosition.x < 0 || localPosition.x >= BlockInfo.ChunkWidth ||
            localPosition.y < 0 || localPosition.y >= BlockInfo.ChunkHeight ||
            localPosition.z < 0 || localPosition.z >= BlockInfo.ChunkWidth)
            return false;
        return true;
    }
    //raycast�� ����� �Լ� (collider�� �ȽἭ ������ raycast�� ������� ����)
    public BlockLaycast WorldRaycast(Vector3 postion, Vector3 dir, float distance, float checkIncrement = 0.1f)
    {
        BlockLaycast lay = null;
        //step�� checkIncrement�� �þ�鼭 �װ��� ����� �ִ��� Ȯ���ؾ���
        float step = checkIncrement;
        //���������� Ȯ���غ� �����ġ
        Vector3 lastPos = postion;

        //�÷��̾��� ��Ÿ��� ����� �ʴ� ��
        while (step < distance)
        {
            //ī�޶���� step��ŭ �������� ������ �Ÿ���
            Vector3 pos = postion + (dir * step);

            //lastPos == pos �ΰ�� continue �ҷ� �ߴµ� lastPos�� Vector3Int �� pos �� Vector3�� �񱳰� �ȵų�

            //����� �ִ°�?
            if (Vector3WorldBlock(pos))
            {
                //�ִٸ� �� ����� ��ġ�� �ı��� ����� ��ġ��
                lay = new BlockLaycast();
                lay.position = pos;
                lay.positionToInt = new Vector3Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y), Mathf.RoundToInt(pos.z));
                lay.lastPosition = lastPos;
                lay.lastPositionToInt = new Vector3Int(Mathf.RoundToInt(lastPos.x), Mathf.RoundToInt(lastPos.y), Mathf.RoundToInt(lastPos.z));
                lay.block = Vector3IntWorldBlockToBlockData(lay.positionToInt);
                //�� ���� ���������� Ȯ���ߴ� ����� ��ġ�� ����� ��ġ�� ��ġ
                return lay;
            }
            //Ȯ���� ��ġ�� ������ ��ġ�� �־����
            lastPos = new Vector3Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y), Mathf.RoundToInt(pos.z));
            step += checkIncrement;
        }
        return null;
    }
}


public class BlockLaycast
{
    public Vector3 position;
    public Vector3Int positionToInt;
    public Vector3 lastPosition;
    public Vector3Int lastPositionToInt;
    public BlockData block; 
}