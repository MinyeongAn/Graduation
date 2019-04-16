using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class Block
{
    public int type;    // 무슨 블록인지 판별
    public bool vis;    // 보여지는건지 아닌건지
    public GameObject obj;  // 생성된 오브젝트
    public Block(int t, bool v, GameObject blockobj)
    {
        type = t;
        vis = v;
        obj = blockobj;
    }
}

public class MapSapwn : NetworkBehaviour
{
    [SerializeField]
    private LayerMask mask; //레이어

    [Header("[Block]")]
    public GameObject B_SoilPrefab;
    public GameObject B_GoldPrefab;
    public GameObject B_DiaPrefab;
    public GameObject B_GrassPrefab;
    public GameObject B_SandPrefab;
    public GameObject B_SnowPrefab;

    [Header("[Map Data]")]
    public static int Width_x = 80;
    public static int Width_z = 80;
    public static int height = 80;
    public float Wavelength = 0;
    public float Amplitude = 0;
    public float GroundHeightOffset = 20; // 지면을 띄워줌(동굴 등 때문에)
    public Block[,,] worldBlock = new Block[Width_x, height, Width_z];

    public override void OnStartServer()
    {
        StartCoroutine(InitGame());
    }

    IEnumerator InitGame()
    {
        // 맵생성
        yield return StartCoroutine(MapInit());
    }
    IEnumerator MapInit()
    {
        for (int x = 0; x < Width_x; x++)
        {
            for (int z = 0; z < Width_z; z++)
            {
                float xCoord = (x + 0) / Wavelength;
                float zCoord = (z + 0) / Wavelength;
                int y = (int)(Mathf.PerlinNoise(xCoord, zCoord) * Amplitude + GroundHeightOffset);
                Vector3 pos = new Vector3(x, y, z);
                StartCoroutine(CreateBlock(y, pos, true));
                while (y > 0)
                {
                    y--;
                    pos = new Vector3(x, y, z);
                    StartCoroutine(CreateBlock(y, pos, false));
                }
            }
        }
        yield return null;
    }

    IEnumerator CreateBlock(int y, Vector3 blockpos, bool visual)
    {
        if (y > 20)
        {
            if (visual)
            {
                GameObject BlockObj = (GameObject)Instantiate(B_SnowPrefab, blockpos, Quaternion.identity);
                NetworkServer.Spawn(BlockObj);
                worldBlock[(int)blockpos.x, (int)blockpos.y, (int)blockpos.z] = new Block(1, visual, BlockObj);
            }
            else
            {
                worldBlock[(int)blockpos.x, (int)blockpos.y, (int)blockpos.z] = new Block(1, visual, null);
            }
        }
        else if (y > 14)
        {
            if (visual)
            {
                GameObject BlockObj = (GameObject)Instantiate(B_GrassPrefab, blockpos, Quaternion.identity);
                NetworkServer.Spawn(BlockObj);
                worldBlock[(int)blockpos.x, (int)blockpos.y, (int)blockpos.z] = new Block(2, visual, BlockObj);
            }
            else
            {
                worldBlock[(int)blockpos.x, (int)blockpos.y, (int)blockpos.z] = new Block(2, visual, null);
            }
        }
        else
        {
            if (visual)
            {
                GameObject BlockObj = (GameObject)Instantiate(B_SoilPrefab, blockpos, Quaternion.identity);
                NetworkServer.Spawn(BlockObj);
                worldBlock[(int)blockpos.x, (int)blockpos.y, (int)blockpos.z] = new Block(3, visual, BlockObj);
            }
            else
            {
                worldBlock[(int)blockpos.x, (int)blockpos.y, (int)blockpos.z] = new Block(3, visual, null);
            }
        }
        if (y > 0 && y < 7 && Random.Range(0, 100) < 3)
        {
            // if문 없어도 됌
            if (worldBlock[(int)blockpos.x, (int)blockpos.y, (int)blockpos.z].obj != null)
            {
                Destroy(worldBlock[(int)blockpos.x, (int)blockpos.y, (int)blockpos.z].obj);
                NetworkServer.Destroy(worldBlock[(int)blockpos.x, (int)blockpos.y, (int)blockpos.z].obj);
            }
            if (visual)
            {
                GameObject BlockObj = (GameObject)Instantiate(B_GoldPrefab, blockpos, Quaternion.identity);
                NetworkServer.Spawn(BlockObj);
                worldBlock[(int)blockpos.x, (int)blockpos.y, (int)blockpos.z] = new Block(4, visual, BlockObj);
            }
            else
            {
                worldBlock[(int)blockpos.x, (int)blockpos.y, (int)blockpos.z] = new Block(4, visual, null);
            }
        }

        if (0 == y)
        {
            if (visual)
            {
                GameObject BlockObj = (GameObject)Instantiate(B_DiaPrefab, blockpos, Quaternion.identity);
                NetworkServer.Spawn(BlockObj);
                worldBlock[(int)blockpos.x, (int)blockpos.y, (int)blockpos.z] = new Block(5, visual, BlockObj);
            }
            else
            {
                worldBlock[(int)blockpos.x, (int)blockpos.y, (int)blockpos.z] = new Block(5, visual, null);
            }
        }
        yield return null;
    }
    // 제거 후 주변의 블럭을 생성하기 위한 코드
    internal void DrawBlock(Vector3 blockPos)
    {
        if (worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z] == null) return;

        if (!worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z].vis)
        {
            GameObject newBlock = null;
            worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z].vis = true;

            if (worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z].type == 1)
            {
                newBlock = (GameObject)Instantiate(B_SnowPrefab, blockPos, Quaternion.identity);
                NetworkServer.Spawn(newBlock);
            }
            else if (worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z].type == 2)
            {
                newBlock = (GameObject)Instantiate(B_GrassPrefab, blockPos, Quaternion.identity);
                NetworkServer.Spawn(newBlock);
            }
            else if (worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z].type == 3)
            {
                newBlock = (GameObject)Instantiate(B_SoilPrefab, blockPos, Quaternion.identity);
                NetworkServer.Spawn(newBlock);
            }
            else if (worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z].type == 4)
            {
                newBlock = (GameObject)Instantiate(B_GoldPrefab, blockPos, Quaternion.identity);
                NetworkServer.Spawn(newBlock);
            }
            else if (worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z].type == 5)
            {
                newBlock = (GameObject)Instantiate(B_DiaPrefab, blockPos, Quaternion.identity);
                NetworkServer.Spawn(newBlock);
            }
            else // 비어있는 공간을 만들 때 사용
                worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z].vis = false;
            if (newBlock != null)
                worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z].obj = newBlock;

        }
    }
}