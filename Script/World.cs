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
    //그려야 하는 추가적인 블록들

    // Start is called before the first frame update
    void Start()
    {
        //청크 만들기
        chunks = new Chunk[BlockInfo.startChunkSize, BlockInfo.startChunkSize];
        for (int x = 0; x < BlockInfo.startChunkSize; x++)
        {
            for (int z = 0; z < BlockInfo.startChunkSize; z++)
            {
                chunks[x, z] = new GameObject("Chunk " + x + " " + z).AddComponent<Chunk>();
                chunks[x, z].transform.SetParent(transform);
                //여기에 기본적인 맵을 만드는 코드가 들어있음
                chunks[x, z].Init(this, new Vector3Int(x * BlockInfo.ChunkWidth, 0, z * BlockInfo.ChunkWidth), biomes[0]);
            }
        }

        //나무 데이터 넣기
        for (int x = 0; x < BlockInfo.startChunkSize; x++)
        {
            for (int z = 0; z < BlockInfo.startChunkSize; z++)
            {
                chunks[x, z].biome.CreateTreeMap(chunks[x, z]);
            }
        }

        //데이터 다 넣었으면 그리기
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

    //블록을 설치하면서 다시 draw하면서 주위 청크까지 다시 draw함
    public void InstallBlock(BlockOrder order)
    {
        //무슨 청크를 draw해야할지 넣어두는 리스트
        List<Chunk> Site = new List<Chunk>();
        //어디에 블록을 바꿀지 (설치할지)
        Chunk c = WorldPositionToChunk(order.world);
        //제대로 된 청크라면 
        if(c != null)
        {
            //블록 바꾸고
            c.EditMap(order.world - c.Position, order.type);
            //draw해야하는 리스트에 넣기
            Site.Add(c);
        }

        //주위가 다른 청크인지 다른 청크라면 그 청크또한 새로 draw해야함
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

        //리스트에 있는 청크들 전부 draw
        for(int i = 0; i < Site.Count; i++)
        {
            Site[i].Draw();
        }
    }


    //블록을 설치하지만 다시 Draw하지 않음 (맵을 처음 생성할때 호출함 {tree를 만들때 호출함})
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

    //월드 위치에 있는 블록의 데이터를 리턴해줌
    public BlockData Vector3IntWorldBlockToBlockData(Vector3Int position)
    {
        //위치가 월드에 존재 가능한 위치인가
        if (WorldBlockPositon(position))
        {
            //어느 청크인지
            Vector2Int index = new Vector2Int(position.x / BlockInfo.ChunkWidth, position.z / BlockInfo.ChunkWidth);
            //청크안에 위치해 있는 블록이 뭔지 리턴해줌
            return blockDatas[chunks[index.x, index.y].WorldPositionBlock(position)];
        }
        return null;
    }

    //이 위치에 블록이 있는지
    public bool Vector3WorldBlock(Vector3 position)
    {
        //위치를 찾고
        Vector3Int v = new Vector3Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y), Mathf.RoundToInt(position.z));
        //그 위치가 존재 가능한지 확인하고
        if(WorldBlockPositon(v))
        {
            //어느 청크인지 찾고
            Vector2Int index = new Vector2Int(v.x / BlockInfo.ChunkWidth, v.z / BlockInfo.ChunkWidth);
            //그곳의 블록id가 0이 아니라면 (0은 air로 없는 블록 취급이니까)
            if (chunks[index.x,index.y].WorldPositionBlock(v) > 0)
            {
                return true;
            }
        }
        return false;
    }

    //이 위치에 청크를 리턴해줘라
    public Chunk WorldPositionToChunk(Vector3Int worldPosition)
    {
        //0보다 작다면 없는거임
        if (worldPosition.x < 0 || worldPosition.z < 0)
            return null;
        //청크의 인덱스를 찾고
        Vector2Int index = new Vector2Int(worldPosition.x / BlockInfo.ChunkWidth, worldPosition.z / BlockInfo.ChunkWidth);
        //리턴함
        return chunks[index.x, index.y];
    }

    //이 위치에 블록이 투명한지
    public bool WorldBlockPositionTransparent(Vector3Int worldPosition)
    {
        //-1 / 8 = 0, 1 / 8 = 0 이므로 예외처리를 해줘야겠다
        //이게 true면 -면들이 보임 (맵의 가장 처음 바깥 부분을 그린다는 뜻)
        if(worldPosition.x < 0 || worldPosition.z < 0)
            return false;

        //청크의 인덱스
        Vector2Int index = new Vector2Int(worldPosition.x / BlockInfo.ChunkWidth, worldPosition.z / BlockInfo.ChunkWidth);

        //이게 true라면 없는 쪽 면 부분을 그림 (맵의 끝부분 바깥면을 그린다는 뜻 맨 아래의 배드락도 그림)
        if (index.x < 0 || index.x >= chunks.GetLength(0) || index.y < 0 || index.y >= chunks.GetLength(1))
            return false;

        return blockDatas[chunks[index.x, index.y].WorldPositionBlock(worldPosition)].transparent;
    }

    //이 위치가 월드 내에 존재 가능한지
    public bool WorldBlockPositon(Vector3Int worldPosition)
    {
        //0보다 작은값은 월드에 없음
        if (worldPosition.x < 0 || worldPosition.z < 0)
            return false;
        Vector2Int index = new Vector2Int(worldPosition.x / BlockInfo.ChunkWidth, worldPosition.z / BlockInfo.ChunkWidth);
        //청크 크기를 벗어났다는 것 index.y는 z축을 의미함
        if (index.x < 0 || index.x >= chunks.GetLength(0) || index.y < 0 || index.y >= chunks.GetLength(1))
            return false;
        return true;
    }
    //이 위치가 청크 내에 존재 가능한지
    public bool ChunkBlockPosition(Vector3Int localPosition)
    {
        //0보다 작거나 청크 크기를 벗어난 localposition은 존재 불가능함
        if(localPosition.x < 0 || localPosition.x >= BlockInfo.ChunkWidth ||
            localPosition.y < 0 || localPosition.y >= BlockInfo.ChunkHeight ||
            localPosition.z < 0 || localPosition.z >= BlockInfo.ChunkWidth)
            return false;
        return true;
    }
    //raycast를 대신할 함수 (collider를 안써서 기존의 raycast를 사용하지 못함)
    public BlockLaycast WorldRaycast(Vector3 postion, Vector3 dir, float distance, float checkIncrement = 0.1f)
    {
        BlockLaycast lay = null;
        //step은 checkIncrement씩 늘어나면서 그곳에 블록이 있는지 확인해야함
        float step = checkIncrement;
        //마지막으로 확인해본 블록위치
        Vector3 lastPos = postion;

        //플레이어의 사거리를 벋어나지 않는 한
        while (step < distance)
        {
            //카메라부터 step만큼 정면으로 떨어진 거리에
            Vector3 pos = postion + (dir * step);

            //lastPos == pos 인경우 continue 할려 했는데 lastPos는 Vector3Int 고 pos 는 Vector3라서 비교가 안돼네

            //블록이 있는가?
            if (Vector3WorldBlock(pos))
            {
                //있다면 그 블록의 위치가 파괴될 블록의 위치고
                lay = new BlockLaycast();
                lay.position = pos;
                lay.positionToInt = new Vector3Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y), Mathf.RoundToInt(pos.z));
                lay.lastPosition = lastPos;
                lay.lastPositionToInt = new Vector3Int(Mathf.RoundToInt(lastPos.x), Mathf.RoundToInt(lastPos.y), Mathf.RoundToInt(lastPos.z));
                lay.block = Vector3IntWorldBlockToBlockData(lay.positionToInt);
                //그 전에 마지막으로 확인했던 블록의 위치가 블록이 설치될 위치
                return lay;
            }
            //확인한 위치를 마지막 위치에 넣어놓기
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