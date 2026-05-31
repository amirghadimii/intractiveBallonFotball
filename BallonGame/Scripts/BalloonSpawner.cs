using UnityEngine;

public class BalloonSpawner : MonoBehaviour
{
    [SerializeField] private BalloonPool poolOnePlayer;
    [SerializeField] private BalloonPool poolTwoPlayer;
    [SerializeField] private float spawnInterval = 1.5f;
    [SerializeField] private float speedMin = 1f, speedMax = 3f;
    [SerializeField] private GameManagerBalloon _gameManagerBalloon;
    private float timer;
    private int spawnCount = 0;
    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            if (_gameManagerBalloon.isGameActive)
            {
                SpawnBalloon();
                timer = 0f;
            }
        }
    }

    private void SpawnBalloon()
    {
        if (_gameManagerBalloon.currentGameMode == GameManagerBalloon.GameMode.OnePlayer)
        {
            GameObject balloon = poolOnePlayer.GetBalloon();
            Vector3 spawnPosition = GetRandomScreenPosition();
            balloon.transform.position = spawnPosition;
            float randomSpeed = Random.Range(speedMin, speedMax);
            balloon.GetComponent<Balloon>().Initialize(randomSpeed, poolOnePlayer);
        }
        else if (_gameManagerBalloon.currentGameMode == GameManagerBalloon.GameMode.TwoPlayer)
        {
        
            GameObject balloon = poolTwoPlayer.GetBalloon();
            Vector3 spawnPosition = GetRandomScreenPosition();
            balloon.transform.position = spawnPosition;
            float randomSpeed = Random.Range(speedMin, speedMax);
            balloon.GetComponent<Balloon>().Initialize(randomSpeed, poolTwoPlayer);
        }
    }

    private Vector3 GetRandomScreenPosition()
    {
        float screenX = Random.Range(0f, Screen.width);
        float worldY = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, Camera.main.nearClipPlane)).y - 1f;
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenX, 0, 10f));
        return new Vector3(worldPos.x, worldY, 0);
    }
}