using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class InventoryUIManager : MonoBehaviour
{

    [Range(1, 10)]
    [SerializeField] private float spacingCards = 1.5f;
    [Range(0, 1)]
    [SerializeField] private float borderDistance;
    [SerializeField] private float defaultCameraDistance = 24f;
    [SerializeField] private AnimationCurve cardReshuffleCurve;
    [field: SerializeField] public int displayableCards { get; private set; } = 0;
    private List<Card> cards;
    private MeshRenderer referenceCardRenderer;

    private float spaceOccupiedByCard;

    private Vector3 worldStartSpawn;

    /// <summary>
    /// Amount of world units occupied by the width of one card
    /// </summary>
    private float cardsWorldWidth;

    private const string effectsLabel = "effectsList";

    public void Awake()
    {
        defaultCameraDistance += Camera.main.nearClipPlane;
    }

    private void Start()
    {
        GenerateReferenceCard();
        worldStartSpawn = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, defaultCameraDistance));
        cardsWorldWidth = referenceCardRenderer.bounds.max.x - referenceCardRenderer.bounds.min.x;
        spaceOccupiedByCard = cardsWorldWidth + (cardsWorldWidth * (spacingCards - 1));

        InventoryManager.OnCardAddedToHandDeck += HandleCardAdded;

        HandleCardGeneration();
    }

    private void HandleCardAdded(CardScriptable scriptable)
    {
        this.cards = CardGenerationManager.Instance.GenerateCards(displayableCards);
        
        HandleAnimateCards();
        //Position(this.cards);
    }

    private void HandleAnimateCards()
    {
        Coroutine[] cardsCoroutines = new Coroutine[cards.Count];
        for(int i = 0; i < cards.Count; i++)
        {
            if (cardsCoroutines[i] != null)
                StopCoroutine(cardsCoroutines[i]);

            cardReshuffleAnimationParams parameters = new cardReshuffleAnimationParams()
            {
                targetPosition = CalculateCardTargetPosition(GetScreenBorderForCards(cards.Count).x, i),
                duration = 3f,
                curve = cardReshuffleCurve
            };

            print(parameters.targetPosition);
            
            cardsCoroutines[i] = StartCoroutine(cards[i].AnimateCard(parameters));
        }
    }


    private void Update()
    {
        CalculateDisplayebleCards();
        /*if(cards != null)
            Position(cards);*/
    }


    private void HandleCardGeneration()
    {
        //here need to establish the number of displayable cards so to be able to move the extra ones in the side deck and not have to spawn them
        CalculateDisplayebleCards();
        InventoryManager.Instance.MoveCardsToSideDeck(displayableCards - 1, InventoryManager.Instance.GetHandDeckCount() - displayableCards);
        this.cards = CardGenerationManager.Instance.GenerateCards(displayableCards);
        Position(this.cards);
    }

    private Vector3 CalculateCardTargetPosition(float borderStartX, int cardsBeforeCurrent)
    {
        return
            new Vector3(
                //+ spaceOccupiedByCard/2 --> doing this to fix a small centering problem...
                borderStartX + (spaceOccupiedByCard * cardsBeforeCurrent) + (spaceOccupiedByCard / 2),
                worldStartSpawn.y,
                worldStartSpawn.z
            );
    }

    /// <summary>
    /// Calculates the number of cards displayable at any time 
    /// </summary>
    private void CalculateDisplayebleCards()
    {
        float cardPixelsWidth = GetRefCardWidth();
        //amount of space occupied by a single card
        float individualCardScreenSpace = cardPixelsWidth + (cardPixelsWidth * (spacingCards - 1)); //-1 as that's required so not to make the cards overlap, the rest is actual spacing...


        //Mathf.FloorToInt -> need a whole number
        displayableCards = Mathf.FloorToInt(
            //the space available on the screen in world units divided by th espace occupied on the screen by a single card
            Screen.width / individualCardScreenSpace
        );
    }

    private void Position(List<Card> cards)
    {

        //print(Camera.main.ScreenToWorldPoint(new Vector3(Screen.width/2, 0, defaultCameraDistance + Camera.main.nearClipPlane)));
        //by calculating the starting position of the border relative to the number of cards times ("times" means multiplied)
        //half of the total space that will be occupied by each card I will be able to obtain the start of the border

        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].transform.parent.transform.position = CalculateCardTargetPosition(GetScreenBorderForCards(cards.Count).x, i);
        }

    }
    
    private Vector2 GetScreenBorderForCards(int cardsCount)
    {
        return new Vector2(Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, 0, defaultCameraDistance + Camera.main.nearClipPlane)).x
         - (spaceOccupiedByCard * cardsCount / 2), 0);
    }

    private void Position(Card card)
    {
        Position(new List<Card>() { card });
    }

    private void GenerateReferenceCard()
    {
        Card firstCard = CardGenerationManager.Instance.GenerateRefCard();
        referenceCardRenderer = firstCard.GetComponent<MeshRenderer>();
        Position(firstCard);
        firstCard.transform.parent.gameObject.SetActive(false);
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

        //this only when the card doesn't have any effects
        if (card.cardEffects == null || card.cardEffects.Length == 0) return;
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

    private void OnDestroy()
    {
        InventoryManager.OnCardAddedToHandDeck -= HandleCardAdded;
    }

}