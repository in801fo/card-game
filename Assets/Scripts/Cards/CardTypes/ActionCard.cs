using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;

public class ActionCard : Card
{
    public override void UseCard()
    {
        base.UseCard();
        DecideDamageArea();
        if (cardData.cardEffects == null || cardData.cardEffects.Length == 0) return;
            EffectManager.ApplyEffects(cardData.cardEffects.ToList());
            
        OnCardUseReady?.Invoke(this);
    }

    public void DecideDamageArea()
    {

        switch (cardData.consequenceTarget)
        {
            case consequenceTarget.LOCAL:
                HpManager.Instance.LowerHpServer_Rpc(cardData.damageAmount, NetworkManager.Singleton.LocalClientId);
                break;
            case consequenceTarget.ALL_EX:
                HpManager.Instance.LowerHpSpecificGroupServer_Rpc(cardData.damageAmount, GameManager.playersDict.Values
                                                                                    .Select((playerInfo info) => info.playerId)
                                                                                    .ToArray()
                                                                                    .Where((ulong id) => id != NetworkManager.Singleton.LocalClientId).ToArray());
                break;
            case consequenceTarget.ALL_INC:
                HpManager.Instance.LowerHpSpecificGroupServer_Rpc(cardData.damageAmount, GameManager.playersDict.Values
                                                                                    .ToArray()
                                                                                    .Select((playerInfo info) => info.playerId)
                                                                                    .ToArray());
                break;
            case consequenceTarget.SPECIFIC_SINGLE:
                GameScreensManager.Instance.AskForSinglePlayer(cardData.consequenceTarget);
                PlayerConsequenceScreenHandler.OnDoneDeciding += HandleLowerHpServerSingle;
                break;
            default:
                HandleRequestForSpecificGroup();
                PlayerConsequenceScreenHandler.OnDoneDeciding += LowerHpSpecificGroupServer;
                break;
        }
    }
    
    private void HandleRequestForSpecificGroup()
    {
        if (cardData.numberOfAffectedPlayers > 0)
            GameScreensManager.Instance.AskForPlayerGroup(cardData.numberOfAffectedPlayers, cardData.consequenceTarget);
        else HpManager.Instance.HandleAffectedTagsServer_Rpc(cardData.affectedTags, cardData.damageAmount, cardData.Heals);
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