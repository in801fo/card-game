using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework.Constraints;
using UnityEditor.Search;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    [SerializeField] private DebugUIManager debugUIManager;
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private GameScreensManager screensManager;
    [SerializeField] private InventoryUIManager inventoryUIManager;
    [SerializeField] private CardGenerationManager cardGenerationManager;
    [SerializeField] private CardInteractionManager cardInteractionManager;

    private static List<GameObject> managers = new List<GameObject>();

    public static GameObject managersHolder { get; private set; }
    public static Action OnDoneGenerating;

    private static int count;

    private static int actualInheriting;

    private void Awake()
    {
        managersHolder = new GameObject("== Managers ==");
        this.transform.SetParent(managersHolder.transform);
        managers.Add(Instantiate(inventoryManager.gameObject));
        managers.Add(Instantiate(screensManager.gameObject));
        InventoryUIManager inventoryUiManager = Instantiate(inventoryUIManager.gameObject).GetComponent<InventoryUIManager>();
        managers.Add(inventoryUiManager.gameObject);
        managers.Add(Instantiate(cardInteractionManager.gameObject));
        managers.Add(Instantiate(cardGenerationManager.gameObject));
        for (int i = 0; i < managers.Count; i++)
        {
            Component[] list = managers[i].GetComponents<Component>();
            if (list[1].GetType().IsSubclassOf(typeof(CoordinatedMonoBehaviour))) actualInheriting++;
            managers[i].transform.SetParent(managersHolder.transform);
        }
        managers.Add(Instantiate(debugUIManager.gameObject));
        DontDestroyOnLoad(managers[managers.Count - 1]);

        //generates the debug menu which drives all the fields in the inventoryUI
        DebugUIManager.GenerateUIForValue(inventoryUiManager, false, false, new List<string>()
        {
            "displayableCards",
            "referenceCardRenderer",
            "spaceOccupiedByCard",
            "cardsCoroutines",
            "currentCardHolded"
        });

        //if the number of CoordinatedMonoBehaviours is equal to the actual which inherited that means that all of them have
        //completed their initialization 
        if (count == actualInheriting) OnDoneGenerating?.Invoke();
    }

    public static void ValidateInitialization(){ count++; }
    
}
