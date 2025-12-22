using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject screenManager;

    private GameObject _screenManager;

    private void Start()
    {
        _screenManager = Instantiate(screenManager);
    }

    

}
