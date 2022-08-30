using System.Linq;
using System.IO;
using System;

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
            AnimatorController generatedController = installQuest
                ? GenerateQuest(descriptor, ref marker, directory)
                : GeneratePC(descriptor, ref marker, directory);

            if(generatedController == null)
                throw new NullReferenceException("Failed to generate marker controller");

            // Generate and apply master Avatar Mask
            if (marker.generateMasterMask)
            {
                AnimatorControllerLayer[] layers = generatedController.layers;
                layers[0].avatarMask = AvatarMaskFunctions.GenerateFXMasterMask(descriptor, directory);
                generatedController.layers = layers;
            }

            // merge
            ScriptFunctions.MergeController(
                descriptor,
                generatedController,
                ScriptFunctions.PlayableLayer.FX,
                directory
            );

            // finishing touches
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            marker.finished = true;
            Debug.Log("Successfully generated Marker!");
        }

        public static void Uninstall(VRCAvatarDescriptor descriptor)
        {
            Animator avatar = descriptor.GetComponent<Animator>();

            ScriptFunctions.UninstallControllerByPrefix(descriptor, "M_", ScriptFunctions.PlayableLayer.FX);
            ScriptFunctions.UninstallControllerByPrefix(descriptor, "M_", ScriptFunctions.PlayableLayer.Gesture);
            ScriptFunctions.UninstallParametersByPrefix(descriptor, "M_");
            ScriptFunctions.UninstallMenu(descriptor, "Marker");
            if (descriptor != null)
            {
                Transform foundMarker = descriptor.transform.Find("Marker");
                if (foundMarker != null)
                    GameObject.DestroyImmediate(foundMarker.gameObject);
                if (avatar.isHuman)
                {
                    HumanBodyBones[] bonesToSearch = { HumanBodyBones.LeftHand, HumanBodyBones.RightHand,
                        HumanBodyBones.LeftIndexDistal, HumanBodyBones.RightIndexDistal };
                    for (int i = 0; i < bonesToSearch.Length; i++)
                    {
                        GameObject foundTargetPC = ScriptFunctions.FindObject(descriptor, bonesToSearch[i], "MarkerTarget", true);
                        if (foundTargetPC != null)
                            GameObject.DestroyImmediate(foundTargetPC);

                        GameObject foundTargetQuest = ScriptFunctions.FindObject(descriptor, bonesToSearch[i], "Marker", true);
                        if (foundTargetQuest != null)
                            GameObject.DestroyImmediate(foundTargetQuest);
                    }
                }
            }
        }

        #region PC
        public static AnimatorController GeneratePC(VRCAvatarDescriptor descriptor, ref Marker marker, string directory)
        {
            return null;
            Animator avatar = descriptor.GetComponent<Animator>();

            // Install layers, parameters, and menu before prefab setup
            // FX layer
            if (marker.useIndexFinger)
            {
                AssetDatabase.CopyAsset(
                    $"{A_PC_MARKER_DIR}/M_FX (Finger).controller",
                    directory + "FXtemp.controller");
            }
            else
            {
                AssetDatabase.CopyAsset(
                    $"{A_PC_MARKER_DIR}/M_FX.controller",
                    directory + "FXtemp.controller");
            }

            AnimatorController FX = AssetDatabase.LoadAssetAtPath(
                directory + "FXtemp.controller",
                typeof(AnimatorController)
            ) as AnimatorController;

            // remove controller layers before merging to avatar, corresponding to setup
            /*
            if (leftHanded)
                ScriptFunctions.RemoveLayer(FX, "M_Marker R");
            else
                ScriptFunctions.RemoveLayer(FX, "M_Marker L");

            if (!brushSize)
            {
                ScriptFunctions.RemoveLayer(FX, "M_Size");
                ScriptFunctions.RemoveParameter(FX, "M_Size");
            }

            if (!eraserSize)
            {
                ScriptFunctions.RemoveLayer(FX, "M_EraserSize");
                ScriptFunctions.RemoveParameter(FX, "M_EraserSize");
            }*/

            if (!marker.localSpace)
            {
                ScriptFunctions.RemoveLayer(FX, "M_Space");
                ScriptFunctions.RemoveParameter(FX, "M_Space");
                ScriptFunctions.RemoveLayer(FX, "M_Cull");
            }
            else
            {
                ScriptFunctions.RemoveLayer(FX, "M_SpaceSimple");
                ScriptFunctions.RemoveParameter(FX, "M_SpaceSimple");
                ScriptFunctions.RemoveLayer(FX, "M_CullSimple");
            }

            if (marker.wdSetting)
            {
                ScriptFunctions.SetWriteDefaults(FX);
            }
            if (marker.gestureToDraw != 3) // uses fingerpoint by default
            {
                ChangeGestureCondition(FX, 0, marker.gestureToDraw);
            }

            // Set parameter driver on 'Clear' state to reset local space
            AnimatorState state = FX.layers[0].stateMachine.states.FirstOrDefault(s => s.state.name.Equals("Clear")).state;
            VRCAvatarParameterDriver driver = (VRCAvatarParameterDriver)state.behaviours[0];
            string driverParamName = marker.localSpace ? "M_Space" : "M_SpaceSimple";
            VRC.SDKBase.VRC_AvatarParameterDriver.Parameter param = new VRC.SDKBase.VRC_AvatarParameterDriver.Parameter()
            {
                name = driverParamName,
                type = VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType.Set,
                value = 0f
            };
            driver.parameters.Add(param);

            EditorUtility.SetDirty(FX);
            ScriptFunctions.MergeController(descriptor, FX, ScriptFunctions.PlayableLayer.FX, directory);
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(FX)); // delete temporary FX layer

            // Gesture layer
            AssetDatabase.CopyAsset(
                $"{A_PC_MARKER_DIR}/M_Gesture.controller",
                directory + "gestureTemp.controller"); // to modify
            AnimatorController gesture = AssetDatabase.LoadAssetAtPath(
                directory + "gestureTemp.controller",
                typeof(AnimatorController)
            ) as AnimatorController;

            if (descriptor.baseAnimationLayers[2].isDefault == true
                || descriptor.baseAnimationLayers[2].animatorController == null)
            {
                AssetDatabase.CopyAsset(
                    $"{A_SHARED_RESOURCES_DIR}/Default/M_DefaultGesture.controller",
                    directory + "Gesture.controller");
                AnimatorController gestureOriginal = AssetDatabase.LoadAssetAtPath(
                    directory + "Gesture.controller", typeof(AnimatorController)
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
            ScriptFunctions.MergeController(descriptor, gesture, ScriptFunctions.PlayableLayer.Gesture, directory);
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(gesture)); // delete temporary gesture layer

            // layer weight control from merged layer may need index set correctly
            AnimatorController avatarGesture = (AnimatorController)descriptor.baseAnimationLayers[2].animatorController;
            for (int i = 0; i < avatarGesture.layers.Length; i++)
            {   // the controls' layer is normally 3 (AllParts, LeftHand, RightHand, >>>M_Gesture<<<)
                if (avatarGesture.layers[i].name.Contains("M_Gesture") && (i != 3))
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

            EditorUtility.SetDirty(avatarGesture);

            // Parameters
            VRCExpressionParameters.Parameter
            p_marker = new VRCExpressionParameters.Parameter
            { name = "M_Marker", valueType = VRCExpressionParameters.ValueType.Bool, saved = false },
            p_eraser = new VRCExpressionParameters.Parameter
            { name = "M_Eraser", valueType = VRCExpressionParameters.ValueType.Bool, saved = false },
            p_clear = new VRCExpressionParameters.Parameter
            { name = "M_Clear", valueType = VRCExpressionParameters.ValueType.Bool, saved = false },
            p_color = new VRCExpressionParameters.Parameter
            { name = "M_Color", valueType = VRCExpressionParameters.ValueType.Float, saved = true };
            ScriptFunctions.AddParameter(descriptor, p_marker, directory);
            ScriptFunctions.AddParameter(descriptor, p_eraser, directory);
            ScriptFunctions.AddParameter(descriptor, p_clear, directory);
            ScriptFunctions.AddParameter(descriptor, p_color, directory);

            if (marker.localSpace)
            {
                VRCExpressionParameters.Parameter p_space = new VRCExpressionParameters.Parameter
                { name = "M_Space", valueType = VRCExpressionParameters.ValueType.Int, saved = false };
                ScriptFunctions.AddParameter(descriptor, p_space, directory);
            }
            else
            {
                VRCExpressionParameters.Parameter p_spaceSimple = new VRCExpressionParameters.Parameter
                { name = "M_SpaceSimple", valueType = VRCExpressionParameters.ValueType.Bool, saved = false };
                ScriptFunctions.AddParameter(descriptor, p_spaceSimple, directory);
            }

            if (marker.brushSize)
            {
                VRCExpressionParameters.Parameter p_size = new VRCExpressionParameters.Parameter
                { name = "M_Size", valueType = VRCExpressionParameters.ValueType.Float, saved = false };
                ScriptFunctions.AddParameter(descriptor, p_size, directory);
            }
            if (marker.eraserSize)
            {
                VRCExpressionParameters.Parameter p_eraserSize = new VRCExpressionParameters.Parameter
                { name = "M_EraserSize", valueType = VRCExpressionParameters.ValueType.Float, saved = false };
                ScriptFunctions.AddParameter(descriptor, p_eraserSize, directory);
            }

            VRCExpressionParameters.Parameter p_menu = new VRCExpressionParameters.Parameter
            { name = "M_Menu", valueType = VRCExpressionParameters.ValueType.Bool, saved = false };
            ScriptFunctions.AddParameter(descriptor, p_menu, directory);

            // handle menu instancing
            AssetDatabase.CopyAsset($"{A_PC_MARKER_DIR}/M_Menu.asset", directory + "Marker Menu.asset");
            VRCExpressionsMenu markerMenu = AssetDatabase.LoadAssetAtPath(
                directory + "Marker Menu.asset", typeof(VRCExpressionsMenu)
            ) as VRCExpressionsMenu;

            if (!marker.localSpace) // change from submenu to 1 toggle
            {
                VRCExpressionsMenu.Control.Parameter pm_spaceSimple = new VRCExpressionsMenu.Control.Parameter
                { name = "M_SpaceSimple" };
                markerMenu.controls[6].type = VRCExpressionsMenu.Control.ControlType.Toggle;
                markerMenu.controls[6].parameter = pm_spaceSimple;
                markerMenu.controls[6].subMenu = null; // or else the submenu is still there internally, SDK complains
            }
            else
            {
                AssetDatabase.CopyAsset(
                    $"{A_PC_MARKER_DIR}/M_Menu Space.asset",
                    directory + "Marker Space Submenu.asset");
                VRCExpressionsMenu subMenu = AssetDatabase.LoadAssetAtPath(
                    directory + "Marker Space Submenu.asset", typeof(VRCExpressionsMenu)
                ) as VRCExpressionsMenu;

                if (marker.localSpaceFullBody == 0) // remove left and right foot controls
                {
                    subMenu.controls.RemoveAt(7);
                    subMenu.controls.RemoveAt(6);
                }
                markerMenu.controls[6].subMenu = subMenu;
                EditorUtility.SetDirty(subMenu);
            }

            if (!marker.brushSize)
                ScriptFunctions.RemoveMenuControl(markerMenu, "Brush Size");

            if (!marker.eraserSize)
                ScriptFunctions.RemoveMenuControl(markerMenu, "Eraser Size");

            EditorUtility.SetDirty(markerMenu);

            VRCExpressionsMenu.Control.Parameter pm_menu = new VRCExpressionsMenu.Control.Parameter
            { name = "M_Menu" };
            Texture2D markerIcon = AssetDatabase.LoadAssetAtPath(
                $"{A_SHARED_RESOURCES_DIR}/Icons/M_Icon_Menu.png",
                typeof(Texture2D)
            ) as Texture2D;
            ScriptFunctions.AddSubMenu(descriptor, markerMenu, "Marker", directory, pm_menu, markerIcon);

            // setup in scene
            GameObject markerPrefab = PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath(
                "Assets/VRLabs/Marker/Resources/Marker.prefab",
                typeof(GameObject))
            ) as GameObject;

            if (PrefabUtility.IsPartOfPrefabInstance(markerPrefab))
                PrefabUtility.UnpackPrefabInstance(markerPrefab, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            markerPrefab.transform.SetParent(avatar.transform, false);

            Transform system = markerPrefab.transform.Find("System");
            Transform targets = markerPrefab.transform.Find("Targets");
            Transform markerTarget = targets.Find("MarkerTarget");
            Transform markerModel = targets.Find("Model");
            Transform eraser = system.Find("Eraser");
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
                GameObject.DestroyImmediate(markerTarget.GetChild(0).gameObject); // destroy Flip
                Transform indexDistal = marker.leftHanded
                    ? avatar.GetBoneTransform(HumanBodyBones.LeftIndexDistal)
                    : avatar.GetBoneTransform(HumanBodyBones.RightIndexDistal);

                // prefer the end bone of the index finger if it exists
                if (indexDistal.Find(indexDistal.gameObject.name + "_end") != null)
                    markerTarget.SetParent(indexDistal.Find(indexDistal.gameObject.name + "_end"), true);
                else
                    markerTarget.SetParent(indexDistal, true);
                markerTarget.localPosition = Vector3.zero;
                markerTarget.localRotation = Quaternion.Euler(0f, 0f, 0f);
            }
            else // using model: scale Model to target freely, and until script is destroyed, scale System to target uniformly with X-axis 
            {
                markerModel.SetParent(markerPrefab.transform); // move it out of Targets hierarchy

                Transform hand = marker.leftHanded
                    ? avatar.GetBoneTransform(HumanBodyBones.LeftHand)
                    : avatar.GetBoneTransform(HumanBodyBones.RightHand);

                Transform elbow = marker.leftHanded
                    ? avatar.GetBoneTransform(HumanBodyBones.LeftLowerArm)
                    : avatar.GetBoneTransform(HumanBodyBones.RightLowerArm);

                // need to flip the target(model). we can use the Flip object by resetting markertarget transform,
                // getting Flip's position, then rotating markertarget
                markerTarget.SetParent(hand, true);
                markerTarget.localPosition = Vector3.zero;
                markerTarget.localRotation = Quaternion.Euler(0f, 0f, 0f);
                markerTarget.position = markerTarget.GetChild(0).transform.position;
                markerTarget.localPosition = new Vector3(0f, markerTarget.localPosition.y, 0f); // ignore offset on x and z
                markerTarget.localRotation = Quaternion.Euler(0f, 0f, 180f); // and flip the rotation

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

            GameObject.DestroyImmediate(targets.gameObject); // remove the "Targets" container object when finished

            // set anything not adjustable to a medium-ish amount
            if (!marker.eraserSize)
            {
                eraser.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
            }
            if (!marker.brushSize)
            {
                ParticleSystem.MinMaxCurve size = new ParticleSystem.MinMaxCurve(0.024f);
                Transform draw = system.transform.Find("Draw");
                Transform preview = draw.GetChild(0);
                ParticleSystem.MainModule main = draw.GetComponent<ParticleSystem>().main;
                main.startSize = size;
                main = preview.GetComponent<ParticleSystem>().main;
                main.startSize = size;
            }

            // scale MarkerTarget, which controls prefab size, according to a (normalized) worldspace distance
            // between avatar hips and head
            Transform hips = avatar.GetBoneTransform(HumanBodyBones.Hips);
            Transform head = avatar.GetBoneTransform(HumanBodyBones.Head);
            Vector3 dist = (head.position - hips.position);

            float normalizedDist = (Math.Max(Math.Max(dist.x, dist.y), dist.z) / KSIVL_UNIT);
            float newScale = markerTarget.localScale.x * normalizedDist;
            markerTarget.localScale = new Vector3(newScale, newScale, newScale);

            marker.system = system;
            marker.markerTarget = markerTarget;
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
        public static AnimatorController GenerateQuest(VRCAvatarDescriptor descriptor, ref Marker marker, string directory)
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

            return penController;
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

                AnimatorState drawState = new AnimatorState()
                {
                    name = "M_Draw",
                    writeDefaultValues = marker.wdSetting,
                    motion = drawClip,
                    timeParameterActive = true,
                    timeParameter = M_COLOR_PARAM
                };
                markerLayer.stateMachine.AddState(drawState, new Vector2(-130, 120));

                AnimatorState noDrawState = new AnimatorState() {
                    name="M_NoDraw",
                    writeDefaultValues = marker.wdSetting,
                    motion = noDrawClip,
                    timeParameterActive = true,
                    timeParameter = M_COLOR_PARAM
                };
                markerLayer.stateMachine.AddState(noDrawState, new Vector2(130, 120));

                AnimatorState clearState = new AnimatorState() {
                    name="M_Clear",
                    writeDefaultValues = marker.wdSetting,
                    motion = clearClip,
                    timeParameterActive = true,
                    timeParameter = M_COLOR_PARAM
                };
                markerLayer.stateMachine.AddState(clearState, new Vector2(0, 190));


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
            layers[layerIdx].name = "M_Marker";
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

            // trail renderer state
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

            // preview color
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

            // marker enable
            clip.SetCurve(MARKER_PATH, typeof(GameObject), "m_IsActive", new AnimationCurve() {
                keys = new Keyframe[] {
                    new Keyframe() { time = 0, value=markerEnabled ? 1 : 0, inTangent=0, outTangent=0 }
                }
            });

            return clip;
        }
        #endregion Quest
    }
}