using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        {effectsEnum.TIREDNESS, typeof(Tiredness)},
        {effectsEnum.REGENHEALTH, typeof(HealthRegen)},
        {effectsEnum.PERTURNDAMAGE, typeof(PerTurnDamage)}
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

            if (appliedEffectsWithTurnsOrTimeLeft[eff.effect].turnsLeft <= 0)
            {
                PlayerMaskAssigner.Instance.HandleRemoveEffectFromPlayerDataServer_Rpc((int)eff.effect, NetworkManager.LocalClientId);
                
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

        print($"Applying to local: {effectParameters.effect}");
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

        terminateMethod.Invoke(effectInstance, null);

        OnLocalPlayerEffectTerminated?.Invoke(data.effect);
    
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

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Server)]
    public void ApplyEffectClient_Rpc(ulong[] players, effectData effect)
    {
        if (!players.Contains(NetworkManager.Singleton.LocalClientId)) return;
        ApplyEffectToLocalClient(players, effect);
    }

    public void RemoveEffect(effectData[] effectsData)
    {

        for (int i = 0; i < effectsData.Length; i++)
        {
            RemoveEffectFromLocalClient(effectsData[i]);
        }
    }

    /// <summary>
    /// Method to request the host/server to apply the passed effects
    /// </summary>
    /// <param name="cardTarget">The consequence target for the card's attack</param>
    /// <param name="parameters">The data for all the card's effects</param>
    /// <param name="cardsAffectedPlayers">An array containing all of the ids affected by the player (used in case the <c>cardTarget</c> requires a consequence screen)</param>
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void RequestApplyEffectsServer_Rpc(consequenceTarget cardTarget, effectData[] parameters, ulong[] cardsAffectedPlayers)
    {
        for (int i = 0; i < parameters.Length; i++)
        {
            //if the effect's target requires a consequence screen override the effect's target and set it to the card's target
            if (parameters[i].effectTarget != consequenceTarget.LOCAL
                && parameters[i].effectTarget != consequenceTarget.RANDOM_MULTIPLE_RANDOM
                    && parameters[i].effectTarget != consequenceTarget.RANDOM_SINGLE_INC
                        && parameters[i].effectTarget != consequenceTarget.RANDOM_SINGLE_EX)
                parameters[i].effectTarget = cardTarget;

            //if the effect's target has been overridden its affected players should be equal to the card's ones
            //doing this as I cannot open a screen for the effect
            ulong[] affectedPlayers = (parameters[i].effectTarget == cardTarget && cardsAffectedPlayers != null) ?
                cardsAffectedPlayers : ConsequenceTargetHandler.GetPlayerIdsForConsequenceTarget(parameters[i].effectTarget).ToArray(); 
            
            ApplyEffectClient_Rpc(affectedPlayers, parameters[i]);
            //handle updating the data representing the player in question with the current effect
            PlayerMaskAssigner.Instance.HandleAddEffectToPlayerDataServer_Rpc((int)parameters[i].effect, affectedPlayers[i]);
        }
   
    }
}

