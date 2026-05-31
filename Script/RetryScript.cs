using UnityEngine;

public class RetryScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private GameManager _gameManager;
  public  void _Retry()
    {
        _gameManager.RestartGame();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
