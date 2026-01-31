using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;

public class ScreensManager : MonoBehaviour
{
    [SerializedDictionary("Screen Name", "Screen")]
    [SerializeField] private SerializedDictionary<string, GameObject> screens;
    public static ScreensManager Instance;
    public static Action<string> OnScreenClosure;
    public static Action<KeyValuePair<string, GameObject>> OnScreenOpen;
    //public static Action<string> OnScreenHide;


    private static Dictionary<string, GameObject> currentlyActiveScreens = new Dictionary<string, GameObject>();


    private void Awake()
    {
        if (!Instance) Instance = this;
        else Destroy(this.gameObject);
    }

    public GameObject SpawnScreen<T>(string screenID)
    {
        //if another screen is being used or an instance of the requested screen is already on screen return a reference to it
        if (currentlyActiveScreens.ContainsKey(screenID)) return currentlyActiveScreens[screenID];
        KeyValuePair<string, GameObject> currentScreen = new KeyValuePair<string, GameObject>
        (
            screenID,
            Instantiate(screens[screenID], Vector3.zero, Quaternion.identity)
        );

        //ought'ta set the screen ID of the screen
        currentScreen.Value.GetComponent<ScreenInitializer<T>>().screenID = screenID;

        currentlyActiveScreens.Add(currentScreen.Key, currentScreen.Value);

        OnScreenOpen?.Invoke(currentScreen);

        return currentScreen.Value;

    }

    public static void CloseScreen(string screenID)
    {
        if (!currentlyActiveScreens.ContainsKey(screenID))
        {
            RuntimeMsg.Error("Unable to close screen", "The requested screen does not exist!");
            return;
        }

        OnScreenClosure?.Invoke(screenID);
        currentlyActiveScreens.Remove(screenID);
    }


    //not using currently
    /*public static void OnHideCurrentScreen()
    {
        OnScreenHide?.Invoke(currentScreen.Key);
        currentScreen.Value.GetComponent<Renderer>().enabled = false;
    }*/
}