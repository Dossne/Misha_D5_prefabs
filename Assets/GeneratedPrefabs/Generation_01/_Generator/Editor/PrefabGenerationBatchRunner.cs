#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace GeneratedPrefabs.Generation01.Editor
{
    public static class PrefabGenerationBatchRunner
    {
        public static void Run()
        {
            const string profilePath = "Assets/GeneratedPrefabs/Generation_01/_Generator/Asset/PrefabGenerationProfile.asset";
            var profile = AssetDatabase.LoadAssetAtPath<PrefabGenerationProfile>(profilePath);
            if (profile == null)
            {
                Debug.LogError($"[Generation_01] Profile was not found at path: {profilePath}");
                return;
            }

            PrefabGenerationPipeline.Generate(profile);
        }
    }
}
#endif
