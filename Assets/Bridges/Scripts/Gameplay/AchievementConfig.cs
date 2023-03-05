using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bridges.Scripts.Gameplay
{
    [Serializable]
    public class AchievementConfigData
    {
        public EAchievementType type;
        public EObstacleType obstacleType;
        public int count;
        public string title;
        public string rewardText;
    }
    [CreateAssetMenu(fileName = "AchievementConfig", menuName = "Data/AchievementConfig")]
    public class AchievementConfig : ScriptableObject
    {
        [SerializeField] private List<AchievementConfigData> data;
    }
}