using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using NUnit.Framework;
using Unity.Netcode;
using Unity.VisualScripting;


//TODO: add a way to remove effects after a certain amount of time
public class EffectsManager : NetworkBehaviour
{

    public static EffectsManager Instance { get; private set; }

    private const string applyMethodName = "Apply";
    private const string initializeMethodName = "Initialize";
    private const string terminateMethodName = "TerminateEffect";

    public static Action<effectsEnum> OnLocalPlayerEffectFelt;
    public static Action<effectsEnum> OnLocalPlayerEffectTerminated;

    private static Dictionary<effectsEnum, Type> effectsDict = new Dictionary<effectsEnum, Type>()
    {
        {effectsEnum.BLINDNESS, typeof(Blindness)},
        {effectsEnum.CONFUSION, typeof(Confusion)},
        {effectsEnum.TIREDNESS, typeof(Tiredness)}
    };

    //this is local data
    private static Dictionary<effectsEnum, effectData> appliedEffectsWithTurnsOrTimeLeft = new Dictionary<effectsEnum, effectData>();

    public override void OnNetworkSpawn()
    {
        if (Instance == null) Instance = this;
        else Destroy(Instance);

        if (IsServer) TurnManager.OnTurnOver += UpdateEffectsDictOnTurnOver;
        else TurnManager.OnLocalTurnOver += UpdateEffectsDictOnTurnOver;
    }

    /// <summary>
    /// Updates the <c>appliedEffectsWithTurnsOrTimeLeft</c> dictionary also telling the server to remove te effect's tag from its player data
    /// </summary>
    private void UpdateEffectsDictOnTurnOver()
    {
        List<effectData> currentEffects = appliedEffectsWithTurnsOrTimeLeft.Values.ToList();

        foreach (effectData eff in currentEffects)
        {
            if (appliedEffectsWithTurnsOrTimeLeft[eff.effect].doesTurns)
            {
                appliedEffectsWithTurnsOrTimeLeft[eff.effect] = new effectData()
                {
                    doesTurns = eff.doesTurns,
                    effect = eff.effect,
                    turnsLeft = eff.turnsLeft - 1,
                    timeLeft = eff.timeLeft
                };
            }

            if (appliedEffectsWithTurnsOrTimeLeft[eff.effect].turnsLeft == 0)
            {
                PlayerMaskAssigner.Instance.HandleRemoveEffectFromPlayerDataServer_Rpc((ushort)eff.effect, NetworkManager.LocalClientId);
                
                RemoveEffect(new effectData[] { eff });
                
                print($"Effect {eff.effect} has finished its effect!");
                appliedEffectsWithTurnsOrTimeLeft.Remove(eff.effect);
            }
        }
    }

    //this happens on the individual clients
    /// <summary>
    /// This method applies the passed effect to the local player
    /// </summary>
    /// <param name="playerIds">The list of players affected by the effect</param>
    /// <param name="effectParameters">The parameters for the effect in question</param>
    private static void ApplyEffectToLocalClient(ulong[] playerIds, effectData effectParameters)
    {
        if (!effectsDict.ContainsKey(effectParameters.effect))
        {
            RuntimeMsg.Error("Unable to apply effect",
                            $"Unable to apply effect: {effectParameters.effect} because it's not registered in the effectsDict dictionary (silly mistake oopsie)");

            return;
        }

        if (!effectsDict[effectParameters.effect].IsSubclassOf(typeof(Effect)))
        {
            RuntimeMsg.Error("Effects list contains intruder", $"Effects dictionary contains {effectsDict[effectParameters.effect].FullName}, which does not inherit from abstract class Effect.");
            return;
        }

        if (!playerIds.Contains(GameManager.localPlayerInfo.playerId))
        {
            RuntimeMsg.Info($"Local player not affected by received effect {effectParameters.effect}");
            return;
        }

        //instantiate the effect
        object effectInstance = effectsDict[effectParameters.effect].Instantiate(false, new object[] { effectParameters });

        BindingFlags bindingFlagsOr = BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Instance;

        //get references to the methods: "Initialize" and "Apply"
        MethodInfo applyMethod = effectsDict[effectParameters.effect].GetMethod(applyMethodName, bindingFlagsOr);
        MethodInfo initializeMethod = effectsDict[effectParameters.effect].GetMethod(initializeMethodName, bindingFlagsOr);

        //if the effect hasn't already been applied
        if (!appliedEffectsWithTurnsOrTimeLeft.ContainsKey(effectParameters.effect))
        {
            appliedEffectsWithTurnsOrTimeLeft.Add(effectParameters.effect, effectParameters);

            //invoking instance methods, of course, requires the reference of the instance in question
            //passed as the first parameter

            //invoke the method "Initialize" passing the required parameters
            initializeMethod.Invoke(effectInstance, new object[] { effectParameters });
            //then invoke the apply methods
            applyMethod.Invoke(effectInstance, null);

            OnLocalPlayerEffectFelt.Invoke(effectParameters.effect);
        }

    }

    
    /// <summary>
    /// Triggers the TerminateEffect method inside the passed effect
    /// </summary>
    /// <param name="effect"></param>
    private void RemoveEffectFromLocalClient(effectData data)
    {
        if (!appliedEffectsWithTurnsOrTimeLeft.ContainsKey(data.effect))
        {
            RuntimeMsg.Error("Unable to remove effect",
                            $"Unable to remove effect: {data.effect} because it's not registered in the effectsDict dictionary (silly mistake oopsie)");

            return;
        }

        //instantiate the effect
        object effectInstance = effectsDict[data.effect].Instantiate(false, new object[] { data });

        BindingFlags bindingFlagsOr = BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Instance;

        MethodInfo terminateMethod = effectsDict[data.effect].GetMethod(terminateMethodName, bindingFlagsOr);

        //if the effect hasn't already been applied
        if (appliedEffectsWithTurnsOrTimeLeft.ContainsKey(data.effect))
        {
            terminateMethod.Invoke(effectInstance, null);
            
            OnLocalPlayerEffectTerminated?.Invoke(data.effect);
        }
    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Server)]
    public void ApplyEffectsClient_Rpc(ulong[] players, effectData[] effects)
    {
        if (!players.Contains(NetworkManager.Singleton.LocalClientId)) return;

        for (int i = 0; i < effects.Length; i++)
        {
            ApplyEffectToLocalClient(players, effects[i]);
        }
    }

    

//    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Server)]
    public void RemoveEffect(effectData[] effectsData)
    {
//        if (NetworkManager.LocalClientId != clientId) return;

        for (int i = 0; i < effectsData.Length; i++)
        {
            RemoveEffectFromLocalClient(effectsData[i]);
        }
    }

    /// <summary>
    /// Method to request the host/server to apply the passed effects
    /// </summary>
    /// <param name="affectedPlayers">The affected players</param>
    /// <param name="parameters">The data for the desired effects</param>
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void RequestApplyEffectsServer_Rpc(ulong[] affectedPlayers, effectData[] parameters)
    {
        for (int i = 0; i < affectedPlayers.Length; i++)
        {
            //check for the existance of the currentPlayer in the players dictionary
            if (!GameManager.playersDict.ContainsKey(affectedPlayers[i]))
            {
                RuntimeMsg.Warning($"PlayerId: {affectedPlayers[i]} not found while applying effects");
                continue;
            }

            //apply the effects to the current player
            ApplyEffectsClient_Rpc(affectedPlayers, parameters);
            for (int j = 0; j < parameters.Length; j++)
            {
                PlayerMaskAssigner.Instance.HandleAddEffectToPlayerDataServer_Rpc((ushort)parameters[j].effect, affectedPlayers[i]);
            }
        }
    }
}

