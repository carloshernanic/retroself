using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MenuMusic : MonoBehaviour
{
    public AudioClip musicClip;
    public float fadeInDuration = 2f;
    public float targetVolume = 0.6f;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = musicClip;
        audioSource.loop = true;
        audioSource.volume = 0f;
        audioSource.Play();
    }

    void Update()
    {
        if (audioSource.volume < targetVolume)
        {
            audioSource.volume = Mathf.MoveTowards(
                audioSource.volume,
                targetVolume,
                (targetVolume / fadeInDuration) * Time.deltaTime
            );
        }
    }
}
