using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerConsequenceScreenHandler : MonoBehaviour
{
    public static PlayerConsequenceScreenHandler Instance;

    private playerConsequenceScreenInitializerStruct initializerStruct;
    /// <summary>
    /// The list of included players
    /// </summary>
    private Dictionary<ulong, bool> players = new Dictionary<ulong, bool>();

    public static Action<ulong> OnLimitPlayersIncluded;
    public static Action<List<ulong>> OnDoneDeciding;

    private PlayerConsequenceScreenInitializer playerConsequenceScreenInitializer;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);
        playerConsequenceScreenInitializer = GetComponent<PlayerConsequenceScreenInitializer>();
    }

    public void Initialize(playerConsequenceScreenInitializerStruct initStruct)
    {
        initializerStruct = initStruct;
    }

    public void HandlePlayerTicked(ulong playerId, bool value)
    {
        if (players.Keys.Count + 1 > initializerStruct.maxCount)
        {
            OnLimitPlayersIncluded?.Invoke(playerId);
            return;
        }

        //if the player wants to add a specific key (value is true) and the key is not contained in the dict, then add it
        if (value && !players.ContainsKey(playerId)) players.Add(playerId, value);
        //if the player wants to remove a specific key (value is false) and the key is contained in the dict, then remove it
        if (!value && players.ContainsKey(playerId)) players.Remove(playerId);
        
        //update the screen heading for better player experience
        playerConsequenceScreenInitializer.
            UpdateScreenHeading($"Select {initializerStruct.maxCount} players ({initializerStruct.maxCount - players.Keys.Count} left)");
    }

    public void HandleDoneButton()
    {
        if (players.Count == initializerStruct.maxCount)
        {
            OnDoneDeciding?.Invoke(players.Keys.ToList());
            //not subscribing GameScreensManager to OnDoneDeciding as that would decrease modularity
            //and also entanglement between classes (which is no good!)
            ScreensManager.CloseScreen(playerConsequenceScreenInitializer.screenID);
        }
        else RuntimeMsg.Warning("Not enough players selected!");
    }


}