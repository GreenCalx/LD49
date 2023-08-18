using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TestCopyObjectMenu : MonoBehaviour
{
    static Transform s_transf;
    // Add Example1 into a new menu list
    [MenuItem("GameObject/Copy transform", false, -2)]
    public static void Example1()
    {
        s_transf = Selection.activeTransform;
    }

    // Add Example2 into the same menu list
    [MenuItem("GameObject/Paste transform", false, -1)]
    public static void Example2()
    {
        s_transf.GetPositionAndRotation(out Vector3 p, out Quaternion r);
        Selection.activeGameObject.transform.SetPositionAndRotation(p,r);
        Selection.activeGameObject.transform.localScale = s_transf.localScale;
    }
}
