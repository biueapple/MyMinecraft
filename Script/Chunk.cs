using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    //ûũ�� �׸��� ûũ�� ���� ������ �ٸ� ûũ�� ���� �׸��� ������ �����Ǵ� ������ ������ �޾Ƽ� �ٸ� ûũ�� ������ �޾ƿ�
    private World _world;
    public World world { get { return _world; } }
    //���� �׷��� �ϴ� ûũ�� ���� ���̿����� ���� ûũ�� ����°� �ƴ϶� biome�� ������ִ°� 
    private Biome _biome;
    public Biome biome { get { return _biome; } }
    //ûũ�� �׷��� ������Ʈ��
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    //��
    private byte[,,] map;

    //ûũ�� �׸��� ����ϴ� ������
    int vertexIndex = 0;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();

    //�� ûũ�� ��ġ
    private Vector3Int position;
    public Vector3Int Position { get { return position; } }

    //�� ûũ�� Ȱ��ȭ ��������
    public bool Active { get { return gameObject.activeSelf; } set { gameObject.SetActive(value); } }

    //ûũ�����
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

    //ûũ ���� ������ �ٲܶ� ���
    public void EditMap(Vector3Int local, _BLOCK type)
    {
        if (_world.ChunkBlockPosition(local))
        {
            map[local.x, local.y, local.z] = (byte)type;
        }
    }

    //�� ��ġ�� ���� ����� �ִ��� �������ִ� �Լ� (��ġ�� ����� 0�� ����)
    public byte WorldPositionBlock(Vector3Int pos)
    {
        pos = pos - position;
        if (!_world.ChunkBlockPosition(pos))
            return 0;

        return map[pos.x, pos.y, pos.z];
    }


    //�׸���
    public void Draw()
    {
        ClearMesh();
        CreateMeshData();
        CreateMesh();
    }

    //ûũ�� �ٽ� �׸��� ������ ����ϴ� �������� �ʱ�ȭ �ؾ��� �ȱ׷��� �Ȱ����� ������ �׸��鼭 index�� �ʹ� �������ٰ� ������ ���� ��
    void ClearMesh()
    {
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();
        vertexIndex = 0;
    }

    //mesh ����� (map�� �������� mesh�� �׸�)
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

    //����� �׸��� �ʿ��� 6���� �ﰢ���� �׸���
    void AddBlockMeshToChunk(Vector3Int pos)
    {
        byte blockID = map[pos.x, pos.y, pos.z];
        if (blockID == 0)
            return;
        for (int p = 0; p < 6; p++)
        {
            if (_world.WorldBlockPositionTransparent(position + pos + BlockInfo.faceChecks[p]))
            {
                //�� ���� �׸��� ���� 4���� ���
                vertices.Add(pos + BlockInfo.voxelVerts[BlockInfo.voxelTris[p, 0]]);
                vertices.Add(pos + BlockInfo.voxelVerts[BlockInfo.voxelTris[p, 1]]);
                vertices.Add(pos + BlockInfo.voxelVerts[BlockInfo.voxelTris[p, 2]]);
                vertices.Add(pos + BlockInfo.voxelVerts[BlockInfo.voxelTris[p, 3]]);

                //�ؽ��� �����
                AddTexture(_world.blockDatas[blockID].GetTextureID(p));

                //�ð�������� �׸��� �ٱ��� ���̰� �ݽð�� ���ʸ��� �׸��°�
                triangles.Add(vertexIndex);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 3);
                //������ 4���� �����ϱ� +4����
                vertexIndex += 4;
            }
        }
    }

    //�ؽ��İ� �ϴϴϱ� id�� �޾Ƽ� ���߿� ���� ã�Ƽ� uv �־��ֱ�
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

    //mesh �׸��� (CreateMeshData -> AddBlockMeshToChunk ���� �����鿡�� �Ҵ��� mesh�����͵��� ������Ʈ�� �ְ� �׸��� ����)
    void CreateMesh()
    {
        Mesh mesh = new Mesh();

        //����
        mesh.vertices = vertices.ToArray();
        //������ ����� �ﰢ���� �׸�
        mesh.triangles = triangles.ToArray();
        //�ؽ���
        mesh.uv = uvs.ToArray();

        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }
}
