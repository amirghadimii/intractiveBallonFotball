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

        [Header("Menu")]
        [SerializeField] private GameObject _menuContainer;
        [SerializeField] private TextMeshProUGUI _menuTitleText;
        [SerializeField] private Button _startButton;

        [Header("Countdown")]
        [SerializeField] private GameObject _countdownContainer;
        [SerializeField] private TextMeshProUGUI _countdownText;

        [Header("Floating Text")]
        [SerializeField] private TextMeshProUGUI _floatingTextPrefab;
        [SerializeField] private Canvas _floatingCanvas;

        [Header("Screen Effects")]
        [SerializeField] private Image _redFlashImage;
        [SerializeField] private float _flashDuration = 0.3f;

        [Header("Camera Shake")]
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private float _shakeDuration = 0.2f;
        [SerializeField] private float _shakeStrength = 10f;

        [Header("Game Over")]
        [SerializeField] private GameObject _gameOverContainer;
        [SerializeField] private TextMeshProUGUI _finalScoreText;
        [SerializeField] private TextMeshProUGUI _gameOverTitleText;
        [SerializeField] private Button _restartButton;

        public static UIManager Instance { get; private set; }

        private Vector3 _cameraInitialPos;

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

            if (_redFlashImage != null)
            {
                _redFlashImage.color = new Color(1, 0, 0, 0);
            }

            if (_startButton != null)
                _startButton.onClick.AddListener(OnStartClicked);

            if (_restartButton != null)
                _restartButton.onClick.AddListener(OnRestartClicked);

            var gm = GameManager.Instance;
            if (gm != null)
            {
                gm.OnScoreChanged += UpdateScore;
                gm.OnTimerUpdated += UpdateTimer;
                gm.OnCountdownTick += ShowCountdown;
                gm.OnStateChanged += OnGameStateChanged;
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnScoreChanged -= UpdateScore;
                GameManager.Instance.OnTimerUpdated -= UpdateTimer;
                GameManager.Instance.OnCountdownTick -= ShowCountdown;
                GameManager.Instance.OnStateChanged -= OnGameStateChanged;
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

        private void OnStartClicked()
        {
            AudioManager.Instance?.PlayButtonClick();
            _menuContainer.SetActive(false);
            GameManager.Instance.StartCountdown();
        }

        private void OnRestartClicked()
        {
            AudioManager.Instance?.PlayButtonClick();
            GameManager.Instance.ResetGame();
        }

        private void OnGameStateChanged(GameState state)
        {
            switch (state)
            {
                case GameState.Menu:
                    if (_menuContainer != null) _menuContainer.SetActive(true);
                    if (_hudContainer != null) _hudContainer.SetActive(false);
                    if (_countdownContainer != null) _countdownContainer.SetActive(false);
                    if (_gameOverContainer != null) _gameOverContainer.SetActive(false);
                    break;

                case GameState.Countdown:
                    if (_menuContainer != null) _menuContainer.SetActive(false);
                    if (_hudContainer != null) _hudContainer.SetActive(false);
                    if (_countdownContainer != null) _countdownContainer.SetActive(true);
                    if (_gameOverContainer != null) _gameOverContainer.SetActive(false);
                    break;

                case GameState.Playing:
                    if (_hudContainer != null) _hudContainer.SetActive(true);
                    if (_countdownContainer != null) _countdownContainer.SetActive(false);
                    if (_gameOverContainer != null) _gameOverContainer.SetActive(false);
                    break;

                case GameState.GameOver:
                    ShowGameOver();
                    break;
            }
        }

        private void UpdateScore(int score)
        {
            if (_scoreText != null)
            {
                _scoreText.text = score.ToString();
                _scoreText.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 5, 0.5f);
            }
        }

        private void UpdateTimer(float time)
        {
            if (_timerText == null) return;
            int seconds = Mathf.CeilToInt(time);
            _timerText.text = $"{seconds}s";

            if (time <= 10f)
            {
                _timerText.color = Color.red;
                _timerText.transform.DOPunchScale(Vector3.one * 0.1f, 0.5f, 3, 0.3f);
            }
        }

        private void ShowCountdown(int tick)
        {
            if (_countdownText == null || _countdownContainer == null) return;

            _countdownContainer.SetActive(true);
            AudioManager.Instance?.PlayCountdownTick();

            if (tick > 0)
            {
                _countdownText.text = tick.ToString();
                _countdownText.transform.localScale = Vector3.one * 1.2f;
                _countdownText.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
            }
            else if (tick == 0)
            {
                _countdownText.text = "GO!";
                _countdownText.transform.localScale = Vector3.one * 1.5f;
                _countdownText.transform.DOScale(Vector3.one * 0.8f, 0.3f).SetEase(Ease.OutBack);
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

        public void ScreenShake()
        {
            if (_mainCamera == null) return;
            _mainCamera.transform.DOShakePosition(_shakeDuration, _shakeStrength, 20, 90f)
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

        private void ShowGameOver()
        {
            if (_gameOverContainer == null) return;

            if (_hudContainer != null) _hudContainer.SetActive(false);
            AudioManager.Instance?.PlayGameOver();

            _gameOverContainer.SetActive(true);
            _gameOverContainer.transform.localScale = Vector3.one * 0.8f;

            if (_finalScoreText != null)
                _finalScoreText.text = GameManager.Instance.Score.ToString();

            _gameOverContainer.transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack);
        }
    }
}
