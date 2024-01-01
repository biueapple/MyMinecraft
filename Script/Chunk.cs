using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    //청크를 그릴때 청크의 가장 끝쪽은 다른 청크에 따라 그릴지 말지가 결정되니 월드의 도움을 받아서 다른 청크의 정보를 받아옴
    private World _world;
    public World world { get { return _world; } }
    //내가 그려야 하는 청크가 무슨 바이옴인지 맵은 청크가 만드는게 아니라 biome이 만들어주는것 
    private Biome _biome;
    public Biome biome { get { return _biome; } }
    //청크를 그려줄 컴포넌트들
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    //맵
    private byte[,,] map;

    //청크를 그릴때 사용하는 정보들
    int vertexIndex = 0;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();

    //이 청크의 위치
    private Vector3Int position;
    public Vector3Int Position { get { return position; } }

    //이 청크가 활성화 상태인지
    public bool Active { get { return gameObject.activeSelf; } set { gameObject.SetActive(value); } }

    //청크만들기
    public void Init(World world, Vector3Int position, Biome biome)
    {
        _world = world;
        _biome = biome;
        this.position = position;
        transform.position = position;
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.material = world.blockMaterial;
        map = new byte[BlockInfo.ChunkWidth, BlockInfo.ChunkHeight, BlockInfo.ChunkWidth];
        _biome.CreateBaseMap(this, ref map);
    }

    //청크 맵의 정보를 바꿀때 사용
    public void EditMap(Vector3Int local, _BLOCK type)
    {
        if (_world.ChunkBlockPosition(local))
        {
            map[local.x, local.y, local.z] = (byte)type;
        }
    }

    //이 위치에 무슨 블록이 있는지 리턴해주는 함수 (위치를 벗어나면 0을 리턴)
    public byte WorldPositionBlock(Vector3Int pos)
    {
        pos = pos - position;
        if (!_world.ChunkBlockPosition(pos))
            return 0;

        return map[pos.x, pos.y, pos.z];
    }


    //그리기
    public void Draw()
    {
        ClearMesh();
        CreateMeshData();
        CreateMesh();
    }

    //청크를 다시 그릴때 기존에 사용하던 변수들을 초기화 해야함 안그러면 똑같은걸 여러번 그리면서 index가 너무 높아진다고 에러도 나고 함
    void ClearMesh()
    {
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();
        vertexIndex = 0;
    }

    //mesh 만들기 (map을 바탕으로 mesh를 그림)
    void CreateMeshData()
    {
        for (int y = 0; y < BlockInfo.ChunkHeight; y++)
        {
            for (int x = 0; x < BlockInfo.ChunkWidth; x++)
            {
                for (int z = 0; z < BlockInfo.ChunkWidth; z++)
                {
                    AddBlockMeshToChunk(new Vector3Int(x, y, z));
                }
            }
        }
    }

    //블록을 그릴때 필요한 6개의 삼각형을 그리기
    void AddBlockMeshToChunk(Vector3Int pos)
    {
        byte blockID = map[pos.x, pos.y, pos.z];
        if (blockID == 0)
            return;
        for (int p = 0; p < 6; p++)
        {
            if (_world.WorldBlockPositionTransparent(position + pos + BlockInfo.faceChecks[p]))
            {
                //한 면을 그릴때 점은 4개로 충분
                vertices.Add(pos + BlockInfo.voxelVerts[BlockInfo.voxelTris[p, 0]]);
                vertices.Add(pos + BlockInfo.voxelVerts[BlockInfo.voxelTris[p, 1]]);
                vertices.Add(pos + BlockInfo.voxelVerts[BlockInfo.voxelTris[p, 2]]);
                vertices.Add(pos + BlockInfo.voxelVerts[BlockInfo.voxelTris[p, 3]]);

                //텍스쳐 씌우기
                AddTexture(_world.blockDatas[blockID].GetTextureID(p));

                //시계방향으로 그리면 바깥쪽 면이고 반시계는 안쪽면을 그리는것
                triangles.Add(vertexIndex);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 3);
                //정점은 4개씩 넣으니까 +4해줌
                vertexIndex += 4;
            }
        }
    }

    //텍스쳐가 하니니까 id를 받아서 그중에 뭔지 찾아서 uv 넣어주기
    void AddTexture(int textureID)
    {
        float y = textureID / BlockInfo.TextureAtlasSizeInBlocks;
        float x = textureID - (y * BlockInfo.TextureAtlasSizeInBlocks);

        x *= BlockInfo.NormalizedBlockTextureSize;
        y *= BlockInfo.NormalizedBlockTextureSize;

        y = 1f - y - BlockInfo.NormalizedBlockTextureSize;

        uvs.Add(new Vector2(x, y));
        uvs.Add(new Vector2(x, y + BlockInfo.NormalizedBlockTextureSize));
        uvs.Add(new Vector2(x + BlockInfo.NormalizedBlockTextureSize, y));
        uvs.Add(new Vector2(x + BlockInfo.NormalizedBlockTextureSize, y + BlockInfo.NormalizedBlockTextureSize));
    }

    //mesh 그리기 (CreateMeshData -> AddBlockMeshToChunk 에서 변수들에게 할당한 mesh데이터들을 컴포넌트에 넣고 그리는 과정)
    void CreateMesh()
    {
        Mesh mesh = new Mesh();

        //정점
        mesh.vertices = vertices.ToArray();
        //정점을 사용해 삼각형을 그림
        mesh.triangles = triangles.ToArray();
        //텍스쳐
        mesh.uv = uvs.ToArray();

        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }
}
