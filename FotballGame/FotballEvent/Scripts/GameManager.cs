using UnityEngine;
using System.Collections;

namespace GoalRush
{
    public enum GameState
    {
        Menu,
        Countdown,
        Playing,
        GameOver
    }

    public class GameManager : MonoBehaviour
    {
        [Header("Game Settings")]
        [SerializeField] private float _gameDuration = 120f;
        [SerializeField] private float _countdownInterval = 0.8f;

        [Header("Scoring")]
        [SerializeField] private int _goldScoreMin = 15;
        [SerializeField] private int _goldScoreMax = 35;
        [SerializeField] private int _penaltyBaseScore = -15;
        [SerializeField] private float _goldScorePerSec = 0.25f;
        [SerializeField] private float _penaltyScorePerSec = 0.2f;

        [Header("Combo")]
        [SerializeField] private int _comboBonusPerStep = 2;

        [Header("Difficulty")]
        [SerializeField] private int _basePenaltyCount = 4;
        [SerializeField] private int _maxPenaltyCount = 10;
        [SerializeField] private float _penaltyStepInterval = 20f;
        [SerializeField] private int _scorePerStep = 100;
        [SerializeField] private float _goldShrinkPerSec = 0.004f;
        [SerializeField] private float _penaltyGrowPerSec = 0.003f;
        [SerializeField] private float _goldMinScale = 0.5f;
        [SerializeField] private float _penaltyMaxScale = 1.5f;

        private static GameManager _instance;
        public static GameManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Object.FindFirstObjectByType<GameManager>();
                    if (_instance == null)
                        Debug.LogError("GoalRush.GameManager not found in scene!");
                }
                return _instance;
            }
        }

        public GameState State { get; private set; } = GameState.Menu;
        public int Score { get; private set; }
        public float RemainingTime { get; private set; }

        public float ElapsedTime
        {
            get { return _gameDuration - RemainingTime; }
        }

        public int CurrentDifficultyStep
        {
            get
            {
                int scoreStep = Score / _scorePerStep;
                int timeStep = Mathf.FloorToInt(ElapsedTime / (_penaltyStepInterval * 2f));
                return scoreStep + timeStep;
            }
        }

        public int CurrentPenaltyCount
        {
            get
            {
                return Mathf.Min(_basePenaltyCount + CurrentDifficultyStep, _maxPenaltyCount);
            }
        }

        public int GetRandomGoldScore()
        {
            int bonus = Mathf.FloorToInt(ElapsedTime * _goldScorePerSec);
            int min = _goldScoreMin + bonus;
            int max = _goldScoreMax + bonus;
            return Random.Range(min, max + 1);
        }

        public int PenaltyTargetScore
        {
            get
            {
                int bonus = Mathf.FloorToInt(ElapsedTime * _penaltyScorePerSec);
                return _penaltyBaseScore - bonus;
            }
        }

        public float GoldTargetScale
        {
            get
            {
                return Mathf.Max(_goldMinScale, 1f - (ElapsedTime * _goldShrinkPerSec));
            }
        }

        public float PenaltyTargetScale
        {
            get
            {
                return Mathf.Min(_penaltyMaxScale, 1f + (ElapsedTime * _penaltyGrowPerSec));
            }
        }

        public int Combo { get; private set; }
        public int HighScore
        {
            get { return PlayerPrefs.GetInt("GoalRush_HighScore", 0); }
            private set { PlayerPrefs.SetInt("GoalRush_HighScore", value); PlayerPrefs.Save(); }
        }
        public bool IsNewHighScore { get; private set; }
        public int TotalClicks { get; private set; }
        public int GoldHits { get; private set; }
        public int PenaltyHits { get; private set; }
        public float Accuracy
        {
            get { return TotalClicks > 0 ? (float)GoldHits / TotalClicks * 100f : 0f; }
        }

        public System.Action<int> OnScoreChanged;
        public System.Action<float> OnTimerUpdated;
        public System.Action<GameState> OnStateChanged;
        public System.Action<int> OnCountdownTick;
        public System.Action<int> OnComboChanged;
        public System.Action<int> OnDifficultyStepChanged;

        private Coroutine _countdownCoroutine;
        private Coroutine _gameTimerCoroutine;
        private int _lastDifficultyStep = -1;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        public void StartCountdown()
        {
            if (_countdownCoroutine != null) StopCoroutine(_countdownCoroutine);
            _countdownCoroutine = StartCoroutine(CountdownRoutine());
        }

        private IEnumerator CountdownRoutine()
        {
            State = GameState.Countdown;
            OnStateChanged?.Invoke(State);
            RemainingTime = _gameDuration;

            for (int i = 3; i > 0; i--)
            {
                OnCountdownTick?.Invoke(i);
                yield return new WaitForSeconds(_countdownInterval);
            }

            OnCountdownTick?.Invoke(0);
            yield return new WaitForSeconds(_countdownInterval * 0.5f);

            BeginPlaying();
        }

        private void BeginPlaying()
        {
            _lastDifficultyStep = -1;
            State = GameState.Playing;
            OnStateChanged?.Invoke(State);

            if (_gameTimerCoroutine != null) StopCoroutine(_gameTimerCoroutine);
            _gameTimerCoroutine = StartCoroutine(GameTimerRoutine());
        }

        private IEnumerator GameTimerRoutine()
        {
            while (RemainingTime > 0f)
            {
                RemainingTime -= Time.deltaTime;

                int step = CurrentDifficultyStep;
                if (step != _lastDifficultyStep)
                {
                    _lastDifficultyStep = step;
                    OnDifficultyStepChanged?.Invoke(step);
                }

                OnTimerUpdated?.Invoke(RemainingTime);
                yield return null;
            }

            RemainingTime = 0f;
            OnTimerUpdated?.Invoke(0f);
            EndGame();
        }

        private void EndGame()
        {
            IsNewHighScore = Score > HighScore;
            if (IsNewHighScore)
                HighScore = Score;

            State = GameState.GameOver;
            OnStateChanged?.Invoke(State);
        }

        public void AddScore(int amount)
        {
            Score += amount;
            OnScoreChanged?.Invoke(Score);
        }

        public void RecordClick()
        {
            TotalClicks++;
        }

        public void RecordGoldHit()
        {
            GoldHits++;
            Combo++;
            OnComboChanged?.Invoke(Combo);

            int bonus = (Combo > 1 ? Combo * _comboBonusPerStep : 0);
            if (bonus > 0)
                AddScore(bonus);
        }

        public void RecordPenaltyHit()
        {
            PenaltyHits++;
            if (Combo > 0)
            {
                Combo = 0;
                OnComboChanged?.Invoke(0);
            }
        }

        public void ResetCombo()
        {
            if (Combo > 0)
            {
                Combo = 0;
                OnComboChanged?.Invoke(0);
            }
        }

        public void ResetGame()
        {
            if (_gameTimerCoroutine != null) StopCoroutine(_gameTimerCoroutine);
            Score = 0;
            Combo = 0;
            TotalClicks = 0;
            GoldHits = 0;
            PenaltyHits = 0;
            IsNewHighScore = false;
            _lastDifficultyStep = -1;
            RemainingTime = _gameDuration;
            OnScoreChanged?.Invoke(0);
            OnTimerUpdated?.Invoke(_gameDuration);
            State = GameState.Menu;
            OnStateChanged?.Invoke(State);
        }
    }
}
