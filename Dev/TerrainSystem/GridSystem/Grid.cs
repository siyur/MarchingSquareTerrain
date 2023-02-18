using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Grid
{
    public int xIndex;
    public int yIndex;
    public Vector2 worldCoord;
    public WeightPoint[] wayPoints;
    public Color selfColor;

    public Grid(int _xIndex, int _yIndex, Vector2 _worldCoord)
    {
        xIndex = _xIndex;
        xIndex = _yIndex;
        worldCoord = _worldCoord;
        wayPoints = new WeightPoint[4];
        selfColor = Color.black;
    }

    public void setWeightPoint(int i, WeightPoint newPoint)
    {
        wayPoints[i] = newPoint;
    }

    public int[] weightCode()
    {
        int[] weightList = new int[4];
        for (int i = 0; i < 4; i++)
        {
            weightList[i] = (int)(wayPoints[i].weight * 5);
        }
        return weightList;
    }
}
