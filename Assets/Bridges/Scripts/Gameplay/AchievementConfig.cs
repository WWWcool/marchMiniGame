using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bridges.Scripts.Gameplay
{
    [Serializable]
    public class AchievementConfigData
    {
        public EAchievementType type;
        public EObstacleType obstacleType = EObstacleType.None;
        public int count;
        public string id;
        public string title;
        [TextAreaAttribute]
        public string rewardText;
    }
    [CreateAssetMenu(fileName = "AchievementConfig", menuName = "Data/AchievementConfig")]
    public class AchievementConfig : ScriptableObject
    {
        [SerializeField] private List<AchievementConfigData> data;

        public IReadOnlyList<AchievementConfigData> Data => data;
        
        public AchievementConfigData GetFor(string id)
        {
            return data.Find(d => d.id == id);
        }
    }
}