using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BlockInfo
{
    //배고픔 최대치 (최대치가 배부르단 뜻)
    public static readonly int maxHunger = 20;
    //배고픔 상태 (이거 이하는 달리지 못한다는 뜻)
    public static readonly int minHunger = 6;
    //중력
    public static readonly float gravity = -9.8f;


    /// startChunkSize * startChunkSize + startChunkSize * startChunkSize + startChunkSize * startChunkSize + startChunkSize * startChunkSize
    public static int startChunkSize = 5;
    //가로와 세로의 크기
    public static readonly int ChunkWidth = 16;
    //높이
    public static readonly int ChunkHeight = 40;

    //월드에 청크가 몇개 있는가
    public static readonly int WorldSizeInChunks = 100;

    //텍스쳐의 가로에 몇개의 블록이 들어가 있는가 
    public static readonly int TextureAtlasSizeInBlocks = 32;
    //텍스쳐의 (0~1) 하나의 블록 이미지가 차지하는 크기 
    public static float NormalizedBlockTextureSize
    {
        get { return 1f / (float)TextureAtlasSizeInBlocks; }
    }

    //블록 하나를 그릴때 필요한 삼각형의 위치들
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

    //블록하나의 각면에대한 위치
    public static readonly Vector3Int[] faceChecks = new Vector3Int[6]
    {
        new Vector3Int(0, 0, -1),
        new Vector3Int(0, 0, 1),
        new Vector3Int(0, 1, 0),
        new Vector3Int(0, -1, 0),
        new Vector3Int(-1, 0, 0),
        new Vector3Int(1, 0, 0)
    };

    //블록 하나를 그릴때 사용할 점들의 인덱스들
    public static readonly int[,] voxelTris = new int[6, 4]
    {
		// 0 1 2 2 1 3
		{0, 3, 1, 2}, // Back Face
		{5, 6, 4, 7}, // Front Face
		{3, 7, 2, 6}, // Top Face
		{1, 5, 0, 4}, // Bottom Face
		{4, 7, 0, 3}, // Left Face
		{1, 2, 5, 6} // Right Face
	};

    //텍스쳐 uvs
    public static readonly Vector2[] voxelUvs = new Vector2[4]
    {
        new Vector2 (0.0f, 0.0f),
        new Vector2 (0.0f, 1.0f),
        new Vector2 (1.0f, 0.0f),
        new Vector2 (1.0f, 1.0f)
    };
}
