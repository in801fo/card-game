using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using Unity.PlasticSCM.Editor.WebApi;
using Unity.VisualScripting;
using UnityEngine;

public class ScreensManager : MonoBehaviour
{
    [SerializedDictionary("Screen Name", "Screen")]
    [SerializeField] private SerializedDictionary<string, GameObject> screens;
    public static ScreensManager Instance;
    public static Action<string> OnScreenClosure;
    public static Action<KeyValuePair<string, GameObject>> OnScreenOpen;
    public static Action<string> OnScreenHide;

    private static KeyValuePair<string, GameObject> currentScreen = new KeyValuePair<string, GameObject>(null, null);


    private void Awake()
    {
        if (!Instance) Instance = this;
        else Destroy(this.gameObject);
    }

    public GameObject SpawnScreen<T>(string screenID)
    {
        //if another screen is being used or an instance of the requested screen is already on screen return a reference to it
        if (currentScreen.Value != null && screenID == currentScreen.Key) return currentScreen.Value;
        currentScreen = new KeyValuePair<string, GameObject>
        (
            screenID,
            Instantiate(screens[screenID], Vector3.zero, Quaternion.identity)
        );

        //ought'ta set the screen ID of the screen
        currentScreen.Value.GetComponent<ScreenInitializer<T>>().screenID = screenID;

        OnScreenOpen?.Invoke(currentScreen);

        return currentScreen.Value;

    }

    //TODO: move somewhere else, maybe its own manager
    /// <summary>
    /// Creates a UI screen to ask the player which players to damage
    /// </summary>
    /// <param name="localExclusive">Should the local player be excluded from the damage</param>
    /// <returns></returns>
    public PlayerConsequenceScreenHandler AskForPlayerGroup(int players, consequenceTarget target)
    {
        SpawnScreen<playerConsequenceScreenInitializerStruct>("playerConsequence");
        currentScreen.Value.GetComponent<ScreenInitializer<playerConsequenceScreenInitializerStruct>>()
            .Initialize(new playerConsequenceScreenInitializerStruct()
                {
                    playersInfo = GameManager.playersDict.Values.ToList(),
                    screenHeading = $"Select {players} players ({players} left)",
                    target = target,
                    maxCount = players
                });

        return currentScreen.Value.GetComponent<PlayerConsequenceScreenHandler>();
    }

    public PlayerConsequenceScreenHandler AskForSinglePlayer(consequenceTarget target)
    {
        return AskForPlayerGroup(1, target);
    }

    public static void CloseCurrentScreen()
    {
        OnScreenClosure?.Invoke(currentScreen.Key);
        currentScreen = new KeyValuePair<string, GameObject>();
    }

    public static void OnHideCurrentScreen()
    {
        OnScreenHide?.Invoke(currentScreen.Key);
        currentScreen.Value.GetComponent<Renderer>().enabled = false;
    }
}