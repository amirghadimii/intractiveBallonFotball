using UnityEngine;
using System.Collections.Generic;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;

namespace GoalRush
{
    public class TargetSpawner : MonoBehaviour
    {
        [Header("Prefab References")]
        [SerializeField] private GameObject _goldTargetPrefab;
        [SerializeField] private GameObject _penaltyTargetPrefab;
        [SerializeField] private GameObject _ringEffectPrefab;

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
        private Canvas _mainCanvas;

        private void Start()
        {
            var gm = GameManager.Instance;
            if (gm != null)
                gm.OnStateChanged += OnGameStateChanged;

            _mainCanvas = GetComponentInParent<Canvas>() ?? FindFirstObjectByType<Canvas>();
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

        public void MoveCluster(Vector2 clickScreenPos)
        {
            Vector2 oldPos = _clusterParent.anchoredPosition;

            ClearPenalties();
            _clusterParent.gameObject.SetActive(true);

            Vector2 newPos = GetRandomPositionInGoal();
            _clusterParent.anchoredPosition = newPos;

            SpawnRingEffect(clickScreenPos);
            PlayClusterMoveTween(oldPos, newPos);

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
            float goldScale = gm.GoldTargetScale;
            float angleStep = 360f / count;

            float goldHalf = _goldTargetPrefab.GetComponent<RectTransform>().sizeDelta.x * 0.5f * goldScale;
            float penaltyHalf = _penaltyTargetPrefab.GetComponent<RectTransform>().sizeDelta.x * 0.5f * penaltyScale;
            float penaltyDiameter = penaltyHalf * 2;
            float minRadius = goldHalf + penaltyHalf + 15f;
            float minDist = penaltyDiameter + 10f;

            List<Vector2> placedPositions = new List<Vector2>();

            for (int i = 0; i < count; i++)
            {
                Vector2 pos;
                int attempts = 0;
                int maxAttempts = 20;

                do
                {
                    float angle = i * angleStep + Random.Range(-_angleJitter * Mathf.Rad2Deg, _angleJitter * Mathf.Rad2Deg);
                    float radius = Mathf.Max(minRadius, _baseRadius * penaltyScale) + Random.Range(0f, _radiusRandomRange);
                    float rad = angle * Mathf.Deg2Rad;
                    pos = new Vector2(Mathf.Cos(rad) * radius, Mathf.Sin(rad) * radius);
                    attempts++;
                } while (TooClose(pos, placedPositions, minDist) && attempts < maxAttempts);

                placedPositions.Add(pos);

                GameObject penaltyObj = Object.Instantiate(_penaltyTargetPrefab, _clusterParent);
                RectTransform penalty = penaltyObj.GetComponent<RectTransform>();
                penalty.anchoredPosition = pos;
                penalty.localScale = Vector3.one * penaltyScale;

                TargetInteraction penaltyInteraction = penalty.GetComponent<TargetInteraction>();
                penaltyInteraction.Setup(TargetType.Penalty, gm.PenaltyTargetScore);

                TextMeshProUGUI label = penalty.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null)
                    label.text = $"{gm.PenaltyTargetScore}";

                _currentPenalties.Add(penalty.gameObject);
            }
        }

        private bool TooClose(Vector2 pos, List<Vector2> others, float minDist)
        {
            foreach (var other in others)
            {
                if (Vector2.Distance(pos, other) < minDist)
                    return true;
            }
            return false;
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

        private void SpawnRingEffect(Vector2 screenPos)
        {
            if (_ringEffectPrefab == null || _mainCanvas == null) return;

            Vector2 center = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            Vector2 anchoredPos = (screenPos - center) / _mainCanvas.scaleFactor;

            GameObject ring = Object.Instantiate(_ringEffectPrefab, _mainCanvas.transform, false);
            RectTransform rt = ring.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchoredPosition = anchoredPos;
                rt.sizeDelta = new Vector2(60, 60);
            }
            Image img = ring.GetComponent<Image>();
            if (img != null)
            {
                img.color = new Color(1, 1, 1, 0.3f);
                img.DOFade(0f, 0.4f);
            }
            if (rt != null)
                rt.DOSizeDelta(new Vector2(120, 120), 0.4f).SetEase(Ease.OutCubic);
            Object.Destroy(ring, 0.5f);
        }

        private void PlayClusterMoveTween(Vector2 from, Vector2 to)
        {
            _clusterParent.localScale = Vector3.one * 0.85f;
            _clusterParent.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);
        }
    }
}
