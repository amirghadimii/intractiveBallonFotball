using UnityEngine;

namespace GoalRush
{
    public class AudioManager : MonoBehaviour
    {
        [Header("Audio Sources")]
        [SerializeField] private AudioSource _sfxSource;
        [SerializeField] private AudioSource _musicSource;

        [Header("Sound Clips")]
        [SerializeField] private AudioClip _goldHitClip;
        [SerializeField] private AudioClip _penaltyHitClip;
        [SerializeField] private AudioClip _buttonClickClip;
        [SerializeField] private AudioClip _countdownTickClip;
        [SerializeField] private AudioClip _countdownGoClip;
        [SerializeField] private AudioClip _gameOverClip;
        [SerializeField] private AudioClip _levelUpClip;
        [SerializeField] private AudioClip _comboClip;
        [SerializeField] private AudioClip _lowTimeClip;
        [SerializeField] private AudioClip _timerTickClip;

        [Header("Music")]
        [SerializeField] private AudioClip _bgmClip;
        [SerializeField] private bool _loopBgm = true;

        [Header("Settings")]
        [SerializeField, Range(0f, 1f)] private float _sfxVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float _musicVolume = 0.5f;
        [SerializeField, Range(0f, 0.5f)] private float _pitchVariation = 0.12f;

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

        private void Start()
        {
            if (_musicSource != null && _bgmClip != null)
            {
                _musicSource.clip = _bgmClip;
                _musicSource.loop = _loopBgm;
                _musicSource.volume = _musicVolume;
                _musicSource.Play();
            }
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

        public void PlayCountdownGo()
        {
            Play(_countdownGoClip);
        }

        public void PlayGameOver()
        {
            Play(_gameOverClip);
            if (_musicSource != null)
                _musicSource.Stop();
            if (_sfxSource != null)
                _sfxSource.Stop();
        }

        public void PlayLevelUp()
        {
            Play(_levelUpClip);
        }

        public void PlayCombo()
        {
            Play(_comboClip);
        }

        public void PlayLowTime()
        {
            Play(_lowTimeClip);
        }

        public void PlayTimerTick()
        {
            Play(_timerTickClip);
        }

        private void Play(AudioClip clip)
        {
            if (clip == null || _sfxSource == null) return;
            float originalPitch = _sfxSource.pitch;
            _sfxSource.pitch = 1f + Random.Range(-_pitchVariation, _pitchVariation);
            _sfxSource.PlayOneShot(clip, _sfxVolume);
            _sfxSource.pitch = originalPitch;
        }
    }
}
