using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GeneratedPrefabs.Generation01
{
    public sealed class CountdownWidgetView : MonoBehaviour
    {
        [SerializeField] private Image clockIconImg;
        [SerializeField] private TextMeshProUGUI timeTmp;

        public Image ClockIconImg => clockIconImg;
        public TextMeshProUGUI TimeTmp => timeTmp;

        public void Bind(Image clockIcon, TextMeshProUGUI timeText)
        {
            clockIconImg = clockIcon;
            timeTmp = timeText;
        }
    }
}
