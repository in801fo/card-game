using System;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;

public class PlayerTagAssigner : NetworkBehaviour
{

    //TODO: change this shit, smells like my ass
    private readonly string[] southerners = {"In801fo", "Rin", "Wocy", "Info"};

    private void Awake()
    {
        InventoryManager.OnCardAddedToHandDeck += HandleHasMeridione;
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
        HandleHasMeridioneServer_Rpc(NetworkManager.LocalClientId, scriptable.Type, scriptable.Name);
    }
    

    
}