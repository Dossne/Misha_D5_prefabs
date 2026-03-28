using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GeneratedPrefabs.Generation01
{
    public sealed class RewardPanelView : MonoBehaviour
    {
        [SerializeField] private Image panelBgImg;
        [SerializeField] private RectTransform rewardItemListTr;
        [SerializeField] private List<RewardItemView> rewardItems = new List<RewardItemView>();

        public Image PanelBgImg => panelBgImg;
        public RectTransform RewardItemListTr => rewardItemListTr;
        public IReadOnlyList<RewardItemView> RewardItems => rewardItems;

        public void Bind(Image background, RectTransform itemList, List<RewardItemView> items)
        {
            panelBgImg = background;
            rewardItemListTr = itemList;
            rewardItems = items ?? new List<RewardItemView>();
        }
    }
}
