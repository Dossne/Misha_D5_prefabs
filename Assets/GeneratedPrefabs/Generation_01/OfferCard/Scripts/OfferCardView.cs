using UnityEngine;
using UnityEngine.UI;

namespace GeneratedPrefabs.Generation01
{
    public sealed class OfferCardView : MonoBehaviour
    {
        [SerializeField] private Image cardBgImg;
        [SerializeField] private RectTransform rewardPanelParentTr;
        [SerializeField] private RewardPanelView rewardPanel;
        [SerializeField] private RectTransform actionButtonParentTr;
        [SerializeField] private ActionButtonView actionButton;

        public Image CardBgImg => cardBgImg;
        public RectTransform RewardPanelParentTr => rewardPanelParentTr;
        public RewardPanelView RewardPanel => rewardPanel;
        public RectTransform ActionButtonParentTr => actionButtonParentTr;
        public ActionButtonView ActionButton => actionButton;

        public void Bind(
            Image background,
            RectTransform rewardPanelParent,
            RewardPanelView rewardPanelView,
            RectTransform actionButtonParent,
            ActionButtonView actionButtonView)
        {
            cardBgImg = background;
            rewardPanelParentTr = rewardPanelParent;
            rewardPanel = rewardPanelView;
            actionButtonParentTr = actionButtonParent;
            actionButton = actionButtonView;
        }
    }
}
