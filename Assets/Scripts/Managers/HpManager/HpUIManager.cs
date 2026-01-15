using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HpUIManager : MonoBehaviour
{
    [SerializeField] private Canvas healthUICanvasPrefab;
    [SerializeField] private GameObject healthUIPlayerBarPrefab;

    private Canvas healthUICanvasInstance;

    private HealthBarUIHandler healthBarUI;

    private void Awake()
    {
        healthUICanvasInstance = Instantiate(healthUICanvasPrefab);
        GameManager.OnDoneGenerating += GenerateHpUI;
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
            GeneratePlayerHealthEntry(playersInfos[i]);
        }
    }

    private void GeneratePlayerHealthEntry(playerInfo? info)
    {
        GameObject playerHeathBarUI = Instantiate(healthUIPlayerBarPrefab);
        playerHeathBarUI.transform.SetParent(healthUICanvasInstance.transform);
        healthBarUI = playerHeathBarUI.GetComponentInChildren<HealthBarUIHandler>();
        if (healthBarUI != null) healthBarUI.Initialize(info);
        else RuntimeMsg.Error("Unable to initialize player health bar.", "Unable to initialize player health bar as the required component was not attached.");
    }

}
