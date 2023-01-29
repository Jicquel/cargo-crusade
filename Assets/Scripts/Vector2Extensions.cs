using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Vector2Extensions
{
    public static void Rotate(this Vector2 v, float delta)
    {
        float x, y;
        x = v.x * Mathf.Cos(delta) - v.y * Mathf.Sin(delta);
        y = v.x * Mathf.Sin(delta) + v.y * Mathf.Cos(delta);
        Debug.Log("V before: " + v);

        v.x = x;
        v.y = y;
        Debug.Log("V after: " + v);
    }
    public static Vector2 RotateRet(Vector2 v, float delta)
    {
        return new Vector2(
            v.x * Mathf.Cos(delta) - v.y * Mathf.Sin(delta),
            v.x * Mathf.Sin(delta) + v.y * Mathf.Cos(delta)
        );
    }
}