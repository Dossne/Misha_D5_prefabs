#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace GeneratedPrefabs.Generation01.Editor
{
    [CustomEditor(typeof(PrefabGenerationProfile))]
    public sealed class PrefabGenerationProfileEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            GUILayout.Space(8f);
            if (GUILayout.Button("Generate", GUILayout.Height(32f)))
            {
                var profile = (PrefabGenerationProfile)target;
                PrefabGenerationPipeline.Generate(profile);
            }
        }
    }
}
#endif
