using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;
using System;
using Unity.VisualScripting;
using System.Linq;
using JetBrains.Annotations;
public class GameManager : NetworkBehaviour
{
    [SerializeField] private DebugUIManager debugUIManager;
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private ScreensManager screensManager;
    [SerializeField] private InventoryUIManager inventoryUIManager;
    [SerializeField] private CardGenerationManager cardGenerationManager;
    [SerializeField] private CardInteractionManager cardInteractionManager;
    [SerializeField] private HpManager healthManager;
    [SerializeField] private HpUIManager healthUIManager;
    [SerializeField] private PlayerTagAssigner playerTagAssigner;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private NetworkManager networkManager;
    [SerializeField] private GameObject networkUI;

    [SerializeField] private EffectManager effectManager;

    private NetworkUIHandler networkUIHandler;

    /// <summary>
    /// A dictionary with: 
    /// <para><strong>Key</strong>: the players's client ids</para>
    /// <para><strong>Value</strong>: the playerInfo relating to a player</para>
    /// Only used for ease of access to player data
    /// </summary>
    public static Dictionary<ulong, playerInfo> playersDict { get; private set; } = new Dictionary<ulong, playerInfo>();
    
    /// <summary>
    /// This network list is used to then create the dictionary on the single clients,
    /// the playersNetList si synchronized across all clients, the dictionary is not.
    /// The dictionary only exists as an easier way to save player refs
    /// </summary>
    private NetworkList<playerInfo> playersNetList = new NetworkList<playerInfo>(null, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public static playerInfo localPlayerInfo { get; private set; }

    /// <summary>
    /// Stores the player username if no username was provided on load
    /// </summary>
    public static string noUsernameProvidedUsernameFallback;

    private static List<GameObject> managers = new List<GameObject>();

    /// <summary>
    /// Event called once all CoordinatedMonoBehaviours have finished initializing
    /// </summary>
    public static Action OnDoneGenerating;

    /// <summary>
    /// The number of CoordinatedMonoBehaviours which signaled finishing their initialization
    /// </summary>
    private static int coordinatedMonoBehaviourCount;

    /// <summary>
    /// The number of scripts actually inheriting from CoordinatedMonobehaviour
    /// </summary>
    private static int actualInheriting;

    /// <summary>
    /// Prefix used while generating the username for a player whom didn't input a username
    /// </summary>
    private const string namePrefixWhenNameEmpty = "CSN"; //Coglione Senza Nome

    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        DontDestroyOnLoad(HandleScreenManager());
        HandleNetworkManagers();

        managers.Add(Instantiate(debugUIManager.gameObject));

        if (Instance == null) Instance = this;
        else Destroy(this.gameObject);
    }
    
    public override void OnNetworkSpawn()
    {
        NetworkManager.OnClientConnectedCallback += HandleOnLocalClientConnected;
        NetworkManager.OnServerStarted += HandleLocalIsServer;
        NetworkManager.OnConnectionEvent += OnClientDisconnect;
        playersNetList.OnListChanged += HandleListChanged;
    }

    private void HandleListChanged(NetworkListEvent<playerInfo> changeEvent)
    {
        switch (changeEvent.Type)
        {
            case NetworkListEvent<playerInfo>.EventType.Add:
                SafeAddPlayerToDictionary(changeEvent.Value, changeEvent.Value.playerId);
                break;
            case NetworkListEvent<playerInfo>.EventType.Insert:
                //Tested: when inserted the previousValue == the inserted value
                SafeAddPlayerToDictionary(changeEvent.Value, changeEvent.Value.playerId);
                break;
            case NetworkListEvent<playerInfo>.EventType.Remove:
                SafeRemovePlayerFromDictionary(changeEvent.Value.playerId);
                break;
            case NetworkListEvent<playerInfo>.EventType.RemoveAt:
                SafeRemovePlayerFromDictionary(changeEvent.Value.playerId);
                break;

            case NetworkListEvent<playerInfo>.EventType.Clear:
                playersDict.Clear();
                break;
            case NetworkListEvent<playerInfo>.EventType.Value:
                //using the PreviousValue.playerId as the new Value could've have changed it and in the dictionary
                //there would still be the old playerId
                SafeModifyPlayerInDictionary(changeEvent.Value, changeEvent.PreviousValue.playerId);
                break;
            case NetworkListEvent<playerInfo>.EventType.Full:
                RuntimeMsg.Error("Sorry, didn't have time to test this one out",
                    @"Haven't had the time to test this eventuality, 
                        please tell me as in much as great detail as you can remember. 
                        \nNetworkListEvent<playerInfo>.EventType.Full HAPPENED!!");
                break;
        }
    }

    #region Handle Disconnect

    private void OnClientDisconnect(NetworkManager manager, ConnectionEventData data)
    {
        if (data.EventType == ConnectionEvent.ClientDisconnected)
        {
            //i'm the client which disconencted and I must handle it from my perspective
            if (!IsServer)
            {
                HandleLocalClientDisconnected();
            }
            //i'm the server and I must handle all
            else
                HandleServerClientDisconnect(data);
        }


    }

    private void HandleServerClientDisconnect(ConnectionEventData data)
    {
        RuntimeMsg.Info($"Client: {data.ClientId} has just disconnected!");
        //remove the player from the net list.
        playersNetList.Remove(playersDict[data.ClientId]);
    }

    private void HandleLocalClientDisconnected()
    {
        RuntimeMsg.Info("You have just disconnected!");
    }

    #endregion

    private void HandleLocalIsServer()
    {
        //once the scene has changed to the gameScene it'll trigger on all clients + the host: HandleGameStartClient_Rpc 
        NetworkManager.SceneManager.OnLoadEventCompleted += (_, _, _, _) => HandleGameStartClient_Rpc();
    }


    public static string GetFallbackUsername()
    {
        if (string.IsNullOrEmpty(noUsernameProvidedUsernameFallback)) noUsernameProvidedUsernameFallback = namePrefixWhenNameEmpty + Random.Range(1000, 5000).ToString();
        return noUsernameProvidedUsernameFallback;
    }
    
    public static List<ulong> GetAllPlayersWithTags(playerTagsEnum[] playerTagsEnums)
    {
        List<playerInfo> playerInfos = playersDict.Values.ToList();
        List<ulong> res = new List<ulong>();

        for (int i = 0; i < playerInfos.Count; i++)
        {
            for (int j = 0; j < playerTagsEnums.Length; j++)
            {
                if (TagHandler.HasTag(playerInfos[i].playerTagsMask, playerTagsEnums[j]))
                {
                    res.Add(playerInfos[i].playerId);
                    break;
                }
            }
        }

        return res;
    }

    /// <summary>
    /// This is only called once the local client connects to the server
    /// </summary>
    /// <param name="obj"></param>
    private void HandleOnLocalClientConnected(ulong obj)
    {
        
        //set the window name only if this is a runtime instance of the game
#if UNITY_STANDALONE_WIN
        IntPtr windowInt = WindowUtil.GetActiveWindow();
        WindowUtil.SetWindowTitle(windowInt, $"Local Client Id: {NetworkManager.LocalClientId}");
#endif

        RuntimeMsg.Info("<color=green>You are connected!</color>", $"You have successfully connected to the server (id: {NetworkManager.ServerClientId}), your id is {NetworkManager.LocalClientId}");

        //collect the local player info
        localPlayerInfo = GetPlayerInfoFromForm();

        SendPlayerInfoToServer_Rpc(localPlayerInfo.Name.ToString(), (int)localPlayerInfo.Pronouns, localPlayerInfo.playerId);
    }

    /// <summary>
    /// Collect the player data from the handler of the network UI and return it 
    /// </summary>
    /// <returns>The local player's data</returns>
    private playerInfo GetPlayerInfoFromForm()
    {
        playerInfo data = networkUIHandler.playerData;
        data.playerId = NetworkManager.LocalClientId;
        return data;
    } 
    
    

    private void HandleSpawnGameManagers()
    {
        HandleInventoryGeneration();
        HandleCardGeneration();
        HandleHealthGeneration();
        if (IsServer)
        {
            HandleAudioGeneration();
            HandleTagAssigner();
            HandleEffectManager();

        }

        //here getting the setting the parent to the managersHolder
        for (int i = 0; i < managers.Count; i++)
        {
            Component[] list = managers[i].GetComponents<Component>();
            if (list.Length > 2)
                RuntimeMsg.Warning($"Components list for {managers[i].name} is bigger than 2",
                    $"Amount of component of manager {managers[i].name} is bigger than 2. If the manager inherits from CoordinatedMonoBehaviour you are at risk of not initializing the manager as a requirement for CoordinatedMonoBehaviour type objects is that the script inheriting the class must be in second place. Please check that this is the case.");

            //here checking if the current manager is subclass of CoordinatedMonoBehaviour, if so incrementing actualInheriting
            if (list[1].GetType().IsSubclassOf(typeof(CoordinatedMonoBehaviour))) actualInheriting++;
        }

        //if the number of CoordinatedMonoBehaviours is equal to the actual which inherited that means that all of them have
        //completed their initialization 
        if (coordinatedMonoBehaviourCount == actualInheriting)
            OnDoneGenerating?.Invoke();

    }

    private void HandleEffectManager()
    {
        GameObject effManGO = Instantiate(effectManager.gameObject);
        managers.Add(effManGO);
        effManGO.GetComponent<NetworkBehaviour>().NetworkObject.Spawn();
    }

    #region Managers Generation Handlers

    private void HandleNetworkManagers()
    {
        managers.Add(Instantiate(networkManager.gameObject));
        networkUIHandler = Instantiate(networkUI.gameObject).GetComponent<NetworkUIHandler>();
    }

    private void HandleTagAssigner()
    {
        
        managers.Add(Instantiate(playerTagAssigner.gameObject));
        managers[managers.Count - 1].GetComponent<NetworkBehaviour>().NetworkObject.Spawn();
    }

    private void HandleAudioGeneration()
    {
        new GameObject("== Audio Manager ==");
        
        NetworkBehaviour man = Instantiate(audioManager.gameObject).GetComponent<NetworkBehaviour>();
        man.NetworkObject.Spawn();
        managers.Add(man.gameObject);
    }

    private GameObject HandleScreenManager()
    {
        GameObject screenManager = Instantiate(screensManager.gameObject);
        managers.Add(screenManager);
        return screenManager;
    }

    private void HandleHealthGeneration()
    {
        new GameObject("== Health Handling ==");

        if (IsServer)
        {
            managers.Add(Instantiate(healthManager.gameObject));
            managers[managers.Count - 1].GetComponent<NetworkBehaviour>().NetworkObject.Spawn();
        }
        managers.Add(Instantiate(healthUIManager.gameObject));
    }

    private void HandleCardGeneration()
    {
        new GameObject("== Card Handling ==");

        managers.Add(Instantiate(cardInteractionManager.gameObject));
        managers.Add(Instantiate(cardGenerationManager.gameObject));
    }

    private void HandleInventoryGeneration()
    {
        new GameObject("== Inventory Handling ==");
        managers.Add(Instantiate(inventoryManager.gameObject));

        InventoryUIManager inventoryUiManager = Instantiate(inventoryUIManager.gameObject).GetComponent<InventoryUIManager>();
        managers.Add(inventoryUiManager.gameObject);
    }

    private void HandleDebugUIGeneration(InventoryUIManager inventoryUIManager)
    {
        //generates the debug menu which drives all the fields in the inventoryUI
        DebugUIManager.GenerateUIForValue(inventoryUIManager, false, false, new List<string>()
        {
            "displayableCards",
            "referenceCardRenderer",
            "spaceOccupiedByCard",
            "cardsCoroutines",
            "currentCardHolded"
        });
    }

    #endregion

    public static void ValidateInitialization() => coordinatedMonoBehaviourCount++;

#region Network Stuff

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Server)]
    private void HandleGameStartClient_Rpc()
    {
        RuntimeMsg.Info("Game Started!");
        SyncDictionaryToPlayerNetList();
        HandleSpawnGameManagers();
        HandleDebugUIGeneration(inventoryUIManager);
    }

    private void SyncDictionaryToPlayerNetList()
    {
        foreach (playerInfo player in playersNetList)
        {
            SafeAddPlayerToDictionary(player, player.playerId);
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void SendPlayerInfoToServer_Rpc(string username, int pronouns, ulong senderId)
    {
        ulong clientId = senderId;
        string cappedUsername = (username.Length > 61) ? username.Substring(0, 60) : username;

        RuntimeMsg.Info($"Client {clientId} Connected!");

        playerInfo playerInfo = new playerInfo()
        {
            //need to do this on the server as otherwise, in the case in which a player is without a name the random string wouldn't be syncronized
            Name = cappedUsername,
            Pronouns = (pronouns)pronouns,
            playerId = senderId
        };

        //if successful in adding the player to the dictionary then we are sure that the player has never been seen bby the server
        if (SafeAddPlayerToDictionary(playerInfo, clientId))
            playersNetList.Add(playerInfo);

    }

    /// <summary>
    /// Handles the update for the specified client of their tags
    /// </summary>
    /// <param name="newMask"></param>
    /// <param name="playerId"></param>

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void HandleSetTagsServer_Rpc(ushort newMask, ulong playerId)
    {
        int indexInList = playersNetList.IndexOf(playersDict[playerId]);

        //dont know why, but is the only way to "update" a value in a networkList
        playersNetList.RemoveAt(indexInList);
        playersNetList.Insert(indexInList, new playerInfo()
        {
            Name = playersDict[playerId].Name,
            Pronouns = playersDict[playerId].Pronouns,
            playerTagsMask = newMask,
            playerId = playersDict[playerId].playerId
        });
    }

    /// <summary>
    /// Adds to the specified player the given tag
    /// </summary>
    /// <param name="tag"></param>
    /// <param name="playerId">The id of the player to which to add the tag</param>

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void HandleAddTagsServer_Rpc(ushort tag, ulong playerId)
    {
        int indexInList = playersNetList.IndexOf(playersDict[playerId]);

        ushort oldMask = playersNetList[indexInList].playerTagsMask;

        //if it already has the passed tag, then there is no need to add it as that would also mess up the tags
        if (TagHandler.HasTag(oldMask, (playerTagsEnum)tag)) { RuntimeMsg.Info("Avoided tag mess!"); return; }

        playersNetList[indexInList] = new playerInfo()
        {
            Name = playersDict[playerId].Name,
            Pronouns = playersDict[playerId].Pronouns,
            playerTagsMask = (ushort)(oldMask + tag),
            playerId = playersDict[playerId].playerId
        };
        
        RuntimeMsg.Info($"Added tag: {(playerTagsEnum)tag} to player {playerId}");

        //updating the dictionary through the OnListChanged event of the playerNetList
    }
#endregion
    /// <summary>
    /// Checks for the presence of the give player's id. If found returns false otherwise true
    /// </summary>
    /// <param name="player"></param>
    /// <param name="clientId"></param>
    /// <returns></returns>
    private bool SafeAddPlayerToDictionary(playerInfo player, ulong clientId)
    {
        if (playersDict.ContainsKey(clientId))
        {
            RuntimeMsg.Warning($"Player list already contains playerId {clientId}");
            return false;
        }

        playersDict.Add(clientId, player);

        return true;
    }

    private bool SafeRemovePlayerFromDictionary(ulong clientId)
    {
        if (!playersDict.ContainsKey(clientId))
        {
            RuntimeMsg.Warning($"Unable to remove {clientId}", $"Player dictionary doesn't contain {clientId}");
            return false;
        }

        return playersDict.Remove(clientId);
    }

    private bool SafeModifyPlayerInDictionary(playerInfo newInfo, ulong clientId)
    {
        if (!playersDict.ContainsKey(clientId))
        {
            RuntimeMsg.Warning($"Unable to modify {clientId}", $"Player dictionary doesn't contain {clientId}");
            return false;
        }

        if (clientId == localPlayerInfo.playerId) localPlayerInfo = newInfo;

        playersDict[clientId] = newInfo;
        return true;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.D) && IsHost)
            NetworkManager.DisconnectClient(playersNetList[1].playerId);
        if (Input.GetKeyDown(KeyCode.S))
            PrintLocalPlayerTags();
    }
    
    private void PrintLocalPlayerTags()
    {
        List<playerTagsEnum> tags = TagHandler.ExtractPlayerTagsFromMask(localPlayerInfo.playerTagsMask);
        string tagsString = string.Empty;
        for (int i = 0; i < tags.Count; i++)
        {
            tagsString += tags[i].ToString() + "\n";
        }
        RuntimeMsg.Info("----Local Player Tags----", tagsString);
    }
    

    //TODO: once the host decides that the amount of player is sufficient make it so it can press a button and the game starts

}
