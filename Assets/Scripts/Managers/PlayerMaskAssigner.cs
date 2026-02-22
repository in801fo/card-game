using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using Unity.VisualScripting;

/// <summary>
/// Handles assigning the player with tags required to do some stuff to only players which satisfy specific requirements.
/// <para>The checks are done on the server</para>
/// </summary>
public class PlayerMaskAssigner : NetworkBehaviour
{

    //TODO: change this shit, smells like my ass
    private readonly string[] southerners = { "In801fo", "Rin", "Wocy", "Info" };

    public static PlayerMaskAssigner Instance { get; private set; }

    public override void OnNetworkSpawn()
    {
        InventoryManager.OnCardAddedToHandDeck += HandleHasMeridioneClient;
        InventoryManager.OnGroupCardAddedToHandDeck += CheckInGroupHasMeridione;
        HpManager.OnHealthChange += CheckHasJustReceivedDamage;
        EffectsManager.OnLocalPlayerEffectFelt += AddEffectMask;
        EffectsManager.OnLocalPlayerEffectTerminated += RemoveEffectMask;

        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void RemoveEffectMask(effectsEnum effect)
    {
        HandleRemoveEffectFromPlayerDataServer_Rpc((int)effect, NetworkManager.LocalClientId);
    }

    private void CheckHasJustReceivedDamage(ulong playerId, float amount)
    {
        if (playerId != GameManager.localPlayerInfo.playerId) return;

        CheckHasJustReceivedDamageServer_Rpc(playerId, amount);
    }

    private void AddEffectMask(effectsEnum value)
    {
        HandleAddEffectToPlayerDataServer_Rpc((int)value, NetworkManager.LocalClientId);
    }

    /// <summary>
    /// Adds to the specified player the given tag
    /// </summary>
    /// <param name="tag"></param>
    /// <param name="playerId">The id of the player to which to add the tag</param>

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void HandleAddPlayerTagsServer_Rpc(int tag, ulong playerId)
    {
        SafeHandleAddToMask<playerTagsEnum>((playerTagsEnum)tag, playerId);

        RuntimeMsg.Info($"Added tag: {(playerTagsEnum)tag} to player {playerId}");

        //updating the dictionary through the OnListChanged event of the playerNetList
    }

    /// <summary>
    /// Adds to the specified player's data the given effect
    /// </summary>
    /// <param name="effect">The effect whished to be added to the player's effect mask</param>
    /// <param name="playerId">The id of the desired player</param>
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void HandleAddEffectToPlayerDataServer_Rpc(int effect, ulong playerId)
    {
        SafeHandleAddToMask<effectsEnum>((effectsEnum)effect, playerId);
        RuntimeMsg.Info($"Added {(effectsEnum)effect} effect to {playerId}'s player mask");
    }


    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void HandleRemoveEffectFromPlayerDataServer_Rpc(int effect, ulong playerId)
    {
        SafeHandleRemoveFromMask<effectsEnum>((effectsEnum)effect, playerId);

        print($"Removed {(effectsEnum)effect} from {playerId}'s mask");
    }

    /// <summary>
    /// Safely adds a tag (either of effect type of just a player tag) to the corresponding player's mask if not already
    /// present (either of type <c>effectsEnum</c> or <c>playerTagsEnum</c>)
    /// </summary>
    /// <typeparam name="T">The type we're dealing with (either <c>effectsEnum</c> or <c>playerTagsEnum</c>)</typeparam>
    /// <param name="enumValue">The value to add to the masks</param>
    /// <param name="playerId">The id of the player in question</param>
    private void SafeHandleAddToMask<T>(T enumValue, ulong playerId) where T : Enum
    {
        playerInfo info = GameManager.playersDict[playerId];


        if (typeof(T) == typeof(playerTagsEnum))
            EnumMaskHandler<T>.SafeAddTagToMask(ref info.playerTagsMask, enumValue);
        else
            EnumMaskHandler<T>.SafeAddTagToMask(ref info.playerEffectsMask, enumValue);

        //change the corresponding player data on the server which will lead to the local client receiving the change
        //and applying on itself
        GameManager.Instance.ModifyLocalPlayerServer_Rpc(info, info.playerId);
    }

    private void SafeHandleRemoveFromMask<T>(T enumValue, ulong playerId) where T : Enum
    {
        playerInfo info = GameManager.playersDict[playerId];


        if (typeof(T) == typeof(playerTagsEnum))
            EnumMaskHandler<T>.SafeRemoveTagFromMask(ref info.playerTagsMask, enumValue);
        else
            EnumMaskHandler<T>.SafeRemoveTagFromMask(ref info.playerEffectsMask, enumValue);

        //change the corresponding player data on the server which will lead to the local client receiving the change
        //and applying on itself
        GameManager.Instance.ModifyLocalPlayerServer_Rpc(info, info.playerId);
    }

    private void CheckInGroupHasMeridione(List<CardScriptable> cards)
    {
        for (int i = 0; i < cards.Count; i++)
        {
            HandleHasMeridioneClient(cards[i]);
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void CheckHasMeridioneServer_Rpc(ulong caller, cardTypeEnum Type, FixedString64Bytes Name)
    {
        if (Type == cardTypeEnum.CHARACTER &&
            southerners.Contains(Name.ToString()))
        {
            AssignTagClient_Rpc(caller, playerTagsEnum.HAS_MERIDIONE);
        }
    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Server)]
    private void AssignTagClient_Rpc(ulong caller, playerTagsEnum playerTag)
    {
        if (caller == NetworkManager.LocalClientId)
        {
            print($"Must add tag: {playerTag}");
            HandleAddPlayerTagsServer_Rpc((int)playerTag, caller);
        }
    }

    private void HandleHasMeridioneClient(CardScriptable scriptable)
    {
        FixedString64Bytes scriptableName = new FixedString64Bytes(scriptable.Name);
        CheckHasMeridioneServer_Rpc(NetworkManager.LocalClientId, scriptable.Type, scriptableName);
    }
    
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void CheckHasJustReceivedDamageServer_Rpc(ulong caller, float amount)
    {
        //if it's damage (negative for damage, positive for healing)
        if (amount < 0)
            AssignTagClient_Rpc(caller, playerTagsEnum.HAS_JUST_RECEIVED_DAMAGE);
    }

}