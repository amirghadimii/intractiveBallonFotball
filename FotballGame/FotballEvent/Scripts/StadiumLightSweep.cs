using UnityEngine;
using DG.Tweening;

namespace GoalRush
{
    public class StadiumLightSweep : MonoBehaviour
    {
        [SerializeField] private RectTransform _lightBeam;
        [SerializeField] private float _angle = 45f;
        [SerializeField] private float _duration = 3f;
        [SerializeField] private Ease _ease = Ease.InOutSine;

        private Tween _tween;

        private void Awake()
        {
            if (_lightBeam == null)
                _lightBeam = GetComponent<RectTransform>();
        }

        private void OnEnable()
        {
            PlaySweep();
        }

        private void OnDisable()
        {
            _tween?.Kill();
        }

        public void PlaySweep()
        {
            _tween?.Kill();
            _tween = _lightBeam.DORotate(new Vector3(0, 0, _angle), _duration, RotateMode.LocalAxisAdd)
                .SetEase(_ease)
                .SetLoops(-1, LoopType.Yoyo);
        }
    }
}
