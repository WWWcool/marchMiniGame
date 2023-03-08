using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Bridges.Scripts.Gameplay
{
    [Serializable]
    public class AchievementData
    {
        public string id;
        public bool got;
    }
    
    [Serializable]
    public class AchievementsData
    {
        public List<AchievementData> data = new();
    }
    
    public class AchievementContext
    {
        public EAchievementType type;
        public EObstacleType obstacleType = EObstacleType.None;
        public int count;
    }
    
    public class AchievementsContext
    {
        public List<AchievementContext> data = new();
    }
    
    public class AchievementManager : MonoBehaviour
    {
        [SerializeField] private AchievementConfig config;
        public static AchievementManager Instance { get; set; }
        

        private AchievementsData _data;
        
        private void Awake()
        {
            DontDestroyOnLoad(this);

            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }
        
        private void Start()
        {
            if (!PlayerPrefs.HasKey("Achievements"))
                PlayerPrefs.SetString("Achievements", "");

            var data = PlayerPrefs.GetString("Achievements");
            _data = JsonUtility.FromJson<AchievementsData>(data);
            if (_data == null)
            {
                _data = new AchievementsData();
            }
        }

        public List<string> GetAchieved(AchievementsContext context)
        {
            var list = new List<string>();

            var data = new List<AchievementConfigData>(config.Data);
            var filtered = data.FindAll(d => !Got(d.id));
            foreach (var contextData in context.data)
            {
                foreach (var configData in filtered)
                {
                    if (configData.type == contextData.type && configData.obstacleType == contextData.obstacleType && configData.count <= contextData.count)
                    {
                        if(!list.Contains(configData.id))
                        {
                            list.Add(configData.id);
                        }
                    }
                }
            }
            
            return list;
        }

        public bool IsAchieved(AchievementConfigData data)
        {
            foreach (var achievementData in _data.data)
            {
                if (achievementData.id == data.id)
                {
                    return achievementData.got;
                }
            }

            return false;
        }

        public void Achieve(AchievementConfigData data)
        {
            if(!Got(data.id))
            {
                _data.data.Add(new AchievementData{got = true, id = data.id});
                Save();
            }
        }

        private void Save()
        {
            var data = JsonUtility.ToJson(_data, true);
            PlayerPrefs.SetString("Achievements", data);
        }

        private bool Got(string id)
        {
            return _data.data.Find(d => d.id == id) != null;
        }
        
#if UNITY_EDITOR
        [MenuItem("Tools/Reset save data")]
#endif
        public static void ResetSaveData()
        {
            PlayerPrefs.DeleteAll();
        }
    }
}