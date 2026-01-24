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
    public playerInfo playerData { get; private set; }

    private Canvas networkCanvas;

    private void Awake()
    {
        
        hostButton.onClick.AddListener(HandleHost);
        clientButton.onClick.AddListener(HandleClient);
        startGameButton.onClick.AddListener(HandleStartGame);
        openPlayerDataFormButton.onClick.AddListener(HandleShowDataForm);
        networkCanvas = GetComponent<Canvas>();
        HandleShowDataForm();
        PlayerDataFormScreen playerDataFormHandle = playerDataForm.GetComponent<PlayerDataFormScreen>();
        playerDataFormHandle.Initialize(-1);
        playerDataFormHandle.OnGetPlayerDataOnScreenClosure += CollectLocalPlayerData;
        playerDataFormHandle.OnGetPlayerDataOnScreenClosure += (_) => ShowNetworkButtons();

    }

    private void HandleShowDataForm()
    {
        HideNetworkButtons();
        startGameButton.gameObject.SetActive(false);
        if (playerDataForm) playerDataForm.gameObject.SetActive(true);
        else
        {
            playerDataForm = ScreensManager.Instance.SpawnScreen<int>("playerDataForm");
            playerDataForm.transform.SetParent(networkCanvas.transform);
            playerDataForm.transform.localPosition = Vector3.zero;
        }
    }

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
