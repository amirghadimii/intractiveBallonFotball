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
        [SerializeField] private int _penaltyStepBonus = -3;

        [Header("Difficulty (Step-based)")]
        [SerializeField] private int _basePenaltyCount = 4;
        [SerializeField] private int _maxPenaltyCount = 10;
        [SerializeField] private int _stepInterval = 20;
        [SerializeField] private float _goldShrinkPerStep = 0.07f;
        [SerializeField] private float _penaltyGrowPerStep = 0.05f;
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

        public int CurrentDifficultyStep
        {
            get
            {
                float elapsed = _gameDuration - RemainingTime;
                return Mathf.FloorToInt(elapsed / _stepInterval);
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
            int step = CurrentDifficultyStep;
            int min = _goldScoreMin + (step * 3);
            int max = _goldScoreMax + (step * 3);
            return Random.Range(min, max + 1);
        }

        public int PenaltyTargetScore
        {
            get
            {
                return _penaltyBaseScore + (CurrentDifficultyStep * _penaltyStepBonus);
            }
        }

        public float GoldTargetScale
        {
            get
            {
                return Mathf.Max(_goldMinScale, 1f - (CurrentDifficultyStep * _goldShrinkPerStep));
            }
        }

        public float PenaltyTargetScale
        {
            get
            {
                return Mathf.Min(_penaltyMaxScale, 1f + (CurrentDifficultyStep * _penaltyGrowPerStep));
            }
        }

        public System.Action<int> OnScoreChanged;
        public System.Action<float> OnTimerUpdated;
        public System.Action<GameState> OnStateChanged;
        public System.Action<int> OnCountdownTick;

        private Coroutine _countdownCoroutine;
        private Coroutine _gameTimerCoroutine;

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
                OnTimerUpdated?.Invoke(RemainingTime);
                yield return null;
            }

            RemainingTime = 0f;
            OnTimerUpdated?.Invoke(0f);
            EndGame();
        }

        private void EndGame()
        {
            State = GameState.GameOver;
            OnStateChanged?.Invoke(State);
        }

        public void AddScore(int amount)
        {
            Score += amount;
            OnScoreChanged?.Invoke(Score);
        }

        public void ResetGame()
        {
            if (_gameTimerCoroutine != null) StopCoroutine(_gameTimerCoroutine);
            Score = 0;
            RemainingTime = _gameDuration;
            OnScoreChanged?.Invoke(0);
            OnTimerUpdated?.Invoke(_gameDuration);
            State = GameState.Menu;
            OnStateChanged?.Invoke(State);
        }
    }
}
