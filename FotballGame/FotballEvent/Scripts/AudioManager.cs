using UnityEngine;

namespace GoalRush
{
    public class AudioManager : MonoBehaviour
    {
        [Header("Audio Sources")]
        [SerializeField] private AudioSource _sfxSource;

        [Header("Sound Clips")]
        [SerializeField] private AudioClip _goldHitClip;
        [SerializeField] private AudioClip _penaltyHitClip;
        [SerializeField] private AudioClip _buttonClickClip;
        [SerializeField] private AudioClip _countdownTickClip;
        [SerializeField] private AudioClip _gameOverClip;

        public static AudioManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void PlayGoldHit()
        {
            Play(_goldHitClip);
        }

        public void PlayPenaltyHit()
        {
            Play(_penaltyHitClip);
        }

        public void PlayButtonClick()
        {
            Play(_buttonClickClip);
        }

        public void PlayCountdownTick()
        {
            Play(_countdownTickClip);
        }

        public void PlayGameOver()
        {
            Play(_gameOverClip);
        }

        private void Play(AudioClip clip)
        {
            if (clip == null || _sfxSource == null) return;
            _sfxSource.PlayOneShot(clip);
        }
    }
}
