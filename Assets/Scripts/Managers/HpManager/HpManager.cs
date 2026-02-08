using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class HpManager : NetworkBehaviour
{
    public static HpManager Instance;

    /// <summary>
    /// Local player's Health Points
    /// </summary>
    public float Hp { get; private set; } = maxHp;

    public const float maxHp = 100;

    /// <summary>
    /// First int is the playerId of the player which has lost/gained the amount specified by the float 
    /// </summary>
    public static Action<ulong, float> OnHealthChange;
    public static Action<ulong> OnHealthZero;

    private Dictionary<ulong, float> playerHps = new Dictionary<ulong, float>();

    public override void OnNetworkSpawn()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);

        InitializePlayersHps();
    }

    private void InitializePlayersHps()
    {
        List<ulong> playerIds = GameManager.playersDict.Keys.ToList();
        RuntimeMsg.Info("Executing initialization...");

        for (int i = 0; i < playerIds.Count; i++)
        {
            playerHps.Add(playerIds[i], HpManager.maxHp);
        }

    }

    /// <summary>
    /// Ask the server to reduce the HPs of the player specified by the id
    /// </summary>
    /// <param name="damage">The amount of damage to apply</param>
    /// <param name="clientId">The client id to damage</param>
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void LowerHpServer_Rpc(float damage, ulong clientId)
    {
        LowerHpSpecificGroupServer_Rpc(damage, new ulong[1] { clientId });
    }

    /// <summary>
    /// Ask the server to reduce the HPs of the player specified by the id
    /// </summary>
    /// <param name="damage">The amount of damage to apply</param>
    /// <param name="clientId">The client id to damage</param>
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void LowerHpSpecificGroupServer_Rpc(float damage, ulong[] playerIds)
    {
        for (int i = 0; i < playerIds.Length; i++)
        {
            ulong senderClientId = playerIds[i];
            RuntimeMsg.Info($"Received Request to lower HP for {senderClientId}");

            float playerHpAmount;

            if (!SafeGetPlayerHps(senderClientId, out playerHpAmount)) continue;

            if (playerHpAmount - damage <= 0)
            {
                playerHpAmount = 0;
                OnHealthZeroClient_Rpc(senderClientId);
            }
            else
            {
                playerHpAmount -= damage;
                OnHealthChangeClient_Rpc(senderClientId, -damage);
            }

            playerHps[senderClientId] = playerHpAmount;
        }
    }



    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Server)]
    public void OnHealthZeroClient_Rpc(ulong playerId)
    {
        RuntimeMsg.Info("Received: OnHealthZero");
        OnHealthZero?.Invoke(playerId);
    }


    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Server)]
    public void OnHealthChangeClient_Rpc(ulong playerId, float amount)
    {
        RuntimeMsg.Info("Received: OnHealthChangeClientRpc");
        if (playerId == NetworkManager.LocalClientId) Hp -= amount;
        OnHealthChange?.Invoke(playerId, amount);
    }

    public void IncrementHpServer_Rpc(float amount, ulong playerIds)
    {
        IncrementHpSpecificGroupServer_Rpc(amount, new ulong[1] { playerIds });
    }

    /// <summary>
    /// Ask the server to increment the HPs of the player specified by the id
    /// </summary>
    /// <param name="amount">The amount of damage</param>
    /// <param name="serverRpcParams">Leave this as it is. Not passing directly the playerId from the clients as it is bad practice since it would be pretty easy for hackers to send a playerId which is not theirs.</param>

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void IncrementHpSpecificGroupServer_Rpc(float amount, ulong[] playerIds)
    {
        for (int i = 0; i < playerIds.Length; i++)
        {
            ulong senderClientId = playerIds[i];
            RuntimeMsg.Info($"Received Request to increment HP for {senderClientId}");

            float currentAmountSender;


            if (!SafeGetPlayerHps(senderClientId, out currentAmountSender)) continue;

            //cap on the server the health change if needed
            if (currentAmountSender + amount >= maxHp)
            {
                //maybe in the future you could add a "shield" which is made of the surplus of hps garnered through the cards
                currentAmountSender = maxHp;

                //useful only in the case in which _hp + amount > maxHp
                amount = maxHp - Hp;
            }
            else currentAmountSender += amount;

            playerHps[senderClientId] = currentAmountSender;

            //once capping is done broadcast to all clients the amount and the player which received the damage
            OnHealthChangeClient_Rpc(senderClientId, amount);
        }

    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void HandleAffectedTagsServer_Rpc(playerTagsEnum[] affectedTags, float amount, bool cardHeals = false)
    {
        List<playerInfo> players = GameManager.playersDict.Values.ToList();
        //check for every single passed tag...
        for (int i = 0; i < affectedTags.Length; i++)
        {
            //...if any of the players has it
            foreach (playerInfo player in players)
            {
                List<playerTagsEnum> currentPlayerTagsEnums = EnumMaskHandler<playerTagsEnum>.ExtractPlayerTagsFromMask(player.playerTagsMask);

                //if a player has the current tag then handle the damage/the health increase
                if (currentPlayerTagsEnums.Contains(affectedTags[i]))
                    HandleDamageOrHealFromServer(amount, player.playerId, cardHeals);
            }
        }
    }

    ///<summary> 
    /// Decides wether the damage inflicted is actually an amount of hps to give to the specified player, 
    /// or just a damage amount, all based on the sign of the provided damage
    /// </summary>     
    private void HandleDamageOrHealFromServer(float damage, ulong affected, bool heals)
    {
        if (heals) IncrementHpServer_Rpc(damage, affected);
        else LowerHpServer_Rpc(damage, affected);
    }

    /// <summary>
    /// Tries to get the current HPs for the player specified by the clientId
    /// </summary>
    /// <param name="clientId">The id of the player which one wants to retrieve</param>
    /// <param name="currentHps">The variable in which to put the retrieved value</param>
    /// <returns>False if the player specified wasn't found in the dictionary.<para> True if otherwise.</para></returns>
    private bool SafeGetPlayerHps(ulong clientId, out float currentHps)
    {
        try
        {
            currentHps = playerHps[clientId];
        }
        catch (KeyNotFoundException e)
        {
            RuntimeMsg.Error(e);
            currentHps = -1;
            return false;
        }

        return true;
    }

    public float GetPlayerHps(ulong playerId)
    {
        try
        {
            return playerHps[playerId];
        }
        catch (Exception)
        {
            RuntimeMsg.Error($"Unable to get player hp data for player {playerId}");
            return -1;
        }
    }

}
