using System.Collections.Generic;
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
        if (currentScreen.Value != null && screenID == currentScreen.Key) return currentScreen.Value;
        currentScreen = new KeyValuePair<string, GameObject>
        (
            screenID,
            Instantiate(screens[screenID], Vector3.zero, Quaternion.identity)
        );

        GameObject screenCloseButton = GameObject.FindWithTag("closeButton");
        if (!screenCloseButton) RuntimeError.Warning("Current Screen doesn't have a close button");  

        return currentScreen.Value;

    }
    

    public static void CloseCurrentScreen()
    {
        Destroy(currentScreen.Value);
        currentScreen = new KeyValuePair<string, GameObject>();
    }
}