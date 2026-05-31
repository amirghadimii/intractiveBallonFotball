using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UPersian.Components;

public class GameManagerBalloon : MonoBehaviour
{
    public enum GameMode
    {
        OnePlayer,
        TwoPlayer
    }

    [Header("Game Settings")] [SerializeField]
    private float gameTime = 120f; // 2 minutes in seconds

    [SerializeField] public GameMode currentGameMode = GameMode.OnePlayer;

    [Header("UI References")] [SerializeField]
    private RtlText scoreText;

    [SerializeField] private RtlText player2ScoreText;
    [SerializeField] private RtlText timerText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private RtlText finalScoreText;
    [SerializeField] private RtlText HowWin;
    [SerializeField] private GameObject player2UI; // UI element for player 2 score

    [Header("Game Objects")] [SerializeField]
    private BalloonSpawner balloonSpawner;

    [SerializeField] private StartGameBalloon startGameBalloon; // Reference to start game balloon
    [SerializeField] private GameObject uiGameObject; // UI GameObject to show on game over
    [SerializeField] private GameObject _SelectPlayer; // UI GameObject to show on game over

    // Game state
    private int player1Score = 0;
    private int player2Score = 0;
    public float currentTime;
    public bool isGameActive = false;

    // Singleton pattern
    public static GameManagerBalloon Instance { get; private set; }

    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
    }

    private void Update()
    {
        if (isGameActive)
        {
            UpdateTimer();
        }
    }

    private void InitializeGame()
    {
        player1Score = 0;
        player2Score = 0;
        currentTime = gameTime;
        isGameActive = false;

        UpdateScoreUI();
        UpdateTimerUI();

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // Update UI based on game mode
        UpdateGameModeUI();
    }

    public void SetGameMode(bool isTwoPlayer)
    {
        currentGameMode = isTwoPlayer ? GameMode.TwoPlayer : GameMode.OnePlayer;
        UpdateGameModeUI();
    }

    private void UpdateGameModeUI()
    {
        if (player2UI != null)
        {
            if(currentGameMode == GameMode.TwoPlayer) player2UI.SetActive(true);
            else player2UI.SetActive(false);
    
        }
    }

    public void StartGame()
    {
        isGameActive = true;
        currentTime = gameTime;
        player1Score = 0;
        player2Score = 0;

        if (uiGameObject != null)
        {
            uiGameObject.SetActive(true);
        }

        UpdateScoreUI();
        UpdateTimerUI();

        Debug.Log($"Game Started in {currentGameMode} mode!");
    }

    private void UpdateTimer()
    {
        currentTime -= Time.deltaTime;
        UpdateTimerUI();

        if (currentTime <= 0)
        {
            EndGame();
        }
    }

    public void AddScore(int playerNumber = 1, int points = 1)
    {
        if (!isGameActive) return;

        if (playerNumber == 1)
        {
            player1Score += points;
        }
        else if (playerNumber == 2 && currentGameMode == GameMode.TwoPlayer)
        {
            player2Score += points;
        }

        UpdateScoreUI();
        Debug.Log($"Player {playerNumber} scored! P1: {player1Score}, P2: {player2Score}");
    }

    private void EndGame()
    {
        isGameActive = false;

        // Stop balloon spawner
        if (balloonSpawner != null)
        {
            balloonSpawner.enabled = false;
        }

        if (uiGameObject != null)
        {
            uiGameObject.SetActive(false);
        }

        // Reactivate start game balloon
        if (_SelectPlayer != null)
        {
            _SelectPlayer.gameObject.SetActive(true);
        }

        // Show UI GameObject


        // Show game over panel
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        // Update final scores
        if (finalScoreText != null)
        {
            finalScoreText.text = player1Score.ToString();
        }


            if (currentGameMode == GameMode.TwoPlayer)
            {
                // Show the winner
                string winnerText = player1Score > player2Score ? "برنده بازیکن آبی" :
                    player2Score > player1Score ? "برنده بازیکن قرمز" : "مساوی شدید";
                HowWin.text = winnerText;
                if (player1Score>player2Score)
                {
                    finalScoreText.text = player1Score.ToString();
                }
                else
                {
                    finalScoreText.text = player2Score.ToString();

                }
            }
            else
            {
                HowWin.text = "آفرین عالی بودی";
            }
 
        

        Debug.Log($"Game Over! Final Scores - P1: {player1Score}, P2: {player2Score}");
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = player1Score.ToString();
        }

        if (player2ScoreText != null && currentGameMode == GameMode.TwoPlayer)
        {
            player2ScoreText.text = player2Score.ToString();
        }
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int timeAsInt = Mathf.FloorToInt(currentTime);
            Debug.Log("timeAsInt"+timeAsInt);
            timerText.text = timeAsInt.ToString();
        }
    }

    public void RestartGame()
    {
        // Reset all balloons in scene
        Balloon[] balloons = FindObjectsOfType<Balloon>();
        foreach (Balloon balloon in balloons)
        {
            balloon.gameObject.SetActive(false);
        }

        InitializeGame();
        StartGame();

        // Re-enable balloon spawner
        if (balloonSpawner != null)
        {
            balloonSpawner.enabled = true;
        }
    }
}