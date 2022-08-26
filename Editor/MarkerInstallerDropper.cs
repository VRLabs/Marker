#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace VRLabs.Marker
{
    public class MarkerInstallerDropper
    {
        [MenuItem("VRLabs/Marker")]
        static void DropMarkerInScene() {
            Marker existingMarker = Object.FindObjectOfType<Marker>();
            if (existingMarker != null) {
                EditorGUIUtility.PingObject(existingMarker.gameObject);
            } else {
                GameObject newMarker = new GameObject("Marker", typeof(Marker));
                EditorGUIUtility.PingObject(newMarker);
            }
        }
    }
}
#endif