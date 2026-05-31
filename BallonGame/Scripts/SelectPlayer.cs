using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SelectPlayer : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject modeSelectionPanel;
    [SerializeField] private GameManagerBalloon _GameManagerBalloon;
    [SerializeField] private StartGameBalloon startButton; // Reference to the start button
    [SerializeField] private BalloonSpawner balloonSpawner; // Reference to balloon spawner
    [SerializeField] private int PlayerCount;
    [SerializeField] private GameObject GameoverPnl; // Reference to game panel
    
    [Header("Shake Settings")]
    [SerializeField] private float shakeInterval = 3f; // Time between shakes in seconds
    [SerializeField] private float shakeDuration = 0.5f; // Duration of each shake
    [SerializeField] private float shakeIntensity = 5f; // How strong the shake is
    [SerializeField] private RectTransform[] playerButtons; // Assign player buttons in the inspector

    private bool isTwoPlayerMode = false;
    private bool isShaking = false;
    private Vector3[] originalPositions;

    private void Start()
    {
        // Store original positions of buttons
        if (playerButtons != null && playerButtons.Length > 0)
        {
            originalPositions = new Vector3[playerButtons.Length];
            for (int i = 0; i < playerButtons.Length; i++)
            {
                if (playerButtons[i] != null)
                    originalPositions[i] = playerButtons[i].localPosition;
            }
        }
        
        // Make sure mode selection is active at start
        if (modeSelectionPanel != null)
            modeSelectionPanel.SetActive(true);
            
        // Start the shake coroutine
        StartCoroutine(ShakeRoutine());
    }
    
    private IEnumerator ShakeRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(shakeInterval);
            if (!isShaking && modeSelectionPanel.activeSelf)
            {
                StartCoroutine(ShakeButtons());
            }
        }
    }
    
    private IEnumerator ShakeButtons()
    {
        isShaking = true;
        float elapsed = 0f;
        
        while (elapsed < shakeDuration)
        {
            if (playerButtons != null)
            {
                for (int i = 0; i < playerButtons.Length; i++)
                {
                    if (playerButtons[i] != null)
                    {
                        // Add random offset to create shake effect
                        Vector3 randomOffset = Random.insideUnitSphere * shakeIntensity;
                        playerButtons[i].localPosition = originalPositions[i] + randomOffset;
                    }
                }
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Reset positions after shake
        if (playerButtons != null)
        {
            for (int i = 0; i < playerButtons.Length; i++)
            {
                if (playerButtons[i] != null)
                    playerButtons[i].localPosition = originalPositions[i];
            }
        }
        
        isShaking = false;
    }

    public void SelectPlayerButton()
    {

        
        
        if (PlayerCount==1)
        {
            OnSinglePlayerSelected();
        }
        else  if (PlayerCount==2)
        {
            OnTwoPlayerSelected();
        }
        GameoverPnl.gameObject.SetActive(false);
    }
    private void OnSinglePlayerSelected()
    {
        
        SetGameMode(false);
    }
    
    private void OnTwoPlayerSelected()
    {
        SetGameMode(true);
    }
    
    private void SetGameMode(bool isTwoPlayer)
    {
        isTwoPlayerMode = isTwoPlayer;
        // Show start button
        if (startButton != null)
        {
           // startButton. OnStartGameBalloonHit();
        }
        if (_GameManagerBalloon != null)
        {
            _GameManagerBalloon.SetGameMode(isTwoPlayerMode);
        
        }
        if (modeSelectionPanel != null)
        {
            modeSelectionPanel.SetActive(false);
            
        }
    }
    

    
    // Call this method to show the mode selection
    public void ShowModeSelection()
    {
        // Reset UI
        if (modeSelectionPanel != null)
        {
            modeSelectionPanel.SetActive(true);
        }
        
        if (startButton != null)
        {
            startButton.gameObject.SetActive(false);
        }
    }
}
