using UnityEngine;

public class AudioManager : MonoBehaviour
{

    [SerializeField] private AudioClip bgMusic;

    public const string bgMusicAudioSourceLabel = "bgAudioSource";
    public const string cardSFXAudioSourceLabel = "cardSFXAudioSource";

    private static AudioSource bgAudioSource;
    private static AudioSource cardSFXAudioSource;

    private void Awake()
    {
        bgAudioSource = GameObject.FindWithTag(bgMusicAudioSourceLabel).GetComponent<AudioSource>();
        cardSFXAudioSource = GameObject.FindWithTag(cardSFXAudioSourceLabel).GetComponent<AudioSource>();
        if(bgMusic != null)
        {
            bgAudioSource.clip = bgMusic;
            bgAudioSource.Play();
        }
    }

    public static void PlayCardSFX(AudioClip clip)
    {
        cardSFXAudioSource.clip = clip;
        cardSFXAudioSource.Play();
    }


}