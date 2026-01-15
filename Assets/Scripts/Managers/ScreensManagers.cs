using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using UnityEngine;

public class GameScreensManager : MonoBehaviour
{
    [SerializedDictionary("Screen Name", "Screen")]
    [SerializeField] private SerializedDictionary<string, GameObject> screens;
    public static GameScreensManager Instance;
    private static KeyValuePair<string, GameObject> currentScreen = new KeyValuePair<string, GameObject>(null, null);

    private void Awake()
    {
        if (!Instance) Instance = this;
        else Destroy(this.gameObject);
    }

    public GameObject SpawnScreen(string screenID)
    {
        //if another screen is being used or an instance of the requested screen is already on screen return a reference to it
        if (currentScreen.Value != null && screenID == currentScreen.Key) return currentScreen.Value;
        currentScreen = new KeyValuePair<string, GameObject>
        (
            screenID,
            Instantiate(screens[screenID], Vector3.zero, Quaternion.identity)
        );
        
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
        SpawnScreen("playerConsequence");
        currentScreen.Value.GetComponent<ScreenInitializer<playerConsequenceScreenInitializerStruct>>()
            .Initialize(new playerConsequenceScreenInitializerStruct()
                {
                    playersInfo = GameManager.playersDict.Values.ToList(),
                    screenHeading = $"Select {players} players ({players} left)",
                    target = target,
                    maxCount = players
                });
        PlayerConsequenceScreenHandler screenHandler = currentScreen.Value.GetComponent<PlayerConsequenceScreenHandler>();

        return screenHandler;
    }

    public PlayerConsequenceScreenHandler AskForSinglePlayer(consequenceTarget target)
    {
        return AskForPlayerGroup(1, target);
    }

    public static void CloseCurrentScreen()
    {
        Destroy(currentScreen.Value);
        currentScreen = new KeyValuePair<string, GameObject>();
    }
}