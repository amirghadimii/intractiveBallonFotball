using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

namespace GoalRush
{
    public enum TargetType
    {
        Gold,
        Penalty
    }

    public class TargetInteraction : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Target Settings")]
        [SerializeField] private TargetType _targetType = TargetType.Gold;
        [SerializeField] private int _scoreValue = 25;

        [Header("Visual Feedback")]
        [SerializeField] private ParticleSystem _successParticles;
        [SerializeField] private ParticleSystem _failParticles;

        [Header("Gold Pulse")]
        [SerializeField] private float _pulseScale = 1.05f;
        [SerializeField] private float _pulseDuration = 0.75f;

        [Header("Hover")]
        [SerializeField] private float _hoverScale = 1.15f;
        [SerializeField] private float _hoverDuration = 0.15f;

        private RectTransform _rectTransform;
        private CanvasGroup _canvasGroup;
        private bool _isActive = true;
        private TargetSpawner _spawner;
        private Tween _pulseTween;
        private Vector3 _baseScale = Vector3.one;
        private bool _isHovering;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        private void Start()
        {
            _baseScale = _rectTransform.localScale;
            if (_targetType == TargetType.Gold)
                StartPulse();
            else
                AnimateEntry();
        }

        private void OnDestroy()
        {
            _pulseTween?.Kill();
        }

        private void StartPulse()
        {
            _pulseTween?.Kill();
            _pulseTween = _rectTransform.DOScale(_baseScale * _pulseScale, _pulseDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        private void AnimateEntry()
        {
            _rectTransform.localScale = Vector3.zero;
            _rectTransform.DOScale(_baseScale, 0.3f).SetEase(Ease.OutBack);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!_isActive) return;
            _isHovering = true;
            _pulseTween?.Pause();
            _rectTransform.DOScale(_baseScale * _hoverScale, _hoverDuration).SetEase(Ease.OutQuad);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isHovering = false;
            _rectTransform.DOScale(_baseScale, _hoverDuration).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                if (_isActive && _targetType == TargetType.Gold && !_isHovering)
                    StartPulse();
            });
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!_isActive) return;
            var gm = GameManager.Instance;
            if (gm == null || gm.State != GameState.Playing) return;

            gm.RecordClick();
            if (_targetType == TargetType.Gold)
                OnGoldHit(eventData.position);
            else
                OnPenaltyHit(eventData.position);
        }

        private void OnGoldHit(Vector2 clickPos)
        {
            _isActive = false;
            _isHovering = false;
            _pulseTween?.Kill();

            GameManager.Instance.AddScore(_scoreValue);
            GameManager.Instance.RecordGoldHit();
            AudioManager.Instance?.PlayGoldHit();
            UIManager.Instance?.ShowFloatingText(
                $"+{_scoreValue}", new Color(0.298f, 0.686f, 0.314f), clickPos
            );
            PlayParticles(_successParticles);

            _rectTransform.DOScale(_baseScale * _pulseScale * 1.3f, 0.15f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    _spawner?.MoveCluster();
                    Reactivate();
                });
        }

        private void OnPenaltyHit(Vector2 clickPos)
        {
            GameManager.Instance.AddScore(_scoreValue);
            GameManager.Instance.RecordPenaltyHit();
            AudioManager.Instance?.PlayPenaltyHit();
            UIManager.Instance?.ShowFloatingText(
                $"{_scoreValue}", new Color(0.957f, 0.263f, 0.212f), clickPos
            );
            PlayParticles(_failParticles);
            UIManager.Instance?.ScreenShake();
            UIManager.Instance?.FlashRed();

            _rectTransform.DOShakeScale(0.2f, 0.3f, 10, 90);
        }

        private void PlayParticles(ParticleSystem ps)
        {
            if (ps == null) return;
            var instance = Instantiate(ps, transform.position, Quaternion.identity);
            instance.Play();
            Destroy(instance.gameObject, instance.main.duration + 0.5f);
        }

        public void Setup(TargetType type, int scoreValue, TargetSpawner spawner = null)
        {
            _targetType = type;
            _scoreValue = scoreValue;
            _spawner = spawner;
        }

        public void Reactivate()
        {
            _isActive = true;
            _baseScale = transform.localScale;
            if (_targetType == TargetType.Gold)
                StartPulse();
        }
    }
}
