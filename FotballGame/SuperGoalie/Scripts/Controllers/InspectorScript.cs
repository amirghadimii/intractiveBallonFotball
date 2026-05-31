using UnityEngine;

public class InspectorScript : MonoBehaviour
{
    private Animator animator;
    private float timer;
    private float nextAnimationTime;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("No Animator component found on this GameObject!");
        }
        
        // Set initial random time for first animation
        nextAnimationTime = Random.Range(2f, 4f);
    }

    // Update is called once per frame
    void Update()
    {
        // Update the timer
        timer += Time.deltaTime;
        
        // Check if it's time to play the animation
        if (timer >= nextAnimationTime)
        {
            // Play the animation
            if (animator != null)
            {
                animator.Play(0, -1, 0f); // Resets and plays the default animation
            }
            
            // Reset the timer and get a new random interval
            timer = 0f;
            nextAnimationTime = Random.Range(2f, 4f);
        }
    }
}
