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

namespace VRLabs.Marker
{
    public static class MarkerInstaller
    {
        const string A_PC_MARKER_DIR = "Assets/VRLabs/Marker/Resources/PC Marker";
        const string A_GENERATED_ASSETS_DIR = "Assets/VRLabs/GeneratedAssets/Marker";
        const string A_SHARED_RESOURCES_DIR = "Assets/VRLabs/Marker/Resources/Shared/Default";
        const string R_QUEST_MARKER_PATH = "Quest Marker/Marker";
        const float KSIVL_UNIT = 0.4156029f;

        // animator parameters
        const string M_COLOR_PARAM = "VRLabs/Marker/Color";
        const string M_MARKER_PARAM = "VRLabs/Marker/Enable";
        const string M_MARKER_CLEAR_PARAM = "VRLabs/Marker/Clear";
        const string M_NORMALIZED_PARAM_NAME = "VRLabs/Marker/_Normalize";

        public static void Generate(VRCAvatarDescriptor descriptor, ref Marker marker, bool installQuest)
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
                ? GenerateQuest(descriptor, ref marker, directory)
                : GeneratePC(descriptor, ref marker, directory);

            if(generatedControllers == null)
                throw new NullReferenceException("Failed to generate marker controller(s)");

            // Merge Controllers
            foreach (KeyValuePair<ScriptFunctions.PlayableLayer, AnimatorController> kvp in generatedControllers) {
                ScriptFunctions.MergeController(descriptor, kvp.Value, kvp.Key, $"{directory}/");
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(kvp.Value));
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
            Debug.Log("Successfully generated Marker!");
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
                        HumanBodyBones.RightIndexDistal 
                    };

                    for (int i = 0; i < bonesToSearch.Length; i++)
                    {
                        GameObject foundTargetPCLeft = ScriptFunctions.FindObject(descriptor, bonesToSearch[i], "MarkerTargetLeft", true);
                        if (foundTargetPCLeft != null)
                            GameObject.DestroyImmediate(foundTargetPCLeft);

                        GameObject foundTargetPCRight = ScriptFunctions.FindObject(descriptor, bonesToSearch[i], "MarkerTargetRight", true);
                        if (foundTargetPCRight != null)
                            GameObject.DestroyImmediate(foundTargetPCRight);

                        GameObject foundTargetQuest = ScriptFunctions.FindObject(descriptor, bonesToSearch[i], "Marker", true);
                        if (foundTargetQuest != null)
                            GameObject.DestroyImmediate(foundTargetQuest);
                    }
                }
            }
        }

        #region PC

        static AnimatorController GeneratePCAnimatorFX(VRCAvatarDescriptor descriptor, Marker marker, string directory)
        {
            // Install layers, parameters, and menu before prefab setup
            string controllerName = marker.useIndexFinger ? "M_FX (Finger).controller" : "M_FX.controller";
            string tempFXPath = $"{directory}/FXTemp.controller";
            AssetDatabase.CopyAsset($"{A_PC_MARKER_DIR}/{controllerName}", tempFXPath);

            AnimatorController FX = AssetDatabase.LoadAssetAtPath(
                tempFXPath, typeof(AnimatorController)
            ) as AnimatorController;

            // determine local space layers
            // set WD
            if (marker.wdSetting)
            {
                ScriptFunctions.SetWriteDefaults(FX);

                // if we are using WD, we want to set the blendtree's normalize parameter to 1
                AnimatorControllerParameter[] parameters = FX.parameters;
                for(int i = 0; i < parameters.Length; i++) {
                    if (parameters[i].name.Equals(M_NORMALIZED_PARAM_NAME)) {
                        parameters[i].defaultFloat = 1;
                    }
                }
                FX.parameters = parameters;
            }
            if (marker.gestureToDraw != 3) // uses fingerpoint by default
            {
                ChangeGestureCondition(FX, 0, marker.gestureToDraw);
            }

            // Set parameter driver on 'Clear' state to reset local space
            /*AnimatorState state = FX.layers[0].stateMachine.states.FirstOrDefault(s => s.state.name.Equals("Clear")).state;
            //VRCAvatarParameterDriver driver = (VRCAvatarParameterDriver)state.behaviours[0];
            //string driverParamName = marker.localSpace ? "M_Space" : "M_SpaceSimple";
            //VRC.SDKBase.VRC_AvatarParameterDriver.Parameter param = new VRC.SDKBase.VRC_AvatarParameterDriver.Parameter()
            {
                name = driverParamName,
                type = VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType.Set,
                value = 0f
            };
            //driver.parameters.Add(param);
            */
            if(FX) EditorUtility.SetDirty(FX);
            return FX;
        }

        static AnimatorController GeneratePCAnimatorGesture(VRCAvatarDescriptor descriptor, Marker marker, string directory)
        {
            // Gesture layer
            AssetDatabase.CopyAsset(
                $"{A_PC_MARKER_DIR}/M_Gesture.controller",
                $"{directory}/gestureTemp.controller");
            AnimatorController gesture = AssetDatabase.LoadAssetAtPath(
                $"{directory}/gestureTemp.controller",
                typeof(AnimatorController)
            ) as AnimatorController;

            if (descriptor.baseAnimationLayers[2].isDefault == true
                || descriptor.baseAnimationLayers[2].animatorController == null)
            {
                AssetDatabase.CopyAsset(
                    $"{A_SHARED_RESOURCES_DIR}/Default/M_DefaultGesture.controller",
                    $"{directory}/Gesture.controller");
                AnimatorController gestureOriginal = AssetDatabase.LoadAssetAtPath(
                    $"{directory}/Gesture.controller", typeof(AnimatorController)
                ) as AnimatorController;

                descriptor.customExpressions = true;
                descriptor.baseAnimationLayers[2].isDefault = false;
                descriptor.baseAnimationLayers[2].animatorController = gestureOriginal;

                if (marker.wdSetting)
                {
                    ScriptFunctions.SetWriteDefaults(gestureOriginal);
                    EditorUtility.SetDirty(gestureOriginal);
                }
            }

            gesture.RemoveLayer((marker.leftHanded) ? 1 : 0);
            if (marker.useIndexFinger)
            {   // use different hand animations
                for (int i = 0; i < 3; i++)
                {
                    if (gesture.layers[0].stateMachine.states[i].state.motion.name == "M_Gesture")
                    {
                        gesture.layers[0].stateMachine.states[i].state.motion = AssetDatabase.LoadAssetAtPath(
                            $"{A_PC_MARKER_DIR}/Animations/Gesture/M_Gesture (Finger).anim",
                            typeof(AnimationClip)
                        ) as AnimationClip;
                    }
                    else if (gesture.layers[0].stateMachine.states[i].state.motion.name == "M_Gesture Draw")
                    {
                        gesture.layers[0].stateMachine.states[i].state.motion = AssetDatabase.LoadAssetAtPath(
                            $"{A_PC_MARKER_DIR}/Animations/Gesture/M_Gesture Draw (Finger).anim",
                            typeof(AnimationClip)
                        ) as AnimationClip;
                    }
                }
            }
            if (marker.gestureToDraw != 3)
            {
                ChangeGestureCondition(gesture, 0, marker.gestureToDraw);
            }
            if (marker.wdSetting)
            {
                ScriptFunctions.SetWriteDefaults(gesture, true);
            }

            EditorUtility.SetDirty(gesture);
            //ScriptFunctions.MergeController(descriptor, gesture, ScriptFunctions.PlayableLayer.Gesture, directory);
            //AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(gesture)); // delete temporary gesture layer

            return gesture;
        }

        static void GeneratePCPrefab(VRCAvatarDescriptor descriptor, Marker marker)
        {
            Animator avatar = descriptor.GetComponent<Animator>();

            // Physical Setup
            GameObject markerPrefab = PrefabUtility.InstantiatePrefab(
                AssetDatabase.LoadAssetAtPath($"{A_PC_MARKER_DIR}/Marker.prefab", typeof(GameObject))
            ) as GameObject;

            if (PrefabUtility.IsPartOfPrefabInstance(markerPrefab))
                PrefabUtility.UnpackPrefabInstance(markerPrefab, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

            markerPrefab.transform.SetParent(avatar.transform, false);
            Transform system = markerPrefab.transform.Find("System");
            Transform targets = markerPrefab.transform.Find("Targets");
            Transform markerTargetLeft = targets.Find("MarkerTargetLeft");
            Transform markerTargetRight = targets.Find("MarkerTargetRight");
            Transform markerModel = system.Find("Model");
            Transform markerScale = targets.Find("MarkerScale");
            Transform local = markerPrefab.transform.Find("World").Find("Local");

            // constrain cull object to avatar
            Transform cull = markerPrefab.transform.Find("Cull");
            cull.GetComponent<ParentConstraint>().SetSource(0, new ConstraintSource
            {
                sourceTransform = descriptor.transform,
                weight = 1f
            });

            if (marker.useIndexFinger)
            {
                GameObject.DestroyImmediate(targets.Find("Marker Flip").gameObject);// markerTargetLeft.GetChild(0).gameObject); // destroy Flip
                Transform indexDistalLeft = avatar.GetBoneTransform(HumanBodyBones.LeftIndexDistal);
                Transform indexDistalRight = avatar.GetBoneTransform(HumanBodyBones.RightIndexDistal);

                // prefer the end bone of the index finger if it exists
                if (indexDistalLeft.Find(indexDistalLeft.gameObject.name + "_end") != null)
                    markerTargetLeft.SetParent(indexDistalLeft.Find(indexDistalLeft.gameObject.name + "_end"), true);
                else
                    markerTargetLeft.SetParent(indexDistalLeft, true);
                markerTargetLeft.localPosition = Vector3.zero;
                markerTargetLeft.localRotation = Quaternion.Euler(0f, 0f, 0f);

                if (indexDistalRight.Find(indexDistalRight.gameObject.name + "_end") != null)
                    markerTargetRight.SetParent(indexDistalRight.Find(indexDistalRight.gameObject.name + "_end"), true);
                else
                    markerTargetRight.SetParent(indexDistalRight, true);
                markerTargetRight.localPosition = Vector3.zero;
                markerTargetRight.localRotation = Quaternion.Euler(0f, 0f, 0f);
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

            HumanBodyBones[] bones = {
                HumanBodyBones.Hips, HumanBodyBones.Chest, HumanBodyBones.Head,
                HumanBodyBones.LeftHand, HumanBodyBones.RightHand, HumanBodyBones.LeftFoot,
                HumanBodyBones.RightFoot
            };
            ParentConstraint localConstraint = local.GetComponent<ParentConstraint>();

            localConstraint.SetSource(0, new ConstraintSource { sourceTransform = avatar.transform, weight = 1f });
            if (marker.localSpace)
            {
                for (int i = 0; i < 5; i++)
                {
                    localConstraint.SetSource(i + 1, new ConstraintSource
                    {
                        sourceTransform = avatar.GetBoneTransform(bones[i]),
                        weight = 0f
                    });
                }
                if (marker.localSpaceFullBody == 1)
                {
                    for (int i = 5; i < 7; i++)
                    {
                        localConstraint.SetSource(i + 1, new ConstraintSource
                        {
                            sourceTransform = avatar.GetBoneTransform(bones[i]),
                            weight = 0f
                        });
                    }
                }
            }

            //GameObject.DestroyImmediate(targets.gameObject); // remove the "Targets" container object when finished

            // set anything not adjustable to a medium-ish amount
            if (!marker.eraserSize)
            {
                List<Material> sharedMats = new List<Material>();
                markerModel.GetComponent<MeshRenderer>().GetSharedMaterials(sharedMats);
                sharedMats[1].SetFloat("_EraserSize", 0.8419998f);

                SphereCollider collider = markerModel.GetComponent<SphereCollider>();
                collider.radius = 0.018f;
                collider.center = new Vector3(collider.center.x, 0.196f, collider.center.z);

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
            marker.markerTargetRight = markerTargetRight;
            marker.markerTargetLeft = markerTargetLeft;
            marker.markerScale = markerScale;
        }

        public static Dictionary<ScriptFunctions.PlayableLayer, AnimatorController> GeneratePC(VRCAvatarDescriptor descriptor, ref Marker marker, string directory)
        {
            // Physical Install
            GeneratePCPrefab(descriptor, marker);

            // Animators
            Dictionary<ScriptFunctions.PlayableLayer, AnimatorController> controllers = new Dictionary<ScriptFunctions.PlayableLayer, AnimatorController>() {
                { ScriptFunctions.PlayableLayer.FX, GeneratePCAnimatorFX(descriptor, marker, directory) }
            };

            if (!marker.useIndexFinger) {
                controllers.Add(
                    ScriptFunctions.PlayableLayer.Gesture, 
                    GeneratePCAnimatorGesture(descriptor, marker, $"{directory}/")
                );
            }

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
            if (marker.eraserSize)
            {
                VRCExpressionParameters.Parameter p_eraserSize = new VRCExpressionParameters.Parameter
                { name = "VRLabs/Marker/EraserSize", valueType = VRCExpressionParameters.ValueType.Float, saved = false };
                ScriptFunctions.AddParameter(descriptor, p_eraserSize, $"{directory}/");
            }

            VRCExpressionParameters.Parameter p_menu = new VRCExpressionParameters.Parameter
            { name = "VRLabs/Marker/Menu", valueType = VRCExpressionParameters.ValueType.Bool, saved = false };
            ScriptFunctions.AddParameter(descriptor, p_menu, $"{directory}/");

            // handle menu instancing
            AssetDatabase.CopyAsset($"{A_PC_MARKER_DIR}/M_Menu.asset", $"{directory}/Marker Menu.asset");
            VRCExpressionsMenu markerMenu = AssetDatabase.LoadAssetAtPath(
                $"{directory}/Marker Menu.asset", typeof(VRCExpressionsMenu)
            ) as VRCExpressionsMenu;

            if (!marker.localSpace) // change from submenu to 1 toggle
            {
                VRCExpressionsMenu.Control.Parameter pm_spaceSimple = new VRCExpressionsMenu.Control.Parameter
                { name = "VRLabs/Marker/SpaceSimple" };
                markerMenu.controls[6].type = VRCExpressionsMenu.Control.ControlType.Toggle;
                markerMenu.controls[6].parameter = pm_spaceSimple;
                markerMenu.controls[6].subMenu = null; // or else the submenu is still there internally, SDK complains
            }
            else
            {
                string submenuPath = $"{directory}/Marker Space Submenu.asset";
                AssetDatabase.CopyAsset($"{A_PC_MARKER_DIR}/M_Menu Space.asset", submenuPath);
                VRCExpressionsMenu subMenu = AssetDatabase.LoadAssetAtPath(
                    submenuPath, typeof(VRCExpressionsMenu)
                ) as VRCExpressionsMenu;

                /*
                if (marker.localSpaceFullBody == 0) // remove left and right foot controls
                {
                    subMenu.controls.RemoveAt(7);
                    subMenu.controls.RemoveAt(6);
                }
                */
                markerMenu.controls[6].subMenu = subMenu;
                EditorUtility.SetDirty(subMenu);
            }

            if (!marker.brushSize)
            {
                ScriptFunctions.RemoveMenuControl(markerMenu, "Brush Size");
                ScriptFunctions.RemoveMenuControl(markerMenu, "<color=#FFFFFF>Brush Size");
            }

            if (!marker.eraserSize)
            {
                ScriptFunctions.RemoveMenuControl(markerMenu, "Eraser Size");
                ScriptFunctions.RemoveMenuControl(markerMenu, "<color=#FFFFFF>Eraser Size");
            }

            EditorUtility.SetDirty(markerMenu);

            VRCExpressionsMenu.Control.Parameter pm_menu = new VRCExpressionsMenu.Control.Parameter
            { name = "VRLabs/Marker/Menu" };
            Texture2D markerIcon = AssetDatabase.LoadAssetAtPath(
                $"{A_SHARED_RESOURCES_DIR}/Icons/M_Icon_Menu.png",
                typeof(Texture2D)
            ) as Texture2D;
            ScriptFunctions.AddSubMenu(descriptor, markerMenu, "Marker", $"{directory}/", pm_menu, markerIcon);

            return controllers;
        }

        private static void ChangeGestureCondition(AnimatorController controller, int layerToModify, int newGesture)
        {   // helper function: change gesture condition, in all transitions of 1 layer of controller
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
        public static Dictionary<ScriptFunctions.PlayableLayer, AnimatorController> GenerateQuest(VRCAvatarDescriptor descriptor, ref Marker marker, string directory)
        {
            // instantiate marker prefab
            GameObject markerPrefab = Resources.Load(R_QUEST_MARKER_PATH) as GameObject;
            if(markerPrefab == null) {
                throw new NullReferenceException("Quest Marker Prefab not found");
            }
            markerPrefab = PrefabUtility.InstantiatePrefab(markerPrefab) as GameObject;
            PrefabUtility.UnpackPrefabInstance(markerPrefab, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            markerPrefab.name = "Marker";


            // check for index finger usage and set the target in accordance
            Animator avatar = descriptor.gameObject.GetComponent<Animator>();
            if (marker.useIndexFinger) {
                Transform markerTarget = avatar.GetBoneTransform(
                    marker.leftHanded ? HumanBodyBones.LeftIndexDistal : HumanBodyBones.RightIndexDistal
                );

                markerPrefab.transform.SetParent(markerTarget);
                markerPrefab.transform.localPosition = Vector3.zero;
                markerPrefab.transform.localRotation = Quaternion.identity;

                // remove useless stuff 
                GameObject.DestroyImmediate(markerPrefab.transform.Find("Marker").gameObject);
            } else {
                Transform markerTarget = avatar.GetBoneTransform(
                    marker.leftHanded ? HumanBodyBones.LeftHand : HumanBodyBones.RightHand
                );
                markerPrefab.transform.SetParent(markerTarget);
                markerPrefab.transform.localRotation = Quaternion.Euler(180, 0, 0);
                markerPrefab.transform.localPosition = new Vector3(0.05f, 0.1f, 0);
            }


            // create new pen animator
            AnimatorController penController = GenerateQuestPenAnimator(
                markerPrefab.transform.GetHierarchyPath(descriptor.transform),
                marker,
                directory
            );


            // parameters
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

            // menus


            return new Dictionary<ScriptFunctions.PlayableLayer, AnimatorController>() {
                { ScriptFunctions.PlayableLayer.FX, penController }
            };
        }

        static AnimatorController GenerateQuestPenAnimator(string penPrefabPath, Marker marker, string directory)
        {
            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath($"{directory}/MarkerFX.controller");

            // animations
            AnimationClip drawClip = GenerateQuestMarkerClip(penPrefabPath, true, 9999, true);
            AnimationClip noDrawClip = GenerateQuestMarkerClip(penPrefabPath, false, 9999, true);
            AnimationClip clearClip = GenerateQuestMarkerClip(penPrefabPath, false, 0, true);
            AnimationClip offClip = GenerateQuestMarkerClip(penPrefabPath, false, 0, false);

            AssetDatabase.CreateAsset(drawClip, $"{directory}/M_Draw.anim");
            AssetDatabase.CreateAsset(noDrawClip, $"{directory}/M_NoDraw.anim");
            AssetDatabase.CreateAsset(clearClip, $"{directory}/M_Clear.anim");
            AssetDatabase.CreateAsset(offClip, $"{directory}/M_Off.anim");

            // parameters
            string M_GESTURE_PARAM = marker.leftHanded ? "GestureLeft" : "GestureRight";
            controller.AddParameter(M_GESTURE_PARAM, AnimatorControllerParameterType.Int);
            controller.AddParameter(M_MARKER_PARAM, AnimatorControllerParameterType.Bool);
            controller.AddParameter(M_COLOR_PARAM, AnimatorControllerParameterType.Float);
            controller.AddParameter(M_MARKER_CLEAR_PARAM, AnimatorControllerParameterType.Bool);

            // generate marker animator layer
            int layerIdx = 0;
            AnimatorControllerLayer markerLayer = controller.layers[layerIdx];
            {
                // state machine default state positions
                markerLayer.stateMachine.exitPosition = new Vector2(20, 290);
                markerLayer.stateMachine.anyStatePosition = new Vector2(20, -40);
                markerLayer.stateMachine.entryPosition = new Vector2(20, 0);


                // animator states
                AnimatorState offState = new AnimatorState() {
                    name="M_Off",
                    writeDefaultValues = marker.wdSetting,
                    motion = offClip,
                    timeParameterActive = true,
                    timeParameter = M_COLOR_PARAM
                };
                markerLayer.stateMachine.AddState(offState, new Vector2(0, 50));
                offState.hideFlags = HideFlags.HideInHierarchy;

                AnimatorState drawState = new AnimatorState()
                {
                    name = "M_Draw",
                    writeDefaultValues = marker.wdSetting,
                    motion = drawClip,
                    timeParameterActive = true,
                    timeParameter = M_COLOR_PARAM
                };
                markerLayer.stateMachine.AddState(drawState, new Vector2(-130, 120));
                drawState.hideFlags = HideFlags.HideInHierarchy;

                AnimatorState noDrawState = new AnimatorState() {
                    name="M_NoDraw",
                    writeDefaultValues = marker.wdSetting,
                    motion = noDrawClip,
                    timeParameterActive = true,
                    timeParameter = M_COLOR_PARAM
                };
                markerLayer.stateMachine.AddState(noDrawState, new Vector2(130, 120));
                noDrawState.hideFlags = HideFlags.HideInHierarchy;

                AnimatorState clearState = new AnimatorState() {
                    name="M_Clear",
                    writeDefaultValues = marker.wdSetting,
                    motion = clearClip,
                    timeParameterActive = true,
                    timeParameter = M_COLOR_PARAM
                };
                markerLayer.stateMachine.AddState(clearState, new Vector2(0, 190));
                clearState.hideFlags = HideFlags.HideInHierarchy;


                // transitions out of MARKER_CLEAR
                {
                    AnimatorStateTransition exit = clearState.AddExitTransition();
                    exit.duration = 0;
                    exit.AddCondition(AnimatorConditionMode.IfNot, 1, M_MARKER_CLEAR_PARAM);
                }


                // transitions out of MARKER_OFF
                {
                    AnimatorStateTransition t = offState.AddTransition(drawState);
                    t.duration = 0;
                    t.AddCondition(AnimatorConditionMode.If, 1, M_MARKER_PARAM);
                    t.AddCondition(AnimatorConditionMode.Equals, marker.gestureToDraw, M_GESTURE_PARAM);

                    AnimatorStateTransition t1 = offState.AddTransition(noDrawState);
                    t1.duration = 0;
                    t1.AddCondition(AnimatorConditionMode.If, 1, M_MARKER_PARAM);
                    t1.AddCondition(AnimatorConditionMode.NotEqual, marker.gestureToDraw, M_GESTURE_PARAM);

                    AnimatorStateTransition t2 = offState.AddTransition(clearState);
                    t2.duration = 0;
                    t2.canTransitionToSelf = false;
                    t2.AddCondition(AnimatorConditionMode.If, 1, M_MARKER_CLEAR_PARAM);
                }


                // transitions out of MARKER_DRAW
                {
                    AnimatorStateTransition t = drawState.AddTransition(offState);
                    t.duration = 0;
                    t.AddCondition(AnimatorConditionMode.IfNot, 1, M_MARKER_PARAM);

                    AnimatorStateTransition t1 = drawState.AddTransition(noDrawState);
                    t1.duration = 0;
                    t1.AddCondition(AnimatorConditionMode.If, 1, M_MARKER_PARAM);
                    t1.AddCondition(AnimatorConditionMode.NotEqual, marker.gestureToDraw, M_GESTURE_PARAM);


                    AnimatorStateTransition t2 = drawState.AddTransition(clearState);
                    t2.duration = 0;
                    t2.canTransitionToSelf = false;
                    t2.AddCondition(AnimatorConditionMode.If, 1, M_MARKER_CLEAR_PARAM);
                }


                // transitions out of MARKER_NO_DRAW
                {
                    AnimatorStateTransition t = noDrawState.AddTransition(offState);
                    t.duration = 0;
                    t.AddCondition(AnimatorConditionMode.IfNot, 1, M_MARKER_PARAM);

                    AnimatorStateTransition t1 = noDrawState.AddTransition(drawState);
                    t1.duration = 0;
                    t1.AddCondition(AnimatorConditionMode.If, 1, M_MARKER_PARAM);
                    t1.AddCondition(AnimatorConditionMode.Equals, marker.gestureToDraw, M_GESTURE_PARAM);

                    AnimatorStateTransition t2 = noDrawState.AddTransition(clearState);
                    t2.duration = 0;
                    t2.canTransitionToSelf = false;
                    t2.AddCondition(AnimatorConditionMode.If, 1, M_MARKER_CLEAR_PARAM);
                }
            }

            // assign back to controller
            AnimatorControllerLayer[] layers = controller.layers;
            layers[layerIdx] = markerLayer;
            layers[layerIdx].defaultWeight = 1;
            layers[layerIdx].name = "VRLabs/Marker";
            controller.layers = layers;

            return controller;
        }

        static AnimationClip GenerateQuestMarkerClip(string transformPath, bool emitting, 
            float lifetime, bool markerEnabled)
        {
            string DRAW_PATH = $"{transformPath}/Draw";
            string MARKER_PATH = $"{transformPath}/Marker";
            string PREVIEW_PATH = $"{transformPath}/Marker/Preview";

            AnimationClip clip = new AnimationClip();
            EditorUtility.SetDirty(clip);

            // trail renderer drawing
            clip.SetCurve(DRAW_PATH, typeof(TrailRenderer), "m_Emitting", new AnimationCurve()
            {
                keys = new Keyframe[] {
                    new Keyframe() { time = 0, value=emitting ? 1 : 0, inTangent=0, outTangent=0 },
                },
            });
            clip.SetCurve(DRAW_PATH, typeof(TrailRenderer), "m_Time", new AnimationCurve()
            {
                keys = new Keyframe[] {
                    new Keyframe() { time = 0, value=lifetime, inTangent=0, outTangent=0 },
                }
            });

            // trail renderer color
            clip.SetCurve(DRAW_PATH, typeof(TrailRenderer), "material._EmissionColor.r", new AnimationCurve()
            {
                keys = new Keyframe[] {
                    new Keyframe() { time = 0, value=1, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 5/60f, value=1, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 10/60f, value=0, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 15/60f, value=0, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 20/60f, value=0, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 25/60f, value=1, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 30/60f, value=1, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 35/60f, value=1, inTangent=0, outTangent=0 },
                }
            });
            clip.SetCurve(DRAW_PATH, typeof(TrailRenderer), "material._EmissionColor.g", new AnimationCurve()
            {
                keys = new Keyframe[] {
                    new Keyframe() { time = 0, value=0, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 5/60f, value=1, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 10/60f, value=1, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 15/60f, value=1, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 20/60f, value=0, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 25/60f, value=0, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 30/60f, value=0, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 35/60f, value=1, inTangent=0, outTangent=0 },
                }
            });
            clip.SetCurve(DRAW_PATH, typeof(TrailRenderer), "material._EmissionColor.b", new AnimationCurve()
            {
                keys = new Keyframe[] {
                    new Keyframe() { time = 0, value=0, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 5/60f, value=0, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 10/60f, value=0, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 15/60f, value=1, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 20/60f, value=1, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 25/60f, value=1, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 30/60f, value=0, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 35/60f, value=1, inTangent=0, outTangent=0 },
                }
            });

            clip.SetCurve(DRAW_PATH, typeof(TrailRenderer), "material._Color.r", new AnimationCurve()
            {
                keys = new Keyframe[] {
                    new Keyframe() { time = 0, value=1, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 5/60f, value=1, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 10/60f, value=0, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 15/60f, value=0, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 20/60f, value=0, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 25/60f, value=1, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 30/60f, value=1, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 35/60f, value=1, inTangent=0, outTangent=0 },
                }
            });
            clip.SetCurve(DRAW_PATH, typeof(TrailRenderer), "material._Color.g", new AnimationCurve()
            {
                keys = new Keyframe[] {
                    new Keyframe() { time = 0, value=0, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 5/60f, value=1, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 10/60f, value=1, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 15/60f, value=1, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 20/60f, value=0, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 25/60f, value=0, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 30/60f, value=0, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 35/60f, value=1, inTangent=0, outTangent=0 },
                }
            });
            clip.SetCurve(DRAW_PATH, typeof(TrailRenderer), "material._Color.b", new AnimationCurve()
            {
                keys = new Keyframe[] {
                    new Keyframe() { time = 0, value=0, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 5/60f, value=0, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 10/60f, value=0, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 15/60f, value=1, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 20/60f, value=1, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 25/60f, value=1, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 30/60f, value=0, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 35/60f, value=1, inTangent=0, outTangent=0 },
                }
            });

            // marker object color
            clip.SetCurve(MARKER_PATH, typeof(MeshRenderer), "material._EmissionColor.r", new AnimationCurve()
            {
                keys = new Keyframe[] {
                    new Keyframe() { time = 0, value=1, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 5/60f, value=1, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 10/60f, value=0, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 15/60f, value=0, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 20/60f, value=0, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 25/60f, value=1, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 30/60f, value=1, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 35/60f, value=1, inTangent=0, outTangent=0 },
                }
            });
            clip.SetCurve(MARKER_PATH, typeof(MeshRenderer), "material._EmissionColor.g", new AnimationCurve()
            {
                keys = new Keyframe[] {
                    new Keyframe() { time = 0, value=0, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 5/60f, value=1, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 10/60f, value=1, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 15/60f, value=1, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 20/60f, value=0, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 25/60f, value=0, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 30/60f, value=0, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 35/60f, value=1, inTangent=0, outTangent=0 },
                }
            });
            clip.SetCurve(MARKER_PATH, typeof(MeshRenderer), "material._EmissionColor.b", new AnimationCurve()
            {
                keys = new Keyframe[] {
                    new Keyframe() { time = 0, value=0, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 5/60f, value=0, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 10/60f, value=0, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 15/60f, value=1, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 20/60f, value=1, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 25/60f, value=1, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 30/60f, value=0, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 35/60f, value=1, inTangent=0, outTangent=0 },
                }
            });

            // marker object toggle
            clip.SetCurve(MARKER_PATH, typeof(GameObject), "m_IsActive", new AnimationCurve()
            {
                keys = new Keyframe[] {
                    new Keyframe() { time = 0, value=markerEnabled ? 1 : 0, inTangent=0, outTangent=0 }
                }
            });


            /* preview color
            clip.SetCurve(PREVIEW_PATH, typeof(ParticleSystem), "InitialModule.startColor.maxColor.r", new AnimationCurve()
            {
                keys = new Keyframe[] {
                    new Keyframe() { time = 0, value=1, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 5/60f, value=1, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 10/60f, value=0, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 15/60f, value=0, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 20/60f, value=0, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 25/60f, value=1, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 30/60f, value=1, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 35/60f, value=1, inTangent=0, outTangent=0 },
                }
            });
            clip.SetCurve(PREVIEW_PATH, typeof(ParticleSystem), "InitialModule.startColor.maxColor.g", new AnimationCurve()
            {
                keys = new Keyframe[] {
                    new Keyframe() { time = 0, value=0, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 5/60f, value=1, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 10/60f, value=1, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 15/60f, value=1, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 20/60f, value=0, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 25/60f, value=0, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 30/60f, value=0, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 35/60f, value=1, inTangent=0, outTangent=0 },
                }
            });
            clip.SetCurve(PREVIEW_PATH, typeof(ParticleSystem), "InitialModule.startColor.maxColor.b", new AnimationCurve()
            {
                keys = new Keyframe[] {
                    new Keyframe() { time = 0, value=0, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 5/60f, value=0, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 10/60f, value=0, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 15/60f, value=1, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 20/60f, value=1, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 25/60f, value=1, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 30/60f, value=0, inTangent=0, outTangent=0 },
                    new Keyframe() { time = 35/60f, value=1, inTangent=0, outTangent=0 },
                }
            });
            */
            return clip;
        }
        #endregion Quest
    }
}