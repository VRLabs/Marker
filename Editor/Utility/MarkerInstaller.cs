using System.Linq;
using System.IO;
using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Animations;
using UnityEditor;
using UnityEditor.Animations;

using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using VRC.SDK3.Dynamics.Contact.Components;
using Object = UnityEngine.Object;

namespace VRLabs.Marker
{
    public static class MarkerInstaller
    {
        const string A_PC_MARKER_DIR = "Assets/VRLabs/Marker/Resources/PC Marker";
        const string A_QUEST_MARKER_DIR = "Assets/VRLabs/Marker/Resources/Quest Marker";
        const string A_GENERATED_ASSETS_DIR = "Assets/VRLabs/GeneratedAssets/Marker";
        const string A_SHARED_RESOURCES_DIR = "Assets/VRLabs/Marker/Resources/Shared";
        const string R_QUEST_MARKER_PATH = "Quest Marker/Marker";
        const float KSIVL_UNIT = 0.4156029f;

        // animator parameters
        const string M_COLOR_PARAM = "VRLabs/Marker/Color";
        const string M_MARKER_PARAM = "VRLabs/Marker/Enable";
        const string M_MARKER_CLEAR_PARAM = "VRLabs/Marker/Clear";
        const string M_NORMALIZED_PARAM_NAME = "VRLabs/Marker/_Normalize";
        const string M_LEFTHAND_PARAM_NAME = "VRLabs/Marker/LeftHand";

        enum MainMenuItems
        {
            MarkerToggle = 0,
            Eraser = 1,
            MenuToggle = 2,
            Options = 3,
            LocalSpace = 4,
            Clear = 5,
        }

        enum OptionsMenu
        {
            Color = 0,
            BrushAndEraserSize = 1,
            LeftHand = 2,
        }

        public static void Generate(VRCAvatarDescriptor descriptor, ref Marker marker, bool installQuest, bool mergeOnCopy)
        {
            // Ensure we aren't installing more than once
            Uninstall(descriptor);

            // Unique directory setup, named after avatar
            Directory.CreateDirectory(A_GENERATED_ASSETS_DIR);
            AssetDatabase.Refresh();

            // Folder name cannot contain these chars
            string cleanedName = string.Join("", descriptor.name.Split('/', '?', '<', '>', '\\', ':', '*', '|', '\"'));
            string guid = AssetDatabase.CreateFolder(A_GENERATED_ASSETS_DIR, cleanedName);
            string directory = AssetDatabase.GUIDToAssetPath(guid);

            // Setup marker and return generated controller
            Dictionary<ScriptFunctions.PlayableLayer, AnimatorController> generatedControllers = installQuest
                ? GenerateQuest(descriptor, ref marker, directory, mergeOnCopy)
                : GeneratePC(descriptor, ref marker, directory, mergeOnCopy);

            if(generatedControllers == null)
                throw new NullReferenceException("Failed to generate marker controller(s)");

            // Merge Controllers
            if (!mergeOnCopy)
            {
                foreach (KeyValuePair<ScriptFunctions.PlayableLayer, AnimatorController> kvp in generatedControllers)
                {
                    ScriptFunctions.MergeController(descriptor, kvp.Value, kvp.Key, $"{directory}/");
                    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(kvp.Value));
                }
            }
            else
            {
                // Swapping the FX controller and the Gesture controller on the avatar with our new custom one
                descriptor.baseAnimationLayers[(int)ScriptFunctions.PlayableLayer.FX].animatorController = generatedControllers.ElementAt(0).Value;
                descriptor.baseAnimationLayers[(int)ScriptFunctions.PlayableLayer.Gesture].animatorController = generatedControllers.ElementAt(1).Value;
            }

            // For Gesture Layer
            // this is super hardcoded danger in future
            AnimatorController avatarGesture = descriptor.baseAnimationLayers[2].animatorController as AnimatorController;
            if (avatarGesture != null)
            {
                EditorUtility.SetDirty(avatarGesture);
                for (int i = 0; i < avatarGesture.layers.Length; i++)
                {   // the controls' layer is normally 3 (AllParts, LeftHand, RightHand, >>>M_Gesture<<<)
                    if (avatarGesture.layers[i].name.Contains("VRLabs/Marker/Gesture") && (i != 3))
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            if (avatarGesture.layers[i].stateMachine.states[j].state.behaviours.Length != 0)
                            {
                                VRCAnimatorLayerControl ctrl = (VRCAnimatorLayerControl)avatarGesture.layers[i]
                                    .stateMachine.states[j].state.behaviours[0];
                                ctrl.layer = i;
                            }
                        }
                    }
                }
            }

            // Generate and apply master Avatar Mask
            if (marker.generateMasterMask)
            {
                AnimatorController fxController = descriptor.baseAnimationLayers[4].animatorController as AnimatorController;
                EditorUtility.SetDirty(fxController);

                AnimatorControllerLayer[] layers = fxController.layers;
                layers[0].avatarMask = AvatarMaskFunctions.GenerateFXMasterMask(descriptor, directory);
                fxController.layers = layers;
                descriptor.baseAnimationLayers[4].animatorController = fxController;
            }

            // finishing touches
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            marker.finished = true;
            Debug.Log($"{MarkerStaticResources.MarkerLogTag}Successfully generated Marker!");
        }

        public static void Uninstall(VRCAvatarDescriptor descriptor)
        {
            Animator avatar = descriptor.GetComponent<Animator>();

            ScriptFunctions.UninstallControllerByPrefix(descriptor, "VRLabs/Marker", ScriptFunctions.PlayableLayer.FX);
            ScriptFunctions.UninstallControllerByPrefix(descriptor, "VRLabs/Marker", ScriptFunctions.PlayableLayer.Gesture);
            ScriptFunctions.UninstallParametersByPrefix(descriptor, "VRLabs/Marker");
            ScriptFunctions.UninstallMenu(descriptor, "Marker");
            if (descriptor != null)
            {
                Transform foundMarker = descriptor.transform.Find("Marker");
                if (foundMarker != null)
                    GameObject.DestroyImmediate(foundMarker.gameObject);

                if (avatar.isHuman)
                {
                    HumanBodyBones[] bonesToSearch = { 
                        HumanBodyBones.LeftHand, 
                        HumanBodyBones.RightHand,
                        HumanBodyBones.LeftIndexDistal, 
                        HumanBodyBones.RightIndexDistal,
                        HumanBodyBones.LeftLowerArm,
                        HumanBodyBones.RightLowerArm
                    };

                    for (int i = 0; i < bonesToSearch.Length; i++)
                    {
                        // Marker Targets
                        GameObject foundTargetPCLeft = ScriptFunctions.FindObject(descriptor, bonesToSearch[i], "MarkerTargetLeft", searchAllChildren: true);
                        if (foundTargetPCLeft != null)
                            GameObject.DestroyImmediate(foundTargetPCLeft);

                        GameObject foundTargetPCRight = ScriptFunctions.FindObject(descriptor, bonesToSearch[i], "MarkerTargetRight", searchAllChildren: true);
                        if (foundTargetPCRight != null)
                            GameObject.DestroyImmediate(foundTargetPCRight);

                        GameObject foundTargetQuest = ScriptFunctions.FindObject(descriptor, bonesToSearch[i], "Marker", true);
                        if (foundTargetQuest != null)
                            GameObject.DestroyImmediate(foundTargetQuest);

                        // Menu targets
                        GameObject foundMenuTargetPCLeft = ScriptFunctions.FindObject(descriptor, bonesToSearch[i], "MenuTargetLeft", searchAllChildren: true);
                        if (foundMenuTargetPCLeft != null)
                            GameObject.DestroyImmediate (foundMenuTargetPCLeft);

                        GameObject foundMenuTargetPCRight = ScriptFunctions.FindObject(descriptor, bonesToSearch[i], "MenuTargetRight", searchAllChildren: true);
                        if (foundMenuTargetPCRight != null)
                            GameObject.DestroyImmediate(foundMenuTargetPCRight);
                    }
                }
            }
        }
        #region PC

        static AnimatorController GeneratePCAnimatorFXOnExisting(VRCAvatarDescriptor descriptor, Marker marker, string directory)
        {
            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath($"{directory}/Temp_Controller.controller");

            if (controller == null)
            {
                Debug.LogError($"{MarkerStaticResources.MarkerLogTag}Failed to create the temporary controller");
                return new AnimatorController();
            }

            if (controller.layers.Count() > 0)
            {
                if (controller.layers[0].name == "Base Layer")
                {
                    controller.RemoveLayer(0);
                }
            }

            // Draw Layer
            ScriptFunctions.MergeController(controller, ScriptFunctions.GetControllerFromGUID(MarkerStaticResources.DrawLayer), directory);
            ChangeGestureCondition(controller, 0, marker.gestureToDraw);

            // Blend Tree Layer
            if (marker.brushSize && marker.separateEraserScaling)
            {
                ScriptFunctions.MergeController(controller, ScriptFunctions.GetControllerFromGUID(MarkerStaticResources.BlendTreeSeparateSize), directory);
            }
            else if (marker.brushSize && !marker.separateEraserScaling)
            {
                ScriptFunctions.MergeController(controller, ScriptFunctions.GetControllerFromGUID(MarkerStaticResources.BlendTreeCombinedSize), directory);
            }
            else
            {
                ScriptFunctions.MergeController(controller, ScriptFunctions.GetControllerFromGUID(MarkerStaticResources.BlendTreeNoSize), directory);
            }

            // Space Layer
            if (marker.localSpace)
            {
                ScriptFunctions.MergeController(controller, ScriptFunctions.GetControllerFromGUID(MarkerStaticResources.SpaceComplex), directory);
            }
            else
            {
                ScriptFunctions.MergeController(controller, ScriptFunctions.GetControllerFromGUID(MarkerStaticResources.SpaceSimple), directory);
            }

            // Menu Layers
            if (marker.withMenu)
            {
                // Menu Interactions Layer
                if (marker.brushSize)
                {
                    if (marker.separateEraserScaling)
                    {
                        if (marker.localSpace)
                        {
                            ScriptFunctions.MergeController(controller, ScriptFunctions.GetControllerFromGUID(MarkerStaticResources.MenuInteractionsSeparateSizeComplexSpace), directory);
                        }
                        else
                        {
                            ScriptFunctions.MergeController(controller, ScriptFunctions.GetControllerFromGUID(MarkerStaticResources.MenuInteractionsSeparateSizeSimpleSpace), directory);
                        }
                    }
                    else
                    {
                        if (marker.localSpace)
                        {
                            ScriptFunctions.MergeController(controller, ScriptFunctions.GetControllerFromGUID(MarkerStaticResources.MenuInteractionsCombinedSizeComplexSpace), directory);
                        }
                        else
                        {
                            ScriptFunctions.MergeController(controller, ScriptFunctions.GetControllerFromGUID(MarkerStaticResources.MenuInteractionsCombinedSizeSimpleSpace), directory);
                        }
                    }
                }
                else
                {
                    if (marker.localSpace)
                    {
                        ScriptFunctions.MergeController(controller, ScriptFunctions.GetControllerFromGUID(MarkerStaticResources.MenuInteractionsNoSizeComplexSpace), directory);
                    }
                    else
                    {
                        ScriptFunctions.MergeController(controller, ScriptFunctions.GetControllerFromGUID(MarkerStaticResources.MenuInteractionsNoSizeSimpleSpace), directory);
                    }
                }

                ScriptFunctions.MergeController(controller, ScriptFunctions.GetControllerFromGUID(MarkerStaticResources.MenuToggles), directory);
            }
            else
            {
                ScriptFunctions.RemoveTopLevelBlendTreeFromDirectBlendTree(controller, "VRLabs/Marker/BlendTree", "Interactions");
                ScriptFunctions.RemoveParameter(controller, "VRLabs/Marker/InteractAll");
            }

            if (controller) EditorUtility.SetDirty(controller);

            return controller;
        }

        static AnimatorController GeneratePCAnimatorFXOnCopy(VRCAvatarDescriptor descriptor, Marker marker, string directory)
        {
            AnimatorController controller = GeneratePCAnimatorFXOnExisting(descriptor, marker, directory);

            var fx = descriptor.baseAnimationLayers[(int)ScriptFunctions.PlayableLayer.FX].animatorController;

            string nameAddition = "";

            if (!fx.name.EndsWith("_Marker"))
            {
                nameAddition = "_Marker";
            }

            AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(fx), directory + $"/{fx.name}{nameAddition}.controller");
            var newfx = AssetDatabase.LoadAssetAtPath<AnimatorController>(directory + $"/{fx.name}{nameAddition}.controller");

            if (newfx.layers.Length > 0)
            {
                if (controller.layers.Length > 0)
                {
                    if (controller.layers[0].name == "Base Layer")
                    {
                        controller.RemoveLayer(0);
                    }
                }
            }

            ScriptFunctions.MergeController(newfx, controller, directory);
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(controller));

            return newfx;
        }

        static AnimatorController GeneratePCAnimatorGestureOnExisting(VRCAvatarDescriptor descriptor, Marker marker, string directory)
        {
            string animatorControllerPath = "";

            if (!marker.useIndexFinger)
            {
                animatorControllerPath = AssetDatabase.GUIDToAssetPath(MarkerStaticResources.GestureWithPen);
            }
            else
            {
                animatorControllerPath = AssetDatabase.GUIDToAssetPath(MarkerStaticResources.GestureNoPen);
            }

            AssetDatabase.CopyAsset(
                animatorControllerPath,
                $"{directory}/gestureTemp.controller");
            AnimatorController gesture = AssetDatabase.LoadAssetAtPath<AnimatorController>($"{directory}/gestureTemp.controller");

            if (descriptor.baseAnimationLayers[2].isDefault == true || descriptor.baseAnimationLayers[2].animatorController == null)
            {
                string originalControllerPath =
                    AssetDatabase.LoadAssetAtPath<AnimatorController>(
                        $"{A_SHARED_RESOURCES_DIR}/Default/M_DefaultGesture.controller") != null
                        ? $"{A_SHARED_RESOURCES_DIR}/Default/M_DefaultGesture.controller"
                        : AssetDatabase.GUIDToAssetPath("a472e02366708f243a73214dfbdec21f"); 
                AssetDatabase.CopyAsset(
                    originalControllerPath,
                    $"{directory}/Gesture.controller");
                AnimatorController gestureOriginal = AssetDatabase.LoadAssetAtPath<AnimatorController>($"{directory}/Gesture.controller");

                descriptor.customExpressions = true;
                descriptor.baseAnimationLayers[2].isDefault = false;
                descriptor.baseAnimationLayers[2].animatorController = gestureOriginal;

                if (marker.wdSetting)
                {
                    ScriptFunctions.SetWriteDefaults(gestureOriginal);
                    EditorUtility.SetDirty(gestureOriginal);
                }
            }

            if (marker.gestureToDraw != 3)
            {
                ChangeGestureCondition(gesture, 0, marker.gestureToDraw);

                if (gesture.layers.Length > 1)
                {
                    ChangeGestureCondition(gesture, 1, marker.gestureToDraw);
                }
            }
            if (marker.wdSetting)
            {
                ScriptFunctions.SetWriteDefaults(gesture, true);
            }

            EditorUtility.SetDirty(gesture);

            return gesture;
        }

        static AnimatorController GeneratePCAnimatorGestureOnCopy(VRCAvatarDescriptor descriptor, Marker marker, string directory)
        {
            AnimatorController gesture = GeneratePCAnimatorGestureOnExisting(descriptor, marker, directory);

            var aviGesture = descriptor.baseAnimationLayers[(int)ScriptFunctions.PlayableLayer.Gesture].animatorController;

            string nameAddition = "";

            if (!aviGesture.name.EndsWith("_Marker"))
            {
                nameAddition = "_Marker";
            }

            AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(aviGesture), $"{directory}/{aviGesture.name}{nameAddition}.controller");
            AnimatorController newGesture = AssetDatabase.LoadAssetAtPath<AnimatorController>($"{directory}/{aviGesture.name}{nameAddition}.controller");
            ScriptFunctions.MergeController(newGesture, gesture, directory);
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(gesture));

            return newGesture;
        }


        static void GeneratePCPrefab(VRCAvatarDescriptor descriptor, Marker marker)
        {
            Animator avatar = descriptor.GetComponent<Animator>();

            // Physical Setup
            GameObject markerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{A_PC_MARKER_DIR}/Marker.prefab");
            if (markerPrefab == null) markerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath("a91e0ccd554763b428192a6bbadba397"));
            markerPrefab = PrefabUtility.InstantiatePrefab(markerPrefab) as GameObject;
            
            if (PrefabUtility.IsPartOfPrefabInstance(markerPrefab))
                PrefabUtility.UnpackPrefabInstance(markerPrefab, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

            markerPrefab.transform.SetParent(avatar.transform, false);
            Transform system = markerPrefab.transform.Find("System");
            Transform targets = markerPrefab.transform.Find("Targets");
            Transform markerTargetLeft = targets.Find("MarkerTargetLeft");
            Transform markerTargetRight = targets.Find("MarkerTargetRight");
            Transform menuTargetLeft = targets.Find("MenuTargetLeft");
            Transform menuTargetRight = targets.Find("MenuTargetRight");
            Transform markerModel = system.Find("Model");
            Transform markerScale = targets.Find("MarkerScale");
            Transform menuContainer = markerPrefab.transform.Find("Menu");
            Transform menu = (menuContainer == null) ? null : menuContainer.Find("Marker Menu");
            Transform menuScale = targets.Find("MenuScale");

            if (marker.useIndexFinger)
            {
                GameObject.DestroyImmediate(targets.Find("Marker Flip").gameObject);// markerTargetLeft.GetChild(0).gameObject); // destroy Flip
                Transform indexDistalLeft = avatar.GetBoneTransform(HumanBodyBones.LeftIndexDistal);
                Transform indexDistalRight = avatar.GetBoneTransform(HumanBodyBones.RightIndexDistal);

                // prefer the end bone of the index finger if it exists
                if (indexDistalLeft.Find(indexDistalLeft.gameObject.name + "_end") != null)
                    markerTargetLeft.SetParent(indexDistalLeft.Find(indexDistalLeft.gameObject.name + "_end"), worldPositionStays: true);
                else
                    markerTargetLeft.SetParent(indexDistalLeft, worldPositionStays: true);
                markerTargetLeft.localPosition = Vector3.zero;
                markerTargetLeft.localRotation = Quaternion.Euler(0f, 0f, 0f);

                if (indexDistalRight.Find(indexDistalRight.gameObject.name + "_end") != null)
                    markerTargetRight.SetParent(indexDistalRight.Find(indexDistalRight.gameObject.name + "_end"), worldPositionStays: true);
                else
                    markerTargetRight.SetParent(indexDistalRight, worldPositionStays: true);
                markerTargetRight.localPosition = Vector3.zero;
                markerTargetRight.localRotation = Quaternion.Euler(0f, 0f, 0f);

                SphereCollider originalEraserCollider = markerModel.GetComponent<SphereCollider>();

                if (originalEraserCollider != null)
                {
                    GameObject FinalEraser = new GameObject("Eraser");
                    SphereCollider newEraser = FinalEraser.AddComponent<SphereCollider>();
                    newEraser.radius = originalEraserCollider.radius;
                    newEraser.enabled = false;

                    GameObject EraserRenderer = new GameObject("EraserRenderer");
                    EraserRenderer.transform.localScale = new Vector3(newEraser.radius * 2, newEraser.radius * 2, newEraser.radius * 2);
                    EraserRenderer.transform.SetParent(FinalEraser.transform, false);
                    
                    if (system != null)
                    {
                        Transform Draw = system.Find("Draw");

                        if (Draw != null)
                        {
                            ParticleSystem particleSystem = Draw.GetComponent<ParticleSystem>();

                            if (particleSystem != null)
                            {
                                ParticleSystem.TriggerModule triggerModule = particleSystem.trigger;
                                triggerModule.SetCollider(0, newEraser);
                            }
                        }
                    }

                    // Add a Mesh Filter with a Unity Sphere
                    MeshFilter EraserMeshFilter = EraserRenderer.AddComponent<MeshFilter>();
                    GameObject tempSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    Mesh sphereMesh = tempSphere.GetComponent<MeshFilter>().sharedMesh;
                    EraserMeshFilter.sharedMesh = sphereMesh;
                    Object.DestroyImmediate(tempSphere);

                    // Add the material to the Mesh Renderer
                    MeshRenderer EraserMeshRenderer = EraserRenderer.AddComponent<MeshRenderer>();
                    string IndexEraserMaterialGuid = "6659f7fd2b84db74ab6b3bd32b060786"; // M_Mat PC Eraser Index
                    string IndexEraserMaterialPath = AssetDatabase.GUIDToAssetPath(IndexEraserMaterialGuid);

                    if (IndexEraserMaterialPath != null)
                    {
                        Material IndexEraserMaterialMaterial = AssetDatabase.LoadAssetAtPath<Material>(IndexEraserMaterialPath);

                        if (IndexEraserMaterialMaterial != null)
                        {
                            EraserMeshRenderer.material = IndexEraserMaterialMaterial;
                        }
                    }

                    EraserMeshRenderer.enabled = false;
                    FinalEraser.transform.SetParent(system, false);
                }
                
                Object.DestroyImmediate(markerModel.GetComponent<VRCContactSender>());
            }
            else // using model: scale Model to target freely, and until script is destroyed, scale System to target uniformly with X-axis 
            {
                //markerModel.SetParent(system); // move it out of Targets hierarchy

                Transform handLeft = avatar.GetBoneTransform(HumanBodyBones.LeftHand);
                Transform handRight = avatar.GetBoneTransform(HumanBodyBones.RightHand);

                Transform elbowLeft = avatar.GetBoneTransform(HumanBodyBones.LeftLowerArm);
                Transform elbowRight = avatar.GetBoneTransform(HumanBodyBones.RightLowerArm);

                // need to flip the target(model). we can use the Flip object by resetting markertarget transform,
                // getting Flip's position, then rotating markertarget
                {
                    markerTargetLeft.SetParent(handLeft, true);
                    markerTargetLeft.localPosition = Vector3.zero;
                    markerTargetLeft.localRotation = Quaternion.Euler(0f, 0f, 0f);
                    //markerTargetLeft.position = markerTargetLeft.GetChild(0).transform.position;
                    markerTargetLeft.localPosition = new Vector3(0f, markerTargetLeft.localPosition.y, 0f); // ignore offset on x and z
                    markerTargetLeft.localRotation = Quaternion.Euler(0f, 0f, 180f); // and flip the rotation
                }

                {
                    markerTargetRight.SetParent(handRight, true);
                    markerTargetRight.localPosition = Vector3.zero;
                    markerTargetRight.localRotation = Quaternion.Euler(0f, 0f, 0f);
                    //markerTargetRight.position = markerTargetRight.GetChild(0).transform.position;
                    markerTargetRight.localPosition = new Vector3(0f, markerTargetRight.localPosition.y, 0f); // ignore offset on x and z
                    markerTargetRight.localRotation = Quaternion.Euler(0f, 0f, 180f); // and flip the rotation
                }

                marker.markerModel = markerModel; // to turn its mesh renderer off when script is finished
            }

            // We install the menu both for pen and index finger modes
            Transform lowerArmLeft = avatar.GetBoneTransform(HumanBodyBones.LeftLowerArm);
            Transform lowerArmRight = avatar.GetBoneTransform(HumanBodyBones.RightLowerArm);

            menuTargetLeft.SetParent(lowerArmLeft, worldPositionStays: true);
            menuTargetLeft.localPosition = Vector3.zero;
            menuTargetLeft.localRotation = Quaternion.Euler(270f, 0f, 0f);

            menuTargetRight.SetParent(lowerArmRight, worldPositionStays: true);
            menuTargetRight.localPosition = Vector3.zero;
            menuTargetRight.localRotation = Quaternion.Euler(270f, 180f, 0f);

            HumanBodyBones[] bones = {
                HumanBodyBones.Hips, HumanBodyBones.Chest, HumanBodyBones.Head,
                HumanBodyBones.LeftHand, HumanBodyBones.RightHand, HumanBodyBones.LeftFoot,
                HumanBodyBones.RightFoot
            };
            Transform WorldObjectOuter = markerPrefab.transform.Find("World");
            VRC.SDK3.Dynamics.Constraint.Components.VRCParentConstraint LocalConstraintOuter = null;

            if (WorldObjectOuter != null)
            {
                Transform LocalObject = WorldObjectOuter.Find("Local");

                if (LocalObject != null)
                {
                    LocalConstraintOuter = LocalObject.GetComponent<VRC.SDK3.Dynamics.Constraint.Components.VRCParentConstraint>();
                }
            }
            
            if (marker.localSpace)
            {
                LocalConstraintOuter.Sources[0] = new VRC.Dynamics.VRCConstraintSource()
                {
                    SourceTransform = avatar.transform,
                    Weight = 1f
                };

                for (int i = 0; i < 7; i++)
                {
                    LocalConstraintOuter.Sources[i + 1] = new VRC.Dynamics.VRCConstraintSource()
                    {
                        SourceTransform = avatar.GetBoneTransform(bones[i]),
                        Weight = 0f
                    };
                }
            }
            else
            {
                Transform WorldObject = markerPrefab.transform.Find("World");

                if (WorldObject != null)
                {
                    Transform LocalObject = WorldObject.Find("Local");

                    if (LocalObject != null)
                    {
                        VRC.SDK3.Dynamics.Constraint.Components.VRCParentConstraint LocalConstraint = LocalObject.GetComponent<VRC.SDK3.Dynamics.Constraint.Components.VRCParentConstraint>();

                        if (LocalConstraint != null)
                        {
                            LocalConstraint.Sources = new VRC.Dynamics.VRCConstraintSourceKeyableList()
                            {
                                new VRC.Dynamics.VRCConstraintSource()
                                {
                                    SourceTransform = avatar.transform,
                                    Weight = 1f
                                }
                            };
                        }
                    }
                }

                Transform Space = menu.Find("Space");

                if (Space != null)
                    GameObject.DestroyImmediate(Space.gameObject);
            }

            VRC.SDK3.Dynamics.Constraint.Components.VRCScaleConstraint TargetsScaleConstarint = targets.gameObject.GetComponent<VRC.SDK3.Dynamics.Constraint.Components.VRCScaleConstraint>();
            if (TargetsScaleConstarint != null)
            {
                TargetsScaleConstarint.Sources = new VRC.Dynamics.VRCConstraintSourceKeyableList()
                {
                    new VRC.Dynamics.VRCConstraintSource()
                    {
                        SourceTransform = avatar.transform,
                        Weight = 1f
                    }
                };
            }

            // set anything not adjustable to a medium-ish amount
            if (!marker.separateEraserScaling)
            {
                List<Material> sharedMats = new List<Material>();
                markerModel.GetComponent<MeshRenderer>().GetSharedMaterials(sharedMats);
                sharedMats[1].SetFloat("_EraserSize", 0.8419998f);

                SphereCollider collider = markerModel.GetComponent<SphereCollider>();
                collider.radius = 0.018f;
                collider.center = new Vector3(collider.center.x, marker.useIndexFinger ? 0f : 0.196f, collider.center.z);

                //eraser.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
            }
            if (!marker.brushSize)
            {
                ParticleSystem.MinMaxCurve size = new ParticleSystem.MinMaxCurve(0.024f);
                Transform draw = system.transform.Find("Draw");
                //Transform preview = draw.GetChild(0);
                ParticleSystem.MainModule main = draw.GetComponent<ParticleSystem>().main;
                main.startSize = size;
                //main = preview.GetComponent<ParticleSystem>().main;
                main.startSize = size;
            }

            // scale MarkerTarget, which controls prefab size, according to a (normalized) worldspace distance
            // between avatar hips and head
            Transform hips = avatar.GetBoneTransform(HumanBodyBones.Hips);
            Transform head = avatar.GetBoneTransform(HumanBodyBones.Head);
            Vector3 dist = (head.position - hips.position);

            float normalizedDist = (Math.Max(Math.Max(dist.x, dist.y), dist.z) / KSIVL_UNIT);
            float newScale = markerTargetLeft.localScale.x * normalizedDist;
            markerTargetLeft.localScale = new Vector3(newScale, newScale, newScale);

            marker.system = system;
            marker.markerTargetLeft = markerTargetLeft;
            marker.markerTargetRight = markerTargetRight;
            marker.markerScale = markerScale;
            marker.menuTargetLeft = menuTargetLeft;
            marker.menuTargetRight = menuTargetRight;
            marker.menu = menu;
            marker.menuScale = menuScale;

            // We are not using the pen
            if (marker.useIndexFinger)
            {
                Object.DestroyImmediate(markerModel.gameObject);
            }

            if (!marker.withMenu)
            {
                Object.DestroyImmediate(menuContainer.gameObject);
            }

            if (!marker.brushSize && marker.withMenu && menu != null)
            {
                Transform Main = menu.Find("Main");

                if (Main != null)
                {
                    Transform Size = Main.Find("Size");

                    if (Size != null)
                    {
                        GameObject.DestroyImmediate(Size.gameObject);
                    }
                }

                SkinnedMeshRenderer menuMesh = menu.GetComponent<SkinnedMeshRenderer>();

                if (menuMesh != null)
                {
                    menuMesh.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(MarkerStaticResources.MarkerNoSizeMaterial));
                }
            }
        }

        public static Dictionary<ScriptFunctions.PlayableLayer, AnimatorController> GeneratePC(VRCAvatarDescriptor descriptor, ref Marker marker, string directory, bool mergeOnCopy)
        {
            // Physical Install
            GeneratePCPrefab(descriptor, marker);

            Dictionary<ScriptFunctions.PlayableLayer, AnimatorController> controllers = new Dictionary<ScriptFunctions.PlayableLayer, AnimatorController>();


            // Animators
            if (!mergeOnCopy)
            {
                controllers = new Dictionary<ScriptFunctions.PlayableLayer, AnimatorController>() {
                    { ScriptFunctions.PlayableLayer.FX, GeneratePCAnimatorFXOnExisting(descriptor, marker, directory) }
                };
            }
            else
            {
                controllers = new Dictionary<ScriptFunctions.PlayableLayer, AnimatorController>() {
                    { ScriptFunctions.PlayableLayer.FX, GeneratePCAnimatorFXOnCopy(descriptor, marker, directory) }
                };
            }

            if (!mergeOnCopy)
            {
                controllers.Add(
                    ScriptFunctions.PlayableLayer.Gesture,
                    GeneratePCAnimatorGestureOnExisting(descriptor, marker, $"{directory}/")
                );
            }
            else
            {
                controllers.Add(
                    ScriptFunctions.PlayableLayer.Gesture,
                    GeneratePCAnimatorGestureOnCopy(descriptor, marker, $"{directory}/")
                );
            }

            #region Parameters
            // Parameters
            VRCExpressionParameters.Parameter
            p_marker = new VRCExpressionParameters.Parameter
            { name = "VRLabs/Marker/Enable", valueType = VRCExpressionParameters.ValueType.Bool, saved = false },
            p_eraser = new VRCExpressionParameters.Parameter
            { name = "VRLabs/Marker/Eraser", valueType = VRCExpressionParameters.ValueType.Bool, saved = false },
            p_clear = new VRCExpressionParameters.Parameter
            { name = "VRLabs/Marker/Clear", valueType = VRCExpressionParameters.ValueType.Bool, saved = false },
            p_color = new VRCExpressionParameters.Parameter
            { name = "VRLabs/Marker/Color", valueType = VRCExpressionParameters.ValueType.Float, saved = true };
            ScriptFunctions.AddParameter(descriptor, p_marker, $"{directory}/");
            ScriptFunctions.AddParameter(descriptor, p_eraser, $"{directory}/");
            ScriptFunctions.AddParameter(descriptor, p_clear, $"{directory}/");
            ScriptFunctions.AddParameter(descriptor, p_color, $"{directory}/");
            #endregion Parameters

            #region Menus
            // Menus
            if (marker.localSpace)
            {
                VRCExpressionParameters.Parameter p_space = new VRCExpressionParameters.Parameter
                { name = "VRLabs/Marker/Space", valueType = VRCExpressionParameters.ValueType.Int, saved = false };
                ScriptFunctions.AddParameter(descriptor, p_space, $"{directory}/");
            }
            else
            {
                VRCExpressionParameters.Parameter p_spaceSimple = new VRCExpressionParameters.Parameter
                { name = "VRLabs/Marker/SpaceSimple", valueType = VRCExpressionParameters.ValueType.Bool, saved = false };
                ScriptFunctions.AddParameter(descriptor, p_spaceSimple, $"{directory}/");
            }

            if (marker.brushSize)
            {
                VRCExpressionParameters.Parameter p_size = new VRCExpressionParameters.Parameter
                { name = "VRLabs/Marker/Size", valueType = VRCExpressionParameters.ValueType.Float, saved = false };
                ScriptFunctions.AddParameter(descriptor, p_size, $"{directory}/");
            }

            if (marker.separateEraserScaling)
            {
                VRCExpressionParameters.Parameter p_eraserSize = new VRCExpressionParameters.Parameter
                { name = "VRLabs/Marker/EraserSize", valueType = VRCExpressionParameters.ValueType.Float, saved = false };
                ScriptFunctions.AddParameter(descriptor, p_eraserSize, $"{directory}/");
            }

            if (marker.withMenu)
            {
                VRCExpressionParameters.Parameter p_menuEnable = new VRCExpressionParameters.Parameter
                { name = "VRLabs/Marker/Menu/Enable", valueType = VRCExpressionParameters.ValueType.Bool, saved = true };
                ScriptFunctions.AddParameter(descriptor, p_menuEnable, $"{directory}/");

                VRCExpressionParameters.Parameter SpaceOpen = new VRCExpressionParameters.Parameter
                { name = "VRLabs/Marker/Menu/SpaceOpen", valueType = VRCExpressionParameters.ValueType.Bool, saved = false };
                ScriptFunctions.AddParameter(descriptor, SpaceOpen, $"{directory}/");

                VRCExpressionParameters.Parameter SizeOpen = new VRCExpressionParameters.Parameter
                { name = "VRLabs/Marker/Menu/SizeOpen", valueType = VRCExpressionParameters.ValueType.Bool, saved = false };
                ScriptFunctions.AddParameter(descriptor, SizeOpen, $"{directory}/");

                VRCExpressionParameters.Parameter ColorOpen = new VRCExpressionParameters.Parameter
                { name = "VRLabs/Marker/Menu/ColorOpen", valueType = VRCExpressionParameters.ValueType.Bool, saved = false };
                ScriptFunctions.AddParameter(descriptor, ColorOpen, $"{directory}/");

                VRCExpressionParameters.Parameter interactAll = new VRCExpressionParameters.Parameter
                { name = "VRLabs/Marker/InteractAll", valueType = VRCExpressionParameters.ValueType.Float, saved = true, networkSynced = false };
                ScriptFunctions.AddParameter(descriptor, interactAll, $"{directory}/");
            }

            VRCExpressionParameters.Parameter p_leftHand = new VRCExpressionParameters.Parameter
            { name = "VRLabs/Marker/LeftHand", valueType = VRCExpressionParameters.ValueType.Bool, saved = true };
            ScriptFunctions.AddParameter(descriptor, p_leftHand, $"{directory}/");

            // handle menu instancing
            string markerMenuPath =
                AssetDatabase.LoadAssetAtPath<VRCExpressionsMenu>($"{A_PC_MARKER_DIR}/M_Menu.asset") != null
                    ? $"{A_PC_MARKER_DIR}/M_Menu.asset"
                    : AssetDatabase.GUIDToAssetPath("ffb2ec12140d2bc43a5954032d31040c");
            AssetDatabase.CopyAsset(markerMenuPath, $"{directory}/Marker Menu.asset");
            VRCExpressionsMenu markerMenu = AssetDatabase.LoadAssetAtPath<VRCExpressionsMenu>($"{directory}/Marker Menu.asset");

            string optionsMenuPath = AssetDatabase.GUIDToAssetPath("dcba582051fed9d48b8b9a7d594d0432");
            AssetDatabase.CopyAsset(optionsMenuPath, $"{directory}/Marker Menu Options.asset");
            VRCExpressionsMenu optionsMenu = AssetDatabase.LoadAssetAtPath<VRCExpressionsMenu>($"{directory}/Marker Menu Options.asset");

            // Let's replace the Options menu reference with the new, ducplicated one
            for (int i = 0; i < markerMenu.controls.Count; i++)
            {
                if (markerMenu.controls[i].name == "Options" && markerMenu.controls[i].type == VRCExpressionsMenu.Control.ControlType.SubMenu)
                    markerMenu.controls[i].subMenu = optionsMenu;
            }

            if (!marker.localSpace) // change from submenu to 1 toggle
            {
                VRCExpressionsMenu.Control.Parameter pm_spaceSimple = new VRCExpressionsMenu.Control.Parameter
                { name = "VRLabs/Marker/SpaceSimple" };

                markerMenu.controls[(int)MainMenuItems.LocalSpace].type = VRCExpressionsMenu.Control.ControlType.Toggle;
                markerMenu.controls[(int)MainMenuItems.LocalSpace].parameter = pm_spaceSimple;
                markerMenu.controls[(int)MainMenuItems.LocalSpace].subMenu = null; // or else the submenu is still there internally, SDK complains
            }
            else
            {
                string submenuPath = AssetDatabase.LoadAssetAtPath<VRCExpressionsMenu>($"{A_PC_MARKER_DIR}/M_Menu Space.asset") != null ? $"{A_PC_MARKER_DIR}/M_Menu Space.asset"  : AssetDatabase.GUIDToAssetPath("9a2dadf382135c54884a79615a425e44");
                AssetDatabase.CopyAsset(submenuPath, $"{directory}/Marker Space Submenu.asset");
                VRCExpressionsMenu subMenu = AssetDatabase.LoadAssetAtPath(
                    submenuPath, typeof(VRCExpressionsMenu)
                ) as VRCExpressionsMenu;

                markerMenu.controls[(int)MainMenuItems.LocalSpace].subMenu = subMenu;
                EditorUtility.SetDirty(subMenu);
            }

            if (marker.brushSize && marker.separateEraserScaling)
            {
                // Remove the combined size control, as we want to scale them separately
                ScriptFunctions.RemoveMenuControl(optionsMenu, "<color=#FFFFFF><line-height=100%><voffset=-2em>Brush/Eraser Size");
            }

            if (marker.brushSize && !marker.separateEraserScaling)
            {
                // Remove the separate size controls, as we only want to scale them together
                ScriptFunctions.RemoveMenuControl(optionsMenu, "<color=#FFFFFF>Brush Size");
                ScriptFunctions.RemoveMenuControl(optionsMenu, "<color=#FFFFFF>Eraser Size");
            }

            if (!marker.brushSize)
            {
                // Remove all controls, because scaling is disabled
                ScriptFunctions.RemoveMenuControl(optionsMenu, "<color=#FFFFFF><line-height=100%><voffset=-2em>Brush/Eraser Size");
                ScriptFunctions.RemoveMenuControl(optionsMenu, "<color=#FFFFFF>Brush Size");
                ScriptFunctions.RemoveMenuControl(optionsMenu, "<color=#FFFFFF>Eraser Size");
            }

            if (!marker.withMenu)
            {
                ScriptFunctions.RemoveMenuControl(markerMenu, "Menu <color=#6FFF00>On</color>");
                ScriptFunctions.RemoveMenuControl(optionsMenu, "<color=#FFFFFF><line-height=100%><voffset=-2em>Others can use Menu");
            }

            EditorUtility.SetDirty(markerMenu);

            VRCExpressionsMenu.Control.Parameter pm_menu = new VRCExpressionsMenu.Control.Parameter();
            Texture2D markerIcon = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath("d7f5ccd8035dd4d4d9f80706d359fdfa"), typeof(Texture2D)) as Texture2D;
            ScriptFunctions.AddSubMenu(descriptor, markerMenu, "Marker", $"{directory}/", pm_menu, markerIcon);
            #endregion Menus

            return controllers;
        }

        private static void ChangeGestureCondition(AnimatorController controller, int layerToModify, int newGesture)
        {   // helper function: change gesture condition, in all transitions of 1 layer of controller
            if (controller == null)
            {
                Debug.LogError($"{MarkerStaticResources.MarkerLogTag}Couldn't change gesture conditions. controller was null.");
                return;
            }

            AnimatorStateTransition[] transitions = controller.layers[layerToModify]
                .stateMachine.states.SelectMany(x => x.state.transitions).ToArray();

            AnimatorCondition[] conditions;
            for (int i = 0; i < transitions.Length; i++)
            {
                conditions = transitions[i].conditions;
                for (int j = 0; j < conditions.Length; j++)
                {
                    if (conditions[j].parameter.Contains("Gesture"))
                    {
                        AnimatorCondition conditionToRemove = conditions[j];
                        transitions[i].RemoveCondition(conditionToRemove);
                        transitions[i].AddCondition(conditionToRemove.mode, newGesture, conditionToRemove.parameter);
                        break; // in my case, only one condition per transition includes GestureLeft / GestureRight
                    }
                }
            }
        }
        #endregion

        #region Quest
        public static Dictionary<ScriptFunctions.PlayableLayer, AnimatorController> GenerateQuest(VRCAvatarDescriptor descriptor, ref Marker marker, string directory, bool mergeOnCopy)
        {
            // instantiate marker prefab
            GameObject markerPrefab = Resources.Load(R_QUEST_MARKER_PATH) as GameObject;
            if(markerPrefab == null) {
                throw new NullReferenceException("Quest Marker Prefab not found");
            }
            markerPrefab = PrefabUtility.InstantiatePrefab(markerPrefab) as GameObject;
            PrefabUtility.UnpackPrefabInstance(markerPrefab, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            markerPrefab.name = "Marker";

            Animator avatar = descriptor.gameObject.GetComponent<Animator>();

            markerPrefab.transform.SetParent(avatar.transform, false);
            Transform targets = markerPrefab.transform.Find("Targets");
            Transform markerTargetLeft = targets.Find("MarkerTargetLeft");
            Transform markerTargetRight = targets.Find("MarkerTargetRight");

            if (marker.useIndexFinger)
            {
                Transform indexDistalLeft = avatar.GetBoneTransform(HumanBodyBones.LeftIndexDistal);
                Transform indexDistalRight = avatar.GetBoneTransform(HumanBodyBones.RightIndexDistal);

                // prefer the end bone of the index finger if it exists
                if (indexDistalLeft.Find(indexDistalLeft.gameObject.name + "_end") != null)
                    markerTargetLeft.SetParent(indexDistalLeft.Find(indexDistalLeft.gameObject.name + "_end"), worldPositionStays: true);
                else
                    markerTargetLeft.SetParent(indexDistalLeft, worldPositionStays: true);
                markerTargetLeft.localPosition = Vector3.zero;
                markerTargetLeft.localRotation = Quaternion.Euler(0f, 0f, 0f);

                if (indexDistalRight.Find(indexDistalRight.gameObject.name + "_end") != null)
                    markerTargetRight.SetParent(indexDistalRight.Find(indexDistalRight.gameObject.name + "_end"), worldPositionStays: true);
                else
                    markerTargetRight.SetParent(indexDistalRight, worldPositionStays: true);

                markerTargetRight.localPosition = Vector3.zero;
                markerTargetRight.localRotation = Quaternion.Euler(0f, 0f, 0f);

                GameObject.DestroyImmediate(markerPrefab.transform.Find("Marker").gameObject);
            }
            else // using model: scale Model to target freely, and until script is destroyed, scale System to target uniformly with X-axis
            {
                Transform handLeft = avatar.GetBoneTransform(HumanBodyBones.LeftHand);
                Transform handRight = avatar.GetBoneTransform(HumanBodyBones.RightHand);

                // need to flip the target(model). we can use the Flip object by resetting markertarget transform,
                // getting Flip's position, then rotating markertarget
                {
                    markerTargetLeft.SetParent(handLeft, true);
                    markerTargetLeft.localPosition = Vector3.zero;
                    markerTargetLeft.localRotation = Quaternion.Euler(0f, 0f, 0f);
                    //markerTargetLeft.position = markerTargetLeft.GetChild(0).transform.position;
                    markerTargetLeft.localPosition = new Vector3(0f, markerTargetLeft.localPosition.y, 0f); // ignore offset on x and z
                    markerTargetLeft.localRotation = Quaternion.Euler(0f, 0f, 180f); // and flip the rotation
                }

                {
                    markerTargetRight.SetParent(handRight, true);
                    markerTargetRight.localPosition = Vector3.zero;
                    markerTargetRight.localRotation = Quaternion.Euler(0f, 0f, 0f);
                    //markerTargetRight.position = markerTargetRight.GetChild(0).transform.position;
                    markerTargetRight.localPosition = new Vector3(0f, markerTargetRight.localPosition.y, 0f); // ignore offset on x and z
                    markerTargetRight.localRotation = Quaternion.Euler(0f, 0f, 180f); // and flip the rotation
                }
            }

            marker.markerTargetRight = markerTargetRight;
            marker.markerTargetLeft = markerTargetLeft;

            GameObject.DestroyImmediate(targets.gameObject);

            // Animators
            Dictionary<ScriptFunctions.PlayableLayer, AnimatorController> controllers = new Dictionary<ScriptFunctions.PlayableLayer, AnimatorController>();

            if (!mergeOnCopy)
            {
                controllers = new Dictionary<ScriptFunctions.PlayableLayer, AnimatorController>() {
                    { ScriptFunctions.PlayableLayer.FX, GenerateQuestAnimatorFXOnExisting(descriptor, marker, directory) }
                };
            }
            else
            {
                controllers = new Dictionary<ScriptFunctions.PlayableLayer, AnimatorController>() {
                    { ScriptFunctions.PlayableLayer.FX, GenerateQuestAnimatorFXOnCopy(descriptor, marker, directory) }
                };
            }

            if (!mergeOnCopy)
            {
                controllers.Add(
                    ScriptFunctions.PlayableLayer.Gesture,
                    GeneratePCAnimatorGestureOnExisting(descriptor, marker, $"{directory}/")
                );
            }
            else
            {
                controllers.Add(
                    ScriptFunctions.PlayableLayer.Gesture,
                    GeneratePCAnimatorGestureOnCopy(descriptor, marker, $"{directory}/")
                );
            }

            #region Parameters
            // Parameters
            VRCExpressionParameters.Parameter
            p_marker = new VRCExpressionParameters.Parameter
            { name = M_MARKER_PARAM, valueType = VRCExpressionParameters.ValueType.Bool, saved = false },
            p_clear = new VRCExpressionParameters.Parameter
            { name = M_MARKER_CLEAR_PARAM, valueType = VRCExpressionParameters.ValueType.Float, saved = true },
            p_color = new VRCExpressionParameters.Parameter
            { name = M_COLOR_PARAM, valueType = VRCExpressionParameters.ValueType.Float, saved = true };
            
            ScriptFunctions.AddParameter(descriptor, p_marker, directory);
            ScriptFunctions.AddParameter(descriptor, p_clear, directory);
            ScriptFunctions.AddParameter(descriptor, p_color, directory);

            VRCExpressionParameters.Parameter p_leftHand = new VRCExpressionParameters.Parameter
            { name = M_LEFTHAND_PARAM_NAME, valueType = VRCExpressionParameters.ValueType.Bool, saved = true };
            ScriptFunctions.AddParameter(descriptor, p_leftHand, $"{directory}/");
            #endregion Parameters

            #region Menus
            // Menus
            // handle menu instancing
            string markerMenuPath =
                AssetDatabase.LoadAssetAtPath<VRCExpressionsMenu>($"{A_QUEST_MARKER_DIR}/M_Menu.asset") != null
                    ? $"{A_QUEST_MARKER_DIR}/M_Menu.asset"
                    : AssetDatabase.GUIDToAssetPath("9a8aad38a5126744c994e73f7b2aa8b1");
            AssetDatabase.CopyAsset(markerMenuPath, $"{directory}/Marker Menu.asset");
            VRCExpressionsMenu markerMenu = AssetDatabase.LoadAssetAtPath<VRCExpressionsMenu>($"{directory}/Marker Menu.asset");

            EditorUtility.SetDirty(markerMenu);

            VRCExpressionsMenu.Control.Parameter pm_menu = new VRCExpressionsMenu.Control.Parameter();
            Texture2D markerIcon = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath("d7f5ccd8035dd4d4d9f80706d359fdfa"), typeof(Texture2D)) as Texture2D;
            ScriptFunctions.AddSubMenu(descriptor, markerMenu, "Marker", $"{directory}/", pm_menu, markerIcon);
            #endregion Menus

            Debug.Log("Controller count: " + controllers.Count);

            return controllers;
        }

        static AnimatorController GenerateQuestAnimatorFXOnExisting(VRCAvatarDescriptor descriptor, Marker marker, string directory)
        {
            // Install layers, parameters, and menu before prefab setup
            string controllerPath = $"{A_QUEST_MARKER_DIR}/M_Quest_FX.controller";

            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath) == null) controllerPath = AssetDatabase.GUIDToAssetPath("5747863c98af6984bb06ca3f4087913e");
            string tempFXPath = $"{directory}/FXTemp.controller";
            AssetDatabase.CopyAsset(controllerPath, tempFXPath);

            AnimatorController FX = AssetDatabase.LoadAssetAtPath<AnimatorController>(tempFXPath);

            // determine local space layers
            // set WD
            if (marker.wdSetting)
            {
                ScriptFunctions.SetWriteDefaults(FX);
            }
            if (marker.gestureToDraw != 3) // uses fingerpoint by default
            {
                ChangeGestureCondition(FX, 0, marker.gestureToDraw);
            }

            if (FX) EditorUtility.SetDirty(FX);
            return FX;
        }

        static AnimatorController GenerateQuestAnimatorFXOnCopy(VRCAvatarDescriptor descriptor, Marker marker, string directory)
        {
            AnimatorController temp = GenerateQuestAnimatorFXOnExisting(descriptor, marker, directory);
            var originalFx = descriptor.baseAnimationLayers[(int)ScriptFunctions.PlayableLayer.FX].animatorController;

            string nameAddition = "";

            if (!originalFx.name.EndsWith("_Marker"))
            {
                nameAddition = "_Marker";
            }

            AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(originalFx), $"{directory}/{originalFx.name}{nameAddition}.controller");
            AnimatorController newFx = AssetDatabase.LoadAssetAtPath<AnimatorController>($"{directory}/{originalFx.name}{nameAddition}.controller");

            ScriptFunctions.MergeController(newFx, temp, directory);
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(temp));

            return newFx;
        }
        #endregion Quest
    }
}