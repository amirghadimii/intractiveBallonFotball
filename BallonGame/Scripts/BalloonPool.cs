using System.Collections.Generic;
using UnityEngine;

public class BalloonPool : MonoBehaviour
{
    [SerializeField] public GameObject[] balloonPrefabs;  // چندین Prefab
    [SerializeField] private int poolSize = 20;

    private Queue<GameObject> balloons = new Queue<GameObject>();

    private void Awake()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject randomBalloon = Instantiate(GetRandomPrefab(), transform);
            randomBalloon.SetActive(false);
            balloons.Enqueue(randomBalloon);
        }
    }

    private int currentBalloonIndex = 0;

    private GameObject GetRandomPrefab()
    {
        GameObject prefab = balloonPrefabs[currentBalloonIndex];
        currentBalloonIndex = (currentBalloonIndex + 1) % balloonPrefabs.Length;
        return prefab;
    }

    public GameObject GetBalloon()
    {
        if (balloons.Count > 0)
        {
            GameObject balloon = balloons.Dequeue();
            balloon.SetActive(true);
            return balloon;
        }

        // اگه Pool خالی بود یک جدید از Prefab رندوم بساز
        GameObject extraBalloon = Instantiate(GetRandomPrefab(), transform);
        return extraBalloon;
    }

    public void ReturnBalloon(GameObject balloon)
    {
        balloon.SetActive(false);
        balloons.Enqueue(balloon);
    }
}