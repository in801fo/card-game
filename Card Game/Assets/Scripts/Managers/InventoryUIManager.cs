using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryUIManager : MonoBehaviour
{

    [Range(1, 10)]
    [SerializeField] private float spacingCards = 1.5f;

    [Range(0, 1)]
    [SerializeField] private float borderDistance;

    private const float cardsDefaultWidth = 2f;
    [SerializeField] private float defaultCameraDistance = 24f;
    [field: SerializeField] public int displayableCards { get; private set; } = 0;
    private List<Card> cards;

    private MeshRenderer referenceCardRenderer;

    private float spaceWorldX;

    private float cardsWorldWidth;

    private const string effectsLabel = "effectsList";

    private Vector2 leftScreenBorderStart;

    public void Awake()
    {
        defaultCameraDistance += Camera.main.nearClipPlane;

        spaceWorldX = Mathf.Abs((Camera.main.ViewportToScreenPoint(new Vector3(0, 0, defaultCameraDistance + Camera.main.nearClipPlane)) -
                        Camera.main.ViewportToScreenPoint(new Vector3(1, 0, defaultCameraDistance + Camera.main.nearClipPlane))).x);

        //spawn a ref card so to calculate the exact number of displayable cards on the screen
    }

    private void Start()
    {
        Card firstCard = CardGenerationManager.Instance.GenerateRefCard();
        referenceCardRenderer = firstCard.GetComponent<MeshRenderer>();
        Position(firstCard);

        cardsWorldWidth = referenceCardRenderer.bounds.max.x - referenceCardRenderer.bounds.min.x;

        CalculateDisplayebleCards();
        InventoryManager.Instance.MoveCardsToSideDeck(displayableCards - 1, InventoryManager.Instance.GetHandDeckCount() - displayableCards);
        this.cards = CardGenerationManager.Instance.GenerateCards(displayableCards);
        HandleCardGeneration(cards);
    }

    private void Update()
    {
        CalculateDisplayebleCards();
        Position(cards);
    }
    

    private void HandleCardGeneration(List<Card> cards)
    {
        this.cards = new List<Card>(cards);
        Position(this.cards);
    }

    private void CalculateDisplayebleCards()
    {
        float cardPixelsWidth = GetRefCardWidth();

        displayableCards = Mathf.FloorToInt(
            spaceWorldX / (cardPixelsWidth + (cardPixelsWidth * (spacingCards - 1)))); 
            //-1 as that's required so not to make the cards overlap, the rest is actual spacing...
    }

    private void Position(List<Card> cards)
    {
        Vector3 worldStartSpawn = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, defaultCameraDistance));
        float spaceOccupiedByCard = cardsWorldWidth + (cardsWorldWidth * (spacingCards - 1));

        //print(Camera.main.ScreenToWorldPoint(new Vector3(Screen.width/2, 0, defaultCameraDistance + Camera.main.nearClipPlane)));
        //by calculating the starting position of the border relative to the number of cards times ("times" means multiplied)
        //half of the total space that will be occupied by each card I will be able to obtain the start of the border
        leftScreenBorderStart.x = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width/2, 0, defaultCameraDistance + Camera.main.nearClipPlane)).x
         - ((spaceOccupiedByCard * cards.Count)/2);

        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].transform.parent.transform.position = new Vector3(leftScreenBorderStart.x + (spaceOccupiedByCard * i), worldStartSpawn.y, worldStartSpawn.z);
            //doing this to fix a small centering problem...
            if (i == 0) cards[0].transform.parent.transform.position += Vector3.right * spaceOccupiedByCard;
        }

    }

    private void Position(Card card)
    {
        Position(new List<Card>() { card });
    }


    /// <summary>
    /// Calcualtes the current ref card's screen width
    /// </summary>
    /// <returns>The current amount of pixels occupied by the ref card on the screen</returns>
    private float GetRefCardWidth(){
        Rect cardRect = BoundsToScreenRect(referenceCardRenderer);
        return cardRect.max.x - cardRect.min.x;
    }

    public Rect BoundsToScreenRect(MeshRenderer renderer)
    {
        // Get mesh origin and farthest extent (this works best with simple convex meshes)
        Vector3 origin = Camera.main.WorldToScreenPoint(
            new Vector3(
                renderer.bounds.min.x,
                renderer.bounds.max.y,
                defaultCameraDistance + Camera.main.nearClipPlane
                )
            );

        Vector3 extent = Camera.main.WorldToScreenPoint(
            new Vector3(
                renderer.bounds.max.x,
                renderer.bounds.min.y,
                defaultCameraDistance + Camera.main.nearClipPlane
                )
            );

        // Create rect in screen space and return - does not account for camera perspective
        return new Rect(origin.x, Screen.height - origin.y, extent.x - origin.x, origin.y - extent.y);
    }

    public static void GetCardInfoScreen(CardScriptable card)
    {
        GameObject screen = GameScreensManager.Instance.SpawnScreen("cardInfo");
        screen.GetComponentInChildren<Card>().SetUpInfoCard(card);

        GameObject[] effects = CardGenerationManager.Instance.CreateEffectsEntries(card.cardEffects);

        GameObject effectsScrollViewContentObject = GameObject.FindWithTag(effectsLabel);

        //removing previous children
        for (int i = 0; i < effectsScrollViewContentObject.transform.childCount; i++)
        {
            Destroy(effectsScrollViewContentObject.transform.GetChild(i).gameObject);
        }

        for (int i = 0; i < effects.Length; i++)
        {
            effects[i].transform.SetParent(effectsScrollViewContentObject.transform);
        }

    }


    

}