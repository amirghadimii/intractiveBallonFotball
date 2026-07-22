using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;

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
        [SerializeField] private int _goldScoreBase = 6;
        [SerializeField] private int _goldIncreasePerHit = 2;

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
            int score;
            int safety = 0;
            do
            {
                score = Random.Range(_goldScoreBase, _goldScoreBase + 5);
                safety++;
            } while (score == _lastGoldScore && safety < 10);
            _lastGoldScore = score;
            return score;
        }

        public int PenaltyTargetScore
        {
            get
            {
                return -Mathf.Max(3, Mathf.RoundToInt(_goldScoreBase * 0.5f));
            }
        }

        public int[] GetUniquePenaltyScores(int count)
        {
            int[] scores = new int[count];
            int baseVal = PenaltyTargetScore;
            int halfRange = Mathf.Max(1, count / 2);
            for (int i = 0; i < count; i++)
            {
                int candidate;
                bool unique;
                int safety = 0;
                do
                {
                    unique = true;
                    candidate = baseVal + Random.Range(-halfRange, halfRange + 1);
                    if (candidate > -2) candidate = -2;
                    for (int j = 0; j < i; j++)
                    {
                        if (scores[j] == candidate) { unique = false; break; }
                    }
                    safety++;
                } while (!unique && safety < 20);
                scores[i] = candidate;
            }
            return scores;
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
        public int Misses { get; private set; }
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
        public System.Action OnGoldHit;
        public System.Action<int> OnPenaltyHit;
        public System.Action<int> OnConsecutiveWrongsChanged;
        public System.Action OnLeaderboardUpdated;

        public LeaderboardEntry LastPlayerEntry { get; private set; }

        public string PlayerName
        {
            get { return PlayerInfo.LoadName(); }
            set { PlayerInfo.Save(value, TeamIndex); }
        }

        public int TeamIndex
        {
            get { return PlayerInfo.LoadTeamIndex(); }
            set { PlayerInfo.Save(PlayerName, value); }
        }

        private Coroutine _countdownCoroutine;
        private Coroutine _gameTimerCoroutine;
        private int _lastDifficultyStep = -1;
        private int _consecutiveWrongs;

        public int ConsecutiveWrongs { get { return _consecutiveWrongs; } }
        private int _lastGoldScore = -1;
        private int _lastEmptyClickFrame = -1;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        private void Update()
        {
            if (State != GameState.Playing) return;
            if (!Input.GetMouseButtonDown(0)) return;

            var ped = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(ped, results);

            foreach (var r in results)
            {
                if (r.gameObject.GetComponent<TargetInteraction>() != null) return;
                var handler = r.gameObject.GetComponent<IPointerClickHandler>();
                if (handler != null && !(handler is BackgroundClickHandler)) return;
            }

            if (_lastEmptyClickFrame != Time.frameCount)
            {
                _lastEmptyClickFrame = Time.frameCount;
                ProcessEmptyClick();
            }
        }

        public void HandleBackgroundClick()
        {
            if (_lastEmptyClickFrame == Time.frameCount) return;
            _lastEmptyClickFrame = Time.frameCount;
            ProcessEmptyClick();
        }

        private void ProcessEmptyClick()
        {
            RecordEmptyClick();
            AudioManager.Instance?.PlayPenaltyHit();
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ScreenShake();
                UIManager.Instance.FlashRed();
            }
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
            Debug.Log($"EndGame called. Score={Score}, TotalClicks={TotalClicks}, GoldHits={GoldHits}");
            IsNewHighScore = Score > HighScore;
            if (IsNewHighScore)
                HighScore = Score;

            SaveScoreToLeaderboard();

            State = GameState.GameOver;
            OnStateChanged?.Invoke(State);
        }

        public void SaveScoreToLeaderboard()
        {
            Debug.Log($"SaveScoreToLeaderboard: TotalClicks={TotalClicks}, GoldHits={GoldHits}, Score={Score}");
            if (TotalClicks == 0)
            {
                Debug.Log("SaveScoreToLeaderboard: skipped (TotalClicks == 0)");
                return;
            }
            var data = LeaderboardData.Load();
            var entry = new LeaderboardEntry
            {
                playerName = PlayerName,
                teamIndex = TeamIndex,
                score = Score,
                goldHits = GoldHits,
                totalClicks = TotalClicks,
                accuracy = Accuracy,
                date = System.DateTime.Now.ToString("yyyy-MM-dd")
            };
            data.AddEntry(entry);
            LeaderboardData.Save(data);
            LastPlayerEntry = entry;
            Debug.Log($"Leaderboard saved: {entry.playerName} | Score: {entry.score} | Team: {entry.teamIndex}");
            OnLeaderboardUpdated?.Invoke();
        }

        public LeaderboardData GetLeaderboardData()
        {
            return LeaderboardData.Load();
        }

        public void ClearLeaderboardData()
        {
            LeaderboardData.Clear();
            OnLeaderboardUpdated?.Invoke();
        }

        public void ResetAllData()
        {
            ClearLeaderboardData();
            PlayerInfo.Clear();
            PlayerPrefs.DeleteKey("GoalRush_HighScore");
            PlayerPrefs.Save();
            ResetGame();
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
            OnGoldHit?.Invoke();

            _consecutiveWrongs = 0;
            OnConsecutiveWrongsChanged?.Invoke(0);
            _goldScoreBase += _goldIncreasePerHit;

            int bonus = (Combo > 1 ? Combo * _comboBonusPerStep : 0);
            if (bonus > 0)
                AddScore(bonus);
        }

        public void RecordPenaltyHit()
        {
            PenaltyHits++;
            Misses++;
            OnPenaltyHit?.Invoke(PenaltyHits);
            _consecutiveWrongs++;
            OnConsecutiveWrongsChanged?.Invoke(_consecutiveWrongs);
            if (Combo > 0)
            {
                Combo = 0;
                OnComboChanged?.Invoke(0);
            }
        }

        public void RecordEmptyClick()
        {
            TotalClicks++;
            Misses++;
            _consecutiveWrongs++;
            OnConsecutiveWrongsChanged?.Invoke(_consecutiveWrongs);
        }

        public void ResetConsecutiveWrongs()
        {
            _consecutiveWrongs = 0;
            OnConsecutiveWrongsChanged?.Invoke(0);
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
            Misses = 0;
            IsNewHighScore = false;
            _lastDifficultyStep = -1;
            _consecutiveWrongs = 0;
            _lastGoldScore = -1;
            _goldScoreBase = 6;
            LastPlayerEntry = null;
            RemainingTime = _gameDuration;
            OnScoreChanged?.Invoke(0);
            OnTimerUpdated?.Invoke(_gameDuration);
            State = GameState.Menu;
            OnStateChanged?.Invoke(State);
        }
    }
}
