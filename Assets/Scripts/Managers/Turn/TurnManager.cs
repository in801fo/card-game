using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class TurnManager : NetworkBehaviour
{
    [field: SerializeField] public float turnSecondsDuration { get; private set; }

    private float currentTurnTimer;

    public static ulong currentClientTurnId { get; private set; }

    public static TurnManager Instance { get; private set; }

    /// <summary>
    /// Called on the local client once the turn is over
    /// </summary>
    public static Action OnLocalTurnOver;

    /// <summary>
    /// Called on the local client once the turn has started
    /// </summary>
    public static Action OnLocalTurnStart;

    /// <summary>
    /// Called on the server once the turn has begun
    /// </summary>
    public static Action OnTurnBegin;
    /// <summary>
    /// Called on the server once the turn is over
    /// </summary>
    public static Action OnTurnOver;

    public static bool IsMyTurn;

    public override void OnNetworkSpawn()
    {
        if (Instance == null) Instance = this;
        else Destroy(Instance.gameObject);

        //set a random starting clientId so to make the loop start
        HandleTurnOverServer_RPC(1000, TurnOverReason.TURN_OVER);

        //when a turn is over
        OnTurnOver += HandleClientTurnOver;
    }


    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Server)]
    public void HandleMyTurnClient_RPC(ulong clientId)
    {
        if (clientId != NetworkManager.LocalClientId) return;
        RuntimeMsg.Info("It's my turn!");
        IsMyTurn = true;
        OnLocalTurnStart?.Invoke();
    }

    public void HandleSkipTurn()
    {
        HandleTurnOverServer_RPC(NetworkManager.LocalClientId, TurnOverReason.TURN_SKIP);
        RuntimeMsg.Info("Not my turn anymore :(");
        IsMyTurn = false;
        OnLocalTurnOver?.Invoke();
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void HandleTurnOverServer_RPC(ulong overClientId, TurnOverReason reason)
    {
        List<ulong> playerIds = GameManager.playersDict.Keys.ToList();

        //if this method was invoked because the turn time has run out
        //tell the client so...
        if (reason == TurnOverReason.TURN_OVER) 
            SignalTimeUpClient_RPC(overClientId);

        int currentId = playerIds.FindIndex((ulong id) => id == overClientId);
        ulong nextPlayerId = (currentId + 1 >= playerIds.Count) ? playerIds[0] : playerIds[currentId + 1];

        //Tell the next player that their turn is on!! (only if they're alive)
        if (HpManager.Instance.GetPlayerHps(nextPlayerId) > 0 && playerIds.Count > 1)
        {
            StopAllCoroutines();
            HandleMyTurnClient_RPC(nextPlayerId);
            currentClientTurnId = nextPlayerId;

            StartCoroutine(nameof(StartTurnTimer));
        }
        else
        {
            //TODO: handle the case in which the local player is the only one left alive (here)...
            RuntimeMsg.Info("<color=green>YOU WIN</color>", "You won because you're the last one standing!");
        }
    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Server)]
    public void SignalTimeUpClient_RPC(ulong clientId)
    {
        if (clientId != NetworkManager.LocalClientId) return;
        RuntimeMsg.Info("Yo, the server just told me that my time's up!!");
        IsMyTurn = false;
        OnLocalTurnOver?.Invoke();
    }

    private void HandleClientTurnOver()
    {
        HandleTurnOverServer_RPC(currentClientTurnId, TurnOverReason.TURN_OVER);
    }

    private IEnumerator StartTurnTimer()
    {
        currentTurnTimer = 0;

        while (currentTurnTimer < turnSecondsDuration)
        {
            currentTurnTimer += 1;
            yield return new WaitForSecondsRealtime(1);
        }

        OnTurnOver?.Invoke();
    }

    public enum TurnOverReason
    {
        TURN_SKIP,
        TURN_OVER
    }

}