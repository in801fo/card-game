using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


/// <summary>
/// Helper class used to generate cards
/// </summary>
public class CardGenerationManager : MonoBehaviour
{

    private const int cardsHeight = 1185;
    [SerializeField] private Texture2D cardBackAtlas;

    [SerializeField] private GameObject effectOutline;

    [SerializeField] private GameObject cardPrefab;

    public static Action<List<Card>> OnGenerationDone;

    public static CardGenerationManager Instance;

    private GameObject refCard;

    private List<Card> playerCards = new List<Card>();

    private void Awake()
    {
        if (!Instance) Instance = this;
        else Destroy(this);
        CardInteractionManager.OnCardUse += HandleCardUse;
        InventoryManager.OnCardMoved += HandleCardMoved;
    }

    private void HandleCardUse(Card card)
    {
        playerCards.Remove(card);
    }

    private void HandleCardMoved(CardScriptable card, int indexTo)
    {
        int cardMovedIndex = playerCards.FindIndex((Card c) => c.cardData.Equals(card));

        Card cardToMove = playerCards[cardMovedIndex];
        playerCards.RemoveAt(cardMovedIndex);
        playerCards.Insert(indexTo, cardToMove);
    }

    public void SetUpGameCard(CardScriptable cardScriptable, CardGraphics cardGraphics, Card card)
    {
        
        if (cardScriptable == null || card == null)
            return;

        card.InitializeCard(cardScriptable);
        cardGraphics.SetUpGameCard(GetCardFrontIndex(card.cardData.type), card);
    }

    public float GetCardFrontIndex(cardTypeEnum type)
    {
        return (float)cardsHeight * ((int)type) / cardBackAtlas.height;
    }

    public GameObject[] CreateEffectsEntries(effectsEnum[] effects)
    {
        GameObject[] res = new GameObject[effects.Length];
        for (int i = 0; i < effects.Length; i++)
        {
            GameObject currentEffect = Instantiate(effectOutline, Vector3.zero, Quaternion.identity);
            currentEffect.GetComponentInChildren<TextMeshProUGUI>().SetText(effects[i].ToString());
            res[i] = currentEffect;
        }

        return res;
    }

    /// <summary>
    /// Generates the provided <code>CardScriptables</code>
    /// </summary>
    /// <param name="displayable">How many cards are displayable? <para> If left at -1 all the cards in the hand deck will be spawned</para>
    /// <para>If set, only the number of cards indicated will be spawned</para> </param>
    /// <param name="cardScriptables">The list of cards to spawn</param>
    /// <returns></returns>
    public List<Card> GenerateCards(int displayable = -1, List<CardScriptable> cardScriptables = null)
    {
        List<CardScriptable> cards = cardScriptables == null ?
            InventoryManager.Instance.GetHandDeck() : cardScriptables;

        int generationAmount = displayable >= 0 && displayable <= cards.Count ? displayable : cards.Count;

        for (int i = 0; i < generationAmount; i++)
        {
            //if you find another card present in the hand deck with identical info to the current (cards[i]) 
            //then skip creation for current card 
            if (playerCards.FindIndex(
                    (Card card) => card.cardData.Equals(cards[i])
                ) != -1)
                continue;

            GameObject cardParentGO = Instantiate(cardPrefab, Vector3.zero, Quaternion.Euler(new Vector3(0, 0, 90)));
            
            GameObject actualCard = cardParentGO.transform.GetChild(0).gameObject;

            AttachCorrectCardTypeScript(actualCard, cards[i].type);
            playerCards.Add(cardParentGO.GetComponentInChildren<Card>());
            SetUpGameCard(cards[i], cardParentGO.GetComponentInChildren<CardGraphics>(), playerCards[i]);
        }
        OnGenerationDone?.Invoke(playerCards);
        return playerCards;
    }
    
    private void AttachCorrectCardTypeScript(GameObject cardGO, cardTypeEnum type)
    {
        switch (type)
        {
            case cardTypeEnum.CHARACTER:
                cardGO.AddComponent<CharacterCard>();
            break;
            case cardTypeEnum.TRAP:
                cardGO.AddComponent<TrapCard>();
            break;
            case cardTypeEnum.ICARD:
                cardGO.AddComponent<InterestCard>();
                break;
            default:
                cardGO.AddComponent<ActionCard>();
            break;
        
        }
    }

    /// <summary>
    /// Generates a card which is used as reference for rendering all the others
    /// </summary>
    /// <returns>The reference card</returns>
    public GameObject GenerateRefCard()
    {
        if (refCard != null) return refCard;

        refCard = Instantiate(cardPrefab, Vector3.zero, Quaternion.Euler(new Vector3(0, 0, 90)));
        AttachCorrectCardTypeScript(refCard, cardTypeEnum.ACTION);

        /*SetUpGameCard(InventoryManager.Instance.GetHandDeck()[0],
                    refCard);*/

        return refCard;
    }
}
