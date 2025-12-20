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
    public static Action<Card> OnFirstCardGenerated;

    public static CardGenerationManager Instance;

    private Card refCard;

    private void Awake()
    {
        if (!Instance) Instance = this;
        else Destroy(this);
    }

    public void SetUpGameCard(CardScriptable cardScriptable, Card card)
    {
        
        if (cardScriptable == null || card == null)
            return;

        card.SetUpGameCard(GetCardFrontIndex(cardScriptable.type), cardScriptable);
    }

    public float GetCardFrontIndex(CardTypeEnum type)
    {
        return (float)cardsHeight * ((int)type) / cardBackAtlas.height;
    }

    public GameObject[] CreateEffectsEntries(CardEffectEnum[] effects)
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

    public List<Card> GenerateCards(int displayable = -1, List<CardScriptable> cardScriptables = null)
    {
        List<CardScriptable> cards = cardScriptables == null ?
            InventoryManager.Instance.GetHandDeck() : cardScriptables;

        int generationAmount = displayable != -1 ? displayable : cards.Count;

        for (int i = 0; i < generationAmount; i++)
        {
            playerCards.Add(Instantiate(cardPrefab, Vector3.zero, Quaternion.Euler(new Vector3(0, 0, 90))).GetComponentInChildren<Card>());
            SetUpGameCard(cards[i], playerCards[i]);
        }
        OnGenerationDone?.Invoke(playerCards);
        return playerCards;
    }

    public Card GenerateRefCard()
    {
        if (refCard != null) return refCard;
        
        refCard = Instantiate(cardPrefab, Vector3.zero, Quaternion.Euler(new Vector3(0, 0, 90)))
                            .GetComponentInChildren<Card>();

        SetUpGameCard(InventoryManager.Instance.GetHandDeck()[0],
                    refCard);
        
        return refCard;
    }

}
