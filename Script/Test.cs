using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Test : MonoBehaviour
{
    public MeshRenderer meshRenderer;
    public MeshFilter meshFilter;

    public Matrix4x4 S = Matrix4x4.identity;
    public Matrix4x4 R = Matrix4x4.identity;
    public Matrix4x4 T = Matrix4x4.identity;

    public Vector3 size = Vector3.one;
    public Vector3 rotation = Vector3.one;
    public Vector3 position = Vector3.one;

    public bool d = false;
    public float blockSize;
    public int id;
    public float imageSize;


    public static readonly Vector3[] voxelVerts = new Vector3[8]
   {
        new Vector3(-0.5f, -0.5f, -0.5f),
        new Vector3(0.5f, -0.5f, -0.5f),
        new Vector3(0.5f, 0.5f, -0.5f),
        new Vector3(-0.5f, 0.5f, -0.5f),
        new Vector3(-0.5f, -0.5f, 0.5f),
        new Vector3(0.5f, -0.5f, 0.5f),
        new Vector3(0.5f, 0.5f, 0.5f),
        new Vector3(-0.5f, 0.5f, 0.5f),
   };

    // Start is called before the first frame update
    void Start()
    {
        blockSize = 1f / imageSize;
        Draw();
    }

    // Update is called once per frame
    void Update()
    {
        if(d)
        {
            d = false;
            Draw();
        }
    }


    public void Draw()
    {
        int vertexIndex = 0;
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        S = Matrix4x4.Scale(size);

        R = Matrix4x4.Rotate(Quaternion.Euler(rotation));

        T = Matrix4x4.Translate(position);

        Matrix4x4 SRT = S * R * T;

        for (int p = 0; p < 6; p++)
        {
            vertices.Add(SRT.MultiplyPoint(voxelVerts[BlockInfo.voxelTris[p, 0]]));
            vertices.Add(SRT.MultiplyPoint(voxelVerts[BlockInfo.voxelTris[p, 1]]));
            vertices.Add(SRT.MultiplyPoint(voxelVerts[BlockInfo.voxelTris[p, 2]]));
            vertices.Add(SRT.MultiplyPoint(voxelVerts[BlockInfo.voxelTris[p, 3]]));

            triangles.Add(vertexIndex);
            triangles.Add(vertexIndex + 1);
            triangles.Add(vertexIndex + 2);
            triangles.Add(vertexIndex + 2);
            triangles.Add(vertexIndex + 1);
            triangles.Add(vertexIndex + 3);


            float line = id / imageSize;
            float y = 1 - Mathf.Floor(line) * blockSize - blockSize;
            float x = ((float)id % imageSize) * blockSize;

            uvs.Add(new Vector2(x, y));
            uvs.Add(new Vector2(x, y + blockSize));
            uvs.Add(new Vector2(x + blockSize, y));
            uvs.Add(new Vector2(x + blockSize, y + blockSize));

            vertexIndex += 4;
        }
        //0.03125
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }
}
