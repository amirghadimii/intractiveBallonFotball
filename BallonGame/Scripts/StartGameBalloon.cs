using UnityEngine;
using System.Collections;

public class StartGameBalloon : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject objectToDisable; // Object to disable when hit
    [SerializeField] private SelectPlayer selectPlayer; // Reference to select player component
    [SerializeField] private BalloonSpawner _BalloonSpawner; // Reference to select player component
    [SerializeField] private GameManagerBalloon _GameManagerBalloon; // Reference to select player component
    
    [Header("Background Colors")]
    [SerializeField] private Color darkBackgroundColor = Color.black; // Dark color when button is off
    [SerializeField] private Color lightBackgroundColor = Color.white; // White color when button is on
    [SerializeField] private SpriteRenderer _spriteRenderer; // Renderer for background
    [SerializeField] private GameObject gamePnl; // Reference to game panel

    [Header("Bounce Settings")]
    [SerializeField] private float bounceInterval = 2f; // Time between bounces in seconds
    [SerializeField] private float bounceHeight = 0.2f; // How high the balloon bounces
    [SerializeField] private float bounceDuration = 0.5f; // How long the bounce animation takes

    private Vector3 originalScale;
    private bool isBouncing = false;
    
    private void Start()
    {
        // Set initial dark background
        SetBackgroundColor(darkBackgroundColor);
        originalScale = transform.localScale;
        
        // Start the bouncing coroutine
        StartCoroutine(BounceRoutine());
    }

    private IEnumerator BounceRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(bounceInterval);
            if (!isBouncing)
            {
                StartCoroutine(DoBounce());
            }
        }
    }

    private IEnumerator DoBounce()
    {
        isBouncing = true;
        float elapsedTime = 0f;
        Vector3 targetScale = originalScale * (1f + bounceHeight);

        // Scale up
        while (elapsedTime < bounceDuration / 2f)
        {
            transform.localScale = Vector3.Lerp(originalScale, targetScale, (elapsedTime / (bounceDuration / 2f)));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Scale down
        elapsedTime = 0f;
        while (elapsedTime < bounceDuration / 2f)
        {
            transform.localScale = Vector3.Lerp(targetScale, originalScale, (elapsedTime / (bounceDuration / 2f)));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.localScale = originalScale;
        isBouncing = false;
    }
    
    public void ShowPlayerSelection()
    {
        // Change background to white
        SetBackgroundColor(lightBackgroundColor);
        
        // Disable the specified object
        if (objectToDisable != null)
        {
            objectToDisable.SetActive(false);
        }
        
        // Show the game panel
        if (gamePnl != null)
        {
            gamePnl.SetActive(true);
        }

        _BalloonSpawner.enabled = true;
        _GameManagerBalloon.StartGame();
        _BalloonSpawner.enabled = true;
        // Disable this start game balloon
        gameObject.SetActive(false);
    }
    
    public void OnStartGameBalloonHit()
    {
        // This method is kept for backward compatibility
        ShowPlayerSelection();
    }
    
    private void SetBackgroundColor(Color color)
    {
        _spriteRenderer.color = color;
    }
    
    public void ResetToInitialState()
    {
        // Reset background to dark
        SetBackgroundColor(darkBackgroundColor);
        
        // Hide game panel if it was shown
        if (gamePnl != null)
        {
            gamePnl.SetActive(false);
        }
        
        // Reactivate the object that was disabled
        if (objectToDisable != null)
        {
            objectToDisable.SetActive(true);
        }
        
        // Show player selection again
        ShowPlayerSelection();
        
        Debug.Log("Game reset to initial state. Showing player selection.");
    }
}
