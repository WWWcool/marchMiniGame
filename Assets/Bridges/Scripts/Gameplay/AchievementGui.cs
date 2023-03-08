using System.Collections.Generic;
using UnityEngine;

namespace Bridges.Scripts.Gameplay
{
    public class AchievementGui : MonoBehaviour
    {
        [SerializeField] private AchievementConfig achievementConfig;
        [SerializeField] private AchievementManager achievementManager;
        [SerializeField] private Transform viewRoot;
        [SerializeField] private AchievementView viewPrefab;

        private List<AchievementView> _views = new();
        
        private void OnEnable()
        {
            Clear();
            foreach (var configData in achievementConfig.Data)
            {
                var view = Instantiate(viewPrefab, viewRoot);
                view.Init(configData.title, configData.rewardText, !achievementManager.IsAchieved(configData));
                _views.Add(view);
            }
        }

        private void Clear()
        {
            foreach (var achievementView in _views)
            {
                Destroy(achievementView.gameObject);
            }
            _views.Clear();
        }
    }
}