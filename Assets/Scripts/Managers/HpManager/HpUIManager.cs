using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class HpUIManager : CoordinatedMonoBehaviour
{
    [SerializeField] private Canvas healthUICanvasPrefab;
    [SerializeField] private GameObject healthUIPlayerBarPrefab;

    public static Action<ulong> OnHandleHpPlayerDisconnect;

    private Canvas healthUICanvasInstance;


    private List<HealthBarUIHandler> healthHandlers = new List<HealthBarUIHandler>();

    protected override void Awake()
    {
        base.Awake();
        GameManager.OnDoneGenerating += GenerateHpUI;
        NetworkManager.Singleton.OnConnectionEvent += HandleClientDisconnect;
    }

    private void HandleClientDisconnect(NetworkManager manager, ConnectionEventData data)
    {
        OnHandleHpPlayerDisconnect?.Invoke(data.ClientId);
    }

    protected override void Beginning()
    {
        healthUICanvasInstance = Instantiate(healthUICanvasPrefab);
    }

    private void GenerateHpUI()
    {
        RuntimeMsg.Info("Generating player HP UI");
        GenerateHpUI(GameManager.playersDict.Values.ToList());
    }

    private void GenerateHpUI(List<playerInfo> playersInfos)
    {
        for (int i = 0; i < playersInfos.Count; i++)
        {
            healthHandlers.Add(GeneratePlayerHealthEntry(playersInfos[i]));
        }
    }

    private HealthBarUIHandler GeneratePlayerHealthEntry(playerInfo? info)
    {
        GameObject playerHeathBarUI = Instantiate(healthUIPlayerBarPrefab);
        playerHeathBarUI.transform.SetParent(healthUICanvasInstance.transform);
        HealthBarUIHandler healthBarUI = playerHeathBarUI.GetComponentInChildren<HealthBarUIHandler>();

        if (healthBarUI != null) {
            healthBarUI.Initialize(info);
            return healthBarUI;
        }
        else 
            RuntimeMsg.Error("Unable to initialize player health bar",
            "Unable to initialize player health bar as the required component was not attached");
        return null;
    }

}
