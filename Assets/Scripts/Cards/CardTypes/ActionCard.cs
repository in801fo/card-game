using System.Collections.Generic;
using System.Linq;

public class ActionCard : Card
{
    public override void HandleCardLogic()
    {
        if (!cardData.Heals) HandleDamage();
        else HandleHealing();

        HandleReadyToUse();
    }

    private void HandleDamage()
    {
        //when dealing damage we gather the list of affected players
        List<ulong> playerIds = ConsequenceTargetHandler.GetPlayerIdsForConsequenceTarget(
            cardData.consequenceTarget,
            cardData.numberOfAffectedPlayers,
            true,
            cardData.affectedTags
        );

        //if this card uses a player consequence screen to decide whom should get damaged the previous method call will return null...
        if (playerIds == null)
        {
            //that means that a screen has been opened and when the player has decided the players the "OnDoneDeciding" action will be called and it'll return
            //a list containing all included players
            PlayerConsequenceScreenHandler.OnDoneDeciding +=
                (List<ulong> ids) => HpManager.Instance.LowerHpSpecificGroupServer_Rpc(cardData.damageAmount, ids.ToArray());
        }
        else
            HpManager.Instance.LowerHpSpecificGroupServer_Rpc(cardData.damageAmount, playerIds.ToArray());

        attackedPlayers = playerIds.ToArray();
    }

    private void HandleHealing()
    {
        if (cardData.damageAmount == 0)
        {
            //if the healing amount is zero than we must be using a regenhealth effect!
            if (cardData.cardEffects != null &&
                cardData.cardEffects.ToList().FindIndex((effectData data) => data.effect == effectsEnum.REGENHEALTH) == -1)
                RuntimeMsg.Warning($"Healing action card {cardData.Name} does not contain effect {effectsEnum.REGENHEALTH}!",
                    $"This warning is not catastrophic just telling you that the {cardData.Name} card is useless or at least does not heal...");

            /*
                A problem here is that I have to specify, for the effect currently considered (HealthRegen) its parameters
                and affectedPlayers. This should not be the case as all effects must be set in the inspector in the card scriptable.
                TODO: implement a way to make effects more customizable for every single card 
            */
            return;
        }
        else HpManager.Instance.IncrementHpServer_Rpc(cardData.damageAmount, GameManager.localPlayerInfo.playerId);
    }

    /// <summary>
    /// Reduces the card's durability and handles effects and SFXs
    /// </summary>
    /// <param name="affectedPlayers"></param>
    private void HandleReadyToUse()
    {
        ReduceCardUse();
        HandleEffectsAndSFX();
    }

    

    private void HandleLowerHpServerSingle(List<ulong> playerIds)
    {
        RuntimeMsg.Info("---Single Player---", "Included Player: " + playerIds[0]);
        HpManager.Instance.LowerHpServer_Rpc(cardData.damageAmount, playerIds[0]);
    }

    private void LowerHpSpecificGroupServer(List<ulong> playerIds)
    {
        for (int i = 0; i < playerIds.Count; i++)
        {
            RuntimeMsg.Info("---Multiple Players---", "Included Player: " + playerIds[i]);
        }
        HpManager.Instance.LowerHpSpecificGroupServer_Rpc(cardData.damageAmount, playerIds.ToArray());
    }

    private void OnDestroy()
    {
        PlayerConsequenceScreenHandler.OnDoneDeciding -= LowerHpSpecificGroupServer;
        PlayerConsequenceScreenHandler.OnDoneDeciding -= HandleLowerHpServerSingle;
    }
}