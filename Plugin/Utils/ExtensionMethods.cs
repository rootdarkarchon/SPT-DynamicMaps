using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DynamicMaps.Utils;

internal static class ExtensionMethods
{


    public static bool ApproxEquals(this float first, float second, float delta = float.Epsilon)
    {
        return Mathf.Abs(first - second) < delta;
    }

    public static bool ApproxEquals(this Vector3 first, Vector3 second, float delta = float.Epsilon)
    {
        return ApproxEquals(first.x, second.x, delta)
            && ApproxEquals(first.y, second.y, delta)
            && ApproxEquals(first.z, second.z, delta);
    }

    public static Vector3 Abs(this Vector3 v)
        => new(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
}
