using UnityEngine;
using DG.Tweening;

namespace GoalRush
{
    public class MenuBallBounce : MonoBehaviour
    {
        [SerializeField] private RectTransform _ball;
        [SerializeField] private float _bounceHeight = 30f;
        [SerializeField] private float _duration = 1.2f;
        [SerializeField] private Ease _ease = Ease.InOutSine;

        private Tween _tween;

        private void Awake()
        {
            if (_ball == null)
                _ball = GetComponent<RectTransform>();
        }

        private void OnEnable()
        {
            StartBounce();
        }

        private void OnDisable()
        {
            _tween?.Kill();
        }

        public void StartBounce()
        {
            _tween?.Kill();
            Vector2 startPos = _ball.anchoredPosition;
            _tween = _ball.DOAnchorPosY(startPos.y + _bounceHeight, _duration)
                .SetEase(_ease)
                .SetLoops(-1, LoopType.Yoyo);
        }
    }
}
