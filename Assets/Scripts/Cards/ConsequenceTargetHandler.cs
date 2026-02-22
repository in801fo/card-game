using System;
using System.Collections.Generic;
using System.Linq;
using GoodRand = UnityEngine.Random;
public static class ConsequenceTargetHandler
{
    public static Action<List<ulong>> OnDoneGettingPlayers;

    /// <summary>
    /// Gets the playerIds of all affected  players based on the consequence target
    /// </summary>
    /// <param name="target">The scope of the attack</param>
    /// <param name="affectedPlayers">The number of affected players (-1 if you want only a specific tag to be affected)</param>
    /// <param name="allowConsequenceTargetScreen">Allow for the screen to choose to which specific player/group of players to receive the consequence</param>
    /// <returns>Null if the a screen to choose a consequence target has been opened, otherwise the list containing all included players. If <c>allowConsequenceTargetScreen</c><br></returns>
    public static List<ulong> GetPlayerIdsForConsequenceTarget(consequenceTarget target, int affectedPlayers = -1, bool allowConsequenceTargetScreen = false, playerTagsEnum[] affectedTags = null)
    {
        switch (target)
        {
            case consequenceTarget.LOCAL:
                return new List<ulong> { GameManager.localPlayerInfo.playerId };
            case consequenceTarget.ALL_EX:
                return GameManager.playersDict.Keys.ToList().Where((ulong id) => id != GameManager.localPlayerInfo.playerId).ToList();

            case consequenceTarget.ALL_INC:
                return GameManager.playersDict.Keys.ToList();

            case consequenceTarget.SPECIFIC_SINGLE:
                if (allowConsequenceTargetScreen)
                {
                    PlayerConsequenceScreenInitializer.AskForSinglePlayer(target);
                    PlayerConsequenceScreenHandler.OnDoneDeciding += HandleDoneDeciding;
                }
                return allowConsequenceTargetScreen ? null : new List<ulong>();
            case consequenceTarget.RANDOM_SINGLE_INC:
                return new List<ulong> { GameManager.playersDict.Keys.ToList()[GoodRand.Range(0, GameManager.playersDict.Count)] };
            case consequenceTarget.RANDOM_SINGLE_EX:
                /*
                    Here's what happens below:
                    Out of the Key's list I pull out all of the players except for the local player
                    Then I convert the result back to a list and out of that list I pull out a random player 
                */
                return new List<ulong> {
                    ExtractAllPlayersExceptForLocal()[GoodRand.Range(0, GameManager.playersDict.Count-1)]
                };

            case consequenceTarget.RANDOM_MULTIPLE_RANDOM:
                return GameManager.playersDict.Keys.ToList().GetRange(0, GoodRand.Range(1, GameManager.playersDict.Count));
                
            default:
                //if the screen is allowed and the affected players is > 0
                if (allowConsequenceTargetScreen && affectedPlayers > 0)
                {
                    PlayerConsequenceScreenInitializer.AskForPlayerGroup(affectedPlayers, target);
                    PlayerConsequenceScreenHandler.OnDoneDeciding += HandleDoneDeciding;

                } else if (affectedPlayers < 0) //otherwise return all players with tag
                    return GameManager.GetAllPlayersWithTags(affectedTags);

                return allowConsequenceTargetScreen ? null : new List<ulong>();
        }
    }
    
    private static List<ulong> ExtractAllPlayersExceptForLocal()
    {
        return GameManager.playersDict.Keys.ToList()
                .Where((ulong id) => id != GameManager.localPlayerInfo.playerId).ToList();
    }

    private static void HandleDoneDeciding(List<ulong> list)
    {
        OnDoneGettingPlayers?.Invoke(list);
    }
}