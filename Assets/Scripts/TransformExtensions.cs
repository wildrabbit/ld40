using UnityEngine;
using System.Collections;

public static class  TransformExtensions
{

    public static Transform FindRecursive(this Transform transform, string name)
    {
        for (int i = 0; i < transform.childCount; ++i)
        {
            Transform child = transform.GetChild(i);
            if (child.name == name)
            {
                return child;
            }
            else
            {
                Transform grandkid = child.FindRecursive(name);
                if (grandkid != null) return grandkid;
            }
        }
        return null;
    }
}
