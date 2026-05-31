using UnityEngine;

public class HitScript : MonoBehaviour
{
    public int pointValue = 1;
    private GameManager gameManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("GameManager not found in the scene.");
        }
    }

    // Update is called once per frame
 public   void _Hit()
    {
        if (gameManager != null)
        {
            gameManager.TargetHit(gameObject, pointValue);
        }
    }

    void OnMouseDown()
    {
        if (gameManager != null)
        {
            gameManager.TargetHit(gameObject, pointValue);
        }
    }
}
