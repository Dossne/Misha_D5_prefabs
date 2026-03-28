using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GeneratedPrefabs.Generation01
{
    public sealed class RewardItemView : MonoBehaviour
    {
        [SerializeField] private Image rewardIconImg;
        [SerializeField] private TextMeshProUGUI rewardValueTmp;

        public Image RewardIconImg => rewardIconImg;
        public TextMeshProUGUI RewardValueTmp => rewardValueTmp;

        public void Bind(Image icon, TextMeshProUGUI valueText)
        {
            rewardIconImg = icon;
            rewardValueTmp = valueText;
        }
    }
}
