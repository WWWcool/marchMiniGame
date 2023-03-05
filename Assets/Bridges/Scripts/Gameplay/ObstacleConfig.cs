using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bridges.Scripts.Gameplay
{
    [Serializable]
    public class ObstacleConfigData
    {
        public EObstacleType type;
        public Sprite line;
        public Sprite color;
    }
    
    [CreateAssetMenu(fileName = "ObstacleConfig", menuName = "Data/ObstacleConfig")]
    public class ObstacleConfig : ScriptableObject
    {
        [SerializeField] private List<ObstacleConfigData> data;

        public ObstacleConfigData GetFor(EObstacleType type)
        {
            return data.Find(d => d.type == type);
        }
    }
}