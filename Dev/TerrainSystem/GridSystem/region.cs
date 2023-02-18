using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class region
{
    public Vector2 location;
    public int weight;
    public Color selfColor;

    public region(Vector2 _location, int _weight)
    {
        location = _location;
        weight = _weight;
        selfColor = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));
    }
}
