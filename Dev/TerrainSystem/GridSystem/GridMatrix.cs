using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridMatrix
{
    public int mapSize;
    public float gridSize;
    Grid[,] _gridMatrix;
    WeightPoint[,] weightPointMatrix;

    public GridMatrix(int _mapSize, float _gridSize)
    {
        mapSize = _mapSize;
        gridSize = _gridSize;
        if (gridSize <= 0)
        {
            gridSize = 1;
        }
        _gridMatrix = new Grid[mapSize, mapSize];
        weightPointMatrix = new WeightPoint[mapSize + 1, mapSize + 1];

        for (int i = 0; i < mapSize + 1; i++)
        {
            for (int j = 0; j < mapSize + 1; j++)
            {
                weightPointMatrix[i, j] = new WeightPoint(i, j, new Vector2(i * gridSize - 0.5f * gridSize, j * gridSize - 0.5f * gridSize));
            }
        }

        for (int i = 0; i < mapSize; i++)
        {
            for (int j = 0; j < mapSize; j++)
            {
                _gridMatrix[i, j] = new Grid(i, j, new Vector2(i * gridSize, j * gridSize));
                _gridMatrix[i, j].setWeightPoint(0, weightPointMatrix[i, j]);
                _gridMatrix[i, j].setWeightPoint(1, weightPointMatrix[i + 1, j]);
                _gridMatrix[i, j].setWeightPoint(2, weightPointMatrix[i, j + 1]);
                _gridMatrix[i, j].setWeightPoint(3, weightPointMatrix[i + 1, j + 1]);
            }
        }
    }

    public void applyHeightMap(Texture2D heightMap)
    {
        float multiplier = heightMap.width/(mapSize + 1.0f);
        for (int i = 0; i < mapSize + 1; i++)
        {
            for (int j = 0; j < mapSize + 1; j++)
            {
                int xCoord = (int)(i * multiplier);
                int yCoord = (int)(j * multiplier);
                paintWeightPoint(i, j, heightMap.GetPixel(xCoord, yCoord).r);
            }
        }
    }

    public void paintWeightPoint(int x, int y, float weight)
    {
        if(weight > 0.95f)
        {
            weight = 0.95f;
        }else if(weight < 0.0f)
        {
            weight = 0.0f;
        }
        weightPointMatrix[x, y].setWeight(weight);
    }

    public Grid getGrid(int x, int y)
    {
        return _gridMatrix[x, y];
    }
}
