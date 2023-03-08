using System;
using TMPro;
using UnityEngine;

namespace Bridges.Scripts.Gameplay
{
    public class AchievementPopup : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private TextMeshProUGUI rewardText;

        public void Init(string title, string text)
        {
            this.title.text = title;
            rewardText.text = text;
        }
        
    }
}