using TMPro;
using UnityEngine;

namespace Bridges.Scripts.Gameplay
{
    public class AchievementView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private TextMeshProUGUI rewardText;
        [SerializeField] private Transform hidden;

        public void Init(string title, string text, bool hidden)
        {
            this.title.text = title;
            rewardText.text = text;
            this.hidden.gameObject.SetActive(hidden);
            rewardText.gameObject.SetActive(!hidden);
        }
    }
}