using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class ActionCard : Card
{
    private PlayerConsequenceScreenHandler handleToPlayerConsequenceScreen;

    public override void UseCard()
    {
        base.UseCard();
        DecideDamageArea();
        //if this card uses a player consequence screen to decide whom should get damaged...
        if (handleToPlayerConsequenceScreen != null)
            //do this...
            PlayerConsequenceScreenHandler.OnDoneDeciding += HandleReadyToUse;
    }

    public void DecideDamageArea()
    {

        switch (cardData.consequenceTarget)
        {
            case consequenceTarget.LOCAL:
                HpManager.Instance.LowerHpServer_Rpc(cardData.damageAmount, NetworkManager.Singleton.LocalClientId);

                HandleReadyToUse(new List<ulong> { GameManager.localPlayerInfo.playerId });
                break;
            case consequenceTarget.ALL_EX:
                HpManager.Instance.LowerHpSpecificGroupServer_Rpc(cardData.damageAmount, GameManager.playersDict.Keys
                                                                                    .ToArray()
                                                                                    .Where((ulong id) => id != NetworkManager.Singleton.LocalClientId)
                                                                                    .ToArray());
                
                HandleReadyToUse(GameManager.playersDict.Keys
                                            .Where((ulong id) => id != NetworkManager.Singleton.LocalClientId)
                                            .ToList());
                break;
            case consequenceTarget.ALL_INC:
                HpManager.Instance.LowerHpSpecificGroupServer_Rpc(cardData.damageAmount, GameManager.playersDict.Keys
                                                                                    .ToArray());
                HandleReadyToUse(GameManager.playersDict.Keys.ToList());
                break;
            case consequenceTarget.SPECIFIC_SINGLE:

                PlayerConsequenceScreenInitializer.AskForSinglePlayer(cardData.consequenceTarget);
                PlayerConsequenceScreenHandler.OnDoneDeciding += HandleLowerHpServerSingle;
                break;
            default:
                HandleRequestForSpecificGroup();
                PlayerConsequenceScreenHandler.OnDoneDeciding += LowerHpSpecificGroupServer;
                break;
        }
    }

    private void HandleReadyToUse(List<ulong> affectedPlayers = null)
    {
        ReduceCardUse();
        if (cardData.cardEffects != null && cardData.cardEffects.Length != 0)
            //if the affected 
            EffectManager.Instance.RequestApplyEffectServer_Rpc(affectedPlayers == null ? GameManager.playersDict.Keys.ToArray() : affectedPlayers.ToArray(), cardData.cardEffects);
        if (cardData.hasSoundEffect) 
            AudioManager.Instance.RequestPlayCardSFXServer_Rpc(Random.Range(0, cardData.onUseSoundEffect.Length), cardData.name);
        OnCardUseReady?.Invoke(this);
    }

    private void HandleRequestForSpecificGroup()
    {
        if (cardData.numberOfAffectedPlayers > 0)
            handleToPlayerConsequenceScreen = PlayerConsequenceScreenInitializer.AskForPlayerGroup(cardData.numberOfAffectedPlayers, cardData.consequenceTarget);
        else
        {
            HpManager.Instance.HandleAffectedTagsServer_Rpc(cardData.affectedTags, cardData.damageAmount, cardData.Heals);
            HandleReadyToUse(GameManager.GetAllPlayersWithTags(cardData.affectedTags));
        }
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