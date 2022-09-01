#if UNITY_EDITOR
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System;
using System.Linq;


#if VRC_SDK_VRCSDK3
using VRC.SDK3.Avatars.Components;
#endif

namespace VRLabs.Marker
{
    public static class AvatarMaskFunctions
    {
        public static void ClearAllMasks(AnimatorController controller)
        {
            if (controller == null)
                return;

            EditorUtility.SetDirty(controller);

            AnimatorControllerLayer[] layers = controller.layers;
            for (int i = 0; i < layers.Length; i++)
                layers[i].avatarMask = null;

            controller.layers = layers;
        }

        static List<AnimationClip> GetAnimationsInMachine(AnimatorStateMachine machine)
        {
            List<AnimationClip> clips = new List<AnimationClip>();

            for (int stateIdx = 0; stateIdx < machine.states.Length; stateIdx++)
            {
                Motion motion = machine.states[stateIdx].state.motion;
                if (motion == null)
                    continue;

                BlendTree bt = motion as BlendTree;
                if (bt == null)
                {
                    clips.Add((AnimationClip)motion);
                }
                else
                {
                    clips = clips.Concat(GetClipsFromBlendTree(bt)).ToList();
                }
            }

            for (int machineIdx = 0; machineIdx < machine.stateMachines.Length; machineIdx++)
            {
                AnimatorStateMachine curMachine = machine.stateMachines[machineIdx].stateMachine;
                List<AnimationClip> clipsInMachine = GetAnimationsInMachine(curMachine);
                clips = clips.Concat(clipsInMachine).ToList();
            }

            return clips;
        }

        static List<AnimationClip> GetClipsFromBlendTree(BlendTree bt)
        {
            List<AnimationClip> clips = new List<AnimationClip>();

            ChildMotion[] children = bt.children;
            for (int i = 0; i < children.Length; i++)
            {
                BlendTree sbt = children[i].motion as BlendTree;
                if (sbt == null)
                {
                    clips.Add((AnimationClip)children[i].motion);
                }
                else
                {
                    clips = clips.Concat(GetClipsFromBlendTree(sbt)).ToList();
                }
            }

            return clips;
        }

        static List<AvatarMaskBodyPart> GetBodyPartsFromClip(AnimationClip clip)
        {
            List<AvatarMaskBodyPart> bodyParts = new List<AvatarMaskBodyPart>();

            if (clip != null)
            {
                EditorCurveBinding[] curves = AnimationUtility.GetCurveBindings(clip);
                foreach (EditorCurveBinding binding in curves)
                {
                    string propertyName = binding.propertyName;
                    List<AvatarMaskBodyPart> currentBodyParts = PropertyNameToAvatarMaskBodyPart(propertyName);
                    for (int i = 0; i < currentBodyParts.Count; i++)
                    {
                        if (!bodyParts.Contains(currentBodyParts[i]))
                            bodyParts.Add(currentBodyParts[i]);
                    }
                }
            }

            return bodyParts;
        }

        static AvatarMask GetExistingMaskIfExists(AvatarMask avatarMask, List<AvatarMask> existingMasks)
        {
            List<string> oldPaths = GetMaskPaths(avatarMask);
            List<int> oldBones = GetHumanoidBones(avatarMask);
            foreach (AvatarMask currentMask in existingMasks)
            {
                List<string> curPaths = GetMaskPaths(currentMask);
                List<int> curBones = GetHumanoidBones(currentMask);
                if (curPaths.SequenceEqual(oldPaths) && curBones.SequenceEqual(oldBones))
                    return currentMask;
            }

            return null;

            List<int> GetHumanoidBones(AvatarMask mask)
            {
                List<int> humanoidBones = new List<int>();
                for (int i = 0; i < ((int)AvatarMaskBodyPart.LastBodyPart); i++)
                {
                    if (mask.GetHumanoidBodyPartActive((AvatarMaskBodyPart)i))
                        humanoidBones.Add(i);
                }

                return humanoidBones;
            }

            List<string> GetMaskPaths(AvatarMask mask)
            {
                List<string> maskPaths = new List<string>();
                for (int i = 0; i < mask.transformCount; i++)
                    maskPaths.Add(mask.GetTransformPath(i));
                return maskPaths;
            }
        }

        static List<string> GetPathsInAnimation(AnimationClip clip, bool maskTransformsOnly)
        {
            List<string> paths = new List<string>();

            if (clip != null)
            {
                foreach (EditorCurveBinding binding in AnimationUtility.GetCurveBindings(clip))
                {
                    if (!maskTransformsOnly || binding.propertyName.Contains("m_LocalPosition")
                        || binding.propertyName.Contains("m_localEulerAngles")
                        || binding.propertyName.Contains("m_LocalScale")
                    )
                    {
                        paths.Add(binding.path);
                    }
                }
            }

            return paths;
        }

        static AvatarMask GenerateEmptyMask(bool addEmptyTransform)
        {
            AvatarMask mask = new AvatarMask();
            //AvatarMaskBodyPart[] avatarMaskBodyParts = (AvatarMaskBodyPart[])Enum.GetValues(typeof(AvatarMaskBodyPart));
            for (int j = 0; j < (int)AvatarMaskBodyPart.LastBodyPart; j++)
                mask.SetHumanoidBodyPartActive((AvatarMaskBodyPart)j, false);

            if (addEmptyTransform)
            {
                GameObject temp = new GameObject();
                mask.AddTransformPath(temp.transform);
                mask.SetTransformPath(0, "peepeepoopoo");
                GameObject.DestroyImmediate(temp);
            }

            return mask;
        }

#if VRC_SDK_VRCSDK3
        public static AvatarMask GenerateFXMasterMask(VRCAvatarDescriptor descriptor, string directory)
        {
            // lmao wyd 
            if (!descriptor.customizeAnimationLayers || descriptor.baseAnimationLayers.Count() < 1)
            {
                Debug.LogWarning("Custom animator layers are not enabled");
                return null;
            }

            // get gesture and fx layer
            AnimatorController gesture = descriptor.baseAnimationLayers[2].animatorController as AnimatorController;
            if (gesture == null)
            {
                Debug.LogWarning("Gesture controller and FX controller not assigned");
                return null;
            }

            // init mask and set dirty for editing
            AvatarMask mask = new AvatarMask();
            EditorUtility.SetDirty(mask);

            // get all transform hierarchy paths on the avatar
            List<string> allTransformPaths = descriptor.transform.GetComponentsInChildren<Transform>(true)
                .Select(t => t.GetHierarchyPath(descriptor.transform))
                .ToList();

            // create a dictionary that we will be using to reference the index of the masked transform
            Dictionary<string, int> maskDict = new Dictionary<string, int>();

            //create a placeholder transform cuz avatar masks dont let you add a transform by path lol
            // 1. add the placeholder transform as a masked transform
            // 2. because masked transforms just turn into strings anyway we just inject our transform path into the list lmao
            // 3. add to dictionary
            GameObject placeholder = new GameObject();
            for (int i = 0; i < allTransformPaths.Count; i++)
            {
                mask.AddTransformPath(placeholder.transform);
                mask.SetTransformPath(i, allTransformPaths[i]);
                maskDict[allTransformPaths[i]] = i;
            }

            // 1. get all animation clips in the controller
            // 2. for each animation clip, get the paths of all animated properties
            // 3. disable the paths of the animated properties
            foreach (AnimationClip animation in gesture.animationClips)
            {
                foreach (string path in GetPathsInAnimation(animation, false))
                {
                    if (maskDict.ContainsKey(path) && path.Length > 0)
                        mask.SetTransformActive(maskDict[path], false);
                }
            }

            GameObject.DestroyImmediate(placeholder);

            // disable all body parts in mask
            for (int i = 0; i < (int)AvatarMaskBodyPart.LastBodyPart; i++)
                mask.SetHumanoidBodyPartActive((AvatarMaskBodyPart)i, false);

            // save and return
            string savePath = AssetDatabase.GenerateUniqueAssetPath($"{directory}/MasterMask_{gesture.name}.asset");
            AssetDatabase.CreateAsset(mask, savePath);
            return mask;
        }
#endif

        public enum AvatarMaskOverwriteMode
        {
            OverwriteAll,
            OverwriteNone,
            OverwriteOnlyActive,
            OverwriteOnlyInactive,
        }

        public static void MergeAvatarMasks(AvatarMask maskToMergeFrom,
            ref AvatarMask maskToMergeTo, AvatarMaskOverwriteMode xformOverwriteMode, AvatarMaskOverwriteMode boneOverwiteMode)
        {
            EditorUtility.SetDirty(maskToMergeTo);

            // map the old mask
            Dictionary<string, int> mergeToPaths = new Dictionary<string, int>();
            foreach (int index in Enumerable.Range(0, maskToMergeTo.transformCount))
            {
                mergeToPaths[maskToMergeTo.GetTransformPath(index)] = index;
            }

            // transforms
            GameObject placeholder = new GameObject();
            for (int tIdx = 0; tIdx < maskToMergeFrom.transformCount; tIdx++)
            {
                // check if current transform exists in old mask
                //      if exists and we overwrite, overwrite
                //      if exists and we don't overwrite, continue
                //      if !exists, add to end

                string path = maskToMergeFrom.GetTransformPath(tIdx);
                bool isActive = maskToMergeFrom.GetTransformActive(tIdx);

                bool existsInOld = mergeToPaths.TryGetValue(path, out int oldIndex);

                if (existsInOld && xformOverwriteMode != AvatarMaskOverwriteMode.OverwriteNone)
                {
                    switch (xformOverwriteMode)
                    {
                        case AvatarMaskOverwriteMode.OverwriteAll:
                            maskToMergeTo.SetTransformActive(oldIndex, isActive);
                            break;
                        case AvatarMaskOverwriteMode.OverwriteOnlyActive:
                            if (maskToMergeTo.GetTransformActive(oldIndex))
                            {
                                maskToMergeTo.SetTransformActive(oldIndex, isActive);
                            }
                            break;
                        case AvatarMaskOverwriteMode.OverwriteOnlyInactive:
                            if (!maskToMergeTo.GetTransformActive(oldIndex))
                            {
                                maskToMergeTo.SetTransformActive(oldIndex, isActive);
                            }
                            break;
                    }
                }
                else if (!existsInOld)
                {
                    maskToMergeTo.AddTransformPath(placeholder.transform);
                    maskToMergeTo.SetTransformActive(maskToMergeTo.transformCount - 1, isActive);
                }
            }
            GameObject.DestroyImmediate(placeholder);

            // bones only if replace
            if (boneOverwiteMode != AvatarMaskOverwriteMode.OverwriteNone)
            {
                for (int i = 0; i < (int)AvatarMaskBodyPart.LastBodyPart; i++)
                {
                    AvatarMaskBodyPart currentBodyPart = (AvatarMaskBodyPart)i;
                    bool newBodyPartActive = maskToMergeFrom.GetHumanoidBodyPartActive(currentBodyPart);

                    switch (boneOverwiteMode)
                    {
                        case AvatarMaskOverwriteMode.OverwriteAll:
                            maskToMergeTo.SetHumanoidBodyPartActive(currentBodyPart, newBodyPartActive);
                            break;
                        case AvatarMaskOverwriteMode.OverwriteOnlyActive:
                            if (maskToMergeTo.GetHumanoidBodyPartActive(currentBodyPart))
                            {
                                maskToMergeTo.SetHumanoidBodyPartActive(currentBodyPart, newBodyPartActive);
                            }
                            break;
                        case AvatarMaskOverwriteMode.OverwriteOnlyInactive:
                            if (!maskToMergeTo.GetHumanoidBodyPartActive(currentBodyPart))
                            {
                                maskToMergeTo.SetHumanoidBodyPartActive(currentBodyPart, newBodyPartActive);
                            }
                            break;
                    }
                }
            }
        }

        public static void GenerateMasksFromControllerAndSave(AnimatorController controller, bool maskTransformsOnly, string directory)
        {
            // create generated assets path
            EditorUtility.SetDirty(controller);

            // Layers
            AnimatorControllerLayer[] layers = controller.layers;
            List<AvatarMask> existingMasks = new List<AvatarMask>() { };

            for (int i = 0; i < layers.Length; i++)
            {
                if (layers[i].avatarMask != null)
                    continue;

                AvatarMask newMask = GenerateMaskFromLayer(controller, i, maskTransformsOnly);
                AvatarMask existingMask = GetExistingMaskIfExists(newMask, existingMasks);
                if (existingMask != null)
                {
                    layers[i].avatarMask = existingMask;
                }
                else
                {
                    string layerName = IsEmptyMask(newMask)
                        ? "Empty Mask"
                        : layers[i].name.Replace("/", "_").Trim();
                    string layerMaskPath = AssetDatabase.GenerateUniqueAssetPath(
                        $"{directory}/Mask_{layerName}.asset"
                    );

                    existingMasks.Add(newMask);
                    AssetDatabase.CreateAsset(newMask, layerMaskPath);
                    EditorUtility.SetDirty(newMask);
                    layers[i].avatarMask = newMask;
                }
            }

            controller.layers = layers;
        }

        public static AvatarMask GenerateMaskFromLayer(AnimatorController controller, int index, bool maskTransformsOnly)
        {
            AnimatorControllerLayer layer = controller.layers[index];
            List<AnimationClip> layerClips = GetAnimationsInMachine(layer.stateMachine);

            AvatarMask newMask = new AvatarMask();// GenerateEmptyMask(false);

            // disable all bones by default
            for (int i = 0; i < (int)AvatarMaskBodyPart.LastBodyPart; i++)
                newMask.SetHumanoidBodyPartActive((AvatarMaskBodyPart)i, false);

            bool addedBones = false;
            List<string> addedPaths = new List<string>();
            foreach (AnimationClip clip in layerClips)
            {
                if (clip == null)
                    continue;

                Debug.Log($"Clip Name: {clip.name}");

                List<string> allPaths = GetPathsInAnimation(clip, maskTransformsOnly);
                foreach (string path in allPaths)
                {
                    if (path.Length < 1 || addedPaths.Contains(path))
                        continue;

                    addedPaths.Add(path);
                }

                List<AvatarMaskBodyPart> bodyParts = GetBodyPartsFromClip(clip);
                for (int j = 0; j < bodyParts.Count; j++)
                {
                    addedBones = true;
                    newMask.SetHumanoidBodyPartActive(bodyParts[j], true);
                }
            }

            // create mask if needed
            if (addedPaths.Count > 0 || addedBones)
            {
                GameObject placeholder = new GameObject();
                // add transforms
                for (int i = 0; i < addedPaths.Count; i++)
                {
                    Debug.Log($"\tAdding Path: {addedPaths[i]}");
                    newMask.AddTransformPath(placeholder.transform);
                    newMask.SetTransformPath(i, addedPaths[i]);
                }
                GameObject.DestroyImmediate(placeholder);
            }

            return newMask;
        }

        static bool IsEmptyMask(AvatarMask mask)
        {
            if (mask == null || mask.transformCount > 1)
                return false;

            for (int i = 0; i < (int)AvatarMaskBodyPart.LastBodyPart; i++)
            {
                AvatarMaskBodyPart bodyPart = (AvatarMaskBodyPart)i;
                if (mask.GetHumanoidBodyPartActive(bodyPart) == true)
                    return false;
            }

            return true;
        }

        public static void MaskNonMasked(AnimatorController controller, AvatarMask customMask)
        {
            AnimatorControllerLayer[] layers = controller.layers;
            for (int i = 0; i < controller.layers.Length; i++)
            {
                if (layers[i].avatarMask == null)
                    layers[i].avatarMask = customMask;
            }

            EditorUtility.SetDirty(controller);
            controller.layers = layers;
        }

        static List<AvatarMaskBodyPart> PropertyNameToAvatarMaskBodyPart(string propertyName)
        {
            List<AvatarMaskBodyPart> bodyParts = new List<AvatarMaskBodyPart>();
            if (propertyName.Contains("Chest") || propertyName.Contains("Spine"))
            {
                bodyParts.Add(AvatarMaskBodyPart.Body);
            }
            else if (propertyName.Contains("Hips") || propertyName.Contains("Root"))
            {
                bodyParts.Add(AvatarMaskBodyPart.Root);
            }
            else if (propertyName.Contains("Hand"))
            {
                if (propertyName.Contains("Index") || propertyName.Contains("Middle") || propertyName.Contains("Little") || propertyName.Contains("Thumb") || propertyName.Contains("Ring"))
                {
                    bodyParts.Add(propertyName.Contains("Right") ? AvatarMaskBodyPart.RightFingers : AvatarMaskBodyPart.LeftFingers);
                }
                else
                {
                    bodyParts.Add(propertyName.Contains("Right") ? AvatarMaskBodyPart.RightHandIK : AvatarMaskBodyPart.LeftHandIK);
                }
            }
            else if (propertyName.Contains("Arm") || propertyName.Contains("Shoulder"))
            {
                bodyParts.Add(
                    propertyName.Contains("Right") ? AvatarMaskBodyPart.RightArm : AvatarMaskBodyPart.LeftArm
                );
            }
            else if (propertyName.Contains("Leg"))
            {
                bodyParts.Add(
                    propertyName.Contains("Right") ? AvatarMaskBodyPart.RightLeg : AvatarMaskBodyPart.LeftLeg
                );
            }
            else if (propertyName.Contains("Foot"))
            {
                bodyParts.Add(
                    propertyName.Contains("Right") ? AvatarMaskBodyPart.RightFootIK : AvatarMaskBodyPart.LeftFootIK
                );
            }
            else if (propertyName.Contains("Head") || propertyName.Contains("Neck") || propertyName.Contains("Eye") || propertyName.Contains("Jaw"))
            {
                bodyParts.Add(AvatarMaskBodyPart.Head);
            }

            return bodyParts;
        }

        public static void RemoveMaskAtIndex(AnimatorController controller, int index)
        {
            if (controller == null)
                return;

            EditorUtility.SetDirty(controller);

            AnimatorControllerLayer[] layers = controller.layers;
            if (index >= 0 && index < layers.Length)
                layers[index].avatarMask = null;

            controller.layers = layers;
        }

        public static void SetMaskAtIndex(AnimatorController controller, int index, AvatarMask mask)
        {
            if (controller == null)
                return;

            EditorUtility.SetDirty(controller);

            AnimatorControllerLayer[] layers = controller.layers;
            if (index >= 0 && index < layers.Length)
                layers[index].avatarMask = mask;

            controller.layers = layers;
        }
    }

}
#endif