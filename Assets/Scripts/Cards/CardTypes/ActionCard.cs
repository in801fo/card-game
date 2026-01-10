using System;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class ActionCard : Card
{
    [field: SerializeField] public float damangeAmount { get; protected set; }

    [field: SerializeField] public consequenceTarget consequenceTarget { get; private set; }

    public override void UseCard()
    {
        base.UseCard();
        DecideDamageArea();
        if (cardData.cardEffects != null && cardData.cardEffects.Length > 0)
            EffectManager.ApplyEffects(cardData.cardEffects.ToList());
    }

    public void DecideDamageArea()
    {
        
        //.Where((playerInfo info) => info.playerId != NetworkManager.Singleton.LocalClientId));
        switch (consequenceTarget)
        {
            case consequenceTarget.LOCAL:
                HpManager.Instance.LowerHpServer_Rpc(damangeAmount, NetworkManager.Singleton.LocalClientId);
                break;
            case consequenceTarget.ALL_EX:
                HpManager.Instance.LowerHpSpecificGroupServer_Rpc(damangeAmount, GameManager.playersDict.Values
                                                                                    .Select((playerInfo info) => info.playerId)
                                                                                    .ToArray()
                                                                                    .Where((ulong id) => id != NetworkManager.Singleton.LocalClientId).ToArray());
            
                break;
            case consequenceTarget.ALL_INC:
                HpManager.Instance.LowerHpSpecificGroupServer_Rpc(damangeAmount, GameManager.playersDict.Values
                                                                                    .ToArray()
                                                                                    .Select((playerInfo info) => info.playerId)
                                                                                    .ToArray());
                break;
            case consequenceTarget.SPECIFIC_SINGLE:
                HpManager.Instance.LowerHpServer_Rpc(damangeAmount, AskForPlayer());
                break;
            case consequenceTarget.SPECIFIC_GROUP:
                HpManager.Instance.LowerHpSpecificGroupServer_Rpc(damangeAmount, AskForPlayerGroup(false));

                break;
        }
    }

    /// <summary>
    /// Creates a UI screen to ask the player which players to damage
    /// TODO: move it somewhere else
    /// </summary>
    /// <param name="localExclusive">Should the local player be excluded from the damage</param>
    /// <returns></returns>
    private ulong[] AskForPlayerGroup(bool localExclusive = false)
    {

    }

    private ulong AskForPlayer()
    {

    }
}