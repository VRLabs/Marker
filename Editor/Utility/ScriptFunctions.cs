#if VRC_SDK_VRCSDK3

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using ValueType = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters.ValueType;

namespace VRLabs.Marker
{
    /// <summary>
    /// Helpful functions for script writers using the VRChat Avatars 3.0 SDK.
    /// Merge or remove controllers, menus, parameters, layers; get or set the Write Defaults value; etc.
    /// </summary>
    public static class ScriptFunctions
    {
        private const string DEFAULT_DIRECTORY = "Assets/VRLabs/GeneratedAssets/";
        // ideally do not use these default paths; manage it from script's end by providing own copy of default animator.
        public static readonly string[] _defaultLayerPath =
        {
            "Assets/VRCSDK/Examples3/Animation/Controllers/vrc_AvatarV3LocomotionLayer.controller",
            "Assets/VRCSDK/Examples3/Animation/Controllers/vrc_AvatarV3IdleLayer.controller",
            "Assets/VRCSDK/Examples3/Animation/Controllers/vrc_AvatarV3HandsLayer.controller",
            "Assets/VRCSDK/Examples3/Animation/Controllers/vrc_AvatarV3ActionLayer.controller"
        };

        // Default parameters
        public static readonly string[] _vrcDefaultParameters =
        {
            "IsLocal",
            "Viseme",
            "Voice",
            "GestureLeft",
            "GestureRight",
            "GestureLeftWeight",
            "GestureRightWeight",
            "AngularY",
            "VelocityX",
            "VelocityY",
            "VelocityZ",
            "Upright",
            "Grounded",
            "Seated",
            "AFK",
            "TrackingType",
            "VRMode",
            "MuteSelf",
            "InStation",
            "Supine",
            "GroundProximity",
            //VRLabs defaults
            "IsMirror"
        };

        public enum PlayableLayer // for function MergeController
        {
            Base = 0,
            Additive = 1,
            Gesture = 2,
            Action = 3,
            FX = 4
        }

        /// <summary>
        /// Creates a copy of the avatar descriptor's parameter asset or creates one if it doesn't exist, adds a provided parameter,
        /// assigns the new parameter asset, and stores it in the specified directory.
        /// </summary>
        /// <param name="descriptor">The avatar descriptor to add the parameter to.</param>
        /// <param name="parameter">The parameter to add.</param>
        /// <param name="directory">The unique directory to store the new parameter asset, ex. "Assets/MyCoolScript/GeneratedAssets/725638/".</param>
        /// <param name="makeCopy">If false, overwrite existing asset (if it does not exist, creates one in <c>directory</c>). If true, always make a copy in <c>directory</c>.</param>
        public static void AddParameter(VRCAvatarDescriptor descriptor, VRCExpressionParameters.Parameter parameter, string directory, bool makeCopy = false)
        {
            if (descriptor == null)
            {
                Debug.LogError("Couldn't add the parameter, the avatar descriptor is null!");
                return;
            }
            else if ((parameter == null) || (parameter.name == null))
            {
                Debug.LogError("Couldn't add the parameter, it or its name is null!");
                return;
            }
            else if ((directory == null) || (directory == ""))
            {
                Debug.Log("Directory was not specified, storing new parameters asset in " + DEFAULT_DIRECTORY);
                directory = DEFAULT_DIRECTORY;
            }

            descriptor.customExpressions = true;
            VRCExpressionParameters parameters;
            bool wasDefault = false;
            if (descriptor.expressionParameters == null)
            {
                parameters = ScriptableObject.CreateInstance<VRCExpressionParameters>();
                parameters.parameters = new VRCExpressionParameters.Parameter[0];
                AssetDatabase.CreateAsset(parameters, directory + "Parameters.asset");
                descriptor.expressionParameters = parameters;
                wasDefault = true;
            }
            else
            {
                if ((descriptor.expressionParameters.CalcTotalCost() + VRCExpressionParameters.TypeCost(parameter.valueType)) > VRCExpressionParameters.MAX_PARAMETER_COST)
                {
                    Debug.LogError("Couldn't add parameter '" + parameter.name + "', not enough memory free in the avatar's parameter asset!");
                    return;
                }
                parameters = descriptor.expressionParameters;
            }

            if (makeCopy && !wasDefault)
            {
                string path = AssetDatabase.GenerateUniqueAssetPath(directory + descriptor.expressionParameters.name + ".asset");
                AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(descriptor.expressionParameters), path);
                parameters = AssetDatabase.LoadAssetAtPath(path, typeof(VRCExpressionParameters)) as VRCExpressionParameters;
                descriptor.expressionParameters = parameters;
            }

            int count = parameters.parameters.Length;
            VRCExpressionParameters.Parameter[] parameterArray = new VRCExpressionParameters.Parameter[count + 1];
            for (int i = 0; i < count; i++)
                parameterArray[i] = parameters.GetParameter(i);
            parameterArray[count] = parameter;
            parameters.parameters = parameterArray;
            EditorUtility.SetDirty(parameters);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Creates a copy of the avatar descriptor's topmost menu asset or creates one if it doesn't exist, adds the provided menu as a submenu,
        /// assigns the new topmost menu asset, and stores it in the specified directory.
        /// </summary>
        /// <param name="descriptor">The avatar descriptor to add the submenu to.</param>
        /// <param name="menuToAdd">The menu to add, which will become a submenu of the topmost menu.</param>
        /// <param name="controlName">The name of the submenu control for the menu to add.</param>
        /// <param name="directory">The unique directory to store the new topmost menu asset, ex. "Assets/MyCoolScript/GeneratedAssets/725638/".</param>
        /// <param name="controlParameter">Optionally, the parameter to trigger when the submenu is opened.</param>
        /// <param name="icon"> Optionally, the icon to display on this submenu.</param>
        /// <param name="makeCopy">If false, overwrite existing asset (if it does not exist, creates one in <c>directory</c>). If true, always make a copy in <c>directory</c>.</param>
        public static void AddSubMenu(VRCAvatarDescriptor descriptor, VRCExpressionsMenu menuToAdd, string controlName, string directory, VRCExpressionsMenu.Control.Parameter controlParameter = null, Texture2D icon = null, bool makeCopy = false)
        {
            if (descriptor == null)
            {
                Debug.LogError("Couldn't add the menu, the avatar descriptor is null!");
                return;
            }
            else if ((menuToAdd == null) || (controlName == null) || (controlName == ""))
            {
                Debug.LogError("Couldn't add the menu, it or the name of its control is null!");
                return;
            }
            else if ((directory == null) || (directory == ""))
            {
                Debug.Log("Directory was not specified, storing new menu in " + DEFAULT_DIRECTORY);
                directory = DEFAULT_DIRECTORY;
            }

            descriptor.customExpressions = true;
            VRCExpressionsMenu topMenu;
            bool wasDefault = false;
            if (descriptor.expressionsMenu == null)
            {
                topMenu = ScriptableObject.CreateInstance<VRCExpressionsMenu>();
                AssetDatabase.CreateAsset(topMenu, directory + "Menu.asset");
                descriptor.expressionsMenu = topMenu;
                wasDefault = true;
            }
            else
            {
                if (descriptor.expressionsMenu.controls.Count == 8)
                {
                    Debug.LogWarning("Couldn't add menu. Please have an available slot in your avatar's topmost Expression Menu.");
                    return;
                }
                topMenu = descriptor.expressionsMenu;
            }

            if (makeCopy && !wasDefault)
            {
                string path = AssetDatabase.GenerateUniqueAssetPath((directory + descriptor.expressionsMenu.name + ".asset"));
                AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(descriptor.expressionsMenu), path);
                topMenu = AssetDatabase.LoadAssetAtPath(path, typeof(VRCExpressionsMenu)) as VRCExpressionsMenu;
                descriptor.expressionsMenu = topMenu;
            }

            List<VRCExpressionsMenu.Control> controlList = topMenu.controls;
            VRCExpressionsMenu.Control control = new VRCExpressionsMenu.Control
            { name = controlName, type = VRCExpressionsMenu.Control.ControlType.SubMenu, subMenu = menuToAdd, parameter = controlParameter, icon = icon };
            controlList.Add(control);
            topMenu.controls = controlList;
            EditorUtility.SetDirty(topMenu);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Merges a controller "on current" to the specified playable layer on an avatar's descriptor and assigns it to the avatar.
        /// If the avatar has no playable layer to begin with, merges to a fresh controller and stores it at the specified directory.
        /// </summary>
        /// <param name="descriptor">The avatar descriptor that merging is being done on.</param>
        /// <param name="controllerToAdd">The controller to merge to the <c>playable</c> layer.</param>
        /// <param name="playable">The playable layer to merge to.</param>
        /// <param name="directory">The unique directory to store the new merged controller, ex. "Assets/MyCoolScript/GeneratedAssets/725638/".</param>
        /// <param name="makeCopy">If false, overwrite existing asset (if it does not exist, creates one in <c>directory</c>). If true, always make a copy in <c>directory</c>.</param>
        public static void MergeController(VRCAvatarDescriptor descriptor, AnimatorController controllerToAdd, PlayableLayer playable, string directory, bool makeCopy = false)
        {
            int layer = (int)playable;
            if (descriptor == null)
            {
                Debug.LogError("The avatar descriptor is null! Merging was not performed.");
                return;
            }
            else if (controllerToAdd == null)
            {
                Debug.LogError("The controller to add is null! Merging was not performed.");
                return;
            }
            else if (layer < 4) // fx layer has no default layer
            {
                if ((AssetDatabase.LoadAssetAtPath(_defaultLayerPath[layer], typeof(AnimatorController)) as AnimatorController) == null)
                {
                    Debug.LogError("Couldn't find VRChat's default animator controller at path '" + _defaultLayerPath[layer] + "'! Merging was not performed.");
                    return;
                }
            }
            else if (string.IsNullOrEmpty(directory))
            {
                Debug.Log("Directory was not specified, defaulting to " + DEFAULT_DIRECTORY);
                directory = DEFAULT_DIRECTORY;
            }

            AnimatorController controllerOriginal;
            bool wasDefault = false;
            if (descriptor.baseAnimationLayers[layer].isDefault || descriptor.baseAnimationLayers[layer].animatorController == null)
            {
                descriptor.customizeAnimationLayers = true;
                descriptor.baseAnimationLayers[layer].isDefault = false;

                controllerOriginal = new AnimatorController();
                string pathFromNew = directory + playable.ToString() + ".controller";

                if (layer == 4) // fx layer has no default layer
                {   // you cannot add a layer to a controller without creating its asset first
                    AssetDatabase.CreateAsset(controllerOriginal, pathFromNew);
                    controllerOriginal.AddLayer("Base Layer");
                }
                else
                {
                    AssetDatabase.CopyAsset(_defaultLayerPath[layer], pathFromNew);
                    controllerOriginal = AssetDatabase.LoadAssetAtPath(pathFromNew, typeof(AnimatorController)) as AnimatorController;
                }
                descriptor.baseAnimationLayers[layer].animatorController = controllerOriginal;
                wasDefault = true;
            }
            else
                controllerOriginal = (AnimatorController)descriptor.baseAnimationLayers[layer].animatorController;

            if (makeCopy && !wasDefault)
            {
                string path = AssetDatabase.GenerateUniqueAssetPath((directory + descriptor.baseAnimationLayers[layer].animatorController.name + ".controller"));
                AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(descriptor.baseAnimationLayers[layer].animatorController), path);
                controllerOriginal = AssetDatabase.LoadAssetAtPath(path, typeof(AnimatorController)) as AnimatorController;
            }
            AnimatorController mergedController = AnimatorCloner.MergeControllers(controllerOriginal, controllerToAdd, null, false);
            descriptor.baseAnimationLayers[layer].animatorController = mergedController;
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Removes all layers and parameters from the specified avatar layer that start with the provided prefix, and assigns it to the avatar.
        /// </summary>
        /// <param name="descriptor">The avatar descriptor to uninstall by prefix from.</param>
        /// <param name="prefix">If a layer or parameter's name begins with <c>prefix</c>, the layer or parameter will be removed.</param>
        /// <param name="playable">The playable layer to uninstall by prefix from.</param>
        /// <param name="isPrefixCaseSensitive">-BACKWARDS COMPATABILITY- Is the <c>prefix</c> case sensitive or not?</param>
        public static void UninstallControllerByPrefix(VRCAvatarDescriptor descriptor, string prefix, PlayableLayer playable, bool isPrefixCaseSensitive = true)
        {
            int layer = (int)playable;
            if (descriptor == null || descriptor.baseAnimationLayers.Length < 5) // 2nd cond: must have all 5 humanoid layers in descriptor
            {
                //Debug.LogError("The avatar descriptor is null.");
                return;
            }
            if (string.IsNullOrEmpty(prefix))
            {
                Debug.LogError("No string prefix specified.");
                return;
            }
            if (!(descriptor.baseAnimationLayers[layer].animatorController is AnimatorController controllerOriginal) || controllerOriginal == null) return;

            AnimatorControllerLayer[] filteredLayers;
            AnimatorControllerParameter[] filteredParameters;
            if (!isPrefixCaseSensitive)
            {
                prefix = prefix.ToLower();
                filteredLayers = controllerOriginal.layers.Where(x => !x.name.ToLower().StartsWith(prefix)).ToArray();
                filteredParameters = controllerOriginal.parameters.Where(x => !x.name.ToLower().StartsWith(prefix) || _vrcDefaultParameters.Contains(x.name)).ToArray();
            }
            else
            {
                filteredLayers = controllerOriginal.layers.Where(x => !x.name.StartsWith(prefix)).ToArray();
                filteredParameters = controllerOriginal.parameters.Where(x => !x.name.StartsWith(prefix) || _vrcDefaultParameters.Contains(x.name)).ToArray();
            }
            controllerOriginal.layers = filteredLayers;
            controllerOriginal.parameters = filteredParameters;

            EditorUtility.SetDirty(controllerOriginal);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Uninstall parameters that begin with a certain prefix, from an avatar's expressions parameters asset.
        /// </summary>
        /// <param name="descriptor">The avatar descriptor to uninstall the parameters from.</param>
        /// <param name="prefix">If a parameter begins with this prefix, it will be removed.</param>
        public static void UninstallParametersByPrefix(VRCAvatarDescriptor descriptor, string prefix)
        {
            if (descriptor == null)
            {
                //Debug.LogError("The avatar descriptor is null.");
                return;
            }
            if (descriptor.expressionParameters != null)
            {
                VRCExpressionParameters.Parameter[] parameterArray = descriptor.expressionParameters.parameters;
                parameterArray = parameterArray.Where(x => !x.name.StartsWith(prefix) || _vrcDefaultParameters.Contains(x.name)).ToArray();
                descriptor.expressionParameters.parameters = parameterArray;
                EditorUtility.SetDirty(descriptor.expressionParameters);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        /// <summary>
        /// Remove a submenu control from an avatar's expressions menu asset.
        /// </summary>
        /// <param name="descriptor">The avatar descriptor to uninstall the submenu from.</param>
        /// <param name="menu">The name of the submenu control to remove.</param>
        public static void UninstallMenu(VRCAvatarDescriptor descriptor, string menu)
        {
            if (descriptor == null)
            {
                //Debug.LogError("The avatar descriptor is null.");
                return;
            }
            if (descriptor.expressionsMenu != null)
            {
                List<VRCExpressionsMenu.Control> menuControls = descriptor.expressionsMenu.controls;
                menuControls = menuControls.Where(x => !x.name.Equals(menu)).ToList();
                descriptor.expressionsMenu.controls = menuControls;
                EditorUtility.SetDirty(descriptor.expressionsMenu);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        /// <summary>
        /// Determines if any elements of a previous install of the system exists on the avatar.
        /// </summary>
        /// <param name="descriptor">The avatar descriptor to check.</param>
        /// <param name="gameObject">The name of the gameobject at the base of the avatar to find.</param>
        /// <param name="playables">The playable layers to check for layers or parameters beginning with <c>prefix</c>.</param>
        /// <param name="prefix">The prefix of a layer or parameter to check for in the avatar's <c>playables</c> and expression parameters asset.</param>
        /// <param name="menu">The name of the menu to find on the avatar's expressions menu asset.</param>
        /// <param name="isPrefixCaseSensitive">-BACKWARDS COMPATABILITY- Is the <c>prefix</c> case sensitive or not?</param> // this is disgusting but it works
        /// <returns>True if any elements of a previous install are detected, false otherwise.</returns>
        public static bool HasPreviousInstall(VRCAvatarDescriptor descriptor, string gameObject, PlayableLayer[] playables, string prefix, string menu = null, bool isPrefixCaseSensitive = true)
        {
            if (descriptor == null)
            {
                //Debug.LogError("The avatar descriptor is null.");
                return false;
            }
            if (descriptor.transform.Find(gameObject) != null)
                return true;
            if (descriptor.baseAnimationLayers.Length >= 5) // must have all 5 humanoid layers in descriptor
            {
                for (int i = 0; i < playables.Length; i++)
                {
                    if (!(descriptor.baseAnimationLayers[(int)playables[i]].animatorController is AnimatorController controller) || controller == null) continue;

                    if (!isPrefixCaseSensitive)
                    {
                        prefix = prefix.ToLower();
                        if (controller.layers.Select(x => x.name).Any(x => x.ToLower().StartsWith(prefix)))
                            return true;
                    }
                    else
                    {
                        if (controller.layers.Select(x => x.name).Any(x => x.StartsWith(prefix)))
                            return true;
                    }

                }
            }
            if ((descriptor.expressionsMenu != null) && (menu != null))
            {
                for (int i = 0; i < descriptor.expressionsMenu.controls.Count; i++)
                {
                    if (descriptor.expressionsMenu.controls[i].name.Equals(menu))
                        return true;
                }
            }
            if (descriptor.expressionParameters != null)
            {
                string[] avatarParameterNames = descriptor.expressionParameters.parameters.Select(x => x.name).ToArray();
                if (!isPrefixCaseSensitive)
                {
                    if (avatarParameterNames.Any(x => x.ToLower().StartsWith(prefix)))
                        return true;
                }
                else
                {
                    if (avatarParameterNames.Any(x => x.StartsWith(prefix)))
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Determine if the layer exists in an animator controller.
        /// </summary>
        /// <param name="layerName">The name of the layer to find.</param>
        /// <returns>True if the layer was found, false otherwise.</returns>
        public static bool HasLayer(this AnimatorController controller, string layerName)
        {
            if (controller == null) return false;
            for (int i = 0; i < controller.layers.Length; i++)
            {
                if (controller.layers[i].name.Equals(layerName))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Find an object, like a target, under a specific bone.
        /// </summary>
        /// <param name="descriptor">The avatar descriptor to search for.</param>
        /// <param name="bone">The humanoid bone to search under.</param>
        /// <param name="objectName">The name of the object to remove.</param>
        /// <param name="searchAllChildren">If true, search deeper than one level until the object is found, and remove it.</param>
        /// <returns>The GameObject if it was found; null if not found.</returns>
        public static GameObject FindObject(VRCAvatarDescriptor descriptor, HumanBodyBones bone, string objectName, bool searchAllChildren = false)
        {
            if (descriptor == null)
            {
                Debug.LogError("The avatar descriptor is null.");
                return null;
            }
            if (descriptor.GetComponent<Animator>() != null)
            {
                Animator animator = descriptor.GetComponent<Animator>();
                if (animator.GetBoneTransform(bone) != null)
                {
                    Transform foundBone = animator.GetBoneTransform(bone);
                    if (foundBone != null)
                    {
                        if (searchAllChildren)
                        {
                            foreach (Transform t in foundBone.GetComponentsInChildren<Transform>())
                            {
                                Transform foundObject = t.Find(objectName);
                                if (foundObject != null)
                                    return foundObject.gameObject;
                            }
                        }
                        else
                        {
                            Transform foundObject = foundBone.Find(objectName);
                            if (foundObject != null)
                                return foundObject.gameObject;
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Remove a layer from an animator controller by name.
        /// Will modify the controller directly.
        /// </summary>
        /// <param name="controller">The controller to modify.</param>
        /// <param name="name">The name of the layer to remove.</param>
        /// <returns></returns>
        public static void RemoveLayer(AnimatorController controller, string name)
        {   // helper function: remove layer by name
            for (int i = 0; i < controller.layers.Length; i++)
            {
                if (controller.layers[i].name.Equals(name))
                {
                    controller.RemoveLayer(i);
                    break;
                }
            }
        }

        /// <summary>
        /// Remove a parameter from an animator controller by name.
        /// Will modify the controller directly.
        /// </summary>
        /// <param name="controller">The controller to modify.</param>
        /// <param name="name">The name of the parameter to remove.</param>
        /// <returns></returns>
        public static void RemoveParameter(AnimatorController controller, string name)
        {   // helper function: remove parameter by name
            for (int i = 0; i < controller.parameters.Length; i++)
            {
                if (controller.parameters[i].name.Equals(name))
                {
                    controller.RemoveParameter(i);
                    break;
                }
            }
        }

        /// <summary>
        /// Remove a menu control from a VRC Expressions Menu by name.
        /// Will modify the menu asset directly.
        /// </summary>
        /// <param name="menu">The menu to modify.</param>
        /// <param name="name">The name of the menu contorl to remove.</param>
        /// <returns></returns>
        public static void RemoveMenuControl(VRCExpressionsMenu menu, string name)
        {   // helper function: remove menu control
            for (int i = 0; i < menu.controls.Count; i++)
            {
                if (menu.controls[i].name.Equals(name))
                {
                    menu.controls.RemoveAt(i);
                    break;
                }
            }
        }

        /// <summary>
        /// Checks if the avatar descriptor has mixing "Write defaults" settings across its animators.
        /// </summary>
        /// <param name="descriptor">Avatar descriptor to check.</param>
        /// <returns>True if the avatar animators contain mixed write defaults, false otherwise.</returns>
        public static List<WDState> AnalyzeWDState(this VRCAvatarDescriptor descriptor)
        {
            var states = new List<WDState>();
            foreach (var layer in descriptor.baseAnimationLayers)
            {
                if (!(layer.animatorController is AnimatorController controller) || controller == null) continue;
                foreach (var animationLayer in controller.layers)
                    AnalyzeWdStateMachine(animationLayer.stateMachine, states, layer.type.ToString());

            }
            foreach (var layer in descriptor.specialAnimationLayers)
            {
                if (!(layer.animatorController is AnimatorController controller)) continue;
                foreach (var animationLayer in controller.layers)
                    AnalyzeWdStateMachine(animationLayer.stateMachine, states, layer.type.ToString());
            }

            return states;
        }

        private static void AnalyzeWdStateMachine(AnimatorStateMachine stateMachine, List<WDState> states, string layerName)
        {
            foreach (ChildAnimatorState t in stateMachine.states)
            {
                states.Add(new WDState
                {
                    AvatarLayer = layerName,
                    StateName = t.state.name,
                    IsOn = t.state.writeDefaultValues,
                    HasDefault = t.state.name.Contains("(WD On)") || t.state.name.Contains("(WD Off)"),
                    IsDefaultOn = t.state.name.Contains("(WD On)"),
                    HasMotion = t.state.motion != null
                });
            }

            foreach (ChildAnimatorStateMachine t in stateMachine.stateMachines)
                AnalyzeWdStateMachine(t.stateMachine, states, layerName);
        }

        /// <summary>
        /// Checks if the avatar descriptor has mixing "Write defaults" settings across its animators.
        /// </summary>
        /// <param name="states">States to check.</param>
        /// <param name="isOn">Returns true if WD are on, false if WD are off. NOTE if WD are mixed, this value is meaningless. (</param>
        /// <returns>True if the avatar animators contain mixed write defaults, false otherwise.</returns>
        public static bool HaveMixedWriteDefaults(this IEnumerable<WDState> states, out bool isOn)
        {
            isOn = false;
            bool checkedFirst = false;
            foreach (var state in states)
            {
                if (state.HasDefault)
                {
                    if (state.IsOn ^ state.IsDefaultOn)
                        return true;
                    continue;
                }

                if (!checkedFirst)
                {
                    checkedFirst = true;
                    isOn = state.IsOn;
                    continue;
                }

                if (state.IsOn != isOn)
                    return true;
            }

            return false;
        }
        public static bool HaveMixedWriteDefaults(this IEnumerable<WDState> states)
        {
            return HaveMixedWriteDefaults(states, out _);
        }

        /// <summary>
        /// Checks if the avatar descriptor has any empty motions in its states.
        /// </summary>
        /// <param name="states">States to check.</param>
        /// <returns>True if any avatar states contain empty motions, false otherwise.</returns>
        public static bool HaveEmpyMotionsInStates(this IEnumerable<WDState> states)
        {
            foreach (var state in states)
                if (!state.HasMotion)
                    return true;

            return false;
        }


        // ReSharper disable Unity.PerformanceAnalysis
        /// <summary>
        /// Sets the "Write Defaults" value of all the states in an entire animator controller to true or false.
        /// Will modify the controller directly.
        /// </summary>
        /// <param name="controller">The controller to modify.</param>
        /// <param name="writeDefaults">The value of "Write Defaults" to set the controller's states to. True if unspecified.</param>
        /// <returns></returns>
        public static void SetWriteDefaults(AnimatorController controller, bool writeDefaults = true, bool force = false)
        {
            if (controller == null)
            {
                Debug.LogError("Couldn't set Write Defaults value, the controller is null!");
                return;
            }
            for (int i = 0; i < controller.layers.Length; i++)
            {
                SetInStateMachine(controller.layers[i].stateMachine, writeDefaults, force);
            }
            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Get all states of an animator controller.
        /// </summary>
        /// <param name="controller">Controller used.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="AnimatorState"/> contained in the given controller.</returns>
        public static IEnumerable<AnimatorState> GetAnimatorStates(this AnimatorController controller)
        {
            var animatorStates = new List<AnimatorState>();
            foreach (var animationLayer in controller.layers)
                animatorStates.AddRange(GetLayerStatesRecursive(animationLayer.stateMachine));

            return animatorStates;
        }

        /// <summary>
        /// Get all states of an animator layer
        /// </summary>
        /// <param name="layer">Layer used.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="AnimatorState"/> contained in the given layer.</returns>
        public static IEnumerable<AnimatorState> GetLayerStates(this AnimatorControllerLayer layer)
        {
            return GetLayerStatesRecursive(layer.stateMachine);
        }
        private static IEnumerable<AnimatorState> GetLayerStatesRecursive(AnimatorStateMachine stateMachine)
        {
            var animatorStates = stateMachine.states
                .Select(t => t.state)
                .ToList();
            foreach (ChildAnimatorStateMachine t in stateMachine.stateMachines)
                animatorStates.AddRange(GetLayerStatesRecursive(t.stateMachine));

            return animatorStates;
        }

        /// <summary>
        /// Return the VRC ValueType value based on the type of the animator parameter given.
        /// </summary>
        /// <param name="type">Animator parameter type.</param>
        /// <returns>VRC SDK3 ValueType that corresponds to the given animator ValueType.</returns>
        public static ValueType GetValueTypeFromAnimatorParameterType(AnimatorControllerParameterType type)
        {
            return type == AnimatorControllerParameterType.Int
                ? ValueType.Int
                : (type == AnimatorControllerParameterType.Bool ? ValueType.Bool : ValueType.Float);
        }

        private static (bool, bool, bool) GetWdInStateMachine(AnimatorStateMachine stateMachine, bool checkedFirst, bool isOn)
        {
            foreach (ChildAnimatorState t in stateMachine.states)
            {
                if (!checkedFirst)
                {
                    isOn = t.state.writeDefaultValues;
                    checkedFirst = true;
                    continue;
                }

                if (t.state.name.Contains("(WD On)") || t.state.name.Contains("(WD Off)"))
                {
                    if (t.state.writeDefaultValues ^ t.state.name.Contains("(WD On)"))
                        return (true, isOn, true);
                    continue;
                }
                if (isOn != t.state.writeDefaultValues)
                    return (true, isOn, true);
            }

            bool isMixed;
            foreach (ChildAnimatorStateMachine t in stateMachine.stateMachines)
            {
                (checkedFirst, isOn, isMixed) = GetWdInStateMachine(t.stateMachine, checkedFirst, isOn);
                if (isMixed)
                    return (checkedFirst, isOn, true);
            }

            return (checkedFirst, isOn, false);
        }

        private static void SetInStateMachine(AnimatorStateMachine stateMachine, bool wd, bool force)
        {
            foreach (ChildAnimatorState t in stateMachine.states)
            {
                t.state.writeDefaultValues = wd;
                // Force corresponding Write Defaults setting for states with "(WD On)" or "(WD Off)" tags
                if (!force && t.state.name.Contains("(WD On)"))
                    t.state.writeDefaultValues = true;
                else if (!force && t.state.name.Contains("(WD Off)"))
                    t.state.writeDefaultValues = false;
                else
                    t.state.writeDefaultValues = wd;

                //if (t.state.motion == null)
                    //t.state.motion = EmptyClip;

            }

            foreach (ChildAnimatorStateMachine t in stateMachine.stateMachines)
                SetInStateMachine(t.stateMachine, wd, force);
        }

    }
    // ReSharper disable once InconsistentNaming
    public struct WDState
    {
        public string AvatarLayer { get; set; }
        public string StateName { get; set; }
        public bool IsOn { get; set; }
        public bool HasDefault { get; set; }
        public bool IsDefaultOn { get; set; }
        public bool HasMotion { get; set; }
    }
}
#endif
