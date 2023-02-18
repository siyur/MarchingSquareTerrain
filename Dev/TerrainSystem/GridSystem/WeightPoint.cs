using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeightPoint : MonoBehaviour
{
    public int xIndex;
    public int yIndex;
    public Vector2 worldCoord;
    public float weight;

    public WeightPoint(int _xIndex, int _yIndex, Vector2 _worldCoord)
    {
        xIndex = _xIndex;
        yIndex = _yIndex;
        worldCoord = _worldCoord;
        weight = 0.0f;
    }

    public void setWeight(float newWeight)
    {
        weight = newWeight;
    }
}
