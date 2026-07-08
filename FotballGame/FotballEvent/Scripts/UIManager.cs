using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Collections;
using UnityEngine.UI;

namespace GoalRush
{
    public class UIManager : MonoBehaviour
    {
        [Header("HUD")]
        [SerializeField] private GameObject _hudContainer;
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private TextMeshProUGUI _timerText;
        [SerializeField] private TextMeshProUGUI _comboText;
        [SerializeField] private TextMeshProUGUI _difficultyText;

        [Header("Menu")]
        [SerializeField] private GameObject _menuContainer;
        [SerializeField] private TextMeshProUGUI _menuTitleText;
        [SerializeField] private TextMeshProUGUI _menuHighScoreText;
        [SerializeField] private Button _startButton;

        [Header("Countdown")]
        [SerializeField] private GameObject _countdownContainer;
        [SerializeField] private TextMeshProUGUI _countdownText;

        [Header("Floating Text")]
        [SerializeField] private TextMeshProUGUI _floatingTextPrefab;
        [SerializeField] private Canvas _floatingCanvas;

        [Header("Screen Effects")]
        [SerializeField] private Image _redFlashImage;
        [SerializeField] private Image _greenFlashImage;
        [SerializeField] private float _flashDuration = 0.3f;

        [Header("Camera Shake")]
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private float _shakeDuration = 0.2f;
        [SerializeField] private float _shakeStrength = 10f;

        [Header("Game Over")]
        [SerializeField] private GameObject _gameOverContainer;
        [SerializeField] private TextMeshProUGUI _finalScoreText;
        [SerializeField] private TextMeshProUGUI _gameOverTitleText;
        [SerializeField] private TextMeshProUGUI _highScoreText;
        [SerializeField] private TextMeshProUGUI _newHighScoreText;
        [SerializeField] private TextMeshProUGUI _accuracyText;
        [SerializeField] private TextMeshProUGUI _gameOverComboText;
        [SerializeField] private TextMeshProUGUI _gameOverGoldHitsText;
        [SerializeField] private Button _restartButton;

        [Header("Pause")]
        [SerializeField] private GameObject _pauseOverlay;
        [SerializeField] private Button _pauseButton;

        [Header("Notifications")]
        [SerializeField] private GameObject _notificationContainer;
        [SerializeField] private TextMeshProUGUI _notificationText;

        [Header("Difficulty Level Up")]
        [SerializeField] private GameObject _levelUpContainer;
        [SerializeField] private TextMeshProUGUI _levelUpText;

        [Header("Transition")]
        [SerializeField] private float _transitionDuration = 0.3f;

        public static UIManager Instance { get; private set; }

        private Vector3 _cameraInitialPos;
        private int _currentDisplayScore;
        private Tween _scoreTween;
        private bool _isPaused;
        private Coroutine _lowTimeCoroutine;

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

            if (_greenFlashImage != null)
                _greenFlashImage.color = new Color(0, 1, 0, 0);
            if (_redFlashImage != null)
                _redFlashImage.color = new Color(1, 0, 0, 0);

            if (_menuHighScoreText != null)
                _menuHighScoreText.text = $"BEST: {GameManager.Instance.HighScore}";

            if (_startButton != null)
                _startButton.onClick.AddListener(OnStartClicked);

            if (_restartButton != null)
                _restartButton.onClick.AddListener(OnRestartClicked);

            if (_pauseButton != null)
                _pauseButton.onClick.AddListener(TogglePause);

            var gm = GameManager.Instance;
            if (gm != null)
            {
                gm.OnScoreChanged += UpdateScore;
                gm.OnTimerUpdated += UpdateTimer;
                gm.OnCountdownTick += ShowCountdown;
                gm.OnStateChanged += OnGameStateChanged;
                gm.OnComboChanged += OnComboChanged;
                gm.OnDifficultyStepChanged += OnDifficultyStepChanged;
            }

            if (_menuContainer != null)
                _menuContainer.transform.localScale = Vector3.one * 0.9f;
            DOVirtual.DelayedCall(0.05f, () =>
            {
                if (_menuContainer != null)
                    _menuContainer.transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack);
            });
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
            }

            if (_startButton != null)
                _startButton.onClick.RemoveListener(OnStartClicked);
            if (_restartButton != null)
                _restartButton.onClick.RemoveListener(OnRestartClicked);
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
            _isPaused = false;
            Time.timeScale = 1f;

            if (_lowTimeCoroutine != null)
            {
                StopCoroutine(_lowTimeCoroutine);
                _lowTimeCoroutine = null;
            }

            if (state == GameState.Playing)
            {
                _currentDisplayScore = 0;
                _scoreText.text = "0";
                _timerText.color = Color.white;

                if (_comboText != null)
                {
                    _comboText.text = "";
                    _comboText.transform.localScale = Vector3.one;
                }
                if (_difficultyText != null)
                {
                    _difficultyText.text = "LEVEL 0";
                    _difficultyText.transform.localScale = Vector3.one;
                }
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
                _highScoreText.text = $"BEST: {gm.HighScore}";

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
                }
            }

            if (_accuracyText != null)
                _accuracyText.text = $"Accuracy: {gm.Accuracy:F0}%";

            if (_gameOverComboText != null)
                _gameOverComboText.text = $"Best Combo: {gm.Combo}";

            if (_gameOverGoldHitsText != null)
                _gameOverGoldHitsText.text = $"Hits: {gm.GoldHits} / {gm.TotalClicks}";

            _gameOverContainer.transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack);
        }

        private void UpdateScore(int score)
        {
            if (_scoreText == null) return;
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
            _timerText.text = $"{seconds}s";

            if (time <= 30f && time > 10f)
            {
                float t = (time - 10f) / 20f;
                _timerText.color = Color.Lerp(new Color(1, 0.64f, 0), Color.white, t);
                _timerText.transform.DOPunchScale(Vector3.one * 0.05f, 0.5f, 2, 0.3f);

                if (_notificationContainer != null && _notificationText != null && seconds == 30)
                {
                    _notificationText.text = "HURRY UP!";
                    _notificationText.color = new Color(1, 0.64f, 0);
                    ShowNotification();
                }
            }
            else if (time <= 10f && time > 0f)
            {
                _timerText.color = Color.red;
                _timerText.transform.DOPunchScale(Vector3.one * 0.1f, 0.5f, 3, 0.3f);

                if (_lowTimeCoroutine == null)
                {
                    _lowTimeCoroutine = StartCoroutine(LowTimePulse());
                    AudioManager.Instance?.PlayLowTime();
                }
            }
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
                _countdownText.text = "GO!";
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

            TextMeshProUGUI floating = Instantiate(_floatingTextPrefab, _floatingCanvas.transform);
            floating.text = text;
            floating.color = color;
            floating.rectTransform.position = screenPos;

            CanvasGroup cg = floating.GetComponent<CanvasGroup>();
            if (cg == null) cg = floating.gameObject.AddComponent<CanvasGroup>();

            floating.rectTransform.DOAnchorPosY(100f, 0.8f).SetRelative().SetEase(Ease.OutCubic);
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
        }

        public void FlashGreen()
        {
            if (_greenFlashImage == null) return;
            _greenFlashImage.color = new Color(0, 1, 0, 0.2f);
            _greenFlashImage.DOFade(0f, 0.2f).SetEase(Ease.OutCubic);
        }

        private void OnComboChanged(int combo)
        {
            if (_comboText == null) return;

            if (combo > 1)
            {
                _comboText.text = $"COMBO x{combo}";
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
            _difficultyText.text = $"LEVEL {step}";
            _difficultyText.transform.DOKill();
            _difficultyText.transform.localScale = Vector3.one * 0.7f;
            _difficultyText.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);

            AudioManager.Instance?.PlayLevelUp();

            if (_levelUpContainer != null && _levelUpText != null)
            {
                _levelUpText.text = $"LEVEL {step}!";
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
