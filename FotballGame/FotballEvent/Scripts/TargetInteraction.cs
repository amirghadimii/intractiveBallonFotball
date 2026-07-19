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

        [Header("Idle Animation")]
        [SerializeField] private float _idleDelay = 3f;
        [SerializeField] private float _idlePulseScale = 1.08f;
        [SerializeField] private float _idlePulseDuration = 0.5f;

        [Header("Wave Movement")]
        [SerializeField] private float _waveAmplitudePerStep = 2f;
        [SerializeField] private float _waveFrequency = 1.2f;

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

        private float _lastHitTime;
        private Tween _idleTween;
        private bool _isIdling;

        private Tween _waveTween;
        private Vector2 _baseAnchoredPosition;
        public float WaveAmplitude { get; private set; }

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
            _lastHitTime = Time.time;
            _baseAnchoredPosition = _rectTransform.anchoredPosition;

            if (_targetType == TargetType.Gold)
                StartPulse();
            else
                AnimateEntry();

            SetupWave();
            var gm = GameManager.Instance;
            if (gm != null)
                gm.OnDifficultyStepChanged += OnDifficultyStepChanged;
        }

        private void OnDestroy()
        {
            _pulseTween?.Kill();
            _idleTween?.Kill();
            _waveTween?.Kill();
            if (GameManager.Instance != null)
                GameManager.Instance.OnDifficultyStepChanged -= OnDifficultyStepChanged;
        }

        private void Update()
        {
            if (!_isActive || _targetType != TargetType.Gold) return;
            if (_isHovering) return;

            if (!_isIdling && Time.time - _lastHitTime > _idleDelay)
                StartIdlePulse();
        }

        private void OnDifficultyStepChanged(int step)
        {
            SetupWave();
        }

        private void SetupWave()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;

            int step = gm.CurrentDifficultyStep;
            float amp = step * _waveAmplitudePerStep;

            if (_spawner != null && _spawner.GoalArea != null && _targetType == TargetType.Gold)
            {
                Rect gb = _spawner.GetGoalBoundsInAnchoredSpace();
                Vector2 cp = _spawner.ClusterParent.anchoredPosition;
                float halfH = _rectTransform.sizeDelta.y * 0.5f * _rectTransform.localScale.y;
                float maxUp = gb.yMax - cp.y - halfH - 5f;
                float maxDn = cp.y - gb.yMin - halfH - 5f;
                float maxAmp = Mathf.Min(maxUp, maxDn);
                if (maxAmp < 0.5f) maxAmp = 0.5f;
                amp = Mathf.Min(amp, maxAmp);
            }

            WaveAmplitude = amp;

            if (amp < 0.5f)
            {
                _waveTween?.Kill();
                _rectTransform.anchoredPosition = _baseAnchoredPosition;
                return;
            }

            _waveTween?.Kill();
            _rectTransform.anchoredPosition = new Vector2(_baseAnchoredPosition.x, _baseAnchoredPosition.y - amp);
            _waveTween = _rectTransform.DOAnchorPosY(_baseAnchoredPosition.y + amp, _waveFrequency)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        private void StartPulse()
        {
            _pulseTween?.Kill();
            _pulseTween = _rectTransform.DOScale(_baseScale * _pulseScale, _pulseDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        private void StartIdlePulse()
        {
            _isIdling = true;
            _pulseTween?.Kill();
            _idleTween?.Kill();
            _idleTween = _rectTransform.DOScale(_baseScale * _idlePulseScale, _idlePulseDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        private void StopIdlePulse()
        {
            if (!_isIdling) return;
            _isIdling = false;
            _idleTween?.Kill();
            _lastHitTime = Time.time;
            if (_isActive && _targetType == TargetType.Gold && !_isHovering)
                StartPulse();
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
            StopIdlePulse();
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
            StopIdlePulse();

            GameManager.Instance.AddScore(_scoreValue);
            GameManager.Instance.RecordGoldHit();
            AudioManager.Instance?.PlayGoldHit();
            UIManager.Instance?.ShowFloatingText(
                $"+{_scoreValue}", new Color(0.298f, 0.686f, 0.314f), clickPos
            );
            PlayParticles(_successParticles, clickPos);

            _rectTransform.DOScale(_baseScale * _pulseScale * 1.3f, 0.15f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    _spawner?.MoveCluster(clickPos);
                    Reactivate();
                });
        }

        private void OnPenaltyHit(Vector2 clickPos)
        {
            _lastHitTime = Time.time;
            GameManager.Instance.AddScore(_scoreValue);
            GameManager.Instance.RecordPenaltyHit();
            AudioManager.Instance?.PlayPenaltyHit();
            UIManager.Instance?.ShowFloatingText(
                $"{_scoreValue}", new Color(0.957f, 0.263f, 0.212f), clickPos
            );
            PlayParticles(_failParticles, clickPos);
            UIManager.Instance?.ScreenShake();
            UIManager.Instance?.FlashRed();

            _rectTransform.DOShakeScale(0.2f, 0.3f, 10, 90);
        }

        private void PlayParticles(ParticleSystem ps, Vector2 screenPos)
        {
            if (ps == null) return;

            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return;

            RectTransform canvasRect = canvas.transform as RectTransform;
            Vector2 canvasPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, canvas.worldCamera, out canvasPos);

            var instance = Instantiate(ps, canvas.transform);
            var rt = instance.GetComponent<RectTransform>();
            if (rt != null)
                rt.anchoredPosition = canvasPos;
            else
                instance.transform.localPosition = canvasPos;

            var main = instance.main;
            main.startSize = new ParticleSystem.MinMaxCurve(8f, 16f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(10f, 25f);
            main.startLifetime = new ParticleSystem.MinMaxCurve(1.5f, 3f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);

            var emission = instance.emission;
            emission.SetBursts(new ParticleSystem.Burst[] {
                new ParticleSystem.Burst(0f, new ParticleSystem.MinMaxCurve(30, 45))
            });

            var shape = instance.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.5f;

            instance.Play();
            Destroy(instance.gameObject, main.duration + 2f);
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
            _lastHitTime = Time.time;
            _baseAnchoredPosition = _rectTransform.anchoredPosition;
            _waveTween?.Kill();
            SetupWave();
            if (_targetType == TargetType.Gold)
                StartPulse();
        }
    }
}
