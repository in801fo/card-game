using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class AudioManager : NetworkBehaviour
{

    [SerializeField] private AudioClip bgMusic;

    public const string bgMusicAudioSourceLabel = "bgAudioSource";
    public const string cardSFXAudioSourceLabel = "cardSFXAudioSource";

    private static AudioSource bgAudioSource;
    private static AudioSource cardSFXAudioSource;

    public static AudioManager Instance { get; private set; }

    public override void OnNetworkSpawn()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        bgAudioSource = GameObject.FindWithTag(bgMusicAudioSourceLabel).GetComponent<AudioSource>();
        cardSFXAudioSource = GameObject.FindWithTag(cardSFXAudioSourceLabel).GetComponent<AudioSource>();
        if (bgMusic != null)
        {
            bgAudioSource.clip = bgMusic;
            bgAudioSource.Play();
        }

    }

    //TODO: continue implementation make it so that all connected clients, if required so, reproduce the audio
    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Server)]
    public void PlayCardSFXClient_Rpc(int clipId, FixedString64Bytes cardDataPath)
    {
        CardScriptable card = Resources.Load<CardScriptable>("Scriptables\\" + cardDataPath.ToString());

        cardSFXAudioSource.clip = card.onUseSoundEffect[clipId];
        cardSFXAudioSource.Play();
    }
    

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void RequestPlayCardSFXServer_Rpc(int index, FixedString64Bytes cardDataName)
    {
        CardScriptable card = Resources.Load<CardScriptable>("Scriptables\\" + cardDataName.ToString());
        if (card == null) return;

        PlayCardSFXClient_Rpc(index, cardDataName);
    }

}