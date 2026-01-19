using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkUIHandler : MonoBehaviour
{
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private Button startGameButton;

    public static Action OnGameStart;

    private void Awake()
    {
        hostButton.onClick.AddListener(HandleHost);
        clientButton.onClick.AddListener(HandleClient);
        startGameButton.onClick.AddListener(HandleStartGame);
        ShowNetworkButtons();
        startGameButton.gameObject.SetActive(false);
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
