using UnityEngine;
using UnityEditor;

//Made by Dreadrith#3238
//Discord: https://discord.gg/ZsPfrGn
//Github: https://github.com/Dreadrith/DreadScripts
//Gumroad: https://gumroad.com/dreadrith
//Ko-fi: https://ko-fi.com/dreadrith

namespace DreadScripts.AutoBounds
{
    public class AutoBounds
    {
        private const float boundsPercentIncrease = 10;

        //Sets bounds to auto-calculated dimensions starting from the target
        [MenuItem("DreadTools/Utility/AutoBounds/Auto")]
        private static void CalculateBounds()
        {
            //Get Selection
            GameObject go = Selection.activeGameObject;
            if (!go) return;
            Transform rootTransform = go.transform;

            //Get child renderers including disabled
            var renderers = go.GetComponentsInChildren<SkinnedMeshRenderer>(true);


            //Get Max Extent by getting the biggest dimension
            //Avatars can rotate their armature around meaning this dimension can go in any direction, so reuse it for every dimension
            //Probably not the best logic or calculation but it usually works
            float maxExtent = 0;
            foreach (var r in renderers)
            {
                if (!r.sharedMesh) continue;
                var currentExtent = GetMaxAxis(rootTransform.InverseTransformPoint(r.rootBone.position)) + GetMaxAxis(r.sharedMesh.bounds.size);
                if (maxExtent < currentExtent) maxExtent = currentExtent;
            }

            //Increase area by percentage for safe measure
            maxExtent *= 1 + boundsPercentIncrease / 100;
            Bounds myBounds = new Bounds(new Vector3(0, maxExtent / 2, 0), new Vector3(maxExtent, maxExtent, maxExtent));

            //Set auto calculated bounds starting from target as root
            SetBounds(rootTransform, myBounds);
        }


        //Sets sampled bounds from target starting from top most parent of target
        [MenuItem("DreadTools/Utility/AutoBounds/Sample")]
        private static void SampleBounds()
        {
            //Get Selection
            GameObject go = Selection.activeGameObject;
            if (!go) return;

            //Get renderer for sampling
            SkinnedMeshRenderer sampleRenderer = go.GetComponent<SkinnedMeshRenderer>();
            if (!sampleRenderer)
            {
                Debug.LogWarning("No skinned mesh renderer on selected gameobject.");
                return;
            }

            //Set the samples bounds start from the top most parent
            SetBounds(go.transform.root, sampleRenderer.bounds);
        }

        //Sets all children skinned mesh renderers of the root to the given bounds
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
}
