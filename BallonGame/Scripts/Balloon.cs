using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class Balloon : MonoBehaviour
{
    private float speed;
    private BalloonPool pool;
    private bool isActive = false;

    private float waveAmplitude;   // دامنه موج
    private float waveFrequency;   // سرعت موج
    private Vector3 startPos;
    private float waveOffset;
    public int playerNumber = 1; // Default to player 1
    public int XScore = 1; // Default to player 1

    // Special balloon properties
    [SerializeField] private bool isSpecialBalloon = false;
    [SerializeField] private int hitsRequired = 3;
    private int currentHits = 0;
   
    private SpriteRenderer spriteRenderer;
    
    // UI Text for showing remaining hits (child object)
    public TMPro.TextMeshProUGUI hitCountText;
    
    // VFX for hit effect
    [SerializeField] private ParticleSystem hitVFX; // Assign in Inspector
    [SerializeField] private ParticleSystem popVFX; // Assign in Inspector for when balloon pops
    
    // Audio for hit and pop effects
    [SerializeField] private AudioClip hitSound; // Sound when hit but not popped
    [SerializeField] private AudioClip[] popSounds; // Array of pop sounds
    private AudioSource audioSource;

    public void SetPlayerNumber(int playerNum)
    {
        playerNumber = playerNum;
    }
    
    public void Initialize(float moveSpeed, BalloonPool balloonPool)
    {
        speed = moveSpeed;
        pool = balloonPool;
        isActive = true;
        // Reset player number in case this is a recycled balloon
     

        // Reset hits for reuse
        currentHits = 0;
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Get or add AudioSource component
        if (audioSource == null)
        {
            // First try to find AudioSource with "vfx" tag in scene
            GameObject vfxAudioObj = GameObject.FindGameObjectWithTag("VFX");
            if (vfxAudioObj != null)
            {
                audioSource = vfxAudioObj.GetComponent<AudioSource>();
            }
            
            // If not found, try local AudioSource
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                }
            }
        }

        // Special balloon moves slower
        if (isSpecialBalloon)
        {
            speed *= 1.5f; // 50% slower
       
            
            // Make sure text is visible
            if (hitCountText != null)
            {
                hitCountText.gameObject.SetActive(true);
                hitCountText.text = "3";
            }
        }

        startPos = transform.position;
        waveAmplitude = Random.Range(0.2f, 0.5f);     // چقدر به چپ و راست بره
        waveFrequency = Random.Range(1f, 3f);         // سرعت موج
        waveOffset = Random.Range(0f, Mathf.PI * 2);  // شروع موج
    }

    private void Update()
    {
        if (!isActive) return;

        float newY = transform.position.y + speed * Time.deltaTime;
        float newX = startPos.x + Mathf.Sin(Time.time * waveFrequency + waveOffset) * waveAmplitude;

        transform.position = new Vector3(newX, newY, transform.position.z);

        if (newY > 6f)
        {
            Deactivate();
        }
    }

    private void OnMouseDown()
    {
        if (!isActive) return;

        HandleHit();
    }

    public void BalloonHit()
    {
        if (!isActive) return;

        HandleHit();
    }
    private void HandleHit()
    {
        if (isSpecialBalloon)
        {
            currentHits++;
            
            if (currentHits >= hitsRequired)
            {
                // Balloon pops after required hits
                PlayPopVFX();
                PlayPopSound();
                
                // Add score to the player who popped the balloon
                if (GameManagerBalloon.Instance != null)
                {
                    if (isSpecialBalloon)
                    {
                        
                        GameManagerBalloon.Instance.AddScore(playerNumber, XScore);

                    }
                    else
                    {
                        GameManagerBalloon.Instance.AddScore(playerNumber, 1);

                    }
                }
                
                Deactivate();
            }
            else
            {
                // Change appearance based on hits
             
                UpdateHitCountText();
                PlayHitVFX();
                PlayHitSound();
                // Maybe add a small effect to show it was hit but not popped
                StartCoroutine(ScaleEffect());
            }
        }
        else
        {
            // Normal balloon pops immediately
            PlayPopVFX();
            PlayPopSound();
            
            // Add score to the player who popped the balloon
            if (GameManagerBalloon.Instance != null)
            {
                GameManagerBalloon.Instance.AddScore(playerNumber, 1);
            }
            
            Deactivate();
        }
    }



    
    private void UpdateHitCountText()
    {
        if (hitCountText != null)
        {
            int remainingHits = hitsRequired - currentHits;
            hitCountText.text = remainingHits.ToString();
            
            // Change color based on remaining hits
            if (remainingHits == 1)
                hitCountText.color = Color.red;
            else if (remainingHits == 2)
                hitCountText.color = Color.yellow;
            else
                hitCountText.color = Color.white;
        }
    }
    
    private void PlayHitVFX()
    {
        if (hitVFX != null)
        {
            // Create VFX instance at balloon's exact position
            ParticleSystem vfxInstance = Instantiate(hitVFX, transform.position, Quaternion.identity);
            vfxInstance.Play();
            
            // Destroy VFX after it finishes playing
            Destroy(vfxInstance.gameObject, vfxInstance.main.duration + vfxInstance.main.startLifetime.constantMax);
        }
    }
    
    private void PlayPopVFX()
    {
        if (popVFX != null)
        {
            // Create VFX instance at balloon's exact position
            ParticleSystem vfxInstance = Instantiate(popVFX, transform.position, Quaternion.identity);
            vfxInstance.Play();
            
            // Destroy VFX after it finishes playing
            Destroy(vfxInstance.gameObject, vfxInstance.main.duration + vfxInstance.main.startLifetime.constantMax);
        }
    }
    
    private void PlayHitSound()
    {
        if (audioSource != null && hitSound != null)
        {
            audioSource.PlayOneShot(hitSound);
        }
    }
    
    private void PlayPopSound()
    {
        if (audioSource != null && popSounds != null && popSounds.Length > 0)
        {
            // Play random pop sound from array
            AudioClip randomPopSound = popSounds[Random.Range(0, popSounds.Length)];
            audioSource.clip=randomPopSound;
            audioSource.Play();
            Debug.Log("WHYYY");
        }
    }

    // Public method to set balloon as special
    public void SetAsSpecialBalloon(bool special = true, int hits = 3)
    {
        isSpecialBalloon = special;
        hitsRequired = hits;
        currentHits = 0;
    }
    
    private IEnumerator ScaleEffect()
    {
        Vector3 originalScale = transform.localScale;
        transform.localScale = originalScale * 0.9f;
        
        float elapsedTime = 0f;
        float duration = 0.2f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            transform.localScale = Vector3.Lerp(originalScale * 0.9f, originalScale, t);
            yield return null;
        }
        
        transform.localScale = originalScale;
    }


    private void Deactivate()
    {
        isActive = false;
        currentHits = 0;

        // Reset appearance
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }
        
        // Hide hit count text instead of destroying it
        if (hitCountText != null)
        {
            hitCountText.gameObject.SetActive(false);
        }

        pool.ReturnBalloon(gameObject);
    }
}