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
        public bool leftHanded, wdSetting, eraserSize, useIndexFinger;
        public bool brushSize = true, localSpace = true;
        public int localSpaceFullBody = 1;
        public int gestureToDraw = 3;

        public Transform markerTargetLeft, markerTargetRight, markerModel, system, markerScale;
        public bool finished = false;
        public bool showGizmos = true;

        public bool generateMasterMask = true;
        public bool isQuest;

        public void OnDrawGizmos()
        {
            if (!showGizmos || !finished || isQuest)
                return;

            float viewDist = SceneView.currentDrawingSceneView.cameraDistance;
            bool isCrossedOver = markerTargetLeft.transform.position.x >= markerTargetRight.transform.position.x;

            // left

            Gizmos.color = isCrossedOver ? Color.red : Color.gray;
            Handles.color =  isCrossedOver ? Color.red : Color.gray;
            Handles.DrawDottedLine(markerTargetLeft.transform.position, markerTargetLeft.transform.parent.position, Mathf.Max(viewDist * 5, 5));
            Gizmos.DrawWireSphere(markerTargetLeft.transform.position, Mathf.Min(0.025f * viewDist, 0.005f));

            Gizmos.color = isCrossedOver ? Color.red : Color.white;
            Handles.color = isCrossedOver ? Color.red : Color.white;
            Vector3 leftPos = markerTargetLeft.transform.position;
            leftPos.y += Mathf.Max(0.05f * viewDist, 0.0099f);
            Handles.Label(leftPos, "Left Target");
            Gizmos.DrawLine(leftPos, markerTargetLeft.transform.position);

            // right
            Gizmos.color = isCrossedOver ? Color.red : Color.gray;
            Handles.color = isCrossedOver ? Color.red : Color.gray;
            Handles.DrawDottedLine(markerTargetRight.transform.position, markerTargetRight.transform.parent.position, Mathf.Max(viewDist * 5, 5));
            Gizmos.DrawWireSphere(markerTargetRight.transform.position, Mathf.Min(0.025f * viewDist, 0.005f));

            Gizmos.color = isCrossedOver ? Color.red : Color.white;
            Handles.color = isCrossedOver ? Color.red : Color.white;
            Vector3 rightPos = markerTargetRight.transform.position;
            rightPos.y += Mathf.Max(0.05f * viewDist, 0.0099f);
            Handles.Label(rightPos, "Right Target");
            Gizmos.DrawLine(rightPos, markerTargetRight.transform.position);

            Gizmos.color = Color.white;
            Handles.color = Color.white;
        }

        public void Update()
        {
            if (finished && system != null) // constantly uniformly scale Draw and Eraser (System) with MarkerTarget
            {
                Vector3 scale = system.localScale;
                //Transform eraser = system.Find("Eraser");
                /*if (markerTargetRight.lossyScale.x < 1.0f) // don't scale down too much for small avatars, breaks
                {
                    system.GetComponent<ScaleConstraint>().enabled = false;
                    scale.x = 1.0f;
                }
                else
                {
                    system.GetComponent<ScaleConstraint>().enabled = true;
                }*/

                scale.y = scale.x;
                scale.z = scale.x;
                system.localScale = scale;
                // also scale Draw's triggers module radius scale
                Transform draw = system.Find("Draw");
                ParticleSystem.TriggerModule triggerModule = draw.GetComponent<ParticleSystem>().trigger;
                triggerModule.radiusScale = scale.x * 0.6f; // bit more than half is OK

                /*
                markerTargetLeft.transform.SetPositionAndRotation(
                    new Vector3(
                        markerTargetRight.transform.position.x * -1,
                        markerTargetRight.transform.position.y,
                        markerTargetRight.transform.position.z
                    ),
                    Quaternion.Euler(
                        markerTargetRight.transform.rotation.eulerAngles.x * -1,
                        markerTargetRight.transform.rotation.eulerAngles.y * -1,
                        markerTargetRight.transform.rotation.eulerAngles.z * -1
                    )
                );*/
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
            DestroyImmediate(system.GetComponent<ScaleConstraint>()); // was used to scale Draw & Eraser
                                                                      // end script
        }
    }
}
#endif