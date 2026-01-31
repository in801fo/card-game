using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneReloader : NetworkBehaviour
{
    [SerializeField] private string gameSceneName;
    private static Scene previousScene;

    private void Awake()
    {
        NetworkUIHandler.OnGameStart += LoadGameScene;
        GameManager.OnDoneGenerating += UnloadSelectScene;
    }

    private void UnloadSelectScene()
    {
        //TODO: make it so that the server does this only when all players send a singal indicating that they're ready
        NetworkManager.SceneManager.UnloadScene(previousScene);
    }

    private void LoadGameScene()
    {
        LoadScene(gameSceneName);
    }

    public void LoadScene(string sceneName, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
    {
        previousScene = SceneManager.GetActiveScene();
        NetworkManager.SceneManager.LoadScene(sceneName, loadSceneMode);
    }

    private void OnDestroy()
    {
        GameManager.OnDoneGenerating -= UnloadSelectScene;
        NetworkUIHandler.OnGameStart -= LoadGameScene;
    }
}
