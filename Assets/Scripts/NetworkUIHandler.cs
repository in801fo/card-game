using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkUIHandler : MonoBehaviour
{
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button openPlayerDataFormButton;
    private GameObject playerDataForm;

    public static Action OnGameStart;
    public playerInfo playerData { get; private set; } = new playerInfo();

    private Canvas networkCanvas;

    private void Awake()
    {
        
        //prepare the buttons
        hostButton.onClick.AddListener(HandleHost);
        clientButton.onClick.AddListener(HandleClient);
        startGameButton.onClick.AddListener(HandleStartGame);
        openPlayerDataFormButton.onClick.AddListener(HandleShowDataForm);
        networkCanvas = GetComponent<Canvas>();


        //Had to do it static because when i tried to subscribe to the event called by the
        //instance of the PlayerDataFormScreen the data for the pronoun toggle group was not updated
        //this is because, internally to the PlayerDataFormScreen, the instance is deleted right after
        //calling the event OnGetPlayerDataOnScreenClosure, and I think this is what caused the data to get
        //eliminated/lost
        PlayerDataFormScreen.OnGetPlayerDataOnScreenClosure += CollectLocalPlayerData;
        PlayerDataFormScreen.OnGetPlayerDataOnScreenClosure += (_) => ShowNetworkButtons();

        //hides the buttons and shows the player data form
        HandleShowDataForm();

    }

    /// <summary>
    /// Method which shows the player data form
    /// </summary>
    private void HandleShowDataForm()
    {
        HideNetworkButtons();
        startGameButton.gameObject.SetActive(false);
        if (playerDataForm != null) playerDataForm.gameObject.SetActive(true);
        else
        {
            playerDataForm = ScreensManager.Instance.SpawnScreen<playerInfo>("playerDataForm");
            playerDataForm.GetComponent<PlayerDataFormScreen>().Initialize(playerData);
            playerDataForm.transform.SetParent(networkCanvas.transform);
            playerDataForm.transform.localPosition = Vector3.zero;
        }
    }
    
    /// <summary>
    /// Method that executes once the Start Game button is pressed by the host
    /// </summary>
    private void HandleStartGame()
    {
        if (GameManager.playersDict.Count < 2)
        {
            RuntimeMsg.Error("Unable to start game", "Unable to start the game as the number of players was inferior to 2!");
            //TODO: uncomment when releasing/demoing
            //return;
        }

        OnGameStart?.Invoke();
        startGameButton.gameObject.SetActive(false);
    }

    private void CollectLocalPlayerData(playerInfo data)
    {
        playerData = data;
    }

    private void HandleHost()
    {
        NetworkManager.Singleton.StartHost();
        HandleUIGameBeginServer();
    }

    private void HandleClient()
    {
        NetworkManager.Singleton.StartClient();
        HandleUIGameBeginClient();
    }

    private void HandleUIGameBeginServer()
    {
        HideNetworkButtons();
        startGameButton.gameObject.SetActive(true);
    }

    private void HandleUIGameBeginClient()
    {
        HideNetworkButtons();
        startGameButton.gameObject.SetActive(false);
    }

    private void ShowNetworkButtons()
    {
        clientButton.gameObject.SetActive(true);
        hostButton.gameObject.SetActive(true);
    }

    private void HideNetworkButtons()
    {
        clientButton.gameObject.SetActive(false);
        hostButton.gameObject.SetActive(false);
    }

}
