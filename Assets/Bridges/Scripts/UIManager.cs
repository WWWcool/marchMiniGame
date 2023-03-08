using System.Collections.Generic;
using Bridges.Scripts.Gameplay;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Bridges.Scripts
{
    public class UIManager : MonoBehaviour
    {

        [SerializeField] private AchievementManager achievementManager;
        [SerializeField] private AchievementConfig achievementConfig;
        [Header("GUI Components")]
        public GameObject mainMenuGui;
        public GameObject pauseGui, gameplayGui, gameOverGui;
        public GameObject achievementGui;
        public AchievementPopup achievementPopup;

        public GameState gameState;

        private bool _clicked;
        private GameState _previousState;
        private List<string> _achievements = new();

        // Use this for initialization
        void Start () {
            mainMenuGui.SetActive(true);
            pauseGui.SetActive(false);
            gameplayGui.SetActive(false);
            gameOverGui.SetActive(false);
            achievementGui.SetActive(false);
            achievementPopup.gameObject.SetActive(false);
            gameState = GameState.MENU;
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0) && gameState == GameState.MENU && !_clicked)
            {
                if (IsButton())
                    return;

                AudioManager.Instance.PlayEffects(AudioManager.Instance.buttonClick);
                ShowGameplay();
            }
            else if (Input.GetMouseButtonUp(0) && _clicked && gameState == GameState.MENU)
                _clicked = false;
        }

        //show main menu
        public void ShowMainMenu()
        {
            ScoreManager.Instance.ResetCurrentScore();
            _clicked = true;
            mainMenuGui.SetActive(true);
            pauseGui.SetActive(false);
            gameplayGui.SetActive(false);
            gameOverGui.SetActive(false);
            achievementGui.SetActive(false);
            achievementPopup.gameObject.SetActive(false);
            if (gameState == GameState.PAUSED)
                Time.timeScale = 1;

            gameState = GameState.MENU;
            AudioManager.Instance.PlayEffects(AudioManager.Instance.buttonClick);
            GameManager.Instance.NewScene();
        }

        //show pause menu
        public void ShowPauseMenu()
        {
            if (gameState == GameState.PAUSED)
                return;

            pauseGui.SetActive(true);
            Time.timeScale = 0;
            gameState = GameState.PAUSED;
            AudioManager.Instance.PlayEffects(AudioManager.Instance.buttonClick);
        }

        //hide pause menu
        public void HidePauseMenu()
        {
            pauseGui.SetActive(false);
            Time.timeScale = 1;
            gameState = GameState.PLAYING;
            AudioManager.Instance.PlayEffects(AudioManager.Instance.buttonClick);
        }
        
        //show achievement menu
        public void ShowAchievementMenu()
        {
            if (gameState == GameState.ACHIEVEMENT)
                return;

            achievementGui.SetActive(true);
            Time.timeScale = 0;
            _previousState = gameState;
            gameState = GameState.ACHIEVEMENT;
            AudioManager.Instance.PlayEffects(AudioManager.Instance.buttonClick);
        }

        //hide achievement menu
        public void HideAchievementMenu()
        {
            achievementGui.SetActive(false);
            Time.timeScale = 1;
            gameState = _previousState;
            AudioManager.Instance.PlayEffects(AudioManager.Instance.buttonClick);
        }

        public void SetAchievements(List<string> achievements)
        {
            _achievements = achievements;
            if(_achievements.Count > 0)
            {
                var achievement = _achievements[0];
                ShowAchievementPopup(achievement);
                _achievements.Remove(achievement);
            }
        }
        
        //show achievement popup
        public void ShowAchievementPopup(string achievement)
        {
            if (gameState == GameState.ACHIEVEMENT_POPUP)
                return;

            var config = achievementConfig.GetFor(achievement);
            if(config == null)
                return;
            achievementManager.Achieve(config);

            achievementPopup.Init(config.title, config.rewardText);
            achievementPopup.gameObject.SetActive(true);
            Time.timeScale = 0;
            _previousState = gameState;
            gameState = GameState.ACHIEVEMENT_POPUP;
            // AudioManager.Instance.PlayEffects(AudioManager.Instance.buttonClick);
        }

        //hide achievement popup
        public void HideAchievementPopup()
        {
            achievementPopup.gameObject.SetActive(false);
            Time.timeScale = 1;
            gameState = _previousState;
            if (_achievements.Count > 0)
            {
                var achievement = _achievements[0];
                ShowAchievementPopup(achievement);
                _achievements.Remove(achievement);
            }
            // AudioManager.Instance.PlayEffects(AudioManager.Instance.buttonClick);
        }

        //show gameplay gui
        public void ShowGameplay()
        {
            mainMenuGui.SetActive(false);
            pauseGui.SetActive(false);
            gameplayGui.SetActive(true);
            gameOverGui.SetActive(false);
            gameState = GameState.PLAYING;
            AudioManager.Instance.PlayEffects(AudioManager.Instance.buttonClick);
        }

        //show game over gui
        public void ShowGameOver()
        {
            mainMenuGui.SetActive(false);
            pauseGui.SetActive(false);
            gameplayGui.SetActive(false);
            gameOverGui.SetActive(true);
            gameState = GameState.GAMEOVER;
        }

        //check if user click any menu button
        public bool IsButton()
        {
            bool temp = false;

            PointerEventData eventData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            foreach (RaycastResult item in results)
            {
                temp |= item.gameObject.GetComponent<Button>() != null;
            }

            return temp;
        }
    }
}
