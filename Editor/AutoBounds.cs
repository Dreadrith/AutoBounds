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
        private const float boundsPercentIncrease = 25;

        //Sets bounds to auto-calculated dimensions starting from the target
        [MenuItem("DreadTools/Utility/AutoBounds/Auto")]
        private static void CalculateBounds()
        {
            //Get Selection
            GameObject go = Selection.activeGameObject;
            if (!go) return;

            //Check for animator and to use hips instead of target as root
            //Always using target as root may make the bounds follow improperly
            Animator ani = go.GetComponent<Animator>();
            bool isHuman = ani && ani.avatar && ani.isHuman;

            Transform rootBone = isHuman ? ani.GetBoneTransform(HumanBodyBones.Hips) ?? go.transform : go.transform;

            //Get child renderers including disabled
            var renderers = go.GetComponentsInChildren<SkinnedMeshRenderer>(true);

            //Get Max Extent by getting the biggest dimension
            //Avatars can rotate their armature around meaning this dimension can go in any direction, so reuse it for every dimension
            //Probably not the best logic or calculation but it usually works
            float maxExtent = 0;
            foreach (var r in renderers)
            {
                if (!r.sharedMesh) continue;
                Transform currentRootBone = r.rootBone ?? r.transform;
                var currentExtent = GetMaxAxis(rootBone.InverseTransformPoint(currentRootBone.position)) + GetMaxAxis(r.sharedMesh.bounds.size);
                if (maxExtent < currentExtent) maxExtent = currentExtent;
            }

            //If human, hips should stay the center
            //Otherwise, center the bounds vertically based on current dimensions
            Vector3 center = new Vector3(0, isHuman ? 0 : maxExtent / 2, 0);

            //Increase area by percentage for safe measure
            maxExtent *= 1 + boundsPercentIncrease / 100;
            Vector3 extents = new Vector3(maxExtent, maxExtent, maxExtent);

            //Set auto calculated bounds starting from target as root
            SetBounds(go.transform, rootBone, new Bounds(center, extents));
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
            SetBounds(go.transform.root, sampleRenderer.rootBone, sampleRenderer.bounds);
        }

        //Sets all children skinned mesh renderers of the root to the given bounds
        private static void SetBounds(Transform root, Transform rootbone, Bounds myBounds)
        {
            foreach (var r in root.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                r.rootBone = rootbone;
                r.localBounds = myBounds;
                EditorUtility.SetDirty(r);
            }
        }

        private static float GetMaxAxis(Vector3 v) => Mathf.Max(v.x, v.y, v.z);
    }
}
