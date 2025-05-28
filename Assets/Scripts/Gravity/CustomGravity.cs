using System.Collections.Generic;
using UnityEngine;

public static class CustomGravity
{
    private static List<GravitySource> gravitySources = new();

    private static Vector3 SumGravityForces(Vector3 position)
    {
        Vector3 g = Vector3.zero;
        foreach (GravitySource gravity in gravitySources)
        {
            g += gravity.GetGravity(position);
        }

        return g;
    }

    public static Vector3 GetGravity(Vector3 position)
    {
        return SumGravityForces(position);
    }

    public static Vector3 GetGravity(Vector3 position, out Vector3 upAxis)
    {
        Vector3 g = SumGravityForces(position);
        upAxis = -g.normalized;
        return g;
    }

    public static Vector3 GetUpAxis(Vector3 position)
    {
        Vector3 g = SumGravityForces(position);
        return -g.normalized;
    }

    public static void RegisterSource(GravitySource source)
    {
        Debug.Assert(!gravitySources.Contains(source), "Duplicate registration of gravity source!", source);
        gravitySources.Add(source);
    }

    public static void UnregisterSource(GravitySource source)
    {
        Debug.Assert(gravitySources.Contains(source), "Unregistration of a unknown gravity source!", source);
        gravitySources.Remove(source);
    }
}
