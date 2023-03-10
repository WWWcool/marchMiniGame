using System.Collections;
using System.Collections.Generic;
using Bridges.Scripts.Gameplay;
using UnityEngine;

namespace Bridges.Scripts
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; set; }

        public GameObject camObject;
        public UIManager uIManager;
        public ScoreManager scoreManager;
        public AchievementManager achievementManager;

        [Header("Game settings")]
        [Space(5)]
        public Color[] colorTable;
        [Space(5)]
        public Color goodColor, wrongColor;
        [Space(5)]
        public Obstacle obstaclePrefab;
        [Space(5)]
        [Range(1, 2.9f)]
        public float minObstacleSpeed = 1;
        [Range(3, 5)]
        public int maxObstacleSpeed = 3;
        [Space(5)]
        public List<Obstacle> obstacleList = new();
        [Space(5)]
        [Range(5,9)]
        public int minBridgeLength = 5;
        [Range(10, 20)]
        public int maxBridgeLength = 7;
        [Space(5)]
        public float firstObstacleY = 1; //y position of first obstacle
        [Space(5)]
        [Range(0f, 1f)]
        public float playZoneEdgePercent = 0.45f;
        // [Space(5)]
        // public float heightDistanceLastFirst = 1; //difference in y position between first and last obstacle

        [Space(25)]
        Vector2 screenBounds;

        float followSpeed = 2.8f; //how fast camera show scene
        float obstacleSpeed = 2f;
        public int bridgeLength, obstacleIndex;
        float obstacleWidth, obstacleHeight;
        Obstacle lastObstacle, tempObstacle, bridgeEnd;
        bool cameraOnStart, playing, canCreateObstacle;
        Vector2 cameraStartPos, targetPosition, tempPos;
        float obstaclePositionY;

        private int roundCount = 0;
        private int perfectCount = 0;

        void Awake()
        {
            DontDestroyOnLoad(this);

            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        // Start is called before the first frame update
        void Start()
        {
            Application.targetFrameRate = 30;

            camObject = Camera.main.gameObject;
            cameraStartPos = camObject.transform.position;

            //get screen bounds to set position for first square obstacle
            screenBounds = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
            CreateScene();
        }

        void Update()
        {
            if (uIManager.gameState == GameState.PLAYING && Input.GetMouseButton(0))
            {
                //check if player clicked on pause button
                if (uIManager.IsButton())
                    return;

                //stop obstacle and check if position is same or lower than previous block
                if (playing && canCreateObstacle)
                {
                    canCreateObstacle = false;
                    lastObstacle.GetComponent<Obstacle>().StopMoving();

                    var obstaclePos = lastObstacle.transform.position.y;
                    var previousObstaclePos = obstacleList[obstacleIndex - 1].transform.position.y;
                    var bridgeEndPos = bridgeEnd.transform.position.y;

                    //perfect stop (player can stop little higher or lower -> 5% of block high)
                    if (obstaclePos - (.1f * obstacleHeight) < previousObstaclePos &&
                        obstaclePos + (.1f * obstacleHeight) > previousObstaclePos)
                    {
                        lastObstacle.transform.position = new Vector2(lastObstacle.transform.position.x, obstacleList[obstacleIndex - 1].transform.position.y);
                        AudioManager.Instance.PlayEffects(AudioManager.Instance.perfect);
                        ScoreManager.Instance.UpdateScore(2);
                        perfectCount++;
                    }
                    //Check if lowest point of current obstacle is higher than highest point of previous obstacle
                    //or if highest point of current obstacle is lower than lowest point of previous obstacle
                    else if (obstaclePos - (obstacleHeight / 2) > previousObstaclePos + (obstacleHeight / 2) ||
                             obstaclePos + (obstacleHeight / 2) < previousObstaclePos - (obstacleHeight / 2))
                    {
                        AudioManager.Instance.PlayEffects(AudioManager.Instance.wrongColor);
                        playing = false;
                        GameOver();
                        return;
                    }
                    //everything is fine
                    else
                    {
                        AudioManager.Instance.PlayEffects(AudioManager.Instance.sameColor);
                        ScoreManager.Instance.UpdateScore(1);
                    }
                
                    //finished bridge
                    if (obstacleIndex == bridgeLength &&
                        obstaclePos - (obstacleHeight / 2) < bridgeEndPos + (obstacleHeight / 2) &&
                        obstaclePos + (obstacleHeight / 2) > bridgeEndPos - (obstacleHeight / 2))
                    {
                        AudioManager.Instance.PlayEffects(AudioManager.Instance.perfect);

                        for (int i = 0; i < obstacleList.Count; i++)
                        {
                            obstacleList[i].SetColorSprite();
                        }

                        playing = false;
                        StartCoroutine(NewScene(1.5f));
                    }
                    else if (obstacleIndex == bridgeLength) //didn't get the cut
                    {
                        AudioManager.Instance.PlayEffects(AudioManager.Instance.wrongColor);
                        playing = false;
                        GameOver();
                    }
                    else
                        SpawnObstacle();
                }
            }
            else if (uIManager.gameState == GameState.PLAYING) //show first and last part of bridge
            {
                //show first and last obstacle on game start
                if (!cameraOnStart)
                {
                    cameraOnStart = true;
                    StartCoroutine(ShowStartEnd());
                    return;
                }

                if (cameraOnStart && !playing) //move camera when showing scene
                {
                    tempPos = Vector2.Lerp(camObject.transform.position, targetPosition, followSpeed * Time.deltaTime);
                    camObject.transform.position = new Vector3(tempPos.x, 0, -10); //reset z and y of camera (move only of x axis)
                }
                else if (playing) //move camera during gameplay
                {
                    if (lastObstacle.transform.position.x > 0)
                    {
                        tempPos = Vector2.Lerp(camObject.transform.position, lastObstacle.transform.position, followSpeed * Time.deltaTime);
                        camObject.transform.position = new Vector3(tempPos.x, 0, -10); //reset z and y of camera (move only of x axis)
                    }
                }

            }

            if (uIManager.gameState == GameState.PLAYING && Input.GetMouseButtonUp(0)) //prepare for next spawned block
            {
                canCreateObstacle = true;
            }

        }

        //create start scene
        public void CreateScene()
        {
            cameraOnStart = false;
            obstacleIndex = 0;
            perfectCount = 0;
            bridgeLength = Random.Range(minBridgeLength, maxBridgeLength + 1);

            //create first bridge part
            lastObstacle = Instantiate(obstaclePrefab);

            obstacleWidth = lastObstacle.GetComponent<SpriteRenderer>().bounds.size.x; //calculate obstacle width
            obstacleHeight = lastObstacle.GetComponent<SpriteRenderer>().bounds.size.y; //calculate obstacle height

            lastObstacle.GetComponent<SpriteRenderer>().color = colorTable[Random.Range(0, colorTable.Length)];
            lastObstacle.transform.position = new Vector2(
                -screenBounds.x + obstacleWidth /2,
                RandomObstacleY()
                );

            obstacleIndex++;

            //create last bridge part
            bridgeEnd = Instantiate(obstaclePrefab);
            bridgeEnd.GetComponent<SpriteRenderer>().color = colorTable[Random.Range(0, colorTable.Length)];
            bridgeEnd.transform.position = new Vector2(
                lastObstacle.transform.position.x + (bridgeLength * obstacleWidth),
                RandomObstacleY()
                );
            targetPosition = cameraStartPos;

            obstacleList.Add(bridgeEnd);
            obstacleList.Add(lastObstacle);
        }

        //create new obstacle
        void SpawnObstacle()
        {

            obstacleSpeed = Random.Range(minObstacleSpeed, maxObstacleSpeed);
            //create first bridge part
            tempObstacle = Instantiate(obstaclePrefab);

            //random spawn obstacle on top or bottom of screen
            if (Random.Range(0, 2) == 0)
                obstaclePositionY = screenBounds.y + obstacleHeight / 2;
            else
                obstaclePositionY = -screenBounds.y - obstacleHeight / 2;

            // tempObstacle.GetComponent<SpriteRenderer>().color = colorTable[Random.Range(0, colorTable.Length)];
            tempObstacle.transform.position = new Vector2(lastObstacle.transform.position.x + obstacleWidth, obstaclePositionY);
            tempObstacle.Init(screenBounds, playZoneEdgePercent);
            tempObstacle.StartMoving(obstacleSpeed);
            obstacleIndex++;
            lastObstacle = tempObstacle;
            obstacleList.Add(lastObstacle);
        }

        //create new scene
        IEnumerator NewScene(float delay)
        {
            yield return new WaitForSeconds(delay);
            roundCount++;
            ScoreManager.Instance.UpdateHighScore();
            var context = CollectContext();
            var achievements = achievementManager.GetAchieved(context);
            uIManager.SetAchievements(achievements);
            while (uIManager.gameState != GameState.PLAYING || achievements.Count != 0)
            {
                yield return null;
            }
            NewScene();
        }

        public void NewScene()
        {
            ClearScene();
            CreateScene();
            cameraOnStart = false;
            playing = false;
        }

        //show first and last obstacle
        IEnumerator ShowStartEnd()
        {
            yield return new WaitForSeconds(.5f);
            targetPosition = bridgeEnd.transform.position;
            yield return new WaitForSeconds(bridgeLength / 5);
            targetPosition = cameraStartPos;
            yield return new WaitForSeconds(bridgeLength / 5);
            if (uIManager.gameState == GameState.PLAYING)
            {
                playing = true;
                SpawnObstacle();
                canCreateObstacle = true;
                playing = true;
            }
        }

        //restart game, reset score,...
        public void RestartGame()
        {
            if (uIManager.gameState == GameState.PAUSED)
                Time.timeScale = 1;

            ClearScene();
            CreateScene();
            uIManager.ShowGameplay();
            scoreManager.ResetCurrentScore();
            cameraOnStart = true;
            playing = false;
            roundCount = 0;
            StartCoroutine(ShowStartEnd());
        }

        //clear all blocks from scene
        public void ClearScene()
        {

            GameObject[] obstacles = GameObject.FindGameObjectsWithTag("Obstacle");

            foreach (var item in obstacles)
            {
                Destroy(item);
            }

            //clear obstacles list
            obstacleList.Clear();

            //reset camera position
            camObject.transform.position = new Vector3(cameraStartPos.x, 0, -10); //reset z and y of camera (move only of x axis)

        }

        //show game over gui
        public void GameOver()
        {
            if (uIManager.gameState == GameState.PLAYING)
            {
                playing = false;
                AudioManager.Instance.PlayEffects(AudioManager.Instance.gameOver);
                uIManager.ShowGameOver();

                for (int i = 0; i < obstacleList.Count; i++)
                {
                    obstacleList[i].GetComponent<SpriteRenderer>().color = wrongColor;
                }

                scoreManager.UpdateScoreGameover();
            }
        }

        private float RandomObstacleY()
        {
            return Random.Range(
                (-screenBounds.y - obstacleHeight / 2) * playZoneEdgePercent / 1.3f,
                (screenBounds.y + obstacleHeight / 2) * playZoneEdgePercent / 1.3f);
        }

        private AchievementsContext CollectContext()
        {
            var context = new AchievementsContext();
            var obstacleLine = new List<Obstacle>();
            
            for (int i = 0; i < obstacleList.Count; i++)
            {
                var obstacle = obstacleList[i];
                
                // check lines
                if (obstacleLine.Count == 0)
                {
                    obstacleLine.Add(obstacle);
                }
                else
                {
                    if (obstacle.Type == obstacleLine[0].Type)
                    {
                        obstacleLine.Add(obstacle);
                        if (obstacleLine.Count > 2)
                        {
                            var achievement = new AchievementContext
                            {
                                count = obstacleLine.Count,
                                type = EAchievementType.Line,
                                obstacleType = obstacleLine[0].Type
                            };
                            context.data.Add(achievement);
                        }
                    }
                    else
                    {
                        obstacleLine.Clear();
                        obstacleLine.Add(obstacle);
                    }
                }
            }
            
            var scoreAchievement = new AchievementContext
            {
                count = ScoreManager.Instance.currentScore,
                type = EAchievementType.Score
            };
            
            context.data.Add(scoreAchievement);
            
            var roundAchievement = new AchievementContext
            {
                count = roundCount,
                type = EAchievementType.Round
            };
            
            context.data.Add(roundAchievement);
            
            var perfectAchievement = new AchievementContext
            {
                count = perfectCount,
                type = EAchievementType.Perfect
            };
            
            context.data.Add(perfectAchievement);

            return context;
        } 
    }
}
