using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

//TODO: implement a way to Remove tags (very easy)

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
        InventoryManager.OnCardAddedToHandDeck += HandleHasMeridione;
        InventoryManager.OnGroupCardAddedToHandDeck += CheckInGroupHasMeridione;
        EffectsManager.OnLocalPlayerEffectFelt += AddEffectMask;

        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void AddEffectMask(effectsEnum value)
    {
        HandleAddEffectToPlayerDataServer_Rpc((ushort)value, NetworkManager.LocalClientId);
    }

    /// <summary>
    /// Adds to the specified player the given tag
    /// </summary>
    /// <param name="tag"></param>
    /// <param name="playerId">The id of the player to which to add the tag</param>

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void HandleAddPlayerTagsServer_Rpc(ushort tag, ulong playerId)
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
    public void HandleAddEffectToPlayerDataServer_Rpc(ushort effect, ulong playerId)
    {
        SafeHandleAddToMask<effectsEnum>((effectsEnum)effect, playerId);
        RuntimeMsg.Info($"Added {(effectsEnum)effect} effect to {playerId}'s player mask");
    }


    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void HandleRemoveEffectFromPlayerDataServer_Rpc(ushort effect, ulong playerId)
    {
        SafeHandleRemoveFromMask<effectsEnum>((effectsEnum)effect, playerId);
        
        print($"Removed {(effectsEnum)effect} from {playerId}'s mask");
    }

    /// <summary>
    /// Handles the logic of adding values to the player's masks (either of type <c>effectsEnum</c> or <c>playerTagsEnum</c>)
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
            HandleHasMeridione(cards[i]);
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
            HandleAddPlayerTagsServer_Rpc((ushort)playerTag, caller);
        }
    }

    private void HandleHasMeridione(CardScriptable scriptable)
    {
        FixedString64Bytes scriptableName = new FixedString64Bytes(scriptable.Name);
        CheckHasMeridioneServer_Rpc(NetworkManager.LocalClientId, scriptable.Type, scriptableName);
    }

}