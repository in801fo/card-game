using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;

public class PlayerTagAssigner : NetworkBehaviour
{

    //TODO: change this shit, smells like my ass
    private readonly string[] southerners = { "In801fo", "Rin", "Wocy", "Info" };

    private void Awake()
    {
        InventoryManager.OnCardAddedToHandDeck += HandleHasMeridione;
        InventoryManager.OnGroupCardAddedToHandDeck += CheckInGroupHasMeridione;
    }

    private void CheckInGroupHasMeridione(List<CardScriptable> cards)
    {
        for (int i = 0; i < cards.Count; i++)
        {
            HandleHasMeridione(cards[i]);
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void HandleHasMeridioneServer_Rpc(ulong caller, cardTypeEnum Type, FixedString64Bytes Name)
    {
        if (Type == cardTypeEnum.CHARACTER &&
            southerners.Contains(Name.ToString()))
            AssignTagClient_Rpc(caller, playerTagsEnum.HAS_MERIDIONE);
    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Server)]
    private void AssignTagClient_Rpc(ulong caller, playerTagsEnum playerTag)
    {
        if (caller == NetworkManager.LocalClientId)
            GameManager.Instance.HandleAddTagsServer_Rpc((ushort)playerTag, caller);
    }

    private void HandleHasMeridione(CardScriptable scriptable)
    {
        FixedString64Bytes scriptableName = new FixedString64Bytes(scriptable.Name);
        HandleHasMeridioneServer_Rpc(NetworkManager.LocalClientId, scriptable.Type, scriptableName);
    }
}