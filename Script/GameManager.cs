using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UPersian.Components; // Using UPersian.Components for RTL Text

public class GameManager : MonoBehaviour
{
    public GameObject[] targets; // Array of target GameObjects
    public SpriteMask[] targetMasks; // Array of SpriteMasks corresponding to targets
    public CircleRot circleRot; // Reference to the CircleRot script
    public float rotationSpeedIncreasePerHit = 5f; // How much to increase rotation speed per hit
    public float gameDuration = 120f; // 2 minutes in seconds
    public GameObject hitParticlePrefab; // Particle system to play on hit
    public GameObject collisionMarkerPrefab; // Prefab to instantiate at hit location if not already present

    public AudioClip[] hitSounds; // Array of audio clips to play on hit (for random selection)
    public AudioClip[] additionalHitSounds; // An array of audio clips to play in addition to the random hit sound
    private AudioSource audioSource; // AudioSource component to play sounds

    public RtlText scoreText; // UI Text to display score
    public RtlText timerText; // UI Text to display timer
    public GameObject gameOverPanel; // Panel to show when game is over
    public RtlText finalScoreText; // UI Text to display final score
    public RtlText highScoreText; // UI Text to display high score
public   SpriteRenderer hitSpriteRenderer;
    public   SpriteRenderer secondHitSpriteRenderer;

    public int score;
    private int highScore; // Store the high score
    [SerializeField] private Sprite Hit2sprite; // To store the original sprite of the additional SpriteRenderer
    [SerializeField] private Sprite hit1sprite; // To store the original sprite of the second additional SpriteRenderer
    [SerializeField] private Sprite idle1sprite; // To store the original sprite of the additional SpriteRenderer
    [SerializeField] private Sprite idle2sprite; // To store the original sprite of the second additional SpriteRenderer
    private GameObject activeCollisionMarker; // To store the instantiated collision marker prefab
    private List<int> availableTargetIndices;
    private GameObject currentActiveTarget;
    private float gameTimer;
    private bool gameEnded = false;

    private float initialCircleRotationSpeed; // To store the initial rotation speed

    void Awake()
    {
        // Initialize audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false); // Ensure panel is hidden from the very start
        }
        else
        {
            Debug.LogError("Game Over Panel is not assigned in GameManager. Please assign it in the Inspector.");
        }
    }

    void Start()
    {
        initialCircleRotationSpeed = circleRot.rotationSpeed; // Store initial rotation speed
        // gameOverPanel.SetActive(false); // This line is now redundant as it's handled in Awake()
        highScore = PlayerPrefs.GetInt("HighScore", 0); // Load high score, default to 0


        RestartGame(); // Start game in a clean state
    }

    void Update()
    {
        if (gameEnded) return;

        gameTimer -= Time.deltaTime;
        UpdateTimerDisplay();

        if (gameTimer <= 0)
        {
            EndGame();
        }
    }

    void InitializeAvailableTargets()
    {
        availableTargetIndices.Clear();
        for (int i = 0; i < targets.Length; i++)
        {
            availableTargetIndices.Add(i);
            targets[i].SetActive(false); // Ensure all targets are initially inactive
        }
    }

    void ActivateRandomTarget()
    {
        if (gameEnded) return;

        if (availableTargetIndices.Count == 0)
        {
            InitializeAvailableTargets(); // Reset if all targets have been activated
        }

        int randomIndex = Random.Range(0, availableTargetIndices.Count);
        int targetIndex = availableTargetIndices[randomIndex];
        currentActiveTarget = targets[targetIndex];
        currentActiveTarget.SetActive(true);
        availableTargetIndices.RemoveAt(randomIndex);
    }

    public void TargetHit(GameObject hitTarget, int points)
    {
        if (gameEnded) return;

        if (hitTarget == currentActiveTarget)
        {
            hitTarget.SetActive(false);
            // Activate the corresponding SpriteMask when hit
            int hitTargetIndex = System.Array.IndexOf(targets, hitTarget);
            if (hitTargetIndex != -1 && hitTargetIndex < targetMasks.Length && targetMasks[hitTargetIndex] != null)
            {
                targetMasks[hitTargetIndex].enabled = true;
            }

         
            
        

                hitSpriteRenderer.sprite = hit1sprite;
                StartCoroutine(RevertSpriteAfterDelay(hitSpriteRenderer, idle1sprite, 1f));
            

            // Apply sprite change to the second additional hitSpriteRenderer if assigned

                secondHitSpriteRenderer.sprite = Hit2sprite;
                StartCoroutine(RevertSpriteAfterDelay(secondHitSpriteRenderer, idle2sprite, 1f));
            

            // Instantiate and play particle effect at the hit target's position
            if (hitParticlePrefab != null)
            {
                GameObject particle =   Instantiate(hitParticlePrefab, hitTarget.transform.position, Quaternion.identity);
                Destroy(particle, 2f);
            }

            // Instantiate collision marker prefab if not already present

                activeCollisionMarker = Instantiate(collisionMarkerPrefab, hitTarget.transform.position, Quaternion.identity);
                activeCollisionMarker.transform.parent = secondHitSpriteRenderer.gameObject.transform;
                StartCoroutine(DestroyCollisionMarkerAfterDelay(10f));
            

            // Play a random hit sound from the array (if assigned and array has elements)
            if (audioSource != null && hitSounds != null && hitSounds.Length > 0)
            {
                int randomIndex = Random.Range(0, hitSounds.Length);
                audioSource.PlayOneShot(hitSounds[randomIndex]);
            }
            // Also play an additional random hit sound if assigned
            if (audioSource != null && additionalHitSounds != null && additionalHitSounds.Length > 0)
            {
                int randomIndex = Random.Range(0, additionalHitSounds.Length);
                audioSource.PlayOneShot(additionalHitSounds[randomIndex]);
            }

            // Play a random hit sound from the array (if assigned and array has elements)
            if (audioSource != null && hitSounds != null && hitSounds.Length > 0)
            {
                int randomIndex = Random.Range(0, hitSounds.Length);
                audioSource.PlayOneShot(hitSounds[randomIndex]);
            }

            score += points;
            if (scoreText != null)
            {
                scoreText.text = "Score: " + score;
            }
            Debug.Log("Score: " + score);
            
            if (circleRot != null)
            {
                circleRot.rotationSpeed += rotationSpeedIncreasePerHit;
                Debug.Log("Rotation Speed increased to: " + circleRot.rotationSpeed);
            }
            
            ActivateRandomTarget();
        }
    }
    public void LaunchFromNormalizedPosition(float normalizedX, float normalizedY)
    {
        Debug.Log("EYoo");
        float clampedX = Mathf.Clamp01(normalizedX);
        float clampedY = Mathf.Clamp01(normalizedY);
        Vector2 screenPoint = new Vector2(clampedX * Screen.width, clampedY * Screen.height);


        TryLaunchAtScreenPoint(screenPoint);
    }
    private void TryLaunchAtScreenPoint(Vector2 screenPoint)
    {
     

        Camera activeCamera = Camera.main;

 

        if (activeCamera == null)
        {
            Debug.LogWarning("BallLauncher: No camera available for launching.");
            return;
        }
        Debug.Log("EYoo1111");

        Ray ray = activeCamera.ScreenPointToRay(screenPoint);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Debug.Log("EYoo11133331");

            HitScript _HitScript = hit.collider.GetComponent<HitScript>();
            if (_HitScript != null)
            {
                Debug.Log("EYoo111355553331");

                TargetHit(_HitScript.gameObject,_HitScript. pointValue);
            }
            else
            {
                RetryScript _RetryScript = hit.collider.GetComponent<RetryScript>();
                if (_RetryScript != null)
                {
                    _RetryScript._Retry();
                }
            }
           
            
        }
    }
    void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            timerText.text = "Time: " + Mathf.CeilToInt(gameTimer).ToString();
        }
    }

    void EndGame()
    {
        gameEnded = true;
        Debug.Log("Game Over! Final Score: " + score);
        if (timerText != null)
        {
            timerText.text = "Time: 00:00"; // Ensure timer shows 0 at the end
        }
        // Deactivate all targets
        for (int i = 0; i < targets.Length; i++)
        {
            targets[i].SetActive(false);
        }
        // Stop the CircleRot 
        if (circleRot != null) 
        {
            circleRot.enabled = false; // Disable the script to stop rotation
        }

        // Show Game Over Panel and update scores
        gameOverPanel.SetActive(true);
        if (finalScoreText != null)
        {
            finalScoreText.text = "Final Score: " + score;
        }
        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
        }
        if (highScoreText != null)
        {
            highScoreText.text = "High Score: " + highScore;
        }
    }

    public void RestartGame()
    {
        gameEnded = false;
        score = 0;
        gameTimer = gameDuration;
        gameOverPanel.SetActive(false);
        
        // Ensure availableTargetIndices is initialized
        if (availableTargetIndices == null)
        {
            availableTargetIndices = new List<int>();
        }

        if (circleRot != null)
        {
            circleRot.rotationSpeed = initialCircleRotationSpeed; // Reset rotation speed
            circleRot.enabled = true; // Ensure CircleRot is enabled
        }

        // Reset all masks (since they are not reset in InitializeAvailableTargets)
        for (int i = 0; i < targetMasks.Length; i++)
        {
            if (targetMasks[i] != null)
            {
                targetMasks[i].enabled = false;
            }
        }

        InitializeAvailableTargets();
        ActivateRandomTarget();

        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }
        if (timerText != null)
        {
            UpdateTimerDisplay();
        }
    }

    System.Collections.IEnumerator RevertSpriteAfterDelay(SpriteRenderer spriteRenderer, Sprite originalSprite, float delay)
    {
        yield return new WaitForSeconds(delay);
        spriteRenderer.sprite = originalSprite;
    }

    System.Collections.IEnumerator DestroyCollisionMarkerAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (activeCollisionMarker != null)
        {
            Destroy(activeCollisionMarker);
            activeCollisionMarker = null;
        }
    }
}
