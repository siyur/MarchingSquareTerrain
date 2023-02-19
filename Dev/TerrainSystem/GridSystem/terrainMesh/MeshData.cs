using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class MeshData
{
    [HideInInspector]
    public int m_mapSize;
    [HideInInspector]
    public float m_gridSize;
    int vertexCount;

    //[HideInInspector]
    public List<Vector3> vertices;

    // the base grid texture
    [HideInInspector]
    public List<Vector2> uvs;

    [HideInInspector]
    public List<int> triangles;

    public void createMesh(GridMatrix currGridMatrix, Texture2D heightMap)
    /*
     arg skippedPos is for the texlayers > 4
     */
    {
        m_mapSize = currGridMatrix.mapSize;
        m_gridSize = currGridMatrix.gridSize;

        vertices = new List<Vector3>();
        uvs = new List<Vector2>();
        triangles = new List<int>();
        vertexCount = 0;

        float multiplier = heightMap.width / (currGridMatrix.mapSize + 1.0f);

        createQuadTreeMesh(0, 0, m_mapSize, m_mapSize, currGridMatrix);

        for (int i = 0; i < m_mapSize; i++)
        {
            for (int j = 0; j < m_mapSize; j++)
            {
                Grid currGrid = currGridMatrix.getGrid(i, j);
                int[] weightCode = currGrid.weightCode();
                meshOptions(i, j, weightCode, heightMap, multiplier);
            }
        }
    }

    void createSquad(int x, int y, int lenX, int lenY, int height)
    {
        Vector3 v00 = new Vector3(x * m_gridSize, height, y * m_gridSize);
        Vector3 v01 = new Vector3(x * m_gridSize, height, (y + lenY) * m_gridSize);
        Vector3 v02 = new Vector3((x + lenX) * m_gridSize, height, y * m_gridSize);
        Vector3 v03 = new Vector3((x + lenX) * m_gridSize, height, (y + lenY) * m_gridSize);
        vertices.Add(v00);
        vertices.Add(v01);
        vertices.Add(v02);
        vertices.Add(v03);

        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(0, 1f));
        uvs.Add(new Vector2(1f, 0));
        uvs.Add(new Vector2(1f, 1f));

        triangles.Add(vertexCount + 0);
        triangles.Add(vertexCount + 1);
        triangles.Add(vertexCount + 2);

        triangles.Add(vertexCount + 1);
        triangles.Add(vertexCount + 3);
        triangles.Add(vertexCount + 2);
        vertexCount += 4;
    }

    void meshOptions(int x, int y, int[] weightCode, Texture2D heightMap, float multiplier)
    {
        float weight1000 = heightMap.GetPixel((int)(x * multiplier), (int)(y * multiplier)).r * 5.0f;
        float weight0100 = heightMap.GetPixel((int)((x + 1) * multiplier), (int)(y * multiplier)).r * 5.0f;
        float weight0010 = heightMap.GetPixel((int)(x * multiplier), (int)((y + 1) * multiplier)).r * 5.0f;
        float weight0001 = heightMap.GetPixel((int)((x + 1) * multiplier), (int)((y + 1) * multiplier)).r * 5.0f;

        // the desired mid point values
        float targetHeight01 = weightCode[0] > weightCode[1] ? weightCode[0] : weightCode[1];
        float targetHeight23 = weightCode[2] > weightCode[3] ? weightCode[2] : weightCode[3];
        float targetHeight02 = weightCode[0] > weightCode[2] ? weightCode[0] : weightCode[2];
        float targetHeight13 = weightCode[1] > weightCode[3] ? weightCode[1] : weightCode[3];

        /*weight 0 to 1*/
        float weight01 = weight1000 == weight0100 ? 0 : Mathf.Abs(targetHeight01 - weight1000) / Mathf.Abs(weight1000 - weight0100);
        /*weight 2 to 3*/
        float weight23 = weight0010 == weight0001 ? 0 : Mathf.Abs(targetHeight23 - weight0010) / Mathf.Abs(weight0010 - weight0001);
        /*weight 0 to 2*/
        float weight02 = weight1000 == weight0010 ? 0 : Mathf.Abs(targetHeight02 - weight1000) / Mathf.Abs(weight1000 - weight0010);
        /*weight 1 to 3*/
        float weight13 = weight0100 == weight0001 ? 0 : Mathf.Abs(targetHeight13 - weight0100) / Mathf.Abs(weight0100 - weight0001);
        //Debug.Log(targetHeight01 + "||" + weight1000 + "||" + weight0100 + ":::"+ weight01 + "||" + weight01 + "||" + weight02 + "||" + weight13);



        if (weightCode[0] == weightCode[1] && weightCode[1] == weightCode[2] && weightCode[2] == weightCode[3])
        {
            return;
        }

        /* 1000 0111*/
        if (weightCode[0] != weightCode[1] && weightCode[1] == weightCode[2] && weightCode[2] == weightCode[3])
        {
            Vector3 v00 = new Vector3(x * m_gridSize, weightCode[0], y * m_gridSize);
            Vector3 v01 = new Vector3(x * m_gridSize, weightCode[0], (y + weight02) * m_gridSize);
            Vector3 v02 = new Vector3((x + weight01) * m_gridSize, weightCode[0], y * m_gridSize);
            vertices.Add(v00);
            vertices.Add(v01);
            vertices.Add(v02);
            uvs.Add(new Vector2(0, 0));
            uvs.Add(new Vector2(0, 0.5f));
            uvs.Add(new Vector2(0.5f, 0));
            triangles.Add(vertexCount + 0);
            triangles.Add(vertexCount + 1);
            triangles.Add(vertexCount + 2);

            Vector3 v03 = new Vector3((x + weight01) * m_gridSize, weightCode[1], y * m_gridSize);
            Vector3 v04 = new Vector3(x * m_gridSize, weightCode[1], (y + weight02) * m_gridSize);
            Vector3 v05 = new Vector3(x * m_gridSize, weightCode[1], (y + 1) * m_gridSize);
            Vector3 v06 = new Vector3((x + 1) * m_gridSize, weightCode[1], (y + 1) * m_gridSize);
            Vector3 v07 = new Vector3((x + 1) * m_gridSize, weightCode[1], y * m_gridSize);
            vertices.Add(v03);
            vertices.Add(v04);
            vertices.Add(v05);
            vertices.Add(v06);
            vertices.Add(v07);
            uvs.Add(new Vector2(0.5f, 0));
            uvs.Add(new Vector2(0, 0.5f));
            uvs.Add(new Vector2(0, 1f));
            uvs.Add(new Vector2(1f, 1f));
            uvs.Add(new Vector2(1f, 0));
            triangles.Add(vertexCount + 3);
            triangles.Add(vertexCount + 4);
            triangles.Add(vertexCount + 6);

            triangles.Add(vertexCount + 4);
            triangles.Add(vertexCount + 5);
            triangles.Add(vertexCount + 6);

            triangles.Add(vertexCount + 3);
            triangles.Add(vertexCount + 6);
            triangles.Add(vertexCount + 7);

            vertexCount += 8;

            // cliff mesh
            Vector3 v08 = new Vector3((x + weight01) * m_gridSize, weightCode[0], y * m_gridSize);
            Vector3 v09 = new Vector3(x * m_gridSize, weightCode[0], (y + weight02) * m_gridSize);
            Vector3 v10 = new Vector3((x + weight01) * m_gridSize, weightCode[1], y * m_gridSize);
            Vector3 v11 = new Vector3(x * m_gridSize, weightCode[1], (y + weight02) * m_gridSize);
            vertices.Add(v08);
            vertices.Add(v09);
            vertices.Add(v10);
            vertices.Add(v11);
            uvs.Add(new Vector2(0.5f, 0));
            uvs.Add(new Vector2(0, 0.5f));
            uvs.Add(new Vector2(0, 1f));
            uvs.Add(new Vector2(1f, 1f));
            triangles.Add(vertexCount + 0);
            triangles.Add(vertexCount + 1);
            triangles.Add(vertexCount + 2);

            triangles.Add(vertexCount + 2);
            triangles.Add(vertexCount + 1);
            triangles.Add(vertexCount + 3);
            vertexCount += 4;

        }
        /* 0100 1011*/
        else if (weightCode[1] != weightCode[0] && weightCode[0] == weightCode[2] && weightCode[2] == weightCode[3])
        {
            Vector3 v00 = new Vector3((x + 1) * m_gridSize, weightCode[1], y * m_gridSize);
            Vector3 v01 = new Vector3((x + weight01) * m_gridSize, weightCode[1], y * m_gridSize);
            Vector3 v02 = new Vector3((x + 1) * m_gridSize, weightCode[1], (y + weight13) * m_gridSize);
            vertices.Add(v00);
            vertices.Add(v01);
            vertices.Add(v02);
            uvs.Add(new Vector2(0, 0));
            uvs.Add(new Vector2(0, 1f));
            uvs.Add(new Vector2(1f, 0));
            triangles.Add(vertexCount + 0);
            triangles.Add(vertexCount + 1);
            triangles.Add(vertexCount + 2);

            Vector3 v03 = new Vector3((x + weight01) * m_gridSize, weightCode[0], y * m_gridSize);
            Vector3 v04 = new Vector3((x + 1) * m_gridSize, weightCode[0], (y + weight13) * m_gridSize);
            Vector3 v05 = new Vector3((x + 1) * m_gridSize, weightCode[0], (y + 1) * m_gridSize);
            Vector3 v06 = new Vector3(x * m_gridSize, weightCode[0], (y + 1) * m_gridSize);
            Vector3 v07 = new Vector3(x * m_gridSize, weightCode[0], y * m_gridSize);
            vertices.Add(v03);
            vertices.Add(v04);
            vertices.Add(v05);
            vertices.Add(v06);
            vertices.Add(v07);
            uvs.Add(new Vector2(0.5f, 0));
            uvs.Add(new Vector2(1, 0.5f));
            uvs.Add(new Vector2(1, 1f));
            uvs.Add(new Vector2(0, 1f));
            uvs.Add(new Vector2(0, 0));
            triangles.Add(vertexCount + 3);
            triangles.Add(vertexCount + 6);
            triangles.Add(vertexCount + 4);

            triangles.Add(vertexCount + 4);
            triangles.Add(vertexCount + 6);
            triangles.Add(vertexCount + 5);

            triangles.Add(vertexCount + 7);
            triangles.Add(vertexCount + 6);
            triangles.Add(vertexCount + 3);

            vertexCount += 8;

            // cliff mesh
            Vector3 v08 = new Vector3((x + weight01) * m_gridSize, weightCode[1], y * m_gridSize);
            Vector3 v09 = new Vector3((x + 1) * m_gridSize, weightCode[1], (y + weight13) * m_gridSize);
            Vector3 v10 = new Vector3((x + weight01) * m_gridSize, weightCode[0], y * m_gridSize);
            Vector3 v11 = new Vector3((x + 1) * m_gridSize, weightCode[0], (y + weight13) * m_gridSize);
            vertices.Add(v08);
            vertices.Add(v09);
            vertices.Add(v10);
            vertices.Add(v11);
            uvs.Add(new Vector2(0.5f, 0));
            uvs.Add(new Vector2(0, 0.5f));
            uvs.Add(new Vector2(0, 1f));
            uvs.Add(new Vector2(1f, 1f));
            triangles.Add(vertexCount + 0);
            triangles.Add(vertexCount + 2);
            triangles.Add(vertexCount + 1);

            triangles.Add(vertexCount + 1);
            triangles.Add(vertexCount + 2);
            triangles.Add(vertexCount + 3);
            vertexCount += 4;
        }
        /* 0010 1101*/
        else if (weightCode[2] != weightCode[0] && weightCode[0] == weightCode[1] && weightCode[1] == weightCode[3])
        {
            Vector3 v00 = new Vector3(x * m_gridSize, weightCode[2], (y + 1) * m_gridSize);
            Vector3 v01 = new Vector3(x * m_gridSize, weightCode[2], (y + weight02) * m_gridSize);
            Vector3 v02 = new Vector3((x + weight23) * m_gridSize, weightCode[2], (y + 1) * m_gridSize);
            vertices.Add(v00);
            vertices.Add(v01);
            vertices.Add(v02);
            uvs.Add(new Vector2(0, 0));
            uvs.Add(new Vector2(0, 1f));
            uvs.Add(new Vector2(1f, 0));
            triangles.Add(vertexCount + 0);
            triangles.Add(vertexCount + 2);
            triangles.Add(vertexCount + 1);

            Vector3 v03 = new Vector3(x * m_gridSize, weightCode[0], (y + weight02) * m_gridSize);
            Vector3 v04 = new Vector3((x + weight23) * m_gridSize, weightCode[0], (y + 1) * m_gridSize);
            Vector3 v05 = new Vector3((x + 1) * m_gridSize, weightCode[0], (y + 1) * m_gridSize);
            Vector3 v06 = new Vector3((x + 1) * m_gridSize, weightCode[0], y * m_gridSize);
            Vector3 v07 = new Vector3(x * m_gridSize, weightCode[0], y * m_gridSize);
            vertices.Add(v03);
            vertices.Add(v04);
            vertices.Add(v05);
            vertices.Add(v06);
            vertices.Add(v07);
            uvs.Add(new Vector2(0, 0.5f));
            uvs.Add(new Vector2(0.5f, 1));
            uvs.Add(new Vector2(1, 1));
            uvs.Add(new Vector2(1, 0));
            uvs.Add(new Vector2(0, 0));
            triangles.Add(vertexCount + 3);
            triangles.Add(vertexCount + 4);
            triangles.Add(vertexCount + 6);

            triangles.Add(vertexCount + 4);
            triangles.Add(vertexCount + 5);
            triangles.Add(vertexCount + 6);

            triangles.Add(vertexCount + 7);
            triangles.Add(vertexCount + 3);
            triangles.Add(vertexCount + 6);

            vertexCount += 8;

            // cliff mesh
            Vector3 v08 = new Vector3(x * m_gridSize, weightCode[0], (y + weight02) * m_gridSize);
            Vector3 v09 = new Vector3((x + weight23) * m_gridSize, weightCode[0], (y + 1) * m_gridSize);
            Vector3 v10 = new Vector3(x * m_gridSize, weightCode[2], (y + weight02) * m_gridSize);
            Vector3 v11 = new Vector3((x + weight23) * m_gridSize, weightCode[2], (y + 1) * m_gridSize);
            vertices.Add(v08);
            vertices.Add(v09);
            vertices.Add(v10);
            vertices.Add(v11);
            uvs.Add(new Vector2(0.5f, 0));
            uvs.Add(new Vector2(0, 0.5f));
            uvs.Add(new Vector2(0, 1f));
            uvs.Add(new Vector2(1f, 1f));
            triangles.Add(vertexCount + 0);
            triangles.Add(vertexCount + 2);
            triangles.Add(vertexCount + 1);

            triangles.Add(vertexCount + 1);
            triangles.Add(vertexCount + 2);
            triangles.Add(vertexCount + 3);
            vertexCount += 4;
        }
        /* 0001 1110*/
        else if (weightCode[3] != weightCode[0] && weightCode[0] == weightCode[1] && weightCode[1] == weightCode[2])
        {
            Vector3 v00 = new Vector3((x + 1) * m_gridSize, weightCode[3], (y + 1) * m_gridSize);
            Vector3 v01 = new Vector3((x + 1) * m_gridSize, weightCode[3], (y + weight13) * m_gridSize);
            Vector3 v02 = new Vector3((x + weight23) * m_gridSize, weightCode[3], (y + 1) * m_gridSize);
            vertices.Add(v00);
            vertices.Add(v01);
            vertices.Add(v02);
            uvs.Add(new Vector2(1, 1));
            uvs.Add(new Vector2(1, 0.5f));
            uvs.Add(new Vector2(0.5f, 1));
            triangles.Add(vertexCount + 0);
            triangles.Add(vertexCount + 1);
            triangles.Add(vertexCount + 2);

            Vector3 v03 = new Vector3(x * m_gridSize, weightCode[0], y * m_gridSize);
            Vector3 v04 = new Vector3(x * m_gridSize, weightCode[0], (y + 1) * m_gridSize);
            Vector3 v05 = new Vector3((x + weight23) * m_gridSize, weightCode[0], (y + 1) * m_gridSize);
            Vector3 v06 = new Vector3((x + 1) * m_gridSize, weightCode[0], (y + weight13) * m_gridSize);
            Vector3 v07 = new Vector3((x + 1) * m_gridSize, weightCode[0], y * m_gridSize);
            vertices.Add(v03);
            vertices.Add(v04);
            vertices.Add(v05);
            vertices.Add(v06);
            vertices.Add(v07);
            uvs.Add(new Vector2(0, 0));
            uvs.Add(new Vector2(0, 1));
            uvs.Add(new Vector2(0.5f, 1f));
            uvs.Add(new Vector2(1, 0.5f));
            uvs.Add(new Vector2(1, 0));
            triangles.Add(vertexCount + 3);
            triangles.Add(vertexCount + 4);
            triangles.Add(vertexCount + 5);

            triangles.Add(vertexCount + 3);
            triangles.Add(vertexCount + 5);
            triangles.Add(vertexCount + 6);

            triangles.Add(vertexCount + 3);
            triangles.Add(vertexCount + 6);
            triangles.Add(vertexCount + 7);

            vertexCount += 8;
            //-----------------------------------------------------------------error case

            // cliff mesh
            Vector3 v08 = new Vector3((x + 1) * m_gridSize, weightCode[0], (y + weight13) * m_gridSize);
            Vector3 v09 = new Vector3((x + weight23) * m_gridSize, weightCode[0], (y + 1) * m_gridSize);
            Vector3 v10 = new Vector3((x + 1) * m_gridSize, weightCode[3], (y + weight13) * m_gridSize);
            Vector3 v11 = new Vector3((x + weight23) * m_gridSize, weightCode[3], (y + 1) * m_gridSize);
            vertices.Add(v08);
            vertices.Add(v09);
            vertices.Add(v10);
            vertices.Add(v11);
            uvs.Add(new Vector2(0.5f, 0));
            uvs.Add(new Vector2(0, 0.5f));
            uvs.Add(new Vector2(0, 1f));
            uvs.Add(new Vector2(1f, 1f));
            triangles.Add(vertexCount + 0);
            triangles.Add(vertexCount + 1);
            triangles.Add(vertexCount + 2);

            triangles.Add(vertexCount + 2);
            triangles.Add(vertexCount + 1);
            triangles.Add(vertexCount + 3);
            vertexCount += 4;
        }

        /*1100 0011*/
        else if (weightCode[0] == weightCode[1] && weightCode[2] == weightCode[3] && weightCode[0] != weightCode[2])
        {
            Vector3 v00 = new Vector3(x * m_gridSize, weightCode[0], y * m_gridSize);
            Vector3 v01 = new Vector3((x + 1) * m_gridSize, weightCode[0], y * m_gridSize);
            Vector3 v02 = new Vector3(x * m_gridSize, weightCode[0], (y + weight02) * m_gridSize);
            Vector3 v03 = new Vector3((x + 1) * m_gridSize, weightCode[0], (y + weight13) * m_gridSize);
            vertices.Add(v00);
            vertices.Add(v01);
            vertices.Add(v02);
            vertices.Add(v03);
            uvs.Add(new Vector2(0, 0));
            uvs.Add(new Vector2(1, 0));
            uvs.Add(new Vector2(0, 0.5f));
            uvs.Add(new Vector2(1, 0.5f));
            triangles.Add(vertexCount + 0);
            triangles.Add(vertexCount + 2);
            triangles.Add(vertexCount + 1);

            triangles.Add(vertexCount + 1);
            triangles.Add(vertexCount + 2);
            triangles.Add(vertexCount + 3);


            Vector3 v04 = new Vector3(x * m_gridSize, weightCode[2], (y + weight02) * m_gridSize);
            Vector3 v05 = new Vector3((x + 1) * m_gridSize, weightCode[2], (y + weight13) * m_gridSize);
            Vector3 v06 = new Vector3(x * m_gridSize, weightCode[2], (y + 1) * m_gridSize);
            Vector3 v07 = new Vector3((x + 1) * m_gridSize, weightCode[2], (y + 1) * m_gridSize);
            vertices.Add(v04);
            vertices.Add(v05);
            vertices.Add(v06);
            vertices.Add(v07);
            uvs.Add(new Vector2(0, 0.5f));
            uvs.Add(new Vector2(1, 0.5f));
            uvs.Add(new Vector2(0, 1));
            uvs.Add(new Vector2(1, 1));

            triangles.Add(vertexCount + 4);
            triangles.Add(vertexCount + 6);
            triangles.Add(vertexCount + 5);

            triangles.Add(vertexCount + 5);
            triangles.Add(vertexCount + 6);
            triangles.Add(vertexCount + 7);

            vertexCount += 8;

            // cliff mesh
            Vector3 v08 = new Vector3(x * m_gridSize, weightCode[0], (y + weight02) * m_gridSize);
            Vector3 v09 = new Vector3((x + 1) * m_gridSize, weightCode[0], (y + weight13) * m_gridSize);
            Vector3 v10 = new Vector3(x * m_gridSize, weightCode[2], (y + weight02) * m_gridSize);
            Vector3 v11 = new Vector3((x + 1) * m_gridSize, weightCode[2], (y + weight13) * m_gridSize);
            vertices.Add(v08);
            vertices.Add(v09);
            vertices.Add(v10);
            vertices.Add(v11);
            uvs.Add(new Vector2(0.5f, 0));
            uvs.Add(new Vector2(0, 0.5f));
            uvs.Add(new Vector2(0, 1f));
            uvs.Add(new Vector2(1f, 1f));
            triangles.Add(vertexCount + 0);
            triangles.Add(vertexCount + 2);
            triangles.Add(vertexCount + 1);

            triangles.Add(vertexCount + 1);
            triangles.Add(vertexCount + 2);
            triangles.Add(vertexCount + 3);
            vertexCount += 4;
        }

        /*1010 0101*/
        else if (weightCode[0] == weightCode[2] && weightCode[1] == weightCode[3] && weightCode[0] != weightCode[1])
        {
            Vector3 v00 = new Vector3(x * m_gridSize, weightCode[0], y * m_gridSize);
            Vector3 v01 = new Vector3((x + weight01) * m_gridSize, weightCode[0], y * m_gridSize);
            Vector3 v02 = new Vector3(x * m_gridSize, weightCode[0], (y + 1) * m_gridSize);
            Vector3 v03 = new Vector3((x + weight23) * m_gridSize, weightCode[0], (y + 1) * m_gridSize);
            vertices.Add(v00);
            vertices.Add(v01);
            vertices.Add(v02);
            vertices.Add(v03);
            uvs.Add(new Vector2(0, 0));
            uvs.Add(new Vector2(0.5f, 0));
            uvs.Add(new Vector2(0, 1));
            uvs.Add(new Vector2(0.5f, 1));
            triangles.Add(vertexCount + 0);
            triangles.Add(vertexCount + 2);
            triangles.Add(vertexCount + 1);

            triangles.Add(vertexCount + 1);
            triangles.Add(vertexCount + 2);
            triangles.Add(vertexCount + 3);


            Vector3 v04 = new Vector3((x + weight01) * m_gridSize, weightCode[1], y * m_gridSize);
            Vector3 v05 = new Vector3((x + 1) * m_gridSize, weightCode[1], y * m_gridSize);
            Vector3 v06 = new Vector3((x + weight23) * m_gridSize, weightCode[1], (y + 1) * m_gridSize);
            Vector3 v07 = new Vector3((x + 1) * m_gridSize, weightCode[1], (y + 1) * m_gridSize);
            vertices.Add(v04);
            vertices.Add(v05);
            vertices.Add(v06);
            vertices.Add(v07);
            uvs.Add(new Vector2(0.5f, 0));
            uvs.Add(new Vector2(1, 0));
            uvs.Add(new Vector2(0.5f, 1));
            uvs.Add(new Vector2(1, 1));

            triangles.Add(vertexCount + 4);
            triangles.Add(vertexCount + 6);
            triangles.Add(vertexCount + 5);

            triangles.Add(vertexCount + 5);
            triangles.Add(vertexCount + 6);
            triangles.Add(vertexCount + 7);

            vertexCount += 8;

            // cliff mesh
            Vector3 v08 = new Vector3((x + weight01) * m_gridSize, weightCode[0], y * m_gridSize);
            Vector3 v09 = new Vector3((x + weight23) * m_gridSize, weightCode[0], (y + 1) * m_gridSize);
            Vector3 v10 = new Vector3((x + weight01) * m_gridSize, weightCode[1], y * m_gridSize);
            Vector3 v11 = new Vector3((x + weight23) * m_gridSize, weightCode[1], (y + 1) * m_gridSize);
            vertices.Add(v08);
            vertices.Add(v09);
            vertices.Add(v10);
            vertices.Add(v11);
            uvs.Add(new Vector2(0.5f, 0));
            uvs.Add(new Vector2(0, 0.5f));
            uvs.Add(new Vector2(0, 1f));
            uvs.Add(new Vector2(1f, 1f));
            triangles.Add(vertexCount + 0);
            triangles.Add(vertexCount + 1);
            triangles.Add(vertexCount + 2);

            triangles.Add(vertexCount + 2);
            triangles.Add(vertexCount + 1);
            triangles.Add(vertexCount + 3);
            vertexCount += 4;
        }

        /*1001*/
        else if (weightCode[0] == weightCode[3] && weightCode[3] != weightCode[1] && weightCode[0] != weightCode[1])
        {
            Vector3 v00 = new Vector3(x * m_gridSize, weightCode[0], y * m_gridSize);
            Vector3 v01 = new Vector3((x + weight01) * m_gridSize, weightCode[0], y * m_gridSize);
            Vector3 v02 = new Vector3((x + 1) * m_gridSize, weightCode[0], (y + weight13) * m_gridSize);
            Vector3 v03 = new Vector3((x + 1) * m_gridSize, weightCode[0], (y + 1) * m_gridSize);
            Vector3 v04 = new Vector3((x + weight23) * m_gridSize, weightCode[0], (y + 1) * m_gridSize);
            Vector3 v05 = new Vector3(x * m_gridSize, weightCode[0], (y + weight02) * m_gridSize);
            vertices.Add(v00);
            vertices.Add(v01);
            vertices.Add(v02);
            vertices.Add(v03);
            vertices.Add(v04);
            vertices.Add(v05);
            uvs.Add(new Vector2(0, 0));
            uvs.Add(new Vector2(0.5f, 0));
            uvs.Add(new Vector2(1, 0.5f));
            uvs.Add(new Vector2(1, 1));
            uvs.Add(new Vector2(0.5f, 1));
            uvs.Add(new Vector2(0, 0.5f));
            triangles.Add(vertexCount + 0);
            triangles.Add(vertexCount + 2);
            triangles.Add(vertexCount + 1);

            triangles.Add(vertexCount + 0);
            triangles.Add(vertexCount + 3);
            triangles.Add(vertexCount + 2);

            triangles.Add(vertexCount + 0);
            triangles.Add(vertexCount + 4);
            triangles.Add(vertexCount + 3);

            triangles.Add(vertexCount + 0);
            triangles.Add(vertexCount + 5);
            triangles.Add(vertexCount + 4);
            vertexCount += 6;

            Vector3 v06 = new Vector3((x + weight01) * m_gridSize, weightCode[1], y * m_gridSize);
            Vector3 v07 = new Vector3((x + 1) * m_gridSize, weightCode[1], (y + weight13) * m_gridSize);
            Vector3 v08 = new Vector3((x + 1) * m_gridSize, weightCode[1], y * m_gridSize);
            vertices.Add(v06);
            vertices.Add(v07);
            vertices.Add(v08);
            uvs.Add(new Vector2(0.5f, 0));
            uvs.Add(new Vector2(1, 0.5f));
            uvs.Add(new Vector2(0, 1));
            triangles.Add(vertexCount + 0);
            triangles.Add(vertexCount + 1);
            triangles.Add(vertexCount + 2);
            vertexCount += 3;

            Vector3 v09 = new Vector3(x * m_gridSize, weightCode[2], (y + weight02) * m_gridSize);
            Vector3 v10 = new Vector3(x * m_gridSize, weightCode[2], (y + 1) * m_gridSize);
            Vector3 v11 = new Vector3((x + weight23) * m_gridSize, weightCode[2], (y + 1) * m_gridSize);
            vertices.Add(v09);
            vertices.Add(v10);
            vertices.Add(v11);
            uvs.Add(new Vector2(0, 0.5f));
            uvs.Add(new Vector2(0, 1));
            uvs.Add(new Vector2(0.5f, 1));
            triangles.Add(vertexCount + 0);
            triangles.Add(vertexCount + 1);
            triangles.Add(vertexCount + 2);
            vertexCount += 3;

            // cliff
            Vector3 v12 = new Vector3((x + weight01) * m_gridSize, weightCode[1], y * m_gridSize);
            Vector3 v13 = new Vector3((x + 1) * m_gridSize, weightCode[1], (y + weight13) * m_gridSize);
            Vector3 v14 = new Vector3((x + weight01) * m_gridSize, weightCode[0], y * m_gridSize);
            Vector3 v15 = new Vector3((x + 1) * m_gridSize, weightCode[0], (y + weight13) * m_gridSize);
            vertices.Add(v12);
            vertices.Add(v13);
            vertices.Add(v14);
            vertices.Add(v15);
            uvs.Add(new Vector2(0, 0.5f));
            uvs.Add(new Vector2(1, 0.5f));
            uvs.Add(new Vector2(0.5f, 1));
            uvs.Add(new Vector2(1, 1));
            triangles.Add(vertexCount + 0);
            triangles.Add(vertexCount + 2);
            triangles.Add(vertexCount + 1);

            triangles.Add(vertexCount + 1);
            triangles.Add(vertexCount + 2);
            triangles.Add(vertexCount + 3);
            vertexCount += 4;

            Vector3 v16 = new Vector3(x * m_gridSize, weightCode[2], (y + weight02) * m_gridSize);
            Vector3 v17 = new Vector3((x + weight23) * m_gridSize, weightCode[2], (y + 1) * m_gridSize);
            Vector3 v18 = new Vector3(x * m_gridSize, weightCode[0], (y + weight02) * m_gridSize);
            Vector3 v19 = new Vector3((x + weight23) * m_gridSize, weightCode[0], (y + 1) * m_gridSize);
            vertices.Add(v16);
            vertices.Add(v17);
            vertices.Add(v18);
            vertices.Add(v19);
            uvs.Add(new Vector2(0, 0.5f));
            uvs.Add(new Vector2(1, 0.5f));
            uvs.Add(new Vector2(0.5f, 1));
            uvs.Add(new Vector2(1, 1));
            triangles.Add(vertexCount + 0);
            triangles.Add(vertexCount + 1);
            triangles.Add(vertexCount + 2);

            triangles.Add(vertexCount + 1);
            triangles.Add(vertexCount + 3);
            triangles.Add(vertexCount + 2);
            vertexCount += 4;
        }

        /*0110*/
        else if (weightCode[1] == weightCode[2] && weightCode[3] != weightCode[1] && weightCode[0] != weightCode[1])
        {
            Vector3 v00 = new Vector3((x + weight01) * m_gridSize, weightCode[1], y * m_gridSize);
            Vector3 v01 = new Vector3((x + 1) * m_gridSize, weightCode[1], y * m_gridSize);
            Vector3 v02 = new Vector3((x + 1) * m_gridSize, weightCode[1], (y + weight13) * m_gridSize);
            Vector3 v03 = new Vector3((x + weight23) * m_gridSize, weightCode[1], (y + 1) * m_gridSize);
            Vector3 v04 = new Vector3(x * m_gridSize, weightCode[1], (y + 1) * m_gridSize);
            Vector3 v05 = new Vector3(x * m_gridSize, weightCode[1], (y + weight02) * m_gridSize);
            vertices.Add(v00);
            vertices.Add(v01);
            vertices.Add(v02);
            vertices.Add(v03);
            vertices.Add(v04);
            vertices.Add(v05);
            uvs.Add(new Vector2(0, 0));
            uvs.Add(new Vector2(0.5f, 0));
            uvs.Add(new Vector2(1, 0.5f));
            uvs.Add(new Vector2(1, 1));
            uvs.Add(new Vector2(0.5f, 1));
            uvs.Add(new Vector2(0, 0.5f));
            triangles.Add(vertexCount + 1);
            triangles.Add(vertexCount + 3);
            triangles.Add(vertexCount + 2);

            triangles.Add(vertexCount + 1);
            triangles.Add(vertexCount + 4);
            triangles.Add(vertexCount + 3);

            triangles.Add(vertexCount + 1);
            triangles.Add(vertexCount + 5);
            triangles.Add(vertexCount + 4);

            triangles.Add(vertexCount + 0);
            triangles.Add(vertexCount + 5);
            triangles.Add(vertexCount + 1);
            vertexCount += 6;

            Vector3 v06 = new Vector3(x * m_gridSize, weightCode[0], y * m_gridSize);
            Vector3 v07 = new Vector3((x + weight01) * m_gridSize, weightCode[0], y * m_gridSize);
            Vector3 v08 = new Vector3(x * m_gridSize, weightCode[0], (y + weight02) * m_gridSize);
            vertices.Add(v06);
            vertices.Add(v07);
            vertices.Add(v08);
            uvs.Add(new Vector2(0, 0));
            uvs.Add(new Vector2(0.5f, 0));
            uvs.Add(new Vector2(0, 0.5f));
            triangles.Add(vertexCount + 0);
            triangles.Add(vertexCount + 2);
            triangles.Add(vertexCount + 1);
            vertexCount += 3;

            Vector3 v09 = new Vector3((x + 1) * m_gridSize, weightCode[3], (y + weight13) * m_gridSize);
            Vector3 v10 = new Vector3((x + 1) * m_gridSize, weightCode[3], (y + 1) * m_gridSize);
            Vector3 v11 = new Vector3((x + weight23) * m_gridSize, weightCode[3], (y + 1) * m_gridSize);
            vertices.Add(v09);
            vertices.Add(v10);
            vertices.Add(v11);
            uvs.Add(new Vector2(1, 0.5f));
            uvs.Add(new Vector2(1, 1));
            uvs.Add(new Vector2(0.5f, 1));
            triangles.Add(vertexCount + 0);
            triangles.Add(vertexCount + 2);
            triangles.Add(vertexCount + 1);
            vertexCount += 3;

            // cliff
            Vector3 v12 = new Vector3((x + weight01) * m_gridSize, weightCode[1], y * m_gridSize);
            Vector3 v13 = new Vector3(x * m_gridSize, weightCode[1], (y + weight02) * m_gridSize);
            Vector3 v14 = new Vector3((x + weight01) * m_gridSize, weightCode[0], y * m_gridSize);
            Vector3 v15 = new Vector3(x * m_gridSize, weightCode[0], (y + weight02) * m_gridSize);
            vertices.Add(v12);
            vertices.Add(v13);
            vertices.Add(v14);
            vertices.Add(v15);
            uvs.Add(new Vector2(0, 0.5f));
            uvs.Add(new Vector2(1, 0.5f));
            uvs.Add(new Vector2(0.5f, 1));
            uvs.Add(new Vector2(1, 1));
            triangles.Add(vertexCount + 0);
            triangles.Add(vertexCount + 2);
            triangles.Add(vertexCount + 1);

            triangles.Add(vertexCount + 1);
            triangles.Add(vertexCount + 2);
            triangles.Add(vertexCount + 3);
            vertexCount += 4;

            Vector3 v16 = new Vector3((x + 1) * m_gridSize, weightCode[1], (y + weight13) * m_gridSize);
            Vector3 v17 = new Vector3((x + weight23) * m_gridSize, weightCode[1], (y + 1) * m_gridSize);
            Vector3 v18 = new Vector3((x + 1) * m_gridSize, weightCode[3], (y + weight13) * m_gridSize);
            Vector3 v19 = new Vector3((x + weight23) * m_gridSize, weightCode[3], (y + 1) * m_gridSize);
            vertices.Add(v16);
            vertices.Add(v17);
            vertices.Add(v18);
            vertices.Add(v19);
            uvs.Add(new Vector2(0, 0.5f));
            uvs.Add(new Vector2(1, 0.5f));
            uvs.Add(new Vector2(0.5f, 1));
            uvs.Add(new Vector2(1, 1));
            triangles.Add(vertexCount + 0);
            triangles.Add(vertexCount + 1);
            triangles.Add(vertexCount + 2);

            triangles.Add(vertexCount + 1);
            triangles.Add(vertexCount + 3);
            triangles.Add(vertexCount + 2);
            vertexCount += 4;
        }


        /*110x */
        else if (weightCode[0] == weightCode[1] && weightCode[0] != weightCode[2] && weightCode[0] != weightCode[3] && weightCode[2] != weightCode[3])
        {
            List<float> weightArray = new List<float> { weightCode[0], weightCode[2], weightCode[3] };
            weightArray.Sort();
            float lowest = weightArray[0];
            Vector3 v00 = new Vector3(x * m_gridSize, lowest, y * m_gridSize);
            Vector3 v01 = new Vector3((x + 1) * m_gridSize, lowest, y * m_gridSize);
            Vector3 v02 = new Vector3(x * m_gridSize, lowest, (y + 1) * m_gridSize);
            Vector3 v03 = new Vector3((x + 1) * m_gridSize, lowest, (y + 1) * m_gridSize);
            vertices.Add(v00);
            vertices.Add(v01);
            vertices.Add(v02);
            vertices.Add(v03);
            uvs.Add(new Vector2(0, 0));
            uvs.Add(new Vector2(0.5f, 0));
            uvs.Add(new Vector2(0, 1));
            uvs.Add(new Vector2(0.5f, 1));
            triangles.Add(vertexCount + 0);
            triangles.Add(vertexCount + 2);
            triangles.Add(vertexCount + 1);

            triangles.Add(vertexCount + 1);
            triangles.Add(vertexCount + 2);
            triangles.Add(vertexCount + 3);
            vertexCount += 4;
            // 0 lowest, 3 highest
            if (weightCode[0] < weightCode[2] && weightCode[2] < weightCode[3])
            {
                Vector3 v04 = new Vector3((x + weight23) * m_gridSize, weightCode[3], (y + 1) * m_gridSize);
                Vector3 v05 = new Vector3((x + 1) * m_gridSize, weightCode[3], (y + 1) * m_gridSize);
                Vector3 v06 = new Vector3((x + 1) * m_gridSize, weightCode[3], (y + weight13) * m_gridSize);
                vertices.Add(v04);
                vertices.Add(v05);
                vertices.Add(v06);
                uvs.Add(new Vector2(0, 0));
                uvs.Add(new Vector2(0.5f, 0));
                uvs.Add(new Vector2(0, 1));
                triangles.Add(vertexCount + 0);
                triangles.Add(vertexCount + 1);
                triangles.Add(vertexCount + 2);
                vertexCount += 3;


                Vector3 v07 = new Vector3(x * m_gridSize, weightCode[2], (y + weight02) * m_gridSize);
                Vector3 v08 = new Vector3((x + 1) * m_gridSize, weightCode[2], (y + weight13) * m_gridSize);
                Vector3 v09 = new Vector3(x * m_gridSize, weightCode[2], (y + 1) * m_gridSize);
                Vector3 v10 = new Vector3((x + 1) * m_gridSize, weightCode[2], (y + 1) * m_gridSize);
                vertices.Add(v07);
                vertices.Add(v08);
                vertices.Add(v09);
                vertices.Add(v10);
                uvs.Add(new Vector2(0, 0.5f));
                uvs.Add(new Vector2(1, 0.5f));
                uvs.Add(new Vector2(0.5f, 1));
                uvs.Add(new Vector2(1, 1));
                triangles.Add(vertexCount + 0);
                triangles.Add(vertexCount + 2);
                triangles.Add(vertexCount + 1);

                triangles.Add(vertexCount + 1);
                triangles.Add(vertexCount + 2);
                triangles.Add(vertexCount + 3);
                vertexCount += 4;

                // cliff
                Vector3 v11 = new Vector3(x * m_gridSize, weightCode[2], (y + weight02) * m_gridSize);
                Vector3 v12 = new Vector3((x + 1) * m_gridSize, weightCode[2], (y + weight13) * m_gridSize);
                Vector3 v13 = new Vector3(x * m_gridSize, weightCode[0], (y + weight02) * m_gridSize);
                Vector3 v14 = new Vector3((x + 1) * m_gridSize, weightCode[0], (y + weight13) * m_gridSize);
                vertices.Add(v11);
                vertices.Add(v12);
                vertices.Add(v13);
                vertices.Add(v14);
                uvs.Add(new Vector2(0, 0.5f));
                uvs.Add(new Vector2(1, 0.5f));
                uvs.Add(new Vector2(0.5f, 1));
                uvs.Add(new Vector2(1, 1));
                triangles.Add(vertexCount + 0);
                triangles.Add(vertexCount + 1);
                triangles.Add(vertexCount + 2);

                triangles.Add(vertexCount + 1);
                triangles.Add(vertexCount + 3);
                triangles.Add(vertexCount + 2);
                vertexCount += 4;

                Vector3 v15 = new Vector3((x + weight23) * m_gridSize, weightCode[3], (y + 1) * m_gridSize);
                Vector3 v16 = new Vector3((x + 1) * m_gridSize, weightCode[3], (y + weight13) * m_gridSize);
                Vector3 v17 = new Vector3((x + weight23) * m_gridSize, weightCode[2], (y + 1) * m_gridSize);
                Vector3 v18 = new Vector3((x + 1) * m_gridSize, weightCode[2], (y + weight13) * m_gridSize);
                vertices.Add(v15);
                vertices.Add(v16);
                vertices.Add(v17);
                vertices.Add(v18);
                uvs.Add(new Vector2(0, 0.5f));
                uvs.Add(new Vector2(1, 0.5f));
                uvs.Add(new Vector2(0.5f, 1));
                uvs.Add(new Vector2(1, 1));
                triangles.Add(vertexCount + 0);
                triangles.Add(vertexCount + 1);
                triangles.Add(vertexCount + 2);

                triangles.Add(vertexCount + 1);
                triangles.Add(vertexCount + 3);
                triangles.Add(vertexCount + 2);
                vertexCount += 4;

            }
            // 0 lowest, 2 highest
            else if (weightCode[0] < weightCode[3] && weightCode[3] < weightCode[2])
            {
                Vector3 v04 = new Vector3(x * m_gridSize, weightCode[2], (y + weight02) * m_gridSize);
                Vector3 v05 = new Vector3(x * m_gridSize, weightCode[2], (y + 1) * m_gridSize);
                Vector3 v06 = new Vector3((x + weight23) * m_gridSize, weightCode[2], (y + 1) * m_gridSize);
                vertices.Add(v04);
                vertices.Add(v05);
                vertices.Add(v06);
                uvs.Add(new Vector2(0, 0));
                uvs.Add(new Vector2(0.5f, 0));
                uvs.Add(new Vector2(0, 1));
                triangles.Add(vertexCount + 0);
                triangles.Add(vertexCount + 1);
                triangles.Add(vertexCount + 2);
                vertexCount += 3;

                Vector3 v07 = new Vector3(x * m_gridSize, weightCode[3], (y + weight02) * m_gridSize);
                Vector3 v08 = new Vector3((x + 1) * m_gridSize, weightCode[3], (y + weight13) * m_gridSize);
                Vector3 v09 = new Vector3(x * m_gridSize, weightCode[3], (y + 1) * m_gridSize);
                Vector3 v10 = new Vector3((x + 1) * m_gridSize, weightCode[3], (y + 1) * m_gridSize);
                vertices.Add(v07);
                vertices.Add(v08);
                vertices.Add(v09);
                vertices.Add(v10);
                uvs.Add(new Vector2(0, 0.5f));
                uvs.Add(new Vector2(1, 0.5f));
                uvs.Add(new Vector2(0.5f, 1));
                uvs.Add(new Vector2(1, 1));
                triangles.Add(vertexCount + 0);
                triangles.Add(vertexCount + 2);
                triangles.Add(vertexCount + 1);

                triangles.Add(vertexCount + 1);
                triangles.Add(vertexCount + 2);
                triangles.Add(vertexCount + 3);
                vertexCount += 4;
                // cliff
                Vector3 v11 = new Vector3(x * m_gridSize, weightCode[3], (y + weight02) * m_gridSize);
                Vector3 v12 = new Vector3((x + 1) * m_gridSize, weightCode[3], (y + weight13) * m_gridSize);
                Vector3 v13 = new Vector3(x * m_gridSize, weightCode[0], (y + weight02) * m_gridSize);
                Vector3 v14 = new Vector3((x + 1) * m_gridSize, weightCode[0], (y + weight13) * m_gridSize);
                vertices.Add(v11);
                vertices.Add(v12);
                vertices.Add(v13);
                vertices.Add(v14);
                uvs.Add(new Vector2(0, 0.5f));
                uvs.Add(new Vector2(1, 0.5f));
                uvs.Add(new Vector2(0.5f, 1));
                uvs.Add(new Vector2(1, 1));
                triangles.Add(vertexCount + 0);
                triangles.Add(vertexCount + 1);
                triangles.Add(vertexCount + 2);

                triangles.Add(vertexCount + 2);
                triangles.Add(vertexCount + 1);
                triangles.Add(vertexCount + 3);
                vertexCount += 4;

                Vector3 v15 = new Vector3(x * m_gridSize, weightCode[2], (y + weight02) * m_gridSize);
                Vector3 v16 = new Vector3((x + weight23) * m_gridSize, weightCode[2], (y + 1) * m_gridSize);
                Vector3 v17 = new Vector3(x * m_gridSize, weightCode[3], (y + weight02) * m_gridSize);
                Vector3 v18 = new Vector3((x + weight23) * m_gridSize, weightCode[3], (y + 1) * m_gridSize);
                vertices.Add(v15);
                vertices.Add(v16);
                vertices.Add(v17);
                vertices.Add(v18);
                uvs.Add(new Vector2(0, 0.5f));
                uvs.Add(new Vector2(1, 0.5f));
                uvs.Add(new Vector2(0.5f, 1));
                uvs.Add(new Vector2(1, 1));
                triangles.Add(vertexCount + 0);
                triangles.Add(vertexCount + 1);
                triangles.Add(vertexCount + 2);

                triangles.Add(vertexCount + 1);
                triangles.Add(vertexCount + 3);
                triangles.Add(vertexCount + 2);
                vertexCount += 4;
            }

        }

        /*0x11 */
        else if (weightCode[2] == weightCode[3] && weightCode[2] != weightCode[0] && weightCode[2] != weightCode[1] && weightCode[0] != weightCode[1])
        {
            List<float> weightArray = new List<float> { weightCode[0], weightCode[1], weightCode[2] };
            weightArray.Sort();
            float lowest = weightArray[0];
            Vector3 v00 = new Vector3(x * m_gridSize, lowest, y * m_gridSize);
            Vector3 v01 = new Vector3((x + 1) * m_gridSize, lowest, y * m_gridSize);
            Vector3 v02 = new Vector3(x * m_gridSize, lowest, (y + 1) * m_gridSize);
            Vector3 v03 = new Vector3((x + 1) * m_gridSize, lowest, (y + 1) * m_gridSize);
            vertices.Add(v00);
            vertices.Add(v01);
            vertices.Add(v02);
            vertices.Add(v03);
            uvs.Add(new Vector2(0, 0));
            uvs.Add(new Vector2(0.5f, 0));
            uvs.Add(new Vector2(0, 1));
            uvs.Add(new Vector2(0.5f, 1));
            triangles.Add(vertexCount + 0);
            triangles.Add(vertexCount + 2);
            triangles.Add(vertexCount + 1);

            triangles.Add(vertexCount + 1);
            triangles.Add(vertexCount + 2);
            triangles.Add(vertexCount + 3);
            vertexCount += 4;
            //2 lowest, 1 is highest
            if (weightCode[2] < weightCode[0] && weightCode[0] < weightCode[1])
            {
                Vector3 v04 = new Vector3((x + weight01) * m_gridSize, weightCode[1], y * m_gridSize);
                Vector3 v05 = new Vector3((x + 1) * m_gridSize, weightCode[1], (y + weight13) * m_gridSize);
                Vector3 v06 = new Vector3((x + 1) * m_gridSize, weightCode[1], y * m_gridSize);
                vertices.Add(v04);
                vertices.Add(v05);
                vertices.Add(v06);
                uvs.Add(new Vector2(0, 0));
                uvs.Add(new Vector2(0.5f, 0));
                uvs.Add(new Vector2(0, 1));
                triangles.Add(vertexCount + 0);
                triangles.Add(vertexCount + 1);
                triangles.Add(vertexCount + 2);
                vertexCount += 3;


                Vector3 v07 = new Vector3(x * m_gridSize, weightCode[0], y * m_gridSize);
                Vector3 v08 = new Vector3((x + 1) * m_gridSize, weightCode[0], y * m_gridSize);
                Vector3 v09 = new Vector3(x * m_gridSize, weightCode[0], (y + weight02) * m_gridSize);
                Vector3 v10 = new Vector3((x + 1) * m_gridSize, weightCode[0], (y + weight13) * m_gridSize);
                vertices.Add(v07);
                vertices.Add(v08);
                vertices.Add(v09);
                vertices.Add(v10);
                uvs.Add(new Vector2(0, 0.5f));
                uvs.Add(new Vector2(1, 0.5f));
                uvs.Add(new Vector2(0.5f, 1));
                uvs.Add(new Vector2(1, 1));
                triangles.Add(vertexCount + 0);
                triangles.Add(vertexCount + 2);
                triangles.Add(vertexCount + 1);

                triangles.Add(vertexCount + 1);
                triangles.Add(vertexCount + 2);
                triangles.Add(vertexCount + 3);
                vertexCount += 4;

                // cliff
                Vector3 v11 = new Vector3((x + weight01) * m_gridSize, weightCode[1], y * m_gridSize);
                Vector3 v12 = new Vector3((x + 1) * m_gridSize, weightCode[1], (y + weight13) * m_gridSize);
                Vector3 v13 = new Vector3((x + weight01) * m_gridSize, weightCode[0], y * m_gridSize);
                Vector3 v14 = new Vector3((x + 1) * m_gridSize, weightCode[0], (y + weight13) * m_gridSize);
                vertices.Add(v11);
                vertices.Add(v12);
                vertices.Add(v13);
                vertices.Add(v14);
                uvs.Add(new Vector2(0, 0.5f));
                uvs.Add(new Vector2(1, 0.5f));
                uvs.Add(new Vector2(0.5f, 1));
                uvs.Add(new Vector2(1, 1));
                triangles.Add(vertexCount + 0);
                triangles.Add(vertexCount + 2);
                triangles.Add(vertexCount + 1);

                triangles.Add(vertexCount + 1);
                triangles.Add(vertexCount + 2);
                triangles.Add(vertexCount + 3);
                vertexCount += 4;

                Vector3 v15 = new Vector3(x * m_gridSize, weightCode[0], (y + weight02) * m_gridSize);
                Vector3 v16 = new Vector3((x + 1) * m_gridSize, weightCode[0], (y + weight13) * m_gridSize);
                Vector3 v17 = new Vector3(x * m_gridSize, weightCode[2], (y + weight02) * m_gridSize);
                Vector3 v18 = new Vector3((x + 1) * m_gridSize, weightCode[2], (y + weight13) * m_gridSize);
                vertices.Add(v15);
                vertices.Add(v16);
                vertices.Add(v17);
                vertices.Add(v18);
                uvs.Add(new Vector2(0, 0.5f));
                uvs.Add(new Vector2(1, 0.5f));
                uvs.Add(new Vector2(0.5f, 1));
                uvs.Add(new Vector2(1, 1));
                triangles.Add(vertexCount + 0);
                triangles.Add(vertexCount + 2);
                triangles.Add(vertexCount + 1);

                triangles.Add(vertexCount + 1);
                triangles.Add(vertexCount + 2);
                triangles.Add(vertexCount + 3);
                vertexCount += 4;
            }
            // 2 lowest, 0 is highest
            else if (weightCode[2] < weightCode[1] && weightCode[1] < weightCode[0])
            {
                Vector3 v04 = new Vector3(x * m_gridSize, weightCode[0], y * m_gridSize);
                Vector3 v05 = new Vector3(x * m_gridSize, weightCode[0], (y + weight02) * m_gridSize);
                Vector3 v06 = new Vector3((x + weight01) * m_gridSize, weightCode[0], y * m_gridSize);
                vertices.Add(v04);
                vertices.Add(v05);
                vertices.Add(v06);
                uvs.Add(new Vector2(0, 0));
                uvs.Add(new Vector2(0.5f, 0));
                uvs.Add(new Vector2(0, 1));
                triangles.Add(vertexCount + 0);
                triangles.Add(vertexCount + 1);
                triangles.Add(vertexCount + 2);
                vertexCount += 3;


                Vector3 v07 = new Vector3(x * m_gridSize, weightCode[1], y * m_gridSize);
                Vector3 v08 = new Vector3((x + 1) * m_gridSize, weightCode[1], y * m_gridSize);
                Vector3 v09 = new Vector3(x * m_gridSize, weightCode[1], (y + weight02) * m_gridSize);
                Vector3 v10 = new Vector3((x + 1) * m_gridSize, weightCode[1], (y + weight13) * m_gridSize);
                vertices.Add(v07);
                vertices.Add(v08);
                vertices.Add(v09);
                vertices.Add(v10);
                uvs.Add(new Vector2(0, 0.5f));
                uvs.Add(new Vector2(1, 0.5f));
                uvs.Add(new Vector2(0.5f, 1));
                uvs.Add(new Vector2(1, 1));
                triangles.Add(vertexCount + 0);
                triangles.Add(vertexCount + 2);
                triangles.Add(vertexCount + 1);

                triangles.Add(vertexCount + 1);
                triangles.Add(vertexCount + 2);
                triangles.Add(vertexCount + 3);
                vertexCount += 4;

                // cliff
                Vector3 v11 = new Vector3(x * m_gridSize, weightCode[0], (y + weight02) * m_gridSize);
                Vector3 v12 = new Vector3((x + weight01) * m_gridSize, weightCode[0], y * m_gridSize);
                Vector3 v13 = new Vector3(x * m_gridSize, weightCode[2], (y + weight02) * m_gridSize);
                Vector3 v14 = new Vector3((x + weight01) * m_gridSize, weightCode[2], y * m_gridSize);
                vertices.Add(v11);
                vertices.Add(v12);
                vertices.Add(v13);
                vertices.Add(v14);
                uvs.Add(new Vector2(0, 0.5f));
                uvs.Add(new Vector2(1, 0.5f));
                uvs.Add(new Vector2(0.5f, 1));
                uvs.Add(new Vector2(1, 1));
                triangles.Add(vertexCount + 0);
                triangles.Add(vertexCount + 2);
                triangles.Add(vertexCount + 1);

                triangles.Add(vertexCount + 1);
                triangles.Add(vertexCount + 2);
                triangles.Add(vertexCount + 3);
                vertexCount += 4;

                Vector3 v15 = new Vector3(x * m_gridSize, weightCode[1], (y + weight02) * m_gridSize);
                Vector3 v16 = new Vector3((x + 1) * m_gridSize, weightCode[1], (y + weight13) * m_gridSize);
                Vector3 v17 = new Vector3(x * m_gridSize, weightCode[2], (y + weight02) * m_gridSize);
                Vector3 v18 = new Vector3((x + 1) * m_gridSize, weightCode[2], (y + weight13) * m_gridSize);
                vertices.Add(v15);
                vertices.Add(v16);
                vertices.Add(v17);
                vertices.Add(v18);
                uvs.Add(new Vector2(0, 0.5f));
                uvs.Add(new Vector2(1, 0.5f));
                uvs.Add(new Vector2(0.5f, 1));
                uvs.Add(new Vector2(1, 1));
                triangles.Add(vertexCount + 0);
                triangles.Add(vertexCount + 2);
                triangles.Add(vertexCount + 1);

                triangles.Add(vertexCount + 1);
                triangles.Add(vertexCount + 2);
                triangles.Add(vertexCount + 3);
                vertexCount += 4;
            }
            // 0 lowest, 2 is highest
            else if (weightCode[0] < weightCode[1] && weightCode[1] < weightCode[2])
            {
                Vector3 v04 = new Vector3((x + weight01) * m_gridSize, weightCode[1], y * m_gridSize);
                Vector3 v05 = new Vector3((x + 1) * m_gridSize, weightCode[1], y * m_gridSize);
                Vector3 v06 = new Vector3(x * m_gridSize, weightCode[1], (y + weight02) * m_gridSize);
                Vector3 v07 = new Vector3((x + 1) * m_gridSize, weightCode[1], (y + weight13) * m_gridSize);
                vertices.Add(v04);
                vertices.Add(v05);
                vertices.Add(v06);
                vertices.Add(v07);
                uvs.Add(new Vector2(0, 0));
                uvs.Add(new Vector2(0.5f, 0));
                uvs.Add(new Vector2(0, 1));
                uvs.Add(new Vector2(0, 1));
                triangles.Add(vertexCount + 0);
                triangles.Add(vertexCount + 2);
                triangles.Add(vertexCount + 1);

                triangles.Add(vertexCount + 1);
                triangles.Add(vertexCount + 2);
                triangles.Add(vertexCount + 3);
                vertexCount += 4;


                Vector3 v08 = new Vector3(x * m_gridSize, weightCode[2], (y + 1) * m_gridSize);
                Vector3 v09 = new Vector3((x + 1) * m_gridSize, weightCode[2], (y + 1) * m_gridSize);
                Vector3 v10 = new Vector3(x * m_gridSize, weightCode[2], (y + weight02) * m_gridSize);
                Vector3 v11 = new Vector3((x + 1) * m_gridSize, weightCode[2], (y + weight13) * m_gridSize);
                vertices.Add(v08);
                vertices.Add(v09);
                vertices.Add(v10);
                vertices.Add(v11);
                uvs.Add(new Vector2(0, 0.5f));
                uvs.Add(new Vector2(1, 0.5f));
                uvs.Add(new Vector2(0.5f, 1));
                uvs.Add(new Vector2(1, 1));
                triangles.Add(vertexCount + 0);
                triangles.Add(vertexCount + 1);
                triangles.Add(vertexCount + 2);

                triangles.Add(vertexCount + 1);
                triangles.Add(vertexCount + 3);
                triangles.Add(vertexCount + 2);
                vertexCount += 4;

                // cliff
                Vector3 v12 = new Vector3((x + weight01) * m_gridSize, weightCode[1], y * m_gridSize);
                Vector3 v13 = new Vector3(x * m_gridSize, weightCode[1], (y + weight02) * m_gridSize);
                Vector3 v14 = new Vector3((x + weight01) * m_gridSize, weightCode[0], y * m_gridSize);
                Vector3 v15 = new Vector3(x * m_gridSize, weightCode[0], (y + weight02) * m_gridSize);
                vertices.Add(v12);
                vertices.Add(v13);
                vertices.Add(v14);
                vertices.Add(v15);
                uvs.Add(new Vector2(0, 0.5f));
                uvs.Add(new Vector2(1, 0.5f));
                uvs.Add(new Vector2(0.5f, 1));
                uvs.Add(new Vector2(1, 1));
                triangles.Add(vertexCount + 0);
                triangles.Add(vertexCount + 2);
                triangles.Add(vertexCount + 1);

                triangles.Add(vertexCount + 1);
                triangles.Add(vertexCount + 2);
                triangles.Add(vertexCount + 3);
                vertexCount += 4;

                Vector3 v16 = new Vector3(x * m_gridSize, weightCode[1], (y + weight02) * m_gridSize);
                Vector3 v17 = new Vector3((x + 1) * m_gridSize, weightCode[1], (y + weight13) * m_gridSize);
                Vector3 v18 = new Vector3(x * m_gridSize, weightCode[2], (y + weight02) * m_gridSize);
                Vector3 v19 = new Vector3((x + 1) * m_gridSize, weightCode[2], (y + weight13) * m_gridSize);
                vertices.Add(v16);
                vertices.Add(v17);
                vertices.Add(v18);
                vertices.Add(v19);
                uvs.Add(new Vector2(0, 0.5f));
                uvs.Add(new Vector2(1, 0.5f));
                uvs.Add(new Vector2(0.5f, 1));
                uvs.Add(new Vector2(1, 1));
                triangles.Add(vertexCount + 0);
                triangles.Add(vertexCount + 2);
                triangles.Add(vertexCount + 1);

                triangles.Add(vertexCount + 1);
                triangles.Add(vertexCount + 2);
                triangles.Add(vertexCount + 3);
                vertexCount += 4;
            }
            // 1 lowest, 2 is highest
            else if (weightCode[1] < weightCode[0] && weightCode[0] < weightCode[2])
            {
                Vector3 v04 = new Vector3(x * m_gridSize, weightCode[0], y * m_gridSize);
                Vector3 v05 = new Vector3((x + weight01) * m_gridSize, weightCode[0], y * m_gridSize);
                Vector3 v06 = new Vector3(x * m_gridSize, weightCode[0], (y + weight02) * m_gridSize);
                Vector3 v07 = new Vector3((x + 1) * m_gridSize, weightCode[0], (y + weight13) * m_gridSize);
                vertices.Add(v04);
                vertices.Add(v05);
                vertices.Add(v06);
                vertices.Add(v07);
                uvs.Add(new Vector2(0, 0));
                uvs.Add(new Vector2(0.5f, 0));
                uvs.Add(new Vector2(0, 1));
                uvs.Add(new Vector2(0, 1));
                triangles.Add(vertexCount + 0);
                triangles.Add(vertexCount + 2);
                triangles.Add(vertexCount + 1);

                triangles.Add(vertexCount + 1);
                triangles.Add(vertexCount + 2);
                triangles.Add(vertexCount + 3); 
                vertexCount += 4;


                Vector3 v08 = new Vector3(x * m_gridSize, weightCode[2], (y + 1) * m_gridSize);
                Vector3 v09 = new Vector3((x + 1) * m_gridSize, weightCode[2], (y + 1) * m_gridSize);
                Vector3 v10 = new Vector3(x * m_gridSize, weightCode[2], (y + weight02) * m_gridSize);
                Vector3 v11 = new Vector3((x + 1) * m_gridSize, weightCode[2], (y + weight13) * m_gridSize);
                vertices.Add(v08);
                vertices.Add(v09);
                vertices.Add(v10);
                vertices.Add(v11);
                uvs.Add(new Vector2(0, 0.5f));
                uvs.Add(new Vector2(1, 0.5f));
                uvs.Add(new Vector2(0.5f, 1));
                uvs.Add(new Vector2(1, 1));
                triangles.Add(vertexCount + 0);
                triangles.Add(vertexCount + 1);
                triangles.Add(vertexCount + 2);

                triangles.Add(vertexCount + 1);
                triangles.Add(vertexCount + 3);
                triangles.Add(vertexCount + 2);
                vertexCount += 4;

                // cliff
                Vector3 v12 = new Vector3((x + weight01) * m_gridSize, weightCode[1], y * m_gridSize);
                Vector3 v13 = new Vector3((x + 1) * m_gridSize, weightCode[1], (y + weight13) * m_gridSize);
                Vector3 v14 = new Vector3((x + weight01) * m_gridSize, weightCode[0], y * m_gridSize);
                Vector3 v15 = new Vector3((x + 1) * m_gridSize, weightCode[0], (y + weight13) * m_gridSize);
                vertices.Add(v12);
                vertices.Add(v13);
                vertices.Add(v14);
                vertices.Add(v15);
                uvs.Add(new Vector2(0, 0.5f));
                uvs.Add(new Vector2(1, 0.5f));
                uvs.Add(new Vector2(0.5f, 1));
                uvs.Add(new Vector2(1, 1));
                triangles.Add(vertexCount + 0);
                triangles.Add(vertexCount + 2);
                triangles.Add(vertexCount + 1);

                triangles.Add(vertexCount + 1);
                triangles.Add(vertexCount + 2);
                triangles.Add(vertexCount + 3);
                vertexCount += 4;

                Vector3 v16 = new Vector3(x * m_gridSize, weightCode[1], (y + weight02) * m_gridSize);
                Vector3 v17 = new Vector3((x + 1) * m_gridSize, weightCode[1], (y + weight13) * m_gridSize);
                Vector3 v18 = new Vector3(x * m_gridSize, weightCode[2], (y + weight02) * m_gridSize);
                Vector3 v19 = new Vector3((x + 1) * m_gridSize, weightCode[2], (y + weight13) * m_gridSize);
                vertices.Add(v16);
                vertices.Add(v17);
                vertices.Add(v18);
                vertices.Add(v19);
                uvs.Add(new Vector2(0, 0.5f));
                uvs.Add(new Vector2(1, 0.5f));
                uvs.Add(new Vector2(0.5f, 1));
                uvs.Add(new Vector2(1, 1));
                triangles.Add(vertexCount + 0);
                triangles.Add(vertexCount + 2);
                triangles.Add(vertexCount + 1);

                triangles.Add(vertexCount + 1);
                triangles.Add(vertexCount + 2);
                triangles.Add(vertexCount + 3);
                vertexCount += 4;
            }
        }


    }

    void createQuadTreeMesh(int startX, int startY, int lenX, int lenY, GridMatrix currGridMatrix)
    {
        bool isFlat = true;
        int currHeight = 0;
        for (int i = startX; i < startX + lenX; i++)
        {
            for (int j = startY; j < startY + lenY; j++)
            {
                Grid currGrid = currGridMatrix.getGrid(i, j);
                int[] weightCode = currGrid.weightCode();
                if (!(weightCode[0] == weightCode[1] && weightCode[1] == weightCode[2] && weightCode[2] == weightCode[3]))
                {
                    isFlat = false;
                }
                currHeight = weightCode[0];
            }
        }
        if (isFlat)
        {
            createSquad(startX, startY, lenX, lenY, currHeight);
        }
        else
        {
            if (lenX == 1 || lenY == 1)
            {
                return;
            }
            int halfX = lenX / 2;
            int halfY = lenY / 2;
            createQuadTreeMesh(startX, startY, halfX, halfY, currGridMatrix);
            createQuadTreeMesh(startX, startY + halfY, halfX, halfY, currGridMatrix);
            createQuadTreeMesh(startX + halfX, startY, lenX-halfX, lenY-halfY, currGridMatrix);
            createQuadTreeMesh(startX + halfY, startY + halfY, lenX-halfX, lenY-halfY, currGridMatrix);
        }
    }

}
