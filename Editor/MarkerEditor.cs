// Marker by ksivl / VRLabs 3.0 Assets https://github.com/VRLabs/VRChat-Avatars-3.0
#if UNITY_EDITOR
using Boo.Lang;
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Animations;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using Vector3 = UnityEngine.Vector3;

namespace VRLabs.Marker
{
    [CustomEditor(typeof(Marker))]
    class MarkerEditor : Editor
    {
        public VRCAvatarDescriptor descriptor;
        public System.Collections.Generic.List<string> warnings = new System.Collections.Generic.List<string>();
        public int bitCount;
        public Animator avatar;

        public bool leftHanded, wdSetting, useIndexFinger, brushSize, eraserSize, localSpace;
        public int localSpaceFullBody, gestureToDraw;

        public bool isQuest;
        public bool generateMasterMask;

        private bool isWdAutoSet;
        private readonly ScriptFunctions.PlayableLayer[] playablesUsedPC = {
            ScriptFunctions.PlayableLayer.Gesture, ScriptFunctions.PlayableLayer.FX };
        private readonly ScriptFunctions.PlayableLayer[] playablesUsedQuest = { 
            ScriptFunctions.PlayableLayer.Gesture };

        const string R_ICON_DIR = "Shared/Icons/Editor Icons";

        Texture2D platformIcon;

        private void OnEnable()
        {
            isQuest = EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android;
            platformIcon = Resources.Load<Texture2D>(isQuest ? $"{R_ICON_DIR}/Meta" : $"{R_ICON_DIR}/Windows");
        }

        public void Reset()
        {
            if (((Marker)target).gameObject.GetComponent<VRCAvatarDescriptor>() != null)
                descriptor = ((Marker)target).gameObject.GetComponent<VRCAvatarDescriptor>();

            generateMasterMask = ((Marker)target).generateMasterMask;
            leftHanded = ((Marker)target).leftHanded;
            wdSetting = ((Marker)target).wdSetting;
            brushSize = ((Marker)target).brushSize;
            eraserSize = ((Marker)target).eraserSize;
            localSpace = ((Marker)target).localSpace;
            localSpaceFullBody = ((Marker)target).localSpaceFullBody;
            useIndexFinger = ((Marker)target).useIndexFinger;
            gestureToDraw = ((Marker)target).gestureToDraw;

            SetPreviousInstallSettings();
        }

        public override void OnInspectorGUI()
        {
            GUIStyle boxStyle = new GUIStyle("box") { stretchWidth = true };
            boxStyle.normal.textColor = new GUIStyle("label").normal.textColor;
            GUIStyle titleStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, richText = true };
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button) { fontStyle = FontStyle.Bold };

            GUILayout.Space(8);
            EditorGUILayout.BeginHorizontal(boxStyle);
            if (isQuest) {
                EditorGUILayout.LabelField("<b><size=14>Quest Marker 3.0</size></b> <size=12>by ksivl + Cam @ VRLabs</size>",
                    titleStyle, GUILayout.MinHeight(20f));

                // draw platform icons if space is available
            } else {
                EditorGUILayout.LabelField("<b><size=14>PC Marker 3.0</size></b> <size=12>by ksivl @ VRLabs</size>",
                    titleStyle, GUILayout.MinHeight(20f));
            }
            EditorGUILayout.EndHorizontal();

            int minWidth = isQuest ? 370 : 300;
            if (Screen.width > minWidth)
            {
                GUI.DrawTexture(new Rect(25, 8, 32, 32), platformIcon);
                GUI.DrawTexture(new Rect(Screen.width - 45, 8, 32, 32), platformIcon);
            }

            if (EditorApplication.isPlaying)
            {
                if (((Marker)target).finished == false)
                {
                    GUILayout.Space(8);
                    EditorGUILayout.LabelField("Please exit Play Mode to use this script.");
                    return;
                }
            }

            if (((Marker)target).finished == false)
            {
                GUILayout.Space(8);

                EditorGUI.BeginChangeCheck();
                descriptor = (VRCAvatarDescriptor)EditorGUILayout.ObjectField("Avatar", descriptor, typeof(VRCAvatarDescriptor), true);
                if (descriptor != null)
                {
                    avatar = descriptor.gameObject.GetComponent<Animator>();
                }

                GUILayout.Space(8);

                leftHanded = EditorGUILayout.ToggleLeft("Left-handed", leftHanded);
                if (isWdAutoSet)
                {
                    GUI.enabled = false;
                    wdSetting = EditorGUILayout.ToggleLeft(new GUIContent("Write Defaults (auto-detected)", "Check this if you " +
                        "are animating your avatar with Write Defaults on. Otherwise, leave unchecked."), wdSetting);
                    GUI.enabled = true;
                }
                else
                {
                    wdSetting = EditorGUILayout.ToggleLeft(new GUIContent("Write Defaults", "Could not auto-detect.\n" +
                        "Check this if you are animating your avatar with Write Defaults on. Otherwise, leave unchecked."), wdSetting);
                }

                generateMasterMask = EditorGUILayout.ToggleLeft(
                    new GUIContent("Generate Master Mask",
                        "Enable this if you want to generate a master mask for your FX layer - if you animate transforms on your Gesture layer" +
                        ", you will most likely want to check this")
                    , generateMasterMask
                );

                string[] gestureOptions = new string[] {
                    null, "Fist", "Openhand", "Fingerpoint", "Victory", "Rock'n'Roll", "Handgun", "Thumbs up"
                };
                gestureToDraw = EditorGUILayout.Popup(new GUIContent(
                    "Gesture to draw",
                    "Fingerpoint is recommended. Avoid Rock'n'Roll on Oculus controllers; you'll accidentally draw."),
                    gestureToDraw,
                    gestureOptions
                );

                GUILayout.Space(8);

                if (!isQuest)
                {
                    brushSize = EditorGUILayout.ToggleLeft("Adjustable brush size", brushSize);
                    eraserSize = EditorGUILayout.ToggleLeft("Adjustable eraser size", eraserSize);
                }

                useIndexFinger = EditorGUILayout.ToggleLeft(new GUIContent("Use index finger to draw", "By default, you draw " +
                    "with a shiny pen. Check this to draw with your index finger instead."), useIndexFinger);

                if (!isQuest)
                {
                    localSpace = EditorGUILayout.ToggleLeft(new GUIContent("Enable local space", "Check this to be able to " +
                        "attach your drawings to various locations on your body! If unchecked, you can only attach your drawing " +
                        "to your player capsule."), localSpace);
                }

                if (!isQuest && localSpace)
                {
                    GUIContent[] layoutOptions = {
                        new GUIContent("Half-Body (Hips, Chest, Head, Hands)", "You can attach " +
                        "the drawing to your hips, chest, head, or either hand."),

                        new GUIContent("Full-Body (Half-Body Plus Feet)",
                        "You can also attach the drawing to your feet! (For half-body users, the drawing would follow VRChat's " +
                        "auto-footstep IK)")
                    };

                    GUILayout.BeginVertical("Box");
                    localSpaceFullBody = GUILayout.SelectionGrid(localSpaceFullBody, layoutOptions, 1);
                    GUILayout.EndVertical();
                }

                GUILayout.Space(8);

                GetBitCount();


                // WD warning - separately handled since installation should still be allowed
                if (descriptor != null)
                {
                    var states = descriptor.AnalyzeWDState();
                    bool isMixed = states.HaveMixedWriteDefaults(out bool isOn);

                    if (isMixed)
                    {
                        GUILayout.Box("Your avatar has mixed Write Defaults settings on its playable layers' states, " +
                            "which can cause issues with animations. The VRChat standard is Write Defaults OFF. " +
                            "It is recommended that Write Defaults for all states should either be all ON or all OFF.", boxStyle);
                    }
                    else
                    {
                        wdSetting = isOn;
                        isWdAutoSet = true;
                    }

                    bool hasEmptyAnimations = states.HaveEmpyMotionsInStates();

                    if (hasEmptyAnimations)
                    {
                        GUILayout.Box("Some states have no motions, this can be an issue when using WD Off.", boxStyle);
                    }

                }

                EditorGUILayout.LabelField($"Parameter memory needed: {bitCount}");

                // "Generate" button
                CheckRequirements();
                if (warnings.Count == 0)
                {
                    if (GUILayout.Button("Generate Marker", buttonStyle))
                    {
                        Debug.Log("Generating Marker...");
                        try
                        {
                            Marker markerRef = target as Marker;
                            MarkerInstaller.Generate(descriptor, ref markerRef, isQuest);
                        }
                        catch (Exception e)
                        {
                            Debug.LogException(e);
                            EditorUtility.DisplayDialog("Error Generating Marker", "Sorry, an error occured generating the Marker. " +
                                "Please take a snapshot (hint: use shift + windows key + S) of this code monkey information and send " +
                                "it to ksivl#4278 so it can be resolved.\n=================================================\n" +
                                e.Message + "\n" + e.Source + "\n" + e.StackTrace, "OK");
                        };
                    }
                }
                else
                {
                    for (int i = 0; i < warnings.Count; i++)
                    {
                        GUILayout.Box(warnings[i], boxStyle);
                    }
                    GUI.enabled = false;
                    GUILayout.Button("Generate Marker", buttonStyle);
                    GUI.enabled = true;
                }

                // "Remove" button
                if (ScriptFunctions.HasPreviousInstall(descriptor, "Marker", playablesUsedPC, "M_", "Marker")
                    || ScriptFunctions.HasPreviousInstall(descriptor, "Marker", playablesUsedQuest, "M_", "Marker"))
                {
                    if (GUILayout.Button("Remove Marker", buttonStyle))
                    {
                        if (EditorUtility.DisplayDialog("Remove Marker", "Uninstall the VRLabs Marker from the avatar?", "Yes", "No"))
                        {
                            MarkerInstaller.Uninstall(descriptor);
                            Debug.Log("Successfully removed Marker.");
                        }
                    }
                }
                else
                {
                    GUI.enabled = false;
                    GUILayout.Button("Remove Marker", buttonStyle);
                    GUI.enabled = true;
                }
            }
            // Once script is run
            else if (((Marker)target).finished == true && !isQuest)
            {
                GUILayout.Space(8);
                if (GUILayout.Button(new GUIContent("Adjust MarkerTarget transform", "If needed, move, rotate, or scale MarkerTarget " +
                    "so it's either in your hand (marker model) or at the tip of your index finger (no marker model).")))
                {
                    if (((Marker)target).markerTarget.gameObject == null)
                    {
                        Debug.LogError("Can't find MarkerTarget! It may have been moved or deleted.");
                    }
                    else
                    {
                        Selection.activeGameObject = ((Marker)target).markerTarget.gameObject;
                    }
                }

                GUILayout.Space(8);

                if (EditorApplication.isPlaying)
                {
                    GUI.enabled = false;
                    GUILayout.Button("Finish Setup", buttonStyle);
                    GUI.enabled = true;
                }
                else
                {
                    if (GUILayout.Button("Finish Setup", buttonStyle))
                    {
                        DestroyImmediate(((Marker)target));
                        DestroyImmediate(this);
                        // end script
                    }
                }
            }

            ((Marker)target).leftHanded = leftHanded;
            ((Marker)target).wdSetting = wdSetting;
            ((Marker)target).brushSize = brushSize;
            ((Marker)target).eraserSize = eraserSize;
            ((Marker)target).localSpace = localSpace;
            ((Marker)target).localSpaceFullBody = localSpaceFullBody;
            ((Marker)target).useIndexFinger = useIndexFinger;
            ((Marker)target).gestureToDraw = gestureToDraw;
        }

        private void SetPreviousInstallSettings()
        {
            if (descriptor != null)
            {
                if (ScriptFunctions.HasPreviousInstall(descriptor, "Marker", playablesUsedPC, "M_", "Marker")
                    || ScriptFunctions.HasPreviousInstall(descriptor, "Marker", playablesUsedQuest, "M_", "Marker"))
                {
                    if ((descriptor.baseAnimationLayers.Length >= 5)
                        && (descriptor.baseAnimationLayers[4].animatorController is AnimatorController controller)
                        && (controller != null))
                    // 1st cond: must have all 5 humanoid layers in descriptor
                    {
                        leftHanded = controller.HasLayer("M_Marker L");
                        brushSize = controller.HasLayer("M_Size");
                        eraserSize = controller.HasLayer("M_EraserSize");
                        localSpace = (!controller.HasLayer("M_SpaceSimple")) && (!controller.HasLayer("M_CullSimple"));

                        int index = -1;
                        for (int i = 0; i < controller.layers.Length; i++)
                        {
                            if (controller.layers[i].name.StartsWith("M_Marker"))
                            {
                                index = i;
                                break;
                            }
                        }
                        if (index != -1)
                        {
                            AnimatorStateTransition[] transitions = controller.layers[index]
                                .stateMachine.states.SelectMany(x => x.state.transitions)
                                .ToArray();

                            AnimatorCondition[] conditions = transitions
                                .SelectMany(x => x.conditions)
                                .Where(x => x.parameter.Contains("Gesture"))
                                .ToArray();

                            if (conditions.Length > 0)
                                gestureToDraw = (int)conditions[0].threshold;
                        }
                    }
                    if (descriptor.transform.Find("Marker/Model") == null) useIndexFinger = true;
                    if ((descriptor.transform.Find("Marker/World/Local") is Transform t) && (t != null))
                    {
                        if ((t.GetComponent<ParentConstraint>() is ParentConstraint p) && (p != null))
                            if ((p.GetSource(6).sourceTransform != null) && (p.GetSource(7).sourceTransform != null))
                                localSpaceFullBody = 1;
                    }
                }
            }
        }

        private void CheckRequirements()
        {
            warnings.Clear();
            if (!AssetDatabase.IsValidFolder("Assets/VRLabs/Marker"))
                warnings.Add("The folder at path 'Assets/VRLabs/Marker' could not be found. Make sure you are importing " +
                    "a Unity package and not moving the folder.");

            if (descriptor == null)
                warnings.Add("There is no avatar descriptor on this GameObject. Please move this script onto your avatar, " +
                    "or create an avatar descriptor here.");
            else
            {
                if (descriptor.expressionParameters != null
                    && descriptor.expressionParameters.CalcTotalCost() > (VRCExpressionParameters.MAX_PARAMETER_COST - bitCount))
                {
                    warnings.Add("You don't have enough free memory in your avatar's Expression Parameters to generate. " +
                        "You need " + (VRCExpressionParameters.MAX_PARAMETER_COST - bitCount) + " or less bits of parameter " +
                        "memory utilized.");
                }
                if (descriptor.expressionsMenu != null)
                {
                    if (descriptor.expressionsMenu.controls.Count == 8)
                    {
                        warnings.Add("Your avatar's topmost menu is full. Please have at least one empty control slot available.");
                    }
                }

                if (avatar == null)
                {
                    warnings.Add("There is no Animator on this avatar. Please add an Animator component on your avatar.");
                }
                else if (avatar.avatar == null)
                {
                    warnings.Add("Please add an avatar in this avatar's Animator component.");
                }
                else
                {
                    if (!avatar.isHuman)
                    {
                        warnings.Add("Please use this script on an avatar with a humanoid rig.");
                    }
                    else
                    {
                        // check avatar is humanoid and layers are valid
                        // check all humanoid layers exist (since they can disappear when avatar rig is set human -> generic -> human)
                        if (descriptor.baseAnimationLayers.Length < 5)
                        {
                            warnings.Add("You are missing the humanoid playable layers in your avatar descriptor. " +
                                "Try clicking 'Reset to Default' in your avatar descriptor.");
                        }
                        else if (!descriptor.baseAnimationLayers[2].isDefault) // check gesture layer validity
                        {
                            if (descriptor.baseAnimationLayers[2].animatorController != null
                                && descriptor.baseAnimationLayers[2].animatorController.name != "")
                            {
                                if (descriptor.baseAnimationLayers[2].animatorController is AnimatorController gesture)
                                {
                                    if (gesture.layers[0].avatarMask == null || gesture.layers[0].avatarMask.name == "")
                                    {
                                        warnings.Add("The first layer of your avatar's gesture layer is missing a mask. Try " +
                                            "setting a mask, or using a copy of the VRCSDK gesture controller, or removing the " +
                                            "controller from your avatar descriptor.");
                                    }
                                }
                                else
                                {
                                    warnings.Add("The gesture layer on this avatar is not an animator controller.");
                                }
                            }
                        }
                        // check bones are mapped
                        if (useIndexFinger
                            && ((avatar.GetBoneTransform(HumanBodyBones.LeftIndexDistal) == null)
                            || (avatar.GetBoneTransform(HumanBodyBones.RightIndexDistal) == null)))
                        {
                            warnings.Add("Your avatar rig's left and/or right index finger's distal bone is unmapped!");
                        }
                        if ((avatar.GetBoneTransform(HumanBodyBones.LeftHand) == null)
                            || (avatar.GetBoneTransform(HumanBodyBones.RightHand) == null))
                        {
                            warnings.Add("Your avatar rig's left and/or right hand is unmapped!");
                        }
                        if (localSpace)
                        {
                            if ((avatar.GetBoneTransform(HumanBodyBones.Hips) == null)
                                || (avatar.GetBoneTransform(HumanBodyBones.Chest) == null)
                                || (avatar.GetBoneTransform(HumanBodyBones.Head) == null)
                                || (avatar.GetBoneTransform(HumanBodyBones.Neck) == null))
                            {
                                warnings.Add("Your avatar rig's hips, chest, neck, and/or head is unmapped!");
                            }
                            if (localSpaceFullBody == 1)
                            {
                                if ((avatar.GetBoneTransform(HumanBodyBones.LeftFoot) == null)
                                    || (avatar.GetBoneTransform(HumanBodyBones.RightFoot) == null))
                                {
                                    warnings.Add("Your avatar rig's left and/or right foot is unmapped!");
                                }
                            }
                        }
                    }
                }
            }
        }

        private int GetBitCount()
        {
            // PC: M_Marker, M_Clear, M_Eraser, and M_Menu are bools(1+1+1+1); M_Color is a float(+8). always included
            // Quest: M_Marker, M_Color
            bitCount = isQuest ? 9 : 12;

            if (!isQuest)
            {
                if (brushSize && !isQuest) // float
                    bitCount += 8;
                if (eraserSize && !isQuest) // float
                    bitCount += 8;
                if (localSpace && !isQuest) // int
                    bitCount += 8;
                else // bool
                    bitCount += 1;
            }

            return bitCount;
        }
    }
}
#endif