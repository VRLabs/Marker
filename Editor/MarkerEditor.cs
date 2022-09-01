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
        Marker marker;

        public VRCAvatarDescriptor descriptor;
        public System.Collections.Generic.List<string> warnings = new System.Collections.Generic.List<string>();
        public int bitCount;
        public Animator animator;

        public bool leftHanded, wdSetting, useIndexFinger, brushSize, eraserSize, localSpace;
        public int localSpaceFullBody, gestureToDraw;

        public bool isQuest;
        public bool generateMasterMask;

        bool isWdAutoSet;
        int memoryAvailable;


        // styles
        GUIStyle boxStyle, titleStyle, buttonStyle;

        // private stuff
        private string[] gestureOptions = new string[] {
            null, "Fist", "Openhand", "Fingerpoint", "Victory", "Rock'n'Roll", "Handgun", "Thumbs up"
        };
        private readonly ScriptFunctions.PlayableLayer[] playablesUsedPC = {
            ScriptFunctions.PlayableLayer.Gesture, ScriptFunctions.PlayableLayer.FX 
        };
        private readonly ScriptFunctions.PlayableLayer[] playablesUsedQuest = { 
            ScriptFunctions.PlayableLayer.Gesture 
        };

        const string R_ICON_DIR = "Shared/Icons/Editor Icons";

        Texture2D platformIcon;

        private void OnEnable()
        {
            isQuest = EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android;
            platformIcon = Resources.Load<Texture2D>(isQuest ? $"{R_ICON_DIR}/Meta" : $"{R_ICON_DIR}/Windows");
            marker = target as Marker;
        }

        public void Reset()
        {
            if (marker == null)
                return;

            descriptor = marker.gameObject.GetComponent<VRCAvatarDescriptor>();

            wdSetting = marker.wdSetting;
            brushSize = marker.brushSize;
            leftHanded = marker.leftHanded;
            eraserSize = marker.eraserSize;
            localSpace = marker.localSpace;
            gestureToDraw = marker.gestureToDraw;
            useIndexFinger = marker.useIndexFinger;
            localSpaceFullBody = marker.localSpaceFullBody;
            generateMasterMask = marker.generateMasterMask;

            SetPreviousInstallSettings();
        }

        void InitStyles() {
            boxStyle = new GUIStyle("box") { 
                stretchWidth = true 
            };
            boxStyle.normal.textColor = new GUIStyle("label").normal.textColor;

            titleStyle = new GUIStyle(GUI.skin.label) { 
                alignment = TextAnchor.MiddleCenter, 
                richText = true, 
            };

            buttonStyle = new GUIStyle(GUI.skin.button) { 
                fontStyle = FontStyle.Bold 
            };
        }

        public override void OnInspectorGUI()
        {
            // init the styles
            InitStyles();

            // update bit count
            GetBitCount();

            // title
            GUILayout.Space(8);
            using (new EditorGUILayout.HorizontalScope(boxStyle))
            {
                string label = isQuest
                    ? "<b><size=14>Quest Marker 3.0</size></b> <size=12>by ksivl + Cam @ VRLabs</size>"
                    : "<b><size=14>PC Marker 3.0</size></b> <size=12>by ksivl + Cam @ VRLabs</size>";

                EditorGUILayout.LabelField(label, titleStyle, GUILayout.MinHeight(20f));
            }

            if (Screen.width > (isQuest ? 370 : 300)) {
                GUI.DrawTexture(new Rect(25, 8, 32, 32), platformIcon);
                GUI.DrawTexture(new Rect(Screen.width - 45, 8, 32, 32), platformIcon);
            }

            // user cannot run the script while application playing
            if (EditorApplication.isPlaying)
            {
                if (marker.finished == false)
                {
                    GUILayout.Space(8);
                    EditorGUILayout.LabelField("Please exit Play Mode to use this script.");
                    return;
                }
            }

            if (marker.finished == false)
            {
                GUILayout.Space(8);

                EditorGUI.BeginChangeCheck();
                descriptor = (VRCAvatarDescriptor)EditorGUILayout.ObjectField(
                    "Avatar",
                    descriptor,
                    typeof(VRCAvatarDescriptor),
                    true
                );

                // only scan for avatar errors when the descriptor field has been modified
                if (EditorGUI.EndChangeCheck() && descriptor != null) {
                    ScanAvatar();
                    gestureToDraw = 3;
                }

                GUI.enabled = descriptor != null;

                GUILayout.Space(8);

                //leftHanded = EditorGUILayout.ToggleLeft("Left-handed", leftHanded);
                using (new EditorGUI.DisabledGroupScope(isWdAutoSet))
                {
                    string subtext = isWdAutoSet ? String.Empty : "Could not auto-detect.\n";
                    subtext += "Check this if you are animating your avatar with Write Defaults on. Otherwise, leave unchecked.";

                    wdSetting = EditorGUILayout.ToggleLeft(new GUIContent(
                        "Write Defaults (auto-detected)", subtext
                    ), wdSetting);
                }

                generateMasterMask = EditorGUILayout.ToggleLeft(new GUIContent(
                    "Generate Master Mask",
                    "Enable this if you want to generate a master mask for your FX layer - if you animate transforms " +
                    "on your Gesture layer, you will most likely want to check this"
                ), generateMasterMask);

                gestureToDraw = EditorGUILayout.Popup(new GUIContent(
                    "Gesture to draw",
                    "Fingerpoint is recommended. Avoid Rock'n'Roll on Oculus controllers; you'll accidentally draw."),
                    gestureToDraw,
                    gestureOptions
                );

                GUILayout.Space(8);

                EditorGUI.BeginChangeCheck();
                useIndexFinger = EditorGUILayout.ToggleLeft(new GUIContent(
                    "Use index finger to draw", "By default, you draw " +
                    "with a shiny pen. Check this to draw with your index finger instead."
                ), useIndexFinger);

                if (!isQuest)
                {
                    brushSize = EditorGUILayout.ToggleLeft("Adjustable brush size", brushSize);
                    eraserSize = EditorGUILayout.ToggleLeft("Adjustable eraser size", eraserSize);

                    localSpace = EditorGUILayout.ToggleLeft(new GUIContent(
                        "Enable local space",
                        "Check this to be able to attach your drawings to various locations on your body! " +
                        "If unchecked, you can only attach your drawing to yourself."
                    ), localSpace);

                    using (new EditorGUI.DisabledGroupScope(!localSpace))
                    {
                        GUIContent[] layoutOptions = {
                            new GUIContent("Half-Body (Hips, Chest, Head, Hands)", "You can attach " +
                            "the drawing to your hips, chest, head, or either hand."),

                            new GUIContent("Full-Body (Half-Body Plus Feet)",
                            "You can also attach the drawing to your feet!")
                        };

                        GUILayout.BeginVertical("Box");
                        localSpaceFullBody = GUILayout.SelectionGrid(localSpaceFullBody, layoutOptions, 1);
                        GUILayout.EndVertical();
                    }
                }
                if (EditorGUI.EndChangeCheck())
                    ScanAvatar();

                GUILayout.Space(8);


                // display parameter memory stuff
                using (new EditorGUILayout.HorizontalScope(boxStyle))
                {
                    EditorGUILayout.LabelField($"Parameter Memory Used: {bitCount}", titleStyle);
                    if (descriptor != null)
                        EditorGUILayout.LabelField($"Parameter Memory Available: {memoryAvailable}", titleStyle);
                }
                
                // display warnings
                for (int i = 0; i < warnings.Count; i++) {
                    EditorGUILayout.HelpBox(warnings[i], MessageType.Warning);
                }

                // "Generate" button    
                using (new EditorGUI.DisabledGroupScope(warnings.Count > 0))
                {
                    if (GUILayout.Button("Generate Marker", buttonStyle)) {
                        GenerateMarker();
                    }
                }

                // "Remove" button
                bool hasPCInstall = ScriptFunctions.HasPreviousInstall(descriptor, "Marker", playablesUsedPC, "M_", "Marker");
                bool hasQuestInstall = ScriptFunctions.HasPreviousInstall(descriptor, "Marker", playablesUsedQuest, "M_", "Marker");
                using (new EditorGUI.DisabledGroupScope(!(hasPCInstall || hasQuestInstall)))
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
            }
            // Once script is run
            else if (marker.finished == true)
            {
                GUILayout.Space(8);
                if (GUILayout.Button(new GUIContent("Adjust MarkerTarget transform", "If needed, move, rotate, or scale MarkerTarget " +
                    "so it's either in your hand (marker model) or at the tip of your index finger (no marker model).")))
                {
                    if (((Marker)target).markerTargetRight.gameObject == null)
                    {
                        Debug.LogError("Can't find MarkerTarget! It may have been moved or deleted.");
                    }
                    else
                    {
                        Selection.activeGameObject = ((Marker)target).markerTargetRight.gameObject;
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

            marker.wdSetting = wdSetting;
            marker.brushSize = brushSize;
            marker.leftHanded = leftHanded;
            marker.eraserSize = eraserSize;
            marker.localSpace = localSpace;
            marker.gestureToDraw = gestureToDraw;
            marker.useIndexFinger = useIndexFinger;
            marker.localSpaceFullBody = localSpaceFullBody;
            marker.generateMasterMask = generateMasterMask;

            GUI.enabled = true;
        }

        void GenerateMarker()
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

        void ScanAvatar()
        {
            animator = descriptor.gameObject.GetComponent<Animator>();

            memoryAvailable = 256;
            if (descriptor.expressionParameters != null)
                memoryAvailable -= descriptor.expressionParameters.CalcTotalCost();

            // Check WD
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

            CheckRequirements();
        }

        void SetPreviousInstallSettings()
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

        void CheckRequirements()
        {
            /* Possible Warnings */
            /* folder exists
             * descriptor is null
             * descriptor has expression parameters
             * descriptor has expressions menu
             * animator exists
             * avatar is humanoid
             * has base animator layers
             * check avatar is humanoid and layers are valid
             * check all humanoid layers exist (since they can disappear when avatar rig is set human -> generic -> human)
             */

            warnings.Clear();
            if (!AssetDatabase.IsValidFolder("Assets/VRLabs/Marker")) {
                warnings.Add("The folder at path 'Assets/VRLabs/Marker' could not be found. Make sure you are importing " +
                    "a Unity package and not moving the folder.");
            }

            if (descriptor == null) {
                warnings.Add("There is no avatar descriptor on this GameObject. Please move this script onto your avatar, " +
                    "or create an avatar descriptor here.");
                return;
            }

            VRCExpressionParameters parameters = descriptor.expressionParameters;
            if (parameters != null && parameters.CalcTotalCost() > (VRCExpressionParameters.MAX_PARAMETER_COST - bitCount))
            {
                warnings.Add("You don't have enough free memory in your avatar's Expression Parameters to generate. " +
                    "You need " + (VRCExpressionParameters.MAX_PARAMETER_COST - bitCount) + " or less bits of parameter " +
                    "memory utilized.");
            }

            VRCExpressionsMenu menu = descriptor.expressionsMenu;
            if (menu != null && menu.controls.Count == 8) {
                warnings.Add("Your avatar's topmost menu is full. Please have at least one empty control slot available.");
            }

            if (animator == null) {
                warnings.Add("There is no Animator on this avatar. Please add an Animator component on your avatar.");
            }
            else if (animator.avatar == null) 
            {
                warnings.Add("Please add an avatar in this avatar's Animator component.");
            } 
            else 
            {
                if (!animator.isHuman) {
                    warnings.Add("Please use this script on an avatar with a humanoid rig.");
                    return;
                }

                // check avatar is humanoid and layers are valid
                // check all humanoid layers exist (since they can disappear when avatar rig is set human -> generic -> human)
                if (descriptor.baseAnimationLayers.Length < 5)
                {
                    warnings.Add("You are missing the humanoid playable layers in your avatar descriptor. " +
                        "Try clicking 'Reset to Default' in your avatar descriptor.");
                }
                else if (!descriptor.baseAnimationLayers[2].isDefault) // check gesture layer validity
                {
                    // get gesture controller
                    AnimatorController gestureCtrlr = descriptor.baseAnimationLayers[2].animatorController as AnimatorController;
                    bool invalidController = gestureCtrlr != null
                        && gestureCtrlr.name != String.Empty
                        && (gestureCtrlr.layers[0].avatarMask == null || gestureCtrlr.layers[0].avatarMask.name == "");

                    if (invalidController) {
                        warnings.Add("The first layer of your avatar's gesture layer is missing a mask. Try " +
                            "setting a mask, or using a copy of the VRCSDK gesture controller, or removing the " +
                            "controller from your avatar descriptor.");
                    }
                }

                // check bones are mapped
                CheckBoneMappings();
            }
        }

        void CheckBoneMappings()
        {
            // use index finger
            Transform leftIndex = animator.GetBoneTransform(HumanBodyBones.LeftIndexDistal);
            Transform rightIndex = animator.GetBoneTransform(HumanBodyBones.RightIndexDistal);
            Transform leftHand = animator.GetBoneTransform(HumanBodyBones.LeftHand);
            Transform rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);

            // local space
            Transform hips = animator.GetBoneTransform(HumanBodyBones.Hips);
            Transform chest = animator.GetBoneTransform(HumanBodyBones.Chest);
            Transform head = animator.GetBoneTransform(HumanBodyBones.Head);
            Transform neck = animator.GetBoneTransform(HumanBodyBones.Neck);

            // local space fbt
            Transform leftFoot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
            Transform rightFoot = animator.GetBoneTransform(HumanBodyBones.RightFoot);

            if (useIndexFinger)
            {
                if (leftIndex == null)
                    warnings.Add("Your avatar rig's left index finger's last bone is unmapped!");
                if (rightIndex == null)
                    warnings.Add("Your avatar rig's right index finger's last bone is unmapped!");
            }
            else
            {
                if (leftHand == null)
                    warnings.Add("Your avatar rig's left hand is unmapped!");
                if (rightHand == null)
                    warnings.Add("Your avatar rig's right hand is unmapped!");
            }

            if(localSpace && !isQuest) {
                if(hips == null)
                    warnings.Add("Your avatar rig's hips are unmapped!");
                if(chest == null)
                    warnings.Add("Your avatar rig's chest is unmapped!");
                if(head == null)
                    warnings.Add("Your avatar rig's head is unmapped!");
                if(neck == null)
                    warnings.Add("Your avatar rig's neck is unmapped!");

                if(localSpaceFullBody == 1) {
                    if(leftFoot == null)
                        warnings.Add("Your avatar rig's left foot is unmapped!");
                    if(rightFoot == null)
                        warnings.Add("Your avatar rig's right foot is unmapped!");
                }
            }
        }

        int GetBitCount()
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