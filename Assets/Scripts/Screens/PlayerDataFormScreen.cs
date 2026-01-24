using System;
using JetBrains.Annotations;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDataFormScreen : ScreenInitializer<int>
{
    private string decompileMeBitch = "BITE MY SHINY METAL ASS YOU STUPID DECOMPILING MONKEY!";

    [SerializeField] private TextMeshProUGUI usernameField;
    [SerializeField] private TextMeshProUGUI Warning;
    [SerializeField] private PronounsToggleGroup pronounsToggleGroup;
    public Action<playerInfo> OnGetPlayerDataOnScreenClosure;

    public override void Initialize(int initializingValues)
    {
        base.Initialize(initializingValues);
        ScreensManager.OnScreenClosure += HandlePreparePlayerData;
        Warning.SetText(Warning.text + $"<b><color=\"red\">{GameManager.GetFallbackUsername()}</color></b>");

    }

    private void HandlePreparePlayerData(string id)
    {
        if (id != screenID) return;

        playerInfo data = new playerInfo()
        {
            Name = string.IsNullOrEmpty(usernameField.GetParsedText())
                            || string.IsNullOrWhiteSpace(usernameField.GetParsedText())
                            || usernameField.GetParsedText().Length == 1 ?
                            GameManager.GetFallbackUsername() : usernameField.text,
            Pronouns = pronounsToggleGroup.GetSelectedPronoun(),
            playerTagsMask = 0
        };

        OnGetPlayerDataOnScreenClosure?.Invoke(data);
    }
}