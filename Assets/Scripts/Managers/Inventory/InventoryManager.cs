using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : CoordinatedMonoBehaviour
{
    [SerializeField] private List<CardScriptable> handDeck = new List<CardScriptable>();

    private List<CardScriptable> sideDeck = new List<CardScriptable>();

    public static Action<CardScriptable> OnCardAddedToHandDeck;

    public static InventoryManager Instance;
    public Action OnZeroCards;

    protected override void Awake()
    {
        base.Awake();
        if (!Instance) Instance = this;
        else Destroy(this);
    }

    protected override void Beginning()
    {
        CardInteractionManager.OnCardUse += HandleCardUse;
    }

    public void AddCard(CardScriptable card, bool signal = true)
    {
        handDeck.Add(card);
        if (signal) OnCardAddedToHandDeck?.Invoke(card);
    }

    public void AddCardAt(CardScriptable card, int index, bool signal = true)
    {
        handDeck.Insert(index, card);
        if(signal)OnCardAddedToHandDeck?.Invoke(card);
    }

    protected override void ReadyUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Return)) 
            AddCard((CardScriptable)ScriptableObject.CreateInstance(nameof(CardScriptable)));
    }

    /// <summary>
    /// Removes the specified card if found from the specified deck (Either hand or side deck)
    /// </summary>
    /// <param name="card">The card to remove</param>
    /// <param name="removeFromHandDeck">If true, the spefified card will be removed from the hand deck otherwhise from the card deck</param>
    public void RemoveCard(CardScriptable card, bool removeFromHandDeck = true)
    {

        if (removeFromHandDeck)
        {
            handDeck.Remove(card);
            if (handDeck.Count == 0) OnZeroCards?.Invoke();
            return;
        }

        sideDeck.Remove(card);
    }
    
    private void HandleCardUse(Card card)
    {
        RemoveCard(card, true);
    }


    public void RemoveCard(Card card, bool removeFromHandDeck = true)
    {
        RemoveCard(card.cardData, removeFromHandDeck);
    }

    /// <summary>
    /// Moves the cards from the cards inventory to the side deck inventory
    /// 
    /// </summary>
    /// <param name="startIndex">The starting index from which start to move</param>
    /// <param name="count">The number of cards starting from the index to move</param>
    public void MoveCardsToSideDeck(int startIndex, int count)
    {
        
        List<CardScriptable> cards = new List<CardScriptable>(handDeck);

        for (int i = 0; i < count; i++)
            sideDeck.Add(handDeck[i + startIndex]);
        

        for(int i = 0; i < count; i++)
            RemoveCard(cards[i + startIndex], true);
        
    }

    public List<CardScriptable> GetHandDeck()
    {
        return handDeck;
    }

    public int GetHandDeckCount()
    {
        return handDeck.Count;
    }

    private void OnDestroy()
    {
        CardInteractionManager.OnCardUse -= HandleCardUse;
        GameManager.OnDoneGenerating -= Beginning;
    }
}
