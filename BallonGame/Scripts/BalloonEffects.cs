using UnityEngine;

public class BalloonEffects : MonoBehaviour
{
    public static BalloonEffects Instance { get; private set; }

    [SerializeField] private ParticleSystem[] popVFXArray;
    [SerializeField] private AudioClip[] popSounds;
    [SerializeField] private AudioSource audioSource;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void PlayPopEffect(Vector3 position)
    {
        // انتخاب رندوم VFX
        int randomVFXIndex = Random.Range(0, popVFXArray.Length);
        ParticleSystem effect = Instantiate(popVFXArray[randomVFXIndex], position, Quaternion.identity);
        effect.Play();
        Destroy(effect.gameObject, effect.main.duration);

        // انتخاب رندوم صدا
        int randomSoundIndex = Random.Range(0, popSounds.Length);
        audioSource.PlayOneShot(popSounds[randomSoundIndex]);
    }
}