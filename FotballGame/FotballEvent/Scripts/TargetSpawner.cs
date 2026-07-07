using UnityEngine;
using System.Collections.Generic;
using TMPro;

namespace GoalRush
{
    public class TargetSpawner : MonoBehaviour
    {
        [Header("Prefab References")]
        [SerializeField] private GameObject _goldTargetPrefab;
        [SerializeField] private GameObject _penaltyTargetPrefab;

        [Header("Cluster Parent")]
        [SerializeField] private RectTransform _clusterParent;

        [Header("Goal Bounds")]
        [SerializeField] private RectTransform _goalArea;

        [Header("Radial Layout")]
        [SerializeField] private float _baseRadius = 90f;
        [SerializeField] private float _radiusRandomRange = 30f;
        [SerializeField] private float _angleJitter = 0.5f;

        private GameObject _currentGold;
        private List<GameObject> _currentPenalties = new List<GameObject>();

        private void Start()
        {
            var gm = GameManager.Instance;
            if (gm != null)
                gm.OnStateChanged += OnGameStateChanged;
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnStateChanged -= OnGameStateChanged;
        }

        private void OnGameStateChanged(GameState state)
        {
            if (state == GameState.Menu || state == GameState.GameOver)
            {
                ClearCluster();
            }
            else if (state == GameState.Playing)
            {
                SpawnCluster();
            }
        }

        public void MoveCluster()
        {
            ClearPenalties();
            _clusterParent.gameObject.SetActive(true);

            Vector2 newPos = GetRandomPositionInGoal();
            _clusterParent.anchoredPosition = newPos;

            if (_currentGold != null)
            {
                var gm = GameManager.Instance;
                if (gm != null)
                {
                    int newScore = gm.GetRandomGoldScore();
                    TargetInteraction goldInt = _currentGold.GetComponent<TargetInteraction>();
                    if (goldInt != null)
                        goldInt.Setup(TargetType.Gold, newScore, this);
                    TextMeshProUGUI label = _currentGold.GetComponentInChildren<TextMeshProUGUI>();
                    if (label != null)
                        label.text = $"+{newScore}";
                }
            }

            SpawnPenalties();
        }

        private void SpawnCluster()
        {
            ClearCluster();
            _clusterParent.gameObject.SetActive(true);

            Vector2 goldPos = GetRandomPositionInGoal();
            _clusterParent.anchoredPosition = goldPos;

            var gm = GameManager.Instance;
            if (gm == null) return;

            int goldScore = gm.GetRandomGoldScore();
            float goldScale = gm.GoldTargetScale;

            GameObject goldObj = Object.Instantiate(_goldTargetPrefab, _clusterParent);
            RectTransform gold = goldObj.GetComponent<RectTransform>();
            gold.anchoredPosition = Vector2.zero;
            gold.localScale = Vector3.one * goldScale;
            TargetInteraction goldInteraction = gold.GetComponent<TargetInteraction>();
            goldInteraction.Setup(TargetType.Gold, goldScore, this);
            _currentGold = gold.gameObject;

            TextMeshProUGUI goldLabel = gold.GetComponentInChildren<TextMeshProUGUI>();
            if (goldLabel != null)
                goldLabel.text = $"+{goldScore}";

            SpawnPenalties();
        }

        private void SpawnPenalties()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            int count = gm.CurrentPenaltyCount;
            float penaltyScale = gm.PenaltyTargetScale;
            float angleStep = 360f / count;

            for (int i = 0; i < count; i++)
            {
                float angle = i * angleStep + Random.Range(0f, _angleJitter * Mathf.Rad2Deg);
                float radius = (_baseRadius * penaltyScale) + Random.Range(0f, _radiusRandomRange);
                float rad = angle * Mathf.Deg2Rad;

                Vector2 penaltyPos = new Vector2(
                    Mathf.Cos(rad) * radius,
                    Mathf.Sin(rad) * radius
                );

                GameObject penaltyObj = Object.Instantiate(_penaltyTargetPrefab, _clusterParent);
                RectTransform penalty = penaltyObj.GetComponent<RectTransform>();
                penalty.anchoredPosition = penaltyPos;
                penalty.localScale = Vector3.one * penaltyScale;

                TargetInteraction penaltyInteraction = penalty.GetComponent<TargetInteraction>();
                penaltyInteraction.Setup(TargetType.Penalty, gm.PenaltyTargetScore);

                TextMeshProUGUI label = penalty.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null)
                {
                    int basePenalty = Mathf.Abs(gm.PenaltyTargetScore);
                    int displayValue = basePenalty + Random.Range(0, 7);
                    label.text = $"-{displayValue}";
                }

                _currentPenalties.Add(penalty.gameObject);
            }
        }

        private Vector2 GetRandomPositionInGoal()
        {
            if (_goalArea == null)
            {
                float x = Random.Range(100f, Screen.width - 100f);
                float y = Random.Range(100f, Screen.height - 100f);
                return new Vector2(x, y);
            }

            Rect rect = _goalArea.rect;
            var gm = GameManager.Instance;
            float maxScale = gm != null ? gm.PenaltyTargetScale : 1f;
            float clusterHalf = 200f * maxScale;

            float minX = rect.xMin + clusterHalf;
            float maxX = rect.xMax - clusterHalf;
            float minY = rect.yMin + clusterHalf;
            float maxY = rect.yMax - clusterHalf;

            float posX = Random.Range(minX, maxX);
            float posY = Random.Range(minY, maxY);

            return new Vector2(posX, posY);
        }

        private void ClearPenalties()
        {
            foreach (var p in _currentPenalties)
            {
                if (p != null) Destroy(p);
            }
            _currentPenalties.Clear();
        }

        private void ClearCluster()
        {
            ClearPenalties();
            if (_currentGold != null)
            {
                Destroy(_currentGold);
                _currentGold = null;
            }
            if (_clusterParent != null)
                _clusterParent.gameObject.SetActive(false);
        }
    }
}
