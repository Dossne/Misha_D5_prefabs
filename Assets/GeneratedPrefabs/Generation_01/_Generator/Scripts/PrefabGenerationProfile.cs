using UnityEngine;

namespace GeneratedPrefabs.Generation01
{
    [CreateAssetMenu(menuName = "GeneratedPrefabs/Generation01/Prefab Generation Profile", fileName = "PrefabGenerationProfile")]
    public sealed class PrefabGenerationProfile : ScriptableObject
    {
        [SerializeField] private string rootFolder = "Assets/GeneratedPrefabs/Generation_01";
        [SerializeField] private string beachTreasuresScreenPrefabName = "BeachTreasuresScreenPrefab";

        public string RootFolder => rootFolder;
        public string BeachTreasuresScreenPrefabName => beachTreasuresScreenPrefabName;
    }
}
