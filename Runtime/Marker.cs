﻿// Marker by ksivl / VRLabs 3.0 Assets https://github.com/VRLabs/VRChat-Avatars-3.0
#if UNITY_EDITOR

using UnityEngine;
using UnityEngine.Animations;
using VRC.SDK3.Dynamics.Constraint.Components;

namespace VRLabs.Marker
{
	[ExecuteAlways]
	public class Marker : MonoBehaviour
	{	// data storage
		public bool leftHanded, wdSetting, eraserSize, useIndexFinger;
		public bool brushSize = true, localSpace = true;
		public int localSpaceFullBody;
		public int gestureToDraw = 3;

		public Transform markerTarget, markerModel, system;
		public bool finished = false;

		public void Update()
		{
			if (finished) // constantly uniformly scale Draw and Eraser (System) with MarkerTarget
			{
				Vector3 scale = system.localScale;
				Transform eraser = system.Find("Eraser");
				if (markerTarget.lossyScale.x < 1.0f) // don't scale down too much for small avatars, breaks
                {
					system.GetComponent<VRCScaleConstraint>().enabled = false;
					scale.x = 1.0f;
					if (!eraserSize) // but the eraser *does* need adjustment
                    {
						float f = 0.05f * markerTarget.lossyScale.x;
						eraser.localScale =  new Vector3(f, f, f);
					}
                }
                else
                {
					system.GetComponent<VRCScaleConstraint>().enabled = true;
					if (!eraserSize)
					{
						eraser.localScale = new Vector3(0.05f,0.05f,0.05f);
					}
				}
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
                if (markerModel != null)
                {
                    markerModel.GetComponent<MeshRenderer>().enabled = false;  // turn off marker model
                }
                DestroyImmediate(system.GetComponent<VRCScaleConstraint>()); // was used to scale Draw & Eraser
                // end script
            }
        }
    }
}
#endif