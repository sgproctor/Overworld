using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;


public static class Vector3Extension
{
    public static Vector2[] toVector2Array (this Vector3[] v3)
    {
        return System.Array.ConvertAll<Vector3, Vector2> (v3, getV3fromV2);
    }
        
    public static Vector2 getV3fromV2 (Vector3 v3)
    {
        return new Vector2 (v3.x, v3.y);
    }
}