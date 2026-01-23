using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneReloader : NetworkBehaviour
{

    [SerializeField] private string gameSceneName;

    private void Awake()
    {
        NetworkUIHandler.OnGameStart += LoadGameScene;
    }

    private void LoadGameScene()
    {
        NetworkManager.SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
    }

    private void OnDestroy()
    {
        NetworkUIHandler.OnGameStart -= LoadGameScene;
    }
}
