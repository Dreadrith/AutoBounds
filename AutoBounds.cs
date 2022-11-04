using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AutoBounds
{
    private const float boundsPercentIncrease = 10;

    [MenuItem("DreadTools/Personal/Auto Bounds/Auto")]
    private static void CalculateBounds()
    {
        GameObject go = Selection.activeGameObject;
        if (!go) return;
        Transform rootTransform = go.transform;
        var renderers = go.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        float maxExtent = 0;
        foreach (var r in renderers)
        {
            if (!r.sharedMesh) continue;
            Debug.Log(r.sharedMesh.bounds);
            var currentExtent = GetMaxAxis(rootTransform.InverseTransformPoint(r.rootBone.position)) + GetMaxAxis(r.sharedMesh.bounds.size);
            if (maxExtent < currentExtent) maxExtent = currentExtent;
        }
        maxExtent *= 1 + boundsPercentIncrease/100;
        Bounds myBounds = new Bounds(new Vector3(0, maxExtent / 2, 0), new Vector3(maxExtent, maxExtent, maxExtent));
        SetBounds(rootTransform, myBounds);
    }

    [MenuItem("DreadTools/Personal/Auto Bounds/Sample")]
    private static void SampleBounds()
    {
        GameObject go = Selection.activeGameObject;
        if (!go) return;
        SkinnedMeshRenderer sampleRenderer = go.GetComponent<SkinnedMeshRenderer>();
        if (!sampleRenderer)
        {
            Debug.LogWarning("No skinned mesh renderer on selected gameobject.");
            return;
        }
        SetBounds(go.transform.root, sampleRenderer.bounds);
    }

    private static void SetBounds(Transform root, Bounds myBounds)
    {
        foreach (var r in root.GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            r.rootBone = root;
            r.localBounds = myBounds;
            EditorUtility.SetDirty(r);
        }
    }

    private static float GetMaxAxis(Vector3 v) => Mathf.Max(v.x, v.y, v.z);
}
