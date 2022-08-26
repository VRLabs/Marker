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
                existingMarker.gameObject.tag = "EditorOnly";
                EditorGUIUtility.PingObject(existingMarker.gameObject);
            } else {
                GameObject newMarker = new GameObject("Marker Installer", typeof(Marker));
                newMarker.tag = "EditorOnly";
                EditorGUIUtility.PingObject(newMarker);
            }
        }
    }
}
#endif