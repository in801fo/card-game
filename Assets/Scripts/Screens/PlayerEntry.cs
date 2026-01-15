using System;
using JetBrains.Annotations;
using NUnit.Framework;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerEntry : MonoBehaviour
{
    private Toggle toggle;
    private TextMeshProUGUI lable;
    public playerInfo representingPlayer { get; private set; }

    public Action<ulong, bool> onToggleValueChanged;

    private void Awake()
    {
        toggle = GetComponentInChildren<Toggle>();
        lable = GetComponentInChildren<TextMeshProUGUI>();
        toggle.onValueChanged.AddListener(HandleValueChanged);
        PlayerConsequenceScreenHandler.OnLimitPlayersIncluded += UntickOnExceededPlayerCount;
    }

    public void Initialize(playerInfo info)
    {
        representingPlayer = info;
        toggle.isOn = false;
        lable.SetText(info.Name.ToString());
    }

    private void UntickOnExceededPlayerCount(ulong id)
    {
        if (id == representingPlayer.playerId) toggle.isOn = false;
    }

    private void HandleValueChanged(bool newValue)
    {
        PlayerConsequenceScreenHandler.Instance.HandlePlayerTicked(representingPlayer.playerId, newValue);
    }

    public void SetTickValue(bool state)
    {
        toggle.isOn = state;
    }

    public void SetInteractability(bool state)
    {
        toggle.interactable = state;
    }
}