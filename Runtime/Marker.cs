// Marker by ksivl / VRLabs 3.0 Assets https://github.com/VRLabs/VRChat-Avatars-3.0
#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using UnityEngine.Animations;

namespace VRLabs.Marker
{
    [ExecuteAlways]
    public class Marker : MonoBehaviour
    {   // data storage
        public bool leftHanded, wdSetting, separateEraserScaling, useIndexFinger, withMenu;
        public bool brushSize = true, localSpace = true;
        public int localSpaceFullBody = 1;
        public int gestureToDraw = 3;
        [HideInInspector] public bool editorDefaultsApplied;

        public Transform markerTargetLeft, markerTargetRight, menuTargetLeft, menuTargetRight, markerModel, menu, system, markerScale, menuScale;
        public bool finished = false;
        public bool showGizmos = true;
        public bool gizmosMenu = false;

        public bool generateMasterMask = true;
        public bool isQuest;

        public void OnDrawGizmos()
        {
            if (!showGizmos || !finished || isQuest)
                return;

            float viewDist = SceneView.currentDrawingSceneView.cameraDistance;
            Transform leftTarget = gizmosMenu ? menuTargetLeft : markerTargetLeft;
            Transform rightTarget = gizmosMenu ? menuTargetRight : markerTargetRight;
            
            bool isCrossedOver = leftTarget.transform.position.x >= rightTarget.transform.position.x;

            // left

            Gizmos.color = isCrossedOver ? Color.red : Color.gray;
            Handles.color =  isCrossedOver ? Color.red : Color.gray;
            Handles.DrawDottedLine(leftTarget.transform.position, leftTarget.transform.parent.position, Mathf.Max(viewDist * 5, 5));
            Gizmos.DrawWireSphere(leftTarget.transform.position, Mathf.Min(0.025f * viewDist, 0.005f));

            Gizmos.color = isCrossedOver ? Color.red : Color.white;
            Handles.color = isCrossedOver ? Color.red : Color.white;
            Vector3 leftPos = leftTarget.transform.position;
            leftPos.y += Mathf.Max(0.05f * viewDist, 0.0099f);
            Handles.Label(leftPos, "Left Target");
            Gizmos.DrawLine(leftPos, leftTarget.transform.position);

            // right
            Gizmos.color = isCrossedOver ? Color.red : Color.gray;
            Handles.color = isCrossedOver ? Color.red : Color.gray;
            Handles.DrawDottedLine(rightTarget.transform.position, rightTarget.transform.parent.position, Mathf.Max(viewDist * 5, 5));
            Gizmos.DrawWireSphere(rightTarget.transform.position, Mathf.Min(0.025f * viewDist, 0.005f));

            Gizmos.color = isCrossedOver ? Color.red : Color.white;
            Handles.color = isCrossedOver ? Color.red : Color.white;
            Vector3 rightPos = rightTarget.transform.position;
            rightPos.y += Mathf.Max(0.05f * viewDist, 0.0099f);
            Handles.Label(rightPos, "Right Target");
            Gizmos.DrawLine(rightPos, rightTarget.transform.position);

            Gizmos.color = Color.white;
            Handles.color = Color.white;
        }

        public void Update()
        {
            if (finished && system != null) // constantly uniformly scale Draw and Eraser (System) with MarkerTarget
            {
                Vector3 scale = system.localScale;

                scale.y = scale.x;
                scale.z = scale.x;
                system.localScale = scale;
                // also scale Draw's triggers module radius scale
                Transform draw = system.Find("Draw");
                ParticleSystem.TriggerModule triggerModule = draw.GetComponent<ParticleSystem>().trigger;
                triggerModule.radiusScale = scale.x * 0.6f; // bit more than half is OK
            }
        }
        
        public void OnDestroy()
        {
            if (finished)
            {
                return;
            }
            if (markerModel != null)
            {
                markerModel.GetComponent<MeshRenderer>().enabled = false;  // turn off marker model
            }
            if (menu != null)
            {
                menu.gameObject.SetActive(false); // turn off menu
            }
            if (system != null)
            {
                DestroyImmediate(system.GetComponent<ScaleConstraint>()); // was used to scale Draw & Eraser
            }
        }
    }
}
#endif