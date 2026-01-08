// Marker by ksivl / VRLabs 3.0 Assets https://github.com/VRLabs/VRChat-Avatars-3.0
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
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
        public List<string> warnings = new List<string>();
        public int bitCount;
        public Animator animator;

        public bool leftHanded, wdSetting, useIndexFinger, brushSize, separateEraserScaling, combinedSize, localSpace, withMenu;
        public int localSpaceFullBody;

        public bool generateMasterMask;

        bool isWdAutoSet;
        int memoryAvailable;
        private static HashSet<int> initializedMarker = new HashSet<int>();

        // movement
        GameObject markerTargetObject, lastMarkerTargetObject, markerScale, menuScale;
        bool mirrorPosition, mirrorRotation;

        // styles
        GUIStyle boxStyle, titleStyle, buttonStyle;
        Texture2D splash;

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

        RuntimeAnimatorController originalController;

        private void OnEnable()
        {
            mirrorPosition = true;
            mirrorRotation = true;
            marker = target as Marker;
            if (marker == null) return;

            var oldDescriptor = descriptor;
            descriptor = FindDescriptor(marker.transform);
            if (descriptor != oldDescriptor) ScanAvatar();

            // First time initialization
            if (!marker.editorDefaultsApplied)
            {
                marker.editorDefaultsApplied = true;

                SetPreviousInstallSettings();

                // Default values
                marker.brushSize = true;
                marker.localSpace = true;
                marker.withMenu = true;

                brushSize = true;
                localSpace = true;
                withMenu = true;

                EditorUtility.SetDirty(marker);
            }

            wdSetting = marker.wdSetting;
            brushSize = marker.brushSize;
            leftHanded = marker.leftHanded;
            withMenu = marker.withMenu;
            separateEraserScaling = marker.separateEraserScaling;
            localSpace = marker.localSpace;
            useIndexFinger = marker.useIndexFinger;
            localSpaceFullBody = marker.localSpaceFullBody;
            generateMasterMask = marker.generateMasterMask;

            if (marker.markerScale != null)
                markerScale = marker.markerScale.gameObject;

            if (marker.menuScale != null)
                menuScale = marker.menuScale.gameObject;


            platformIcon = Resources.Load<Texture2D>(marker.isQuest ? $"{R_ICON_DIR}/Meta" : $"{R_ICON_DIR}/Windows");
            splash = Resources.Load<Texture2D>("Media/BG");

            StopAnimationPreview();
        }

        public void Reset()
        {
            if (marker == null) return;

            descriptor = FindDescriptor(marker.transform);
            SetPreviousInstallSettings();

            marker.brushSize = true;
            marker.localSpace = true;
            marker.withMenu = true;

            marker.editorDefaultsApplied = true;

            wdSetting = marker.wdSetting;
            brushSize = marker.brushSize;
            leftHanded = marker.leftHanded;
            withMenu = marker.withMenu;
            separateEraserScaling = marker.separateEraserScaling;
            localSpace = marker.localSpace;
            useIndexFinger = marker.useIndexFinger;
            localSpaceFullBody = marker.localSpaceFullBody;
            generateMasterMask = marker.generateMasterMask;

            if (marker.markerScale != null)
                markerScale = marker.markerScale.gameObject;

            if (marker.menuScale != null)
                menuScale = marker.menuScale.gameObject;

            EditorUtility.SetDirty(marker);
            Repaint();
        }

        private void OnSceneGUI()
        {
            if (marker == null || !marker.finished || markerTargetObject == null)
                return;

            // re focus camera on change
            if (lastMarkerTargetObject != markerTargetObject) {
                SceneView.currentDrawingSceneView.pivot = markerTargetObject.transform.position;
                lastMarkerTargetObject = markerTargetObject;
            }

            // pos rot scale
            Vector3 pos = markerTargetObject.transform.position;
            Quaternion rot = markerTargetObject.transform.rotation;
            Vector3 scale;
            if (markerTargetObject.Equals(marker.markerTargetLeft.gameObject) || markerTargetObject.Equals(marker.markerTargetRight.gameObject))
            {
                scale = ((Marker)target).markerScale.transform.localScale;
            }
            else
            {
                scale = ((Marker)target).menuScale.transform.localScale;
            }

            EditorGUI.BeginChangeCheck();
            Handles.TransformHandle(ref pos, ref rot, ref scale);
            if (EditorGUI.EndChangeCheck())
            {
                UnityEngine.Object[] undoObjects = new UnityEngine.Object[mirrorPosition ? 3 : 2];

                bool isMarker = markerTargetObject.Equals(marker.markerTargetLeft.gameObject) ||
                                markerTargetObject.Equals(marker.markerTargetRight.gameObject);

                if (isMarker)
                {
                    undoObjects[0] = ((Marker)target).markerScale.transform;
                }
                else
                {
                    undoObjects[0] = ((Marker)target).menuScale.transform;
                }

                if(mirrorPosition) {
                    if (isMarker)
                    {
                        undoObjects[1] = marker.markerTargetLeft.transform;
                        undoObjects[2] = marker.markerTargetRight.transform;
                    }
                    else
                    {
                        undoObjects[1] = marker.menuTargetLeft.transform;
                        undoObjects[2] = marker.menuTargetRight.transform;
                    }
                }
                else
                {
                    undoObjects[1] = markerTargetObject.transform;
                }
                Undo.RecordObjects(undoObjects, "Move Marker");

                // set rotation 
                markerTargetObject.transform.position = pos;
                markerTargetObject.transform.rotation = rot;
                
                if (isMarker)
                {
                    ((Marker)target).markerScale.transform.localScale = new Vector3(scale.x, scale.y, scale.z);
                }
                else
                {
                    ((Marker)target).menuScale.transform.localScale = new Vector3(scale.x, scale.y, scale.z);
                }

                if (mirrorPosition) {
                    if (markerTargetObject.Equals(marker.markerTargetLeft.gameObject))
                    {
                        marker.markerTargetLeft.transform.position = pos;
                        pos.x *= -1;
                        marker.markerTargetRight.transform.position = pos;
                    }
                    else if (markerTargetObject.Equals(marker.markerTargetRight.gameObject))
                    {
                        marker.markerTargetRight.transform.position = pos;
                        pos.x *= -1;
                        marker.markerTargetLeft.transform.position = pos;
                    }
                    else if (markerTargetObject.Equals(marker.menuTargetLeft.gameObject))
                    {
                        marker.menuTargetLeft.transform.position = pos;
                        pos.x *= -1;
                        marker.menuTargetRight.transform.position = pos;
                    }
                    else if (markerTargetObject.Equals(marker.menuTargetRight.gameObject))
                    {
                        marker.menuTargetRight.transform.position = pos;
                        pos.x *= -1;
                        marker.menuTargetLeft.transform.position = pos;
                    }
                }
                
                // set rotation
                if (mirrorRotation)
                {
                    if (markerTargetObject.Equals(marker.markerTargetLeft.gameObject))
                    {
                        marker.markerTargetLeft.transform.rotation = rot;

                        Vector3 mirroredEuler = rot.eulerAngles;
                        mirroredEuler.y = (mirroredEuler.y * -1f);
                        mirroredEuler.z *= -1;
                        marker.markerTargetRight.transform.rotation = Quaternion.Euler(mirroredEuler);
                    }
                    else if (markerTargetObject.Equals(marker.markerTargetRight.gameObject))
                    {
                        marker.markerTargetRight.transform.rotation = rot;

                        Vector3 mirroredEuler = rot.eulerAngles;
                        mirroredEuler.y = (mirroredEuler.y * -1f);
                        mirroredEuler.z *= -1;
                        marker.markerTargetLeft.transform.rotation = Quaternion.Euler(mirroredEuler);
                    }
                    else if (markerTargetObject.Equals(marker.menuTargetLeft.gameObject))
                    {
                        marker.menuTargetLeft.transform.rotation = rot;

                        Vector3 mirroredEuler = rot.eulerAngles;
                        mirroredEuler.y *= -1;
                        mirroredEuler.z = (mirroredEuler.z * -1f) + 180;
                        marker.menuTargetRight.transform.rotation = Quaternion.Euler(mirroredEuler);
                    }
                    else if (markerTargetObject.Equals(marker.menuTargetRight.gameObject))
                    {
                        marker.menuTargetRight.transform.rotation = rot;

                        Vector3 mirroredEuler = rot.eulerAngles;
                        mirroredEuler.y *= -1;
                        mirroredEuler.z = (mirroredEuler.z * -1f) + 180;
                        marker.menuTargetLeft.transform.rotation = Quaternion.Euler(mirroredEuler);
                    }
                }
            }
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

        public VRCAvatarDescriptor FindDescriptor(Transform startTransform)
        {
            Transform current = startTransform;

            while (current != null)
            {
                VRCAvatarDescriptor component = current.GetComponent<VRCAvatarDescriptor>();

                if (component != null)
                {
                    return component;
                }

                current = current.parent;
            }

            return null;
        }

        public AnimatorController FindAnimatorController(Transform startTransform)
        {
            Transform current = startTransform;

            while (current != null)
            {
                AnimatorController component = current.GetComponent<AnimatorController>();

                if (component != null)
                {
                    return component;
                }

                current = current.parent;
            }

            return null;
        }

        public Animator FindAnimator(Transform startTransform)
        {
            Transform current = startTransform;

            while (current != null)
            {
                Animator component = current.GetComponent<Animator>();

                if (component != null)
                {
                    return component;
                }

                current = current.parent;
            }

            return null;
        }

        public override void OnInspectorGUI()
        {
            if (marker == null) OnEnable();

            // init the styles
            InitStyles();

            // update bit count
            GetBitCount();

            // title
            GUILayout.Space(8);

            //EditorGUI.DrawPreviewTexture(new Rect(0, 0, 1714 / 3f, 959 / 3f), splash);
            //EditorGUI.DrawPreviewTexture(new Rect(0, 0, 1714 / 3f, 959 / 3f), Texture2D.whiteTexture);

            using (new EditorGUILayout.HorizontalScope(boxStyle))
            {
                string label = marker.isQuest
                    ? "<b><size=14>Quest Marker 3.0</size></b> <size=12>by Cam + VRLabs</size>"
                    : "<b><size=14>PC Marker 3.0</size></b> <size=12>by Cam + VRLabs</size>";

                EditorGUILayout.LabelField(label, titleStyle, GUILayout.MinHeight(20f));
            }

            if (Screen.width > (marker.isQuest ? 370 : 340)) {
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
                if (descriptor != null && EditorGUI.EndChangeCheck()) {
                    ScanAvatar();
                    marker.gestureToDraw = 3;
                }

                GUI.enabled = descriptor != null;

                GUILayout.Space(8);
                
                using (new EditorGUI.DisabledGroupScope(isWdAutoSet))
                {
                    string subtext = isWdAutoSet ? String.Empty : "Could not auto-detect.\n";
                    subtext += "Check this if you are animating your avatar with Write Defaults on. Otherwise, leave unchecked.";

                    wdSetting = EditorGUILayout.ToggleLeft(new GUIContent(
                        "Write Defaults (auto-detected)", subtext
                    ), wdSetting);
                }

                marker.gestureToDraw = EditorGUILayout.Popup(new GUIContent(
                    "Gesture to draw",
                    "Fingerpoint is recommended. Avoid Rock'n'Roll on Oculus controllers; you'll accidentally draw."),
                    marker.gestureToDraw,
                    gestureOptions
                );

                GUILayout.Space(8);

                var oldQuest = marker.isQuest;
                marker.isQuest =
                    EditorGUILayout.ToggleLeft(new GUIContent("Install for Quest",
                        "Check this to install the Quest version"), marker.isQuest);
                if (marker.isQuest != oldQuest) platformIcon = Resources.Load<Texture2D>(marker.isQuest ? $"{R_ICON_DIR}/Meta" : $"{R_ICON_DIR}/Windows");

                EditorGUI.BeginChangeCheck();
                useIndexFinger = EditorGUILayout.ToggleLeft(new GUIContent(
                    "Use index finger to draw", "By default, you draw " +
                    "with a shiny pen. Check this to draw with your index finger instead."
                ), useIndexFinger);

                if (!marker.isQuest)
                {
                    GUILayout.Space(8);

                    brushSize = EditorGUILayout.ToggleLeft("Adjustable brush / eraser size", brushSize);

                    if (!brushSize)
                    {
                        GUI.enabled = false;
                    }

                    separateEraserScaling = EditorGUILayout.ToggleLeft("Adjust eraser size separately", separateEraserScaling);

                    if (!brushSize)
                    {
                        separateEraserScaling = false;
                    }

                    GUI.enabled = true;
                    GUILayout.Space(8);

                    localSpace = EditorGUILayout.ToggleLeft(new GUIContent(
                        "Enable local space",
                        "Check this to be able to attach your drawings to various locations on your body! " +
                        "If unchecked, you can only attach your drawing to yourself."
                    ), localSpace);

                    withMenu = EditorGUILayout.ToggleLeft("Arm attached Marker Menu", withMenu);
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
                using (new EditorGUI.DisabledGroupScope(warnings.Count > 0 && !(warnings.Count == 1 && warnings[0].Contains("free memory"))))
                {
                    if (GUILayout.Button("Generate Marker (Merge with existing controllers)", buttonStyle)) {
                        GenerateMarker(mergeOnCopy: false);
                    }
                }

                using (new EditorGUI.DisabledGroupScope(warnings.Count > 0 && !(warnings.Count == 1 && warnings[0].Contains("free memory"))))
                {
                    if (GUILayout.Button("Generate Marker (Merge with a copy of controllers)", buttonStyle))
                    {
                        GenerateMarker(mergeOnCopy: true);
                    }
                }

                GUILayout.Space(16);

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
                            Debug.Log($"{MarkerStaticResources.MarkerLogTag}Successfully removed Marker.");

                            if (descriptor.baseAnimationLayers[(int)ScriptFunctions.PlayableLayer.FX].animatorController.name.ToLower().EndsWith("_marker") ||
                                descriptor.baseAnimationLayers[(int)ScriptFunctions.PlayableLayer.Gesture].animatorController.name.ToLower().EndsWith("_marker"))
                            {
                                Debug.LogWarning($"{MarkerStaticResources.MarkerLogTag}DON'T FORGET TO CHANGE THE FX AND GESTURE CONTROLLERS BACK TO YOUR ORIGINAL ONES!");
                            }
                        }
                    }
                }
            }
            // Once script is run
            //else if(!marker.isQuest)
            else
            {
                GUILayout.Space(8);

                GUIStyle redBoldLabel = new GUIStyle(EditorStyles.label);
                redBoldLabel.normal.textColor = Color.red;
                redBoldLabel.fontStyle = FontStyle.Bold;

                var style = new GUIStyle(EditorStyles.label)
                {
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 16
                };
                style.normal.textColor = Color.red;
                style.hover.textColor = Color.red;
                style.active.textColor = Color.red;
                style.focused.textColor = Color.red;

                GUILayout.Label("Make sure you have gizmos enabled!", style);

                GUILayout.Space(8);

                marker.showGizmos = EditorGUILayout.ToggleLeft(
                    "Show Gizmos", 
                    marker.showGizmos
                );

                // Mirroring is not available on Quest, as the marker is directly on the hand
                using (new EditorGUILayout.HorizontalScope())
                {
                    mirrorPosition = EditorGUILayout.ToggleLeft(
                        new GUIContent("Mirror Position", "Move both marker target positions simultaneously"),
                        mirrorPosition,
                        GUILayout.Width(EditorGUIUtility.currentViewWidth / 3)
                    );
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    mirrorRotation = EditorGUILayout.ToggleLeft(
                        new GUIContent("Mirror Rotation", "Rotate both marker targets simultaneously"),
                        mirrorRotation,
                        GUILayout.Width(EditorGUIUtility.currentViewWidth / 3)
                    );
                }

                EditorGUILayout.Space(10);

                Animator animator = FindAnimator(marker.transform);

                if (!marker.isQuest)
                {
                    GUIStyle centeredBoldLabel = new GUIStyle(EditorStyles.label);
                    centeredBoldLabel.fontSize = 16;
                    centeredBoldLabel.alignment = TextAnchor.MiddleCenter;
                    centeredBoldLabel.fontStyle = FontStyle.Bold;

                    EditorGUILayout.LabelField("Marker", centeredBoldLabel);
                    EditorGUILayout.Space(5);

                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button(new GUIContent("Adjust Right Marker Position", "If needed, move, rotate, or scale MarkerTarget " +
                        "so it's either in your hand (marker model) or at the tip of your index finger (no marker model)."), GUILayout.Height(35)))
                    {
                        if (((Marker)target).markerTargetRight.gameObject == null)
                        {
                            Debug.LogError($"{MarkerStaticResources.MarkerLogTag}Can't find MarkerTarget! It may have been moved or deleted.");
                        }
                        else
                        {
                            VRC.SDK3.Dynamics.Constraint.Components.VRCParentConstraint constraint = marker.system.GetComponentInParent<VRC.SDK3.Dynamics.Constraint.Components.VRCParentConstraint>();
                            VRC.Dynamics.VRCConstraintSource source1 = constraint.Sources[1];
                            source1.Weight = 0f;
                            constraint.Sources[1] = source1;
                            VRC.Dynamics.VRCConstraintSource source2 = constraint.Sources[0];
                            source2.Weight = 1f;
                            constraint.Sources[0] = source2;

                            markerTargetObject = marker.markerTargetRight.gameObject;
                            if (marker.markerModel != null)
                            {
                                marker.markerModel.GetComponent<MeshRenderer>().enabled = true;
                            }
                            if (marker.menu != null)
                            {
                                marker.menu.gameObject.SetActive(false);
                            }

                            marker.gizmosMenu = false;
                            
                            StartAnimationPreview();
                        }
                    }

                    if (GUILayout.Button(new GUIContent("Adjust Left Marker Position", "If needed, move, rotate, or scale MarkerTarget " +
                        "so it's either in your hand (marker model) or at the tip of your index finger (no marker model)."), GUILayout.Height(35)))
                    {
                        if (((Marker)target).markerTargetLeft.gameObject == null)
                        {
                            Debug.LogError($"{MarkerStaticResources.MarkerLogTag}Can't find MarkerTarget! It may have been moved or deleted.");
                        }
                        else
                        {
                            VRC.SDK3.Dynamics.Constraint.Components.VRCParentConstraint constraint = marker.system.GetComponentInParent<VRC.SDK3.Dynamics.Constraint.Components.VRCParentConstraint>();
                            VRC.Dynamics.VRCConstraintSource source1 = constraint.Sources[1];
                            source1.Weight = 1f;
                            constraint.Sources[1] = source1;
                            VRC.Dynamics.VRCConstraintSource source2 = constraint.Sources[0];
                            source2.Weight = 0f;
                            constraint.Sources[0] = source2;

                            markerTargetObject = marker.markerTargetLeft.gameObject;
                            if (marker.markerModel != null)
                            {
                                marker.markerModel.GetComponent<MeshRenderer>().enabled = true;
                            }
                            if (marker.menu != null)
                            {
                                marker.menu.gameObject.SetActive(false);
                            }
                            
                            marker.gizmosMenu = false;
                            
                            StartAnimationPreview();
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    // Quest
                    /*if (GUILayout.Button(new GUIContent("Adjust Marker Position", "If needed, move, rotate or scale the Marker so it lines up with your finger"), GUILayout.Height(35)))
                    {
                        GameObject markerObject = GetQuestMarkerObject();

                        if (markerObject == null)
                        {
                            Debug.LogError($"{MarkerStaticResources.MarkerLogTag}Can't find Marker! It may have been moved or deleted.");
                        }
                        else
                        {
                            mirrorPosition = false;
                            mirrorRotation = false;
                            markerTargetObject = markerObject;
                            ((Marker)target).markerScale = markerObject.transform;
                            StartAnimationPreview();
                        }
                    }*/

                    EditorGUILayout.BeginHorizontal();

                    if (GUILayout.Button(new GUIContent("Adjust Right Marker Position", "If needed, move, rotate, or scale MarkerTarget " +
                        "so it's either in your hand (marker model) or at the tip of your index finger (no marker model)."), GUILayout.Height(35)))
                    {
                        if (((Marker)target).markerTargetRight.gameObject == null)
                        {
                            Debug.LogError($"{MarkerStaticResources.MarkerLogTag}Can't find MarkerTarget! It may have been moved or deleted.");
                        }
                        else
                        {
                            GameObject markerObject = GetQuestMarkerObject();

                            VRC.SDK3.Dynamics.Constraint.Components.VRCParentConstraint constraint = markerObject.GetComponent<VRC.SDK3.Dynamics.Constraint.Components.VRCParentConstraint>();
                            VRC.Dynamics.VRCConstraintSource source1 = constraint.Sources[1];
                            source1.Weight = 1f;
                            constraint.Sources[1] = source1;
                            VRC.Dynamics.VRCConstraintSource source2 = constraint.Sources[0];
                            source2.Weight = 0f;
                            constraint.Sources[0] = source2;

                            if (markerObject == null)
                            {
                                Debug.LogError($"{MarkerStaticResources.MarkerLogTag}Can't find marker");
                            }
                            else
                            {
                                markerTargetObject = marker.markerTargetRight.gameObject;
                                ((Marker)target).markerScale = markerObject.transform;

                                if (marker.markerModel != null)
                                {
                                    marker.markerModel.GetComponent<MeshRenderer>().enabled = true;
                                }
                                marker.gizmosMenu = false;

                                StartAnimationPreview();
                            }
                        }
                    }

                    if (GUILayout.Button(new GUIContent("Adjust Left Marker Position", "If needed, move, rotate, or scale MarkerTarget " +
                        "so it's either in your hand (marker model) or at the tip of your index finger (no marker model)."), GUILayout.Height(35)))
                    {
                        if (((Marker)target).markerTargetLeft.gameObject == null)
                        {
                            Debug.LogError($"{MarkerStaticResources.MarkerLogTag}Can't find MarkerTarget! It may have been moved or deleted.");
                        }
                        else
                        {
                            GameObject markerObject = GetQuestMarkerObject();

                            VRC.SDK3.Dynamics.Constraint.Components.VRCParentConstraint constraint = markerObject.GetComponent<VRC.SDK3.Dynamics.Constraint.Components.VRCParentConstraint>();
                            VRC.Dynamics.VRCConstraintSource source1 = constraint.Sources[1];
                            source1.Weight = 0f;
                            constraint.Sources[1] = source1;
                            VRC.Dynamics.VRCConstraintSource source2 = constraint.Sources[0];
                            source2.Weight = 1f;
                            constraint.Sources[0] = source2;

                            if (markerObject == null)
                            {
                                Debug.LogError($"{MarkerStaticResources.MarkerLogTag}Can't find marker");
                            }
                            else
                            {
                                markerTargetObject = marker.markerTargetLeft.gameObject;
                                ((Marker)target).markerScale = markerObject.transform;

                                if (marker.markerModel != null)
                                {
                                    marker.markerModel.GetComponent<MeshRenderer>().enabled = true;
                                }
                                marker.gizmosMenu = false;

                                StartAnimationPreview();
                            }
                        }
                    }

                    EditorGUILayout.EndHorizontal();
                }

                if (GUILayout.Button("Reset Marker Target"))
                {
                    marker.markerTargetLeft.transform.localPosition = Vector3.zero;
                    marker.markerTargetLeft.transform.localRotation = Quaternion.Euler(180, 0, 0);
                    marker.markerTargetRight.transform.localPosition = Vector3.zero;
                    marker.markerTargetRight.transform.localRotation = Quaternion.Euler(180, 0, 0);
                    ((Marker)target).markerScale.localScale = Vector3.one;
                }

                if (!marker.isQuest && marker.withMenu)
                {
                    GUILayout.Space(40);

                    GUIStyle centeredBoldLabel = new GUIStyle(EditorStyles.label);
                    centeredBoldLabel.fontSize = 16;
                    centeredBoldLabel.alignment = TextAnchor.MiddleCenter;
                    centeredBoldLabel.fontStyle = FontStyle.Bold;

                    EditorGUILayout.LabelField("Menu", centeredBoldLabel);
                    GUILayout.Space(5);
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button(new GUIContent("Adjust Right Menu Target", "If needed, move, rotate or scale MenuTarget"), GUILayout.Height(35)))
                    {
                        menuScale = marker.menuScale.gameObject;
                        markerTargetObject = marker.menuTargetRight.gameObject;

                        VRC.SDK3.Dynamics.Constraint.Components.VRCParentConstraint constraint = marker.menu.GetComponentInParent<VRC.SDK3.Dynamics.Constraint.Components.VRCParentConstraint>();
                        VRC.Dynamics.VRCConstraintSource source1 = constraint.Sources[0];
                        source1.Weight = 1f;
                        constraint.Sources[0] = source1;
                        VRC.Dynamics.VRCConstraintSource source2 = constraint.Sources[1];
                        source2.Weight = 0f;
                        constraint.Sources[1] = source2;

                        if (marker.menu != null)
                        {
                            marker.menu.gameObject.SetActive(true);
                        }
                        if (marker.markerModel != null)
                        {
                            marker.markerModel.GetComponent<MeshRenderer>().enabled = false;
                        }
                        
                        marker.gizmosMenu = true;

                        StartAnimationPreview();
                    }

                    if (GUILayout.Button(new GUIContent("Adjust Left Menu Target", "If needed, move, rotate or scale MenuTarget"), GUILayout.Height(35)))
                    {
                        menuScale = marker.menuScale.gameObject;
                        markerTargetObject = marker.menuTargetLeft.gameObject;

                        VRC.SDK3.Dynamics.Constraint.Components.VRCParentConstraint constraint = marker.menu.GetComponentInParent<VRC.SDK3.Dynamics.Constraint.Components.VRCParentConstraint>();
                        VRC.Dynamics.VRCConstraintSource source1 = constraint.Sources[0];
                        source1.Weight = 0f;
                        constraint.Sources[0] = source1;
                        VRC.Dynamics.VRCConstraintSource source2 = constraint.Sources[1];
                        source2.Weight = 1f;
                        constraint.Sources[1] = source2;
                        
                        if (marker.menu != null)
                        {
                            marker.menu.gameObject.SetActive(true);
                        }
                        if (marker.markerModel != null)
                        {
                            marker.markerModel.GetComponent<MeshRenderer>().enabled = false;
                        }

                        marker.gizmosMenu = true;

                        StartAnimationPreview();
                    }
                    EditorGUILayout.EndHorizontal();

                    if (GUILayout.Button("Reset Menu Target"))
                    {
                        marker.menuTargetLeft.transform.localPosition = Vector3.zero;
                        marker.menuTargetLeft.transform.localRotation = Quaternion.Euler(270, 180, 0);
                        marker.menuTargetRight.transform.localPosition = Vector3.zero;
                        marker.menuTargetRight.transform.localRotation = Quaternion.Euler(270, 0, 0);
                        marker.menu.parent.localScale = Vector3.one;
                    }

                    GUILayout.Space(30);
                }

                GUILayout.Space(8);

                if (EditorApplication.isPlaying)
                {
                    AnimationMode.StopAnimationMode();
                    StopAnimationPreview();

                    GUI.enabled = false;
                    GUILayout.Button("Finish Setup", buttonStyle);
                    GUI.enabled = true;
                }
                else
                {
                    if (GUILayout.Button("Finish Setup", buttonStyle))
                    {
                        if (marker.markerModel != null)
                        {
                            marker.markerModel.GetComponent<MeshRenderer>().enabled = false;
                        }

                        if (marker.menu != null)
                        {
                            marker.menu.gameObject.SetActive(false);
                        }

                        if (marker.isQuest)
                        {
                            GameObject markerObject = GetQuestMarkerObject();

                            if (markerObject != null)
                            {
                                Transform model = markerObject.transform.Find("Marker");

                                if (model != null)
                                {
                                    if (model.GetComponent<MeshRenderer>() != null)
                                    {
                                        model.GetComponent<MeshRenderer>().enabled = false;
                                    }
                                }

                                Transform trailRenderer = markerObject.transform.Find("Draw");

                                if (trailRenderer != null)
                                {
                                    if (trailRenderer.GetComponent<TrailRenderer>() != null)
                                    {
                                        trailRenderer.GetComponent<TrailRenderer>().time = 0;
                                    }
                                }
                            }
                        }

                        StopAnimationPreview();

                        DestroyImmediate(((Marker)target));
                        DestroyImmediate(this);
                        // end script
                    }
                }
            }

            marker.wdSetting = wdSetting;
            marker.brushSize = brushSize;
            marker.leftHanded = leftHanded;
            marker.separateEraserScaling = separateEraserScaling;
            marker.localSpace = localSpace;
            marker.useIndexFinger = useIndexFinger;
            marker.localSpaceFullBody = localSpaceFullBody;
            marker.generateMasterMask = generateMasterMask;
            marker.withMenu = withMenu;

            GUI.enabled = true;
        }

        private GameObject GetQuestMarkerObject()
        {
            Animator avatar = descriptor.gameObject.GetComponent<Animator>();

            GameObject markerObject = null;

            if (avatar.GetBoneTransform(HumanBodyBones.LeftHand).Find("Marker") != null)
                markerObject = avatar.GetBoneTransform(HumanBodyBones.LeftHand).Find("Marker").gameObject;
            else if (avatar.GetBoneTransform(HumanBodyBones.RightHand).Find("Marker") != null)
                markerObject = avatar.GetBoneTransform(HumanBodyBones.RightHand).Find("Marker").gameObject;
            else if (avatar.GetBoneTransform(HumanBodyBones.LeftIndexDistal).Find("Marker") != null)
                markerObject = avatar.GetBoneTransform(HumanBodyBones.LeftIndexDistal).Find("Marker").gameObject;
            else if (avatar.GetBoneTransform(HumanBodyBones.RightIndexDistal).Find("Marker") != null)
                markerObject = avatar.GetBoneTransform(HumanBodyBones.RightIndexDistal).Find("Marker").gameObject;
            else if (descriptor.transform.Find("Marker") != null)
                markerObject = descriptor.transform.Find("Marker").gameObject;

            return markerObject;
        }

        private void StartAnimationPreview()
        {
            AnimatorController previewController = null;

            if (!marker.useIndexFinger)
            {
                previewController = AssetDatabase.LoadAssetAtPath<AnimatorController>(AssetDatabase.GUIDToAssetPath(MarkerStaticResources.PreviewAnimatorPen));
            }
            else
            {
                previewController = AssetDatabase.LoadAssetAtPath<AnimatorController>(AssetDatabase.GUIDToAssetPath(MarkerStaticResources.PreviewAnimatorNoPen));
            }

            if (previewController == null)
            {
                Debug.Log($"{MarkerStaticResources.MarkerLogTag}Preview Controller not found");
                return;
            }

            Animator animator = FindAnimator(marker.transform);

            if (animator == null) animator = descriptor.gameObject.GetComponent<Animator>();
            if (originalController == null && animator.runtimeAnimatorController != previewController)
                originalController = animator.runtimeAnimatorController;

            animator.runtimeAnimatorController = previewController;

            AnimationMode.StartAnimationMode();
            AnimationMode.BeginSampling();
            AnimationMode.SampleAnimationClip(animator.gameObject, previewController.animationClips.ElementAt(0), 0f);
            AnimationMode.EndSampling();
        }

        private void StopAnimationPreview()
        {
            AnimationMode.StopAnimationMode();

            Animator animator = FindAnimator(marker.transform);
            if (animator) animator.runtimeAnimatorController = originalController;
        }

        void GenerateMarker(bool mergeOnCopy)
        {
            Debug.Log($"{MarkerStaticResources.MarkerLogTag}Generating Marker...");
            try
            {
                Marker markerRef = target as Marker;
                MarkerInstaller.Generate(descriptor, ref markerRef, marker.isQuest, mergeOnCopy);
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
            try
            {
                if (descriptor == null) return;
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
            catch (Exception)
            {

            }
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
                        separateEraserScaling = controller.HasLayer("M_EraserSize");
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
                                marker.gestureToDraw = (int)conditions[0].threshold;
                        }
                    }
                    if (descriptor.transform.Find("Marker/Model") == null) useIndexFinger = true;
                    if ((descriptor.transform.Find("Marker/World/Local") is Transform t) && (t != null))
                    {
                        if ((t.GetComponent<VRC.SDK3.Dynamics.Constraint.Components.VRCParentConstraint>() is VRC.SDK3.Dynamics.Constraint.Components.VRCParentConstraint p) && (p != null))
                            if ((p.Sources[6].SourceTransform != null) && (p.Sources[7].SourceTransform != null))
                                localSpaceFullBody = 1;

                        //if ((t.GetComponent<ParentConstraint>() is ParentConstraint p) && (p != null))
                        //    if ((p.GetSource(6).sourceTransform != null) && (p.GetSource(7).sourceTransform != null))
                        //        localSpaceFullBody = 1;
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

            if (descriptor == null) {
                warnings.Add("There is no avatar descriptor on this GameObject. Please move this script onto your avatar, " +
                    "or create an avatar descriptor here.");
                return;
            }

            VRCExpressionParameters parameters = descriptor.expressionParameters;
            if (parameters != null && parameters.CalcTotalCost() > (VRCExpressionParameters.MAX_PARAMETER_COST - bitCount))
            {
                warnings.Add("You don't have enough free memory in your avatar's Expression Parameters to generate. " +
                    "You need " + (VRCExpressionParameters.MAX_PARAMETER_COST - bitCount) + " or fewer bits of parameter " +
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

            if(localSpace && !marker.isQuest) {
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
            bitCount = marker.isQuest ? 9 : 12;

            if (!marker.isQuest)
            {
                if (brushSize && !marker.isQuest) // float
                    bitCount += 8;
                if (separateEraserScaling && !marker.isQuest) // float
                    bitCount += 8;
                if (localSpace && !marker.isQuest) // int
                    bitCount += 8;
                if (withMenu && !marker.isQuest) // bools
                    bitCount += 4;
                else // bool
                    bitCount += 1;
            }

            return bitCount;
        }
    }
}
#endif