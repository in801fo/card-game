using UnityEngine;

public class GameManager : MonoBehaviour
{

    [SerializeField] private GameObject debugUIManager;

    void Start()
    {
        DontDestroyOnLoad(Instantiate(debugUIManager));
    }
}
