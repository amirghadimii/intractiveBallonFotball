using Assets.SuperGoalie.Scripts.Managers;
using System.Collections;
using RTLTMPro;
using UnityEngine;
using UnityEngine.UI;
using UPersian.Components;
namespace Assets.SuperGoalie.Scripts.Managers
{
public class ScoreSystem : MonoBehaviour
{
    // UI Panels
    public GameObject startPanel; // Reference to the start menu panel
    public GameObject inGamePanelSingle; // Reference to the single-player in-game UI panel
    public GameObject inGamePanelMulti;  // Reference to the two-player in-game UI panel
    public GameObject gameOverPanelSingle; // Reference to the single-player game over panel
    public GameObject gameOverPanelMulti;  // Reference to the two-player game over panel

    // Mode selection buttons (start panel)
    public Button startSinglePlayerButton;
    public Button startTwoPlayerButton;
    // Mode selection buttons (game over panel)
    public Button goSinglePlayerButton;
    public Button goTwoPlayerButton;
    public Button retryButton; // Retry button reference

    public RTLTextMeshPro scoreText;
    public RTLTextMeshPro timerText;
    public GameObject gameOverPanel;
    public UPersian.Components.RtlText finalScoreText;
    [Header("Single Player")]
    [SerializeField] private UPersian.Components.RtlText[] singlePlayerScoreTexts;
    
    [Header("Multi Player")]
    [SerializeField] private UPersian.Components.RtlText[] multiPlayerScoreTexts;
    [SerializeField] private Image[] redTeamShotIndicators; // UI Images for red team's shots (3 dots/images)
    [SerializeField] private Image[] blueTeamShotIndicators; // UI Images for blue team's shots (3 dots/images)
    [SerializeField] private RTLTextMeshPro redTeamScoreText; // Text to display red team's current score
    [SerializeField] private RTLTextMeshPro  blueTeamScoreText; // Text to display blue team's current score
    // Turn indicator (shows 1 or 2 with fade/scale)
   
    public Image turnIndicatorImage;
    public Sprite player1Sprite;
    public Sprite player2Sprite;

    public float gameTime = 120f;

    public bool gameStarted;
    private float timeRemaining;
    private int currentScore;

    public GameManager _GameManager;
    // Text colors for turns
    public Color neutralTextColor = Color.white;
    public Color redTextColor = new Color(0.85f, 0.2f, 0.2f);
    public Color blueTextColor = new Color(0.2f, 0.5f, 1f);

    // Two-player mode scaffolding
    public bool twoPlayerMode;
    public int shotsPerPlayer = 5;
    private int redGoals;
    private int blueGoals;
    private int redShots;
    private int blueShots;
    private bool isRedTurn;
    
    // Team sprites for game over panel
    public Image redTeamImage;
    public Image blueTeamImage;
    public Sprite redHappySprite;
    public Sprite redSadSprite;
    public Sprite blueHappySprite;
    public Sprite blueSadSprite;
    public Renderer goalKeeperRenderer;
    public Material redKeeperMaterial;
    public Material blueKeeperMaterial;
    public float turnIndicatorDuration = 0.9f;
    public float turnIndicatorScaleFrom = 0.4f;
    public float turnIndicatorScaleTo = 1.0f;
    
    [Header("Turn Icons")]
    [SerializeField] private Image redTurnIcon;
    [SerializeField] private Image blueTurnIcon;
    [SerializeField] private Color turnOnColor = Color.white;
    [SerializeField] private Color turnOffColor = new Color(1f, 1f, 1f, 0.3f);
    
    [Header("Sound Effects")]
    public AudioClip turnChangeSound; // Assign this in the Unity Inspector

    private void Start()
    {
        gameStarted = false;
        
        timeRemaining = gameTime;
        currentScore = 0;
        
        // Initialize panels
        if (startPanel != null) startPanel.SetActive(true);
        
        // Hide all in-game and game over panels
        if (inGamePanelSingle != null) inGamePanelSingle.SetActive(false);
        if (inGamePanelMulti != null) inGamePanelMulti.SetActive(false);
        if (gameOverPanelSingle != null) gameOverPanelSingle.SetActive(false);
        if (gameOverPanelMulti != null) gameOverPanelMulti.SetActive(false);

        // Set up button listeners
        if (startSinglePlayerButton != null) startSinglePlayerButton.onClick.AddListener(() => StartMode(false));
        if (startTwoPlayerButton != null) startTwoPlayerButton.onClick.AddListener(() => StartMode(true));
        if (goSinglePlayerButton != null) goSinglePlayerButton.onClick.AddListener(() => StartMode(false));
        if (goTwoPlayerButton != null) goTwoPlayerButton.onClick.AddListener(() => StartMode(true));

        UpdateScoreUI();
        if (timerText != null && timerText.gameObject.transform.parent != null)
            timerText.gameObject.transform.parent.gameObject.SetActive(true);
        UpdateTimerUI();
    }

    private void Update()
    {
        if (!gameStarted) return;
        
        // Only update timer in single-player mode
        if (!twoPlayerMode)
        {
            timeRemaining -= Time.deltaTime;
            UpdateTimerUI();
            
            // Check for game over condition (time's up in single-player)
            if (timeRemaining <= 0f)
            {
                timeRemaining = 0f;
                EndGame();
            }
        }

        UpdateTimerUI();
    }

    public void StartMode(bool twoPlayers)
    {
        // Update UI panels
        if (startPanel != null) startPanel.SetActive(false);
        
        // Show the appropriate in-game panel
        if (twoPlayers)
        {
            if (inGamePanelMulti != null) inGamePanelMulti.SetActive(true);
            if (inGamePanelSingle != null) inGamePanelSingle.SetActive(false);
        }
        else
        {
            if (inGamePanelSingle != null) inGamePanelSingle.SetActive(true);
            if (inGamePanelMulti != null) inGamePanelMulti.SetActive(false);
        }
        
        // Hide game over panels
        if (gameOverPanelSingle != null) gameOverPanelSingle.SetActive(false);
        if (gameOverPanelMulti != null) gameOverPanelMulti.SetActive(false);
        
        // Initialize game mode
        twoPlayerMode = twoPlayers;
        gameStarted = true;
        timeRemaining = gameTime;
        if (!twoPlayerMode)
        {
            currentScore = 0;
            if (_GameManager != null)
            {
                _GameManager._score = 0;
               // if (_GameManager._scoreText != null) _GameManager._scoreText.text = "0";
                // Single player: set ball to neutral (white)
                _GameManager.SetBallMaterial(-1);
            }
            UpdateScoreUI();
        }
        else
        {
            redGoals = blueGoals = 0;
            redShots = blueShots = 0;
            isRedTurn = true;
            // Two player: set initial ball material based on starting player (red starts)
            if (_GameManager != null)
            {
                _GameManager.SetBallMaterial(0);
            }
            UpdateKeeperMaterial();
            UpdateTurnIcons();
            UpdateTurnTextColors();
        }
        if (redTeamScoreText != null)       
            redTeamScoreText.text = ToPersianNumber(0);
        if (blueTeamScoreText != null)
            blueTeamScoreText.text = ToPersianNumber(0);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        UpdateTimerUI();
    }

    private void UpdateScoreDisplays()
    {
        if (!twoPlayerMode) return;
        
        if (redTeamScoreText != null)
            redTeamScoreText.text = ToPersianNumber(redGoals);
            
        if (blueTeamScoreText != null)
            blueTeamScoreText.text = ToPersianNumber(blueGoals);
    }
    
    public void AddScore()
    {
        if (!gameStarted) return;
        if (!twoPlayerMode)
        {
            currentScore++;
            if (_GameManager != null) _GameManager._score = currentScore;
            UpdateScoreUI();
        }
        else
        {
            if (isRedTurn) redGoals++; else blueGoals++;
            UpdateScoreDisplays();
        }
    }

    private void EndGame()
    {
        // Stop the game
        gameStarted = false;
        Time.timeScale = 0f; // Pause the game

        // Hide all in-game panels
        if (inGamePanelSingle != null) inGamePanelSingle.SetActive(false);
        if (inGamePanelMulti != null) inGamePanelMulti.SetActive(false);

        // Show the appropriate game over panel
        if (!twoPlayerMode && gameOverPanelSingle != null)
        {
            gameOverPanelSingle.SetActive(true);

            // Update single player score text - only show the number of goals
            if (singlePlayerScoreTexts != null && singlePlayerScoreTexts.Length > 0 && singlePlayerScoreTexts[0] != null)
            {
                // First text shows just the number of goals
                singlePlayerScoreTexts[0].text = ToPersianNumber(currentScore);
                
                // Hide additional text elements if any
                for (int i = 1; i < singlePlayerScoreTexts.Length; i++)
                {
                    if (singlePlayerScoreTexts[i] != null)
                        singlePlayerScoreTexts[i].gameObject.SetActive(false);
                }
            }

            // Set up menu button
            Button menuButton = gameOverPanelSingle.GetComponentInChildren<Button>();
            if (menuButton != null)
            {
                menuButton.onClick.RemoveAllListeners();
                menuButton.onClick.AddListener(ReturnToMenu);

                // Set up retry button
                if (retryButton != null)
                {
                    retryButton.onClick.RemoveAllListeners();
                    retryButton.onClick.AddListener(RestartGame);
                }
            }
        }
        else if (twoPlayerMode && gameOverPanelMulti != null)
        {
            gameOverPanelMulti.SetActive(true);

            // Update team sprites based on game result
            if (redTeamImage != null && blueTeamImage != null)
            {
                if (redGoals > blueGoals)
                {
                    // Red team won
                    if (redHappySprite != null) redTeamImage.sprite = redHappySprite;
                    if (blueSadSprite != null) blueTeamImage.sprite = blueSadSprite;
                }
                else if (blueGoals > redGoals)
                {
                    // Blue team won
                    if (blueHappySprite != null) blueTeamImage.sprite = blueHappySprite;
                    if (redSadSprite != null) redTeamImage.sprite = redSadSprite;
                }
                else
                {
                    // Draw - show neutral or happy faces
                    if (redHappySprite != null) redTeamImage.sprite = redHappySprite;
                    if (blueHappySprite != null) blueTeamImage.sprite = blueHappySprite;
                }
            }

            // Update score texts - just show the numbers
            if (multiPlayerScoreTexts != null && multiPlayerScoreTexts.Length >= 2)
            {
                // First text for red score
                if (multiPlayerScoreTexts[0] != null)
                    multiPlayerScoreTexts[0].text = ToPersianNumber(redGoals);
                
                // Second text for blue score
                if (multiPlayerScoreTexts[1] != null)
                    multiPlayerScoreTexts[1].text = ToPersianNumber(blueGoals);
                
                // Hide any additional text elements
                for (int i = 2; i < multiPlayerScoreTexts.Length; i++)
                {
                    if (multiPlayerScoreTexts[i] != null)
                        multiPlayerScoreTexts[i].gameObject.SetActive(false);
                }
            }

            // Set up menu button
            Button menuButton = gameOverPanelMulti.GetComponentInChildren<Button>();
            if (menuButton != null)
            {
                var buttonText = menuButton.GetComponentInChildren<RTLTextMeshPro>();
                if (buttonText != null)
                {
                    buttonText.text = "بازگشت به منو";
                }

                menuButton.onClick.RemoveAllListeners();
                menuButton.onClick.AddListener(ReturnToMenu);

                // Set up retry button
                if (retryButton != null)
                {
                    var retryButtonText = retryButton.GetComponentInChildren<RTLTextMeshPro>();
                    if (retryButtonText != null)
                    {
                        retryButtonText.text = "دوباره";
                    }
                    retryButton.onClick.RemoveAllListeners();
                    retryButton.onClick.AddListener(RestartGame);
                }
            }
        }
    }

    private void ResetAllShotIndicators()
    {
        // Reset red team's shot indicators
        if (redTeamShotIndicators != null)
        {
            foreach (var indicator in redTeamShotIndicators)
            {
                if (indicator != null) 
                    indicator.gameObject.SetActive(true);
            }
        }
        
        // Reset blue team's shot indicators
        if (blueTeamShotIndicators != null)
        {
            foreach (var indicator in blueTeamShotIndicators)
            {
                if (indicator != null) 
                    indicator.gameObject.SetActive(true);
            }
        }
    }
    
    public void ReturnToMenu()
    {
        // Reset time scale
        Time.timeScale = 1f;
        
        // Reset game state
        gameStarted = false;
        currentScore = 0;
        
        // Reset two-player mode specific variables
        if (twoPlayerMode)
        {
            redGoals = 0;
            blueGoals = 0;
            redShots = 0;
            blueShots = 0;
            isRedTurn = true;
            
            // Reset shot indicators
            ResetAllShotIndicators();
            
            // Reset keeper material
            UpdateKeeperMaterial();
        }
        
        // Hide all panels except start panel
        if (gameOverPanelSingle != null) gameOverPanelSingle.SetActive(false);
        if (gameOverPanelMulti != null) gameOverPanelMulti.SetActive(false);
        if (inGamePanelSingle != null) inGamePanelSingle.SetActive(false);
        if (inGamePanelMulti != null) inGamePanelMulti.SetActive(false);
        
        // Show start panel
        if (startPanel != null) startPanel.SetActive(true);
        
        // Reset ball and keeper
        if (_GameManager != null)
        {
            _GameManager.ResetGame();
        }
    }
    
    public void RestartGame()
    {
        // Reset time scale
        Time.timeScale = 1f;
        
        // Reset game state
        gameStarted = true;
        timeRemaining = gameTime;
        currentScore = 0;
        
        // Reset two-player mode specific variables
        if (twoPlayerMode)
        {
            redGoals = 0;
            blueGoals = 0;
            redShots = 0;
            blueShots = 0;
            isRedTurn = true;
            
            // Reset shot indicators using the helper method
            ResetAllShotIndicators();
            
            UpdateKeeperMaterial();
        }
        
        // Reset UI - show the appropriate in-game panel based on game mode
        if (gameOverPanelSingle != null) gameOverPanelSingle.SetActive(false);
        if (gameOverPanelMulti != null) gameOverPanelMulti.SetActive(false);
        
        if (twoPlayerMode)
        {
            if (inGamePanelMulti != null) inGamePanelMulti.SetActive(true);
            if (inGamePanelSingle != null) inGamePanelSingle.SetActive(false);
        }
        else
        {
            if (inGamePanelSingle != null) inGamePanelSingle.SetActive(true);
            if (inGamePanelMulti != null) inGamePanelMulti.SetActive(false);
        }
        
        // Reset score display
        UpdateScoreUI();
        UpdateTimerUI();
        
        // Reset ball and keeper
        if (_GameManager != null)
        {
            _GameManager.ResetGame();
        }
        
        // If in two-player mode, reset player turns
        if (twoPlayerMode)
        {
            redGoals = blueGoals = 0;
            redShots = blueShots = 0;
            isRedTurn = true;
            UpdateKeeperMaterial();
            UpdateTurnIcons();
            UpdateTurnTextColors();
            UpdateScoreDisplays();
        }
    }

    public void UpdateScoreUI()
    {
        if (scoreText != null) scoreText.text =   ToPersianNumber(currentScore);
    }

    public void UpdateTimerUI()
    {
        int seconds = Mathf.CeilToInt(Mathf.Max(0f, timeRemaining));
        if (timerText != null) timerText.text = ToPersianNumber(seconds);
    }

    public void OnShotResolved()
    {
        if (!twoPlayerMode || !gameStarted) return;
        
        // Update shot count and UI
        if (isRedTurn)
        {
            redShots++;
            // Update red team's shot indicators
            if (redShots > 0 && redShots <= redTeamShotIndicators.Length)
            {
                redTeamShotIndicators[redShots - 1].gameObject.SetActive(false);
            }
        }
        else
        {
            blueShots++;
            // Update blue team's shot indicators
            if (blueShots > 0 && blueShots <= blueTeamShotIndicators.Length)
            {
                blueTeamShotIndicators[blueShots - 1].gameObject.SetActive(false);
            }
        }
        
        // Check if both players have used all their shots
        if (redShots >= shotsPerPlayer && blueShots >= shotsPerPlayer)
        {
            EndGame();
            return;
        }
        
        // Swap turn if current player still has shots left
        isRedTurn = !isRedTurn;
        
        // If next player has no shots left, switch to the other player
        if ((isRedTurn && redShots >= shotsPerPlayer) || (!isRedTurn && blueShots >= shotsPerPlayer))
        {
            isRedTurn = !isRedTurn;
        }
        
        UpdateKeeperMaterial();
        UpdateTurnIcons();
        UpdateTurnTextColors();
        
        // Play turn change sound
        PlayTurnChangeSound();
    }

    public void UpdateKeeperMaterial()
    {
        if (goalKeeperRenderer == null) return;
        // When it's red's turn, keeper should be blue, and vice versa
        if (isRedTurn)
        {
            if (blueKeeperMaterial != null) goalKeeperRenderer.material = blueKeeperMaterial;
        }
        else
        {
            if (redKeeperMaterial != null) goalKeeperRenderer.material = redKeeperMaterial;
        }
    }

    public void ShowTurnIndicator(int playerNumber)
    {
        if (turnIndicatorImage == null) return;
        
        // Set initial color to white
        turnIndicatorImage.color = Color.white;
        turnIndicatorImage.gameObject.SetActive(true);
        turnIndicatorImage.sprite = playerNumber == 1 ? player1Sprite : player2Sprite;
        
        // Start the fade out animation
        StopCoroutineSafe(_turnIndicatorCo);
        _turnIndicatorCo = StartCoroutine(AnimateTurnIndicator());
    }

    private Coroutine _turnIndicatorCo;
    private IEnumerator AnimateTurnIndicator()
    {
        float t = 0f;
        Vector3 from = Vector3.one * turnIndicatorScaleFrom;
        Vector3 to = Vector3.one * turnIndicatorScaleTo;
        
        // Set initial scale
        turnIndicatorImage.transform.localScale = from;
        
        // Fade out animation
        while (t < turnIndicatorDuration)
        {
            t += Time.deltaTime;
            float progress = Mathf.Clamp01(t / turnIndicatorDuration);
            
            // Smooth fade out (ease-out cubic)
            float alpha = 1f - Mathf.Pow(progress, 3f);
            turnIndicatorImage.color = new Color(1f, 1f, 1f, alpha);
            
            // Scale up animation
            float scale = Mathf.Lerp(from.x, to.x, progress);
            turnIndicatorImage.transform.localScale = new Vector3(scale, scale, scale);
            
            yield return null;
        }
        
        // Ensure it's fully transparent at the end
        turnIndicatorImage.color = new Color(1f, 1f, 1f, 0f);
        turnIndicatorImage.gameObject.SetActive(false);
    }

    public void StopCoroutineSafe(Coroutine co)
    {
        if (co != null) StopCoroutine(co);
    }
    
    private void PlayTurnChangeSound()
    {
        if (turnChangeSound != null && Camera.main != null)
        {
            AudioSource.PlayClipAtPoint(turnChangeSound, Camera.main.transform.position);
        }
    }

    public void UpdateTurnTextColors()
    {
        if (!twoPlayerMode)
        {
            if (scoreText != null) scoreText.color = neutralTextColor;
            if (_GameManager != null && _GameManager._scoreText != null) _GameManager._scoreText.color = neutralTextColor;
            return;
        }
        Color c = isRedTurn ? redTextColor : blueTextColor;
        if (scoreText != null) scoreText.color = c;
        if (_GameManager != null && _GameManager._scoreText != null) _GameManager._scoreText.color = c;
        if (GameManager.Instance != null || _GameManager != null)
{
    (GameManager.Instance != null ? GameManager.Instance : _GameManager)
        .SetBallMaterial(isRedTurn ? 0 : 1); // 0=قرمز، 1=آبی
}
    }

    // Updates the persistent red/blue turn icons beside the shot counters
    private void UpdateTurnIcons()
    {
        // Hide old temporary indicator if present
        if (turnIndicatorImage != null)
        {
            turnIndicatorImage.gameObject.SetActive(false);
        }

        if (redTurnIcon != null)
        {
            redTurnIcon.color = isRedTurn ? turnOnColor : turnOffColor;
        }
        if (blueTurnIcon != null)
        {
            blueTurnIcon.color = isRedTurn ? turnOffColor : turnOnColor;
        }
    }

    public string ToPersianNumber(int number)
    {
        return number.ToString().Replace('0', '۰')
            .Replace('1', '۱')
            .Replace('2', '۲')
            .Replace('3', '۳')
            .Replace('4', '۴')
            .Replace('5', '۵')
            .Replace('6', '۶')
            .Replace('7', '۷')
            .Replace('8', '۸')
            .Replace('9', '۹');
    }
}
}