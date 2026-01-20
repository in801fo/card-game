using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : NetworkBehaviour
{

    [SerializeField] private DebugUIManager debugUIManager;
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private GameScreensManager screensManager;
    [SerializeField] private InventoryUIManager inventoryUIManager;
    [SerializeField] private CardGenerationManager cardGenerationManager;
    [SerializeField] private CardInteractionManager cardInteractionManager;
    [SerializeField] private HpManager healthManager;
    [SerializeField] private HpUIManager healthUIManager;
    [SerializeField] private NetworkUIHandler networkUIHandler;
    [SerializeField] private NetworkManager networkManager;
    [SerializeField] private PlayerTagAssigner playerTagAssigner;
    [SerializeField] private AudioManager audioManager;

    public static Dictionary<ulong, playerInfo> playersDict { get; private set; } = new Dictionary<ulong, playerInfo>();
    public static NetworkVariable<int> playerCount { get; private set; } = new NetworkVariable<int>(0);
    /// <summary>
    /// This network list is used to then create the dictionary on the single clients,
    /// the playersNetList si synchronized across all clients, the dictionary is not.
    /// The dictionary only exists as an easier way to save player refs
    /// </summary>
    private static NetworkList<playerInfo> playersNetList = new NetworkList<playerInfo>();

    private static List<GameObject> managers = new List<GameObject>();

    public static Action OnDoneGenerating;

    private static int coordinatedMonoBehaviourCount;

    private static int actualInheriting;

    private const string namePrefixWhenNameEmpty = "CSN"; //Coglione Senza Nome

    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        managers.Add(Instantiate(debugUIManager.gameObject));
        DebugUIManager.GenerateOnlyErrorConsole();

        managers.Add(Instantiate(networkManager.gameObject));
        managers.Add(Instantiate(networkUIHandler.gameObject));

        managers.Add(Instantiate(playerTagAssigner.gameObject));

        NetworkManager.Singleton.OnClientConnectedCallback += HandleOnLocalClientConnected;

        //only happens on the server
        NetworkUIHandler.OnGameStart += HandleGameStart;
        
        if (Instance == null) Instance = this;
        else Destroy(this);
    }

    /// <summary>
    /// This is only called once the local client connects to the server, also calls on the server but not used
    /// </summary>
    /// <param name="obj"></param>
    private void HandleOnLocalClientConnected(ulong obj)
    {
        if (!IsClient) return;

        RuntimeMsg.Info("You are connected!", $"You have successfully connected to the server (id: {NetworkManager.ServerClientId}), your id is {NetworkManager.LocalClientId}");

        SignalConnectionToServer_Rpc(string.Empty, (int)pronouns.HEHIM, NetworkManager.LocalClientId);
    }

    /// <summary>
    /// Copies all elements from <c>playersNetList</c> to <c>playerDict</c>
    /// </summary>
    private void InitializePlayersDictionary()
    {

        for (int i = 0; i < playersNetList.Count; i++)
        {
            playerInfo currentPlayer = playersNetList[i];
            RuntimeMsg.Info("Players syncing...", $"currentPlayer[{i}] == {currentPlayer.Name}");
            SafeAddPlayerToDictionary(currentPlayer, currentPlayer.playerId);
        }
    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Server)]
    private void UpdatePlayerDictionaryClient_Rpc()
    {
        playersDict.Clear();
        InitializePlayersDictionary();
    }

    private void HandleGameStart()
    {
        HandleGameStartClient_Rpc();
    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Server)]
    private void HandleGameStartClient_Rpc()
    {
        RuntimeMsg.Info("Game Started!");
        InitializePlayersDictionary();
        HandleSpawnGameManagers();
        HandleDebugUIGeneration(inventoryUIManager);
    }

    private void HandleSpawnGameManagers()
    {
        HandleScreenManager();
        HandleInventoryGeneration();
        HandleCardGeneration();
        HandleHealthGeneration();
        HandleAudioGeneration();

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

    private void HandleAudioGeneration()
    {
        new GameObject("== Audio Manager ==");
        //cardHandling.transform.SetParent(managersHolder.transform);

        managers.Add(Instantiate(audioManager.gameObject));
    }

    #region Managers Generation Handlers
    private void HandleScreenManager()
    {
        managers.Add(Instantiate(screensManager.gameObject));
    }

    private void HandleHealthGeneration()
    {
        GameObject healthHandling = new GameObject("== Health Handling ==");
        //healthHandling.transform.SetParent(managersHolder.transform);
        if (IsHost)
        {
            managers.Add(Instantiate(healthManager.gameObject));
            managers[managers.Count - 1].GetComponent<NetworkBehaviour>().NetworkObject.Spawn();
        }
        managers.Add(Instantiate(healthUIManager.gameObject));
        //        managers[managers.Count - 1].transform.SetParent(healthHandling.transform);
    }

    private void HandleCardGeneration()
    {
        GameObject cardHandling = new GameObject("== Card Handling ==");
        //cardHandling.transform.SetParent(managersHolder.transform);

        managers.Add(Instantiate(cardInteractionManager.gameObject));
        //managers[managers.Count - 1].transform.SetParent(cardHandling.transform);
        managers.Add(Instantiate(cardGenerationManager.gameObject));
        //managers[managers.Count - 1].transform.SetParent(cardHandling.transform);
    }

    private void HandleInventoryGeneration()
    {
        GameObject inventoryHandling = new GameObject("== Inventory Handling ==");
        //inventoryHandling.transform.SetParent(managersHolder.transform);

        managers.Add(Instantiate(inventoryManager.gameObject));
        //managers[managers.Count - 1].transform.SetParent(inventoryHandling.transform);

        InventoryUIManager inventoryUiManager = Instantiate(inventoryUIManager.gameObject).GetComponent<InventoryUIManager>();
        managers.Add(inventoryUiManager.gameObject);
        //managers[managers.Count - 1].transform.SetParent(inventoryHandling.transform);
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


    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void SignalConnectionToServer_Rpc(string username, int pronouns, ulong senderId)
    {

        ulong clientId = senderId;

        RuntimeMsg.Info($"Client {clientId} Connected!");

        playerInfo playerInfo = new playerInfo()
        {
            //need to do this on the server as otherwise, in the case in which a player is without a name the random string wouldn't be syncronized
            Name = string.IsNullOrEmpty(username) ?
                namePrefixWhenNameEmpty + Random.Range(1000, 5000).ToString() : username,
            Pronouns = (pronouns)pronouns,
            playerId = senderId
        };

        //if successful in adding the player to the dictionary then we are sure that the player has never been seen bby the server
        if (SafeAddPlayerToDictionary(playerInfo, clientId))
            playersNetList.Add(playerInfo);

    }

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
        playerCount.Value = playersDict.Count;

        return true;
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


        UpdatePlayerDictionaryClient_Rpc();
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void HandleAddTagsServer_Rpc(ushort tag, ulong playerId)
    {
        int indexInList = playersNetList.IndexOf(playersDict[playerId]);

        ushort oldMask = playersNetList[indexInList].playerTagsMask;

        //if it already has the passed tag, then there is no need to add it as that would also mess up the tags
        if (TagHandler.HasTag(oldMask, (playerTagsEnum)tag)) { RuntimeMsg.Info("Avoided tag mess!"); return; }

        //dont know why, but is the only way to "update" a value in a networkList
        playersNetList.RemoveAt(indexInList);
        playersNetList.Insert(indexInList, new playerInfo()
        {
            Name = playersDict[playerId].Name,
            Pronouns = playersDict[playerId].Pronouns,
            playerTagsMask = (ushort)(oldMask + tag),
            playerId = playersDict[playerId].playerId
        });


        UpdatePlayerDictionaryClient_Rpc();
    }

    //debug
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
            HandleAddTagsServer_Rpc(1, OwnerClientId);
        if (Input.GetKeyDown(KeyCode.S))
            PrintLocalPlayerTags();
    }
    
    private void PrintLocalPlayerTags()
    {
        List<playerTagsEnum> tags = TagHandler.ExtractPlayerTagsFromMask(playersDict[NetworkManager.LocalClientId].playerTagsMask);
        string tagsString = string.Empty;
        for (int i = 0; i < tags.Count; i++)
        {
            tagsString += tags[i].ToString() + "\n";
        }
        RuntimeMsg.Info("----Local Player Tags----", tagsString);
    }
    

    //TODO: once the host decides that the amount of player is sufficient make it so it can press a button and the game starts

}
