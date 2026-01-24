using System.Diagnostics;
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
        if (bgMusic != null)
        {
            bgAudioSource.clip = bgMusic;
            bgAudioSource.Play();
        }
    }

    //TODO: continue implementation make it so that all connected clients, if required so, reproduce the audio
    public static void PlayCardSFX(AudioClip clip)
    {
        if (clip == null)
        {
            RuntimeMsg.Warning("Tried to play null clip"); 
            return;
        }
        cardSFXAudioSource.clip = clip;
        cardSFXAudioSource.Play();
    }


}