using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GeneratedPrefabs.Generation01
{
    public sealed class ActionButtonView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI actionLabelTmp;
        [SerializeField] private Image lockBadgeImg;

        public TextMeshProUGUI ActionLabelTmp => actionLabelTmp;
        public Image LockBadgeImg => lockBadgeImg;

        public void Bind(TextMeshProUGUI actionLabel, Image lockBadge)
        {
            actionLabelTmp = actionLabel;
            lockBadgeImg = lockBadge;
        }
    }
}
