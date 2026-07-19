using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Collections;
using UnityEngine.UI;
using UPersian.Components;

namespace GoalRush
{
    public class UIManager : MonoBehaviour
    {
        [Header("HUD")]
        [SerializeField] private GameObject _hudContainer;
        [SerializeField] private RtlText _scoreText;
        [SerializeField] private RtlText _hudHitsText;
        [SerializeField] private RtlText _timerText;
        [SerializeField] private RtlText _comboText;
        [SerializeField] private RtlText _difficultyText;
        [SerializeField] private RtlText _missCountText;

        [Header("Menu")]
        [SerializeField] private GameObject _menuContainer;
        [SerializeField] private RtlText _menuTitleText;
        [SerializeField] private RtlText _menuHighScoreText;
        [SerializeField] private Button _startButton;

        [Header("Countdown")]
        [SerializeField] private GameObject _countdownContainer;
        [SerializeField] private RtlText _countdownText;

        [Header("Floating Text")]
        [SerializeField] private RtlText _floatingTextPrefab;
        [SerializeField] private Canvas _floatingCanvas;

        [Header("Screen Effects")]
        [SerializeField] private Image _redFlashImage;
        [SerializeField] private Image _greenFlashImage;
        [SerializeField] private Image _goldFlashImage;
        [SerializeField] private float _flashDuration = 0.6f;

        [Header("Camera Shake")]
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private float _shakeDuration = 0.2f;
        [SerializeField] private float _shakeStrength = 10f;

        [Header("Game Over")]
        [SerializeField] private GameObject _gameOverContainer;
        [SerializeField] private RtlText _finalScoreText;
        [SerializeField] private RtlText _gameOverTitleText;
        [SerializeField] private RtlText _highScoreText;
        [SerializeField] private RtlText _newHighScoreText;
        [SerializeField] private RtlText _accuracyText;
        [SerializeField] private RtlText _gameOverComboText;
        [SerializeField] private RtlText _gameOverGoldHitsText;
        [SerializeField] private Button _restartButton;

        [Header("Pause")]
        [SerializeField] private GameObject _pauseOverlay;
        [SerializeField] private Button _pauseButton;

        [Header("Menu Extended")]
        [SerializeField] private Button _leaderboardButton;
        [SerializeField] private Button _resetButton;
        [SerializeField] private Image[] _teamIconImages;
        [SerializeField] private GameObject[] _teamIconHighlights;
        [SerializeField] private Button[] _teamIconButtons;
        [SerializeField] private InputField _nameInput;
        [SerializeField] private RtlText _namePlaceholder;

        [Header("Leaderboard")]
        [SerializeField] private GameObject _leaderboardContainer;
        [SerializeField] private RectTransform _leaderboardListParent;
        [SerializeField] private GameObject _leaderboardEntryPrefab;
        [SerializeField] private Button _leaderboardBackButton;
        [SerializeField] private Button _leaderboardResetButton;

        [Header("Back Navigation")]
        [SerializeField] private Button _backToMenuButton;

        private Button[] _allTeamButtons;
        private readonly Color[] _teamColors = new Color[]
        {
            new Color(0.957f, 0.263f, 0.212f), // Red
            new Color(0.204f, 0.596f, 0.859f), // Blue
            new Color(0.298f, 0.686f, 0.314f), // Green
            new Color(0.957f, 0.867f, 0.212f), // Yellow
            new Color(0.506f, 0.259f, 0.608f), // Purple
            new Color(0.957f, 0.612f, 0.071f), // Orange
        };

        [Header("Notifications")]
        [SerializeField] private GameObject _notificationContainer;
        [SerializeField] private RtlText _notificationText;

        [Header("Difficulty Level Up")]
        [SerializeField] private GameObject _levelUpContainer;
        [SerializeField] private RtlText _levelUpText;

        [Header("High Score Celebration")]
        [SerializeField] private ParticleSystem _celebrationParticles;

        [Header("Transition")]
        [SerializeField] private float _transitionDuration = 0.3f;

        public static UIManager Instance { get; private set; }

        private Vector3 _cameraInitialPos;
        private int _currentDisplayScore;
        private Tween _scoreTween;
        private bool _isPaused;
        private Coroutine _lowTimeCoroutine;
        private Tween _timerPulseTween;
        private Coroutine _tickCoroutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (_mainCamera == null)
                _mainCamera = Camera.main;
            if (_mainCamera != null)
                _cameraInitialPos = _mainCamera.transform.position;
        }

        private void Start()
        {
            SetAllContainers(false);
            if (_menuContainer != null) _menuContainer.SetActive(true);
            if (_hudContainer != null) _hudContainer.SetActive(false);
            if (_countdownContainer != null) _countdownContainer.SetActive(false);
            if (_gameOverContainer != null) _gameOverContainer.SetActive(false);
            if (_pauseOverlay != null) _pauseOverlay.SetActive(false);
            if (_notificationContainer != null) _notificationContainer.SetActive(false);
            if (_levelUpContainer != null) _levelUpContainer.SetActive(false);
            if (_leaderboardContainer != null) _leaderboardContainer.SetActive(false);

            if (_greenFlashImage != null)
                _greenFlashImage.color = new Color(0, 1, 0, 0);
            if (_redFlashImage != null)
                _redFlashImage.color = new Color(1, 0, 0, 0);
            if (_goldFlashImage != null)
                _goldFlashImage.color =  new Color(1, 0, 0, 0);

            var gm = GameManager.Instance;
            if (gm != null)
            {
                if (_menuHighScoreText != null)
                    _menuHighScoreText.text = $"رکورد: {gm.HighScore}";

                gm.OnScoreChanged += UpdateScore;
                gm.OnTimerUpdated += UpdateTimer;
                gm.OnCountdownTick += ShowCountdown;
                gm.OnStateChanged += OnGameStateChanged;
                gm.OnComboChanged += OnComboChanged;
                gm.OnDifficultyStepChanged += OnDifficultyStepChanged;
                gm.OnGoldHit += OnGoldHit;
                gm.OnPenaltyHit += OnPenaltyHit;
                gm.OnConsecutiveWrongsChanged += OnConsecutiveWrongsChanged;
                gm.OnLeaderboardUpdated += PopulateLeaderboard;
            }

            if (_startButton != null)
                _startButton.onClick.AddListener(OnStartClicked);

            if (_restartButton != null)
                _restartButton.onClick.AddListener(OnRestartClicked);

            if (_pauseButton != null)
                _pauseButton.onClick.AddListener(TogglePause);

            if (_leaderboardButton != null)
                _leaderboardButton.onClick.AddListener(OnLeaderboardClicked);

            if (_resetButton != null)
                _resetButton.onClick.AddListener(OnResetClicked);

            if (_leaderboardBackButton != null)
                _leaderboardBackButton.onClick.AddListener(OnLeaderboardBackClicked);

            if (_leaderboardResetButton != null)
                _leaderboardResetButton.onClick.AddListener(OnLeaderboardResetClicked);

            if (_backToMenuButton != null)
                _backToMenuButton.onClick.AddListener(OnBackToMenuClicked);

            InitTeamIcons();

            if (_nameInput != null)
            {
                string savedName = PlayerInfo.LoadName();
                if (!string.IsNullOrEmpty(savedName))
                {
                    _nameInput.text = savedName;
                    if (_namePlaceholder != null)
                        _namePlaceholder.gameObject.SetActive(false);
                }
                _nameInput.onValueChanged.AddListener(OnNameChanged);
            }

            if (_menuContainer != null)
                _menuContainer.transform.localScale = Vector3.one * 0.9f;
            DOVirtual.DelayedCall(0.05f, () =>
            {
                if (_menuContainer != null)
                    _menuContainer.transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack);
            });
        }

        private void InitTeamIcons()
        {
            int savedTeam = PlayerInfo.LoadTeamIndex();
            for (int i = 0; i < 6; i++)
            {
                if (_teamIconImages != null && i < _teamIconImages.Length && _teamIconImages[i] != null)
                    _teamIconImages[i].color = _teamColors[i];

                if (_teamIconHighlights != null && i < _teamIconHighlights.Length && _teamIconHighlights[i] != null)
                    _teamIconHighlights[i].SetActive(i == savedTeam);

                int idx = i;
                if (_teamIconButtons != null && i < _teamIconButtons.Length && _teamIconButtons[i] != null)
                    _teamIconButtons[i].onClick.AddListener(() => OnTeamIconClicked(idx));
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                TogglePause();
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnScoreChanged -= UpdateScore;
                GameManager.Instance.OnTimerUpdated -= UpdateTimer;
                GameManager.Instance.OnCountdownTick -= ShowCountdown;
                GameManager.Instance.OnStateChanged -= OnGameStateChanged;
                GameManager.Instance.OnComboChanged -= OnComboChanged;
                GameManager.Instance.OnDifficultyStepChanged -= OnDifficultyStepChanged;
                GameManager.Instance.OnGoldHit -= OnGoldHit;
                GameManager.Instance.OnPenaltyHit -= OnPenaltyHit;
                GameManager.Instance.OnConsecutiveWrongsChanged -= OnConsecutiveWrongsChanged;
                GameManager.Instance.OnLeaderboardUpdated -= PopulateLeaderboard;
            }

            if (_startButton != null)
                _startButton.onClick.RemoveListener(OnStartClicked);
            if (_restartButton != null)
                _restartButton.onClick.RemoveListener(OnRestartClicked);
            if (_leaderboardButton != null)
                _leaderboardButton.onClick.RemoveListener(OnLeaderboardClicked);
            if (_resetButton != null)
                _resetButton.onClick.RemoveListener(OnResetClicked);
            if (_leaderboardBackButton != null)
                _leaderboardBackButton.onClick.RemoveListener(OnLeaderboardBackClicked);
            if (_leaderboardResetButton != null)
                _leaderboardResetButton.onClick.RemoveListener(OnLeaderboardResetClicked);
            if (_backToMenuButton != null)
                _backToMenuButton.onClick.RemoveListener(OnBackToMenuClicked);
        }

        private void SetAllContainers(bool active)
        {
            if (_menuContainer != null) _menuContainer.SetActive(active);
            if (_hudContainer != null) _hudContainer.SetActive(active);
            if (_countdownContainer != null) _countdownContainer.SetActive(active);
            if (_gameOverContainer != null) _gameOverContainer.SetActive(active);
        }

        private void TogglePause()
        {
            if (GameManager.Instance.State != GameState.Playing && !_isPaused) return;

            _isPaused = !_isPaused;
            Time.timeScale = _isPaused ? 0f : 1f;

            if (_pauseOverlay != null)
                _pauseOverlay.SetActive(_isPaused);
        }

        private void OnStartClicked()
        {
            AudioManager.Instance?.PlayButtonClick();
            _menuContainer?.SetActive(false);
            GameManager.Instance.StartCountdown();
        }

        private void OnRestartClicked()
        {
            AudioManager.Instance?.PlayButtonClick();
            GameManager.Instance.ResetGame();
        }

        private void OnBackToMenuClicked()
        {
            AudioManager.Instance?.PlayButtonClick();
            GameManager.Instance.ResetGame();
        }

        private void OnLeaderboardClicked()
        {
            AudioManager.Instance?.PlayButtonClick();
            PopulateLeaderboard();
            if (_menuContainer != null) _menuContainer.SetActive(false);
            if (_leaderboardContainer != null) _leaderboardContainer.SetActive(true);
        }

        private void OnLeaderboardBackClicked()
        {
            AudioManager.Instance?.PlayButtonClick();
            if (_leaderboardContainer != null) _leaderboardContainer.SetActive(false);
            if (_menuContainer != null) _menuContainer.SetActive(true);
        }

        private void OnResetClicked()
        {
            AudioManager.Instance?.PlayButtonClick();
            GameManager.Instance.ResetAllData();
            if (_menuHighScoreText != null)
                _menuHighScoreText.text = "رکورد: 0";
            if (_nameInput != null)
            {
                _nameInput.text = "";
                if (_namePlaceholder != null)
                    _namePlaceholder.gameObject.SetActive(true);
            }
            if (_teamIconHighlights != null)
            {
                for (int i = 0; i < _teamIconHighlights.Length; i++)
                {
                    if (_teamIconHighlights[i] != null)
                        _teamIconHighlights[i].SetActive(i == 0);
                }
            }
        }

        private void OnLeaderboardResetClicked()
        {
            AudioManager.Instance?.PlayButtonClick();
            GameManager.Instance.ClearLeaderboardData();
            PopulateLeaderboard();
        }

        private void OnTeamIconClicked(int index)
        {
            AudioManager.Instance?.PlayButtonClick();
            GameManager gm = GameManager.Instance;
            if (gm != null) gm.TeamIndex = index;
            for (int i = 0; i < _teamIconHighlights.Length; i++)
            {
                if (_teamIconHighlights[i] != null)
                    _teamIconHighlights[i].SetActive(i == index);
            }
        }

        private void OnNameChanged(string value)
        {
            GameManager gm = GameManager.Instance;
            if (gm != null)
            {
                gm.PlayerName = value;
                if (_namePlaceholder != null)
                    _namePlaceholder.gameObject.SetActive(string.IsNullOrEmpty(value));
            }
        }

        private void PopulateLeaderboard()
        {
            if (_leaderboardListParent == null || _leaderboardEntryPrefab == null) return;

            for (int i = _leaderboardListParent.childCount - 1; i >= 0; i--)
            {
                Transform child = _leaderboardListParent.GetChild(i);
                if (child.gameObject.activeSelf)
                    Destroy(child.gameObject);
            }

            var data = GameManager.Instance.GetLeaderboardData();
            for (int i = 0; i < data.entries.Count; i++)
            {
                var entry = data.entries[i];
                GameObject row = Object.Instantiate(_leaderboardEntryPrefab, _leaderboardListParent);

                RtlText[] texts = row.GetComponentsInChildren<RtlText>(true);
                foreach (var t in texts)
                {
                    if (t.name == "RankText") t.text = $"#{i + 1}";
                    else if (t.name == "NameText") t.text = entry.playerName;
                    else if (t.name == "ScoreText") t.text = entry.score.ToString();
                }

                Image[] imgs = row.GetComponentsInChildren<Image>(true);
                foreach (var img in imgs)
                {
                    if (img.name == "TeamIconImage" && entry.teamIndex >= 0 && entry.teamIndex < _teamColors.Length)
                        img.color = _teamColors[entry.teamIndex];
                }

                row.transform.localScale = Vector3.one;
            }
        }

        private void OnGameStateChanged(GameState state)
        {
            StopCoroutine(nameof(TransitionRoutine));
            StartCoroutine(TransitionRoutine(state));
        }

        private IEnumerator TransitionRoutine(GameState state)
        {
            if (_menuContainer != null) _menuContainer.SetActive(state == GameState.Menu);
            if (_hudContainer != null) _hudContainer.SetActive(state == GameState.Playing);
            if (_countdownContainer != null) _countdownContainer.SetActive(state == GameState.Countdown);
            if (_gameOverContainer != null) _gameOverContainer.SetActive(state == GameState.GameOver);
            if (_pauseOverlay != null) _pauseOverlay.SetActive(false);
            if (_leaderboardContainer != null) _leaderboardContainer.SetActive(false);
            _isPaused = false;
            Time.timeScale = 1f;

            if (_lowTimeCoroutine != null)
            {
                StopCoroutine(_lowTimeCoroutine);
                _lowTimeCoroutine = null;
            }
            if (_tickCoroutine != null)
            {
                StopCoroutine(_tickCoroutine);
                _tickCoroutine = null;
            }
            StopTimerPulse();

            if (state == GameState.Menu)
            {
                if (_menuHighScoreText != null)
                    _menuHighScoreText.text = $"رکورد: {GameManager.Instance.HighScore}";
            }

            if (state == GameState.Playing)
            {
                _currentDisplayScore = 0;
                _scoreText.text = "0";
                _timerText.color = Color.white;

                if (_hudHitsText != null)
                    _hudHitsText.text = "0";

                if (_comboText != null)
                {
                    _comboText.text = "";
                    _comboText.transform.localScale = Vector3.one;
                }
                if (_difficultyText != null)
                {
                    _difficultyText.text = "0";
                    _difficultyText.transform.localScale = Vector3.one;
                }

                if (_missCountText != null)
                    _missCountText.text = "0";
            }

            if (state == GameState.GameOver)
                ShowGameOver();

            yield return null;
        }

        private void ShowGameOver()
        {
            if (_gameOverContainer == null) return;
            if (_hudContainer != null) _hudContainer.SetActive(false);
            AudioManager.Instance?.PlayGameOver();

            var gm = GameManager.Instance;
            _gameOverContainer.SetActive(true);
            _gameOverContainer.transform.localScale = Vector3.one * 0.8f;

            if (_finalScoreText != null)
                _finalScoreText.text = gm.Score.ToString();

            if (_highScoreText != null)
                _highScoreText.text = $"رکورد: {gm.HighScore}";

            if (_newHighScoreText != null)
            {
                _newHighScoreText.gameObject.SetActive(gm.IsNewHighScore);
                if (gm.IsNewHighScore)
                {
                    _newHighScoreText.transform.localScale = Vector3.zero;
                    _newHighScoreText.transform.DOScale(Vector3.one, 0.5f).SetDelay(1f).SetEase(Ease.OutBack);
                    Color gold = new Color(1, 0.84f, 0);
                    _newHighScoreText.color = gold;
                    _newHighScoreText.DOColor(new Color(1, 1, 0.4f), 0.6f).SetLoops(-1, LoopType.Yoyo);
                    PlayHighScoreCelebration();
                }
            }

            if (_accuracyText != null)
                _accuracyText.text = $"دقت: {gm.Accuracy:F0}%";

            if (_gameOverComboText != null)
                _gameOverComboText.text = $"بهترین ترکیب: {gm.Combo}";

            if (_gameOverGoldHitsText != null)
                _gameOverGoldHitsText.text = $"ضربات: {gm.GoldHits} / {gm.TotalClicks}";

            _gameOverContainer.transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack);
        }

        private void UpdateScore(int score)
        {
            if (_scoreText == null) return;

            if (_hudHitsText != null)
            {
                var gm = GameManager.Instance;
                if (gm != null)
                    _hudHitsText.text = $"{gm.GoldHits}";
            }

            _scoreTween?.Kill();
            _scoreTween = DOVirtual.Int(_currentDisplayScore, score, 0.25f, v =>
            {
                _currentDisplayScore = v;
                _scoreText.text = v.ToString();
            }).SetEase(Ease.OutQuad);

            _scoreText.transform.DOKill();
            _scoreText.transform.localScale = Vector3.one;
            _scoreText.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 5, 0.5f);
        }

        private void UpdateTimer(float time)
        {
            if (_timerText == null) return;
            int seconds = Mathf.CeilToInt(time);
            _timerText.text = $"{seconds} ثانیه";

            if (time <= 30f && time > 10f)
            {
                float t = (time - 10f) / 20f;
                _timerText.color = Color.Lerp(Color.white, new Color(1, 0.64f, 0), 1f - t);

                StartTimerPulse(1.05f, 1f);
                StartTickCoroutine();

                if (_notificationContainer != null && _notificationText != null && seconds == 30)
                {
                    _notificationText.text = "عجله کن!";
                    _notificationText.color = new Color(1, 0.64f, 0);
                    ShowNotification();
                }
            }
            else if (time <= 10f && time > 0f)
            {
                float t = time / 10f;
                _timerText.color = Color.Lerp(Color.red, new Color(1, 0.64f, 0), t);

                StartTimerPulse(1.15f, 0.5f);
                StartTickCoroutine();

                if (_lowTimeCoroutine == null)
                {
                    _lowTimeCoroutine = StartCoroutine(LowTimePulse());
                    AudioManager.Instance?.PlayLowTime();
                }
            }
            else if (time > 30f)
            {
                _timerText.color = Color.white;
                StopTimerPulse();
            }
        }

        private void StartTimerPulse(float maxScale, float duration)
        {
            if (_timerPulseTween != null && _timerPulseTween.IsActive()) return;
            _timerPulseTween?.Kill();
            _timerPulseTween = _timerText.transform.DOScale(maxScale, duration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        private void StopTimerPulse()
        {
            _timerPulseTween?.Kill();
            _timerPulseTween = null;
            if (_timerText != null)
                _timerText.transform.localScale = Vector3.one;
        }

        private void StartTickCoroutine()
        {
            if (_tickCoroutine != null) return;
            _tickCoroutine = StartCoroutine(TickRoutine());
        }

        private IEnumerator TickRoutine()
        {
            while (GameManager.Instance.RemainingTime > 0f && GameManager.Instance.State == GameState.Playing)
            {
                AudioManager.Instance?.PlayTimerTick();
                yield return new WaitForSeconds(1f);
            }
            _tickCoroutine = null;
        }

        private IEnumerator LowTimePulse()
        {
            while (GameManager.Instance.RemainingTime > 0f && GameManager.Instance.State == GameState.Playing)
            {
                if (_notificationContainer != null && _notificationText != null)
                {
                    _notificationText.text = Mathf.CeilToInt(GameManager.Instance.RemainingTime).ToString();
                    _notificationText.color = Color.red;
                    _notificationText.fontSize = 48;
                    ShowNotification();
                }
                yield return new WaitForSeconds(1f);
            }
            _lowTimeCoroutine = null;
        }

        private void ShowCountdown(int tick)
        {
            if (_countdownText == null || _countdownContainer == null) return;

            _countdownContainer.SetActive(true);

            if (tick > 0)
            {
                AudioManager.Instance?.PlayCountdownTick();
                _countdownText.text = tick.ToString();
                _countdownText.transform.localScale = Vector3.one * 1.2f;
                _countdownText.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
            }
            else if (tick == 0)
            {
                AudioManager.Instance?.PlayCountdownGo();
                _countdownText.text = "برو!";
                _countdownText.transform.localScale = Vector3.one * 1.5f;
                _countdownText.transform.DOScale(Vector3.one * 0.8f, 0.3f).SetEase(Ease.OutBack);
                UIManager.Instance?.ScreenShake(0.15f, 5f);
                StartCoroutine(HideCountdownAfterDelay(0.5f));
            }
        }

        private IEnumerator HideCountdownAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (_countdownContainer != null)
                _countdownContainer.SetActive(false);
        }

        public void ShowFloatingText(string text, Color color, Vector2 screenPos)
        {
            if (_floatingTextPrefab == null || _floatingCanvas == null) return;

            RtlText floating = Instantiate(_floatingTextPrefab, _floatingCanvas.transform);
            floating.text = text;
            floating.color = color;

            Vector2 center = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            floating.rectTransform.anchoredPosition = (screenPos - center) / _floatingCanvas.scaleFactor;

            CanvasGroup cg = floating.GetComponent<CanvasGroup>();
            if (cg == null) cg = floating.gameObject.AddComponent<CanvasGroup>();

            floating.rectTransform.DOAnchorPosY(100f, 2f).SetRelative().SetEase(Ease.OutCubic);
            cg.DOFade(0f, 0.6f).SetDelay(0.2f).OnComplete(() => Destroy(floating.gameObject));
        }

        public void ScreenShake(float? duration = null, float? strength = null)
        {
            if (_mainCamera == null) return;
            float d = duration ?? _shakeDuration;
            float s = strength ?? _shakeStrength;
            _mainCamera.transform.DOComplete();
            _mainCamera.transform.DOShakePosition(d, s, 20, 90f)
                .OnComplete(() =>
                {
                    if (_mainCamera != null)
                        _mainCamera.transform.position = _cameraInitialPos;
                });
        }

        public void FlashRed()
        {
            if (_redFlashImage == null) return;
            _redFlashImage.color = new Color(1, 0, 0, 0.4f);
            _redFlashImage.DOFade(0f, _flashDuration).SetEase(Ease.OutCubic);
            if (_goldFlashImage == null) return;
            _goldFlashImage.color=   new Color(1, 0, 0, 1);
            _goldFlashImage.color = Color.red;
            _goldFlashImage.DOFade(0f, 1f).SetEase(Ease.Flash);
        }

        public void FlashGreen()
        {
            if (_greenFlashImage == null) return;
            _greenFlashImage.color = new Color(0, 1, 0, 0.2f);
            _greenFlashImage.DOFade(0f, 0.25f).SetEase(Ease.OutCubic);
        }

        public void FlashGold()
        {
            if (_goldFlashImage == null) return;
            _goldFlashImage.color=   new Color(1, 0, 0, 1);
            _goldFlashImage.color = Color.green;
            _goldFlashImage.DOFade(0f, 0.5f).SetEase(Ease.Flash);
        }

        private void OnGoldHit()
        {
            FlashGold();
            UpdateMissCount();
        }

        private void OnPenaltyHit(int count)
        {
            UpdateMissCount();
        }

        private void OnConsecutiveWrongsChanged(int value)
        {
            UpdateMissCount();
        }

        private void UpdateMissCount()
        {
            if (_missCountText == null) return;
            var gm = GameManager.Instance;
            if (gm != null)
                _missCountText.text = $"تعداد اشتباه: {gm.Misses}";
        }

        public void PlayHighScoreCelebration()
        {
            FlashGreen();
            ScreenShake(0.3f, 5f);

            if (_celebrationParticles != null)
            {
                Vector3 center = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
                var ps = Instantiate(_celebrationParticles, center, Quaternion.identity);
                ps.Play();
                Destroy(ps.gameObject, ps.main.duration + 1f);
            }
        }

        private void OnComboChanged(int combo)
        {
            if (_comboText == null) return;

            if (combo > 1)
            {
                _comboText.text = $"x{combo}";
                _comboText.transform.DOKill();
                _comboText.transform.localScale = Vector3.one * 0.8f;
                _comboText.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
                AudioManager.Instance?.PlayCombo();
            }
            else
            {
                _comboText.text = "";
            }
        }

        private void OnDifficultyStepChanged(int step)
        {
            if (_difficultyText == null || step < 0) return;
            _difficultyText.text = $"سطح {step}";
            _difficultyText.transform.DOKill();
            _difficultyText.transform.localScale = Vector3.one * 0.7f;
            _difficultyText.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);

            AudioManager.Instance?.PlayLevelUp();

            if (_levelUpContainer != null && _levelUpText != null)
            {
                _levelUpText.text = $"سطح {step}!";
                _levelUpContainer.SetActive(true);
                _levelUpContainer.transform.localScale = Vector3.one * 1.3f;
                _levelUpContainer.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack)
                    .OnComplete(() => StartCoroutine(HideLevelUp()));
            }
        }

        private IEnumerator HideLevelUp()
        {
            yield return new WaitForSeconds(1.2f);
            if (_levelUpContainer != null)
            {
                _levelUpContainer.transform.DOScale(Vector3.one * 0.8f, 0.2f).SetEase(Ease.InBack)
                    .OnComplete(() => { if (_levelUpContainer != null) _levelUpContainer.SetActive(false); });
            }
        }

        private void ShowNotification()
        {
            if (_notificationContainer == null || _notificationText == null) return;
            _notificationContainer.SetActive(true);
            _notificationContainer.transform.DOKill();
            _notificationContainer.transform.localScale = Vector3.one * 1.1f;
            _notificationContainer.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack)
                .OnComplete(() => StartCoroutine(HideNotification()));
        }

        private IEnumerator HideNotification()
        {
            yield return new WaitForSeconds(1.5f);
            if (_notificationContainer != null)
            {
                _notificationContainer.transform.DOKill();
                _notificationContainer.transform.DOScale(Vector3.one * 0.8f, 0.2f).SetEase(Ease.InBack)
                    .OnComplete(() => { if (_notificationContainer != null) _notificationContainer.SetActive(false); });
            }
        }
    }
}
