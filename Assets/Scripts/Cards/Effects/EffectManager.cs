using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Netcode;

public class EffectManager : NetworkBehaviour
{

    public static EffectManager Instance { get; private set; }

    

    private static Dictionary<effectsEnum, Type> effectsDict = new Dictionary<effectsEnum, Type>()
    {
        {effectsEnum.BLINDNESS, typeof(Blindness)},
        {effectsEnum.CONFUSION, typeof(Confusion)}
    };

    public override void OnNetworkSpawn()
    {
        if (Instance == null) Instance = this;
        else Destroy(Instance);
    }

    private static void DoIt(effectsEnum effect, ulong[] playerIds)
    {
        if (!effectsDict[effect].GetInterfaces().Contains(typeof(IEffect)))
        {
            RuntimeMsg.Error("Effects list contains intruder", $"Effects dictionary contains {effectsDict[effect].FullName}, which does not implement IEffect interface.");
            return;
        }

        if (!playerIds.Contains(GameManager.localPlayerInfo.playerId)) {
            RuntimeMsg.Info($"Local player not affected by received effect {effect}");
            return;    
        }

        MethodInfo applyMethod = effectsDict[effect].GetMethod("Apply", BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
        if (applyMethod == null) applyMethod = typeof(IEffect).GetMethod("Apply");
        applyMethod.Invoke(null, new object[] { playerIds });
    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Server)]
    public void ApplyEffectsClient_Rpc(ulong[] players, effectsEnum[] effects)
    {
        if (!players.Contains(NetworkManager.Singleton.LocalClientId)) return;

        for (int i = 0; i < effects.Length; i++)
        {
            DoIt(effects[i], players);
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void RequestApplyEffectServer_Rpc(ulong[] affectedPlayers, effectsEnum[] effects)
    {
        for (int i = 0; i < affectedPlayers.Length; i++)
        {
            if (!GameManager.playersDict.ContainsKey(affectedPlayers[i]))
            {
                RuntimeMsg.Warning($"PlayerId: {affectedPlayers[i]} not found while applying effects");
                continue;
            }

            ApplyEffectsClient_Rpc(affectedPlayers, effects);
        }
    }


}