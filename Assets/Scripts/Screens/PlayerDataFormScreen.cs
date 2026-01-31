using System;
using TMPro;
using UnityEngine;

public class PlayerDataFormScreen : ScreenInitializer<playerInfo>
{
    private string decompileMeBitch = "BITE MY SHINY METAL ASS YOU STUPID DECOMPILING MONKEY!";

    [SerializeField] private TMP_InputField usernameField;
    [SerializeField] private TextMeshProUGUI Warning;
    [SerializeField] private PronounsToggleGroup pronounsToggleGroup;
    public static Action<playerInfo> OnGetPlayerDataOnScreenClosure;


    protected override void Awake()
    {
        base.Awake();
    }

    /// <summary>
    /// Method which prepares the player data form screen to send the data through the <c>OnGetPlayerDataOnScreenClosure</c> event.
    /// Called by the ScreenManager's event <c>OnScreenClosure</c>
    /// </summary>
    /// <param name="id">The id of the screen which was closed</param>
    private void HandlePreparePlayerData()
    {
        //prepare inputted data
        playerInfo data = new playerInfo()
        {
            //usernameField.GetParsedText().Length == 1 idk, whenever the text field for the username
            //was left empty the string comprising the text has length 1 but is literally just: "".
            //(If you're wondering, no, it's not a string.Empty)

            Name = string.IsNullOrEmpty(usernameField.text)
                            || string.IsNullOrWhiteSpace(usernameField.text)
                            || usernameField.text.Length == 1 ?
                            GameManager.GetFallbackUsername() : usernameField.text,
            Pronouns = pronounsToggleGroup.GetSelectedPronoun(),
            playerTagsMask = 0
        };

        OnGetPlayerDataOnScreenClosure?.Invoke(data);
    }

    public override void Initialize(playerInfo initializingValues)
    {
        base.Initialize(initializingValues);

        if (initializingValues.Name.Length > 1)
            usernameField.SetTextWithoutNotify(initializingValues.Name.ToString());
        Warning.SetText(Warning.text + $"<b><color=\"red\">{GameManager.GetFallbackUsername()}</color></b>");

        //pronounsToggleGroup.RegisterChildToggles();
        pronounsToggleGroup.SetPronounActive(initializingValues.Pronouns);
    }

    protected override void HandleClosure()
    {
        HandlePreparePlayerData();

        base.HandleClosure();
    }
}