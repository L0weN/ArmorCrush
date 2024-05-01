using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    [SerializeField] AudioClip click;
    [SerializeField] AudioClip deselect;

    [SerializeField] AudioClip match;
    [SerializeField] AudioClip noMatch;

    [SerializeField] AudioClip woosh;
    [SerializeField] AudioClip pop;
    
    AudioSource audioSource;

    private void OnValidate()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    public void PlayClick() => audioSource.PlayOneShot(click);
    public void PlayDeselect() => audioSource.PlayOneShot(deselect);
    public void PlayMatch() => audioSource.PlayOneShot(match);
    public void PlayNoMatch() => audioSource.PlayOneShot(noMatch);
    public void PlayWoosh() => PlayRandomPitch(woosh);
    public void PlayPop() => PlayRandomPitch(pop);
    public void PlayRandomPitch(AudioClip clip)
    {
        audioSource.pitch = Random.Range(0.9f, 1.1f);
        audioSource.PlayOneShot(clip);
        audioSource.pitch = 1;
    }
}
