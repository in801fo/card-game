using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerConsequenceScreenInitializer : ScreenInitializer<playerConsequenceScreenInitializerStruct>
{

    [SerializeField] private GameObject playerEntryPrefab;
    [SerializeField] private GameObject playerListContentGO;
    [SerializeField] private Button doneButton;
    
    public override void Initialize(playerConsequenceScreenInitializerStruct initializingValues)
    {
        SetScreenHeading(initializingValues.screenHeading);

        PlayerConsequenceScreenHandler handler = this.AddComponent<PlayerConsequenceScreenHandler>();
        handler.Initialize(initializingValues);

        doneButton.onClick.AddListener(handler.HandleDoneButton);

        //if we're in a case in which the player has started a game with very little people
        //and this card needs an amount bigger than the number of players
        //TODO: ALSO UN-COMMENT THE CODE BELOW WHEN DEVELOPMENT IS DONE!!! 
        /*initializingValues.target = (initializingValues.maxCount > GameManager.playerCount.Value &&
                                           initializingValues.target == consequenceTarget.SPECIFIC_GROUP_EX ||
                                            initializingValues.target == consequenceTarget.SPECIFIC_GROUP_INC) ?
                                                consequenceTarget.ALL_INC : initializingValues.target;
        */
        //just a precautionary measure as usually in the target passed thought initializingValues
        //the only values passed are consequenceTarget.SPECIFIC_GROUP_EX or consequenceTarget.SPECIFIC_GROUP_INC 
        //TODO: UN-COMMENT THIS CODE BELOW WHEN DEVELOPMENT IS DONE!!!!
        //if (initializingValues.target == consequenceTarget.SPECIFIC_GROUP_EX ||
        //    initializingValues.target == consequenceTarget.SPECIFIC_GROUP_INC)
                CreateEntries(initializingValues.playersInfo, initializingValues.target);
        //else GameScreensManager.CloseCurrentScreen(); 


        base.Initialize(initializingValues);
    }

    private void CreateEntries(List<playerInfo> players, consequenceTarget target)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].playerId == NetworkManager.Singleton.LocalClientId &&
                target == consequenceTarget.SPECIFIC_GROUP_EX)
                continue;

            PlayerEntry currentPlayerEntry = HandleSpawnOfPlayerEntry(players[i], target);
            HandleTogglePlayerEntry(currentPlayerEntry, players[i], target);
        }
    }

    public void UpdateScreenHeading(string str)
    {
        SetScreenHeading(str);
    }

    /// <summary>
    /// Handles initialization of player entry
    /// </summary>
    /// <param name="representing"></param>
    /// <param name="target"></param>
    private PlayerEntry HandleSpawnOfPlayerEntry(playerInfo representing, consequenceTarget target)
    {
        GameObject currentInstance = Instantiate(playerEntryPrefab);
        PlayerEntry currentPlayerEntry = currentInstance.GetComponent<PlayerEntry>();
        currentPlayerEntry.Initialize(representing);
        currentInstance.transform.SetParent(playerListContentGO.transform);

        return currentPlayerEntry;
    }

    /// <summary>
    /// Handles initialization of a playerEntry's toggle
    /// </summary>
    /// <param name="currentPlayerEntry"></param>
    /// <param name="representing"></param>
    /// <param name="target"></param>
    private void HandleTogglePlayerEntry(PlayerEntry currentPlayerEntry,
                                        playerInfo representing,
                                        consequenceTarget target)
    {
        //if the current player is the local player and the target is a SPECIFIC_GROUP_INC
        //then the player is included no matter what and we must make sure this is reflected 
        if (representing.playerId == NetworkManager.Singleton.LocalClientId
            && target == consequenceTarget.SPECIFIC_GROUP_INC)
        {
            currentPlayerEntry.SetTickValue(true);
            //make it so the player cannot un-tick themselves
            currentPlayerEntry.SetInteractability(false);
        }
    }
}

public struct playerConsequenceScreenInitializerStruct
{
    public List<playerInfo> playersInfo;
    public string screenHeading;
    public consequenceTarget target;
    /// <summary>
    /// Only used in cases in which the effect/damage for a specific group includes a limit to how many players can be affected
    /// </summary>
    public int maxCount;

}
