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

    private List<Card> playerCards = new List<Card>();

    public static Action<List<Card>> OnGenerationDone;

    public static CardGenerationManager Instance;

    private GameObject refCard;

    private void Awake()
    {
        if (!Instance) Instance = this;
        else Destroy(this);
        //need to handle card use as to be able to correctly update the cache of current cards
        CardInteractionManager.OnCardUse += HandleCardUse;
    }

    private void HandleCardUse(Card card)
    {
        playerCards.Remove(card);
    }

    public void SetUpGameCard(CardScriptable cardScriptable, CardGraphics cardGraphics, Card card)
    {
        
        if (cardScriptable == null || card == null)
            return;

        card.InitializeCard(cardScriptable);
        cardGraphics.SetUpGameCard(GetCardFrontIndex(card.cardData.type), card);
    }

    public float GetCardFrontIndex(CardTypeEnum type)
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
            //if you find another card in cached playerCards with identical info to the current (cards[i]) 
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
    
    private void AttachCorrectCardTypeScript(GameObject cardGO, CardTypeEnum type)
    {
        switch (type)
        {
            case CardTypeEnum.CHARACTER:
                cardGO.AddComponent<CharacterCard>();
            break;
            case CardTypeEnum.TRAP:
                cardGO.AddComponent<TrapCard>();
            break;
            case CardTypeEnum.ICARD:
                cardGO.AddComponent<ICard>();
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
        AttachCorrectCardTypeScript(refCard, CardTypeEnum.ACTION);

        /*SetUpGameCard(InventoryManager.Instance.GetHandDeck()[0],
                    refCard);*/

        return refCard;
    }

    private void OnDestroy()
    {
        CardInteractionManager.OnCardUse -= HandleCardUse;
    }
}
