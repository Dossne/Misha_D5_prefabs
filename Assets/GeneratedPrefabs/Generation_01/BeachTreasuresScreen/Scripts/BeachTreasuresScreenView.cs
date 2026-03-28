using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GeneratedPrefabs.Generation01
{
    public sealed class BeachTreasuresScreenView : MonoBehaviour
    {
        [SerializeField] private RectTransform countdownWidgetParentTr;
        [SerializeField] private CountdownWidgetView countdownWidget;
        [SerializeField] private Button closeBtn;
        [SerializeField] private RectTransform offerCardListTr;
        [SerializeField] private List<OfferCardView> offerCards = new List<OfferCardView>();

        public RectTransform CountdownWidgetParentTr => countdownWidgetParentTr;
        public CountdownWidgetView CountdownWidget => countdownWidget;
        public Button CloseBtn => closeBtn;
        public RectTransform OfferCardListTr => offerCardListTr;
        public IReadOnlyList<OfferCardView> OfferCards => offerCards;

        public void Bind(
            RectTransform countdownParent,
            CountdownWidgetView countdown,
            Button closeButton,
            RectTransform offerCardList,
            List<OfferCardView> cardViews)
        {
            countdownWidgetParentTr = countdownParent;
            countdownWidget = countdown;
            closeBtn = closeButton;
            offerCardListTr = offerCardList;
            offerCards = cardViews ?? new List<OfferCardView>();
        }
    }
}
