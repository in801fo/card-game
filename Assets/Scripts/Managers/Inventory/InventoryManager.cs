using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryManager : CoordinatedMonoBehaviour
{
    [SerializeField] public List<CardScriptable> handDeck { get; private set;} = new List<CardScriptable>();

    private List<CardScriptable> sideDeck = new List<CardScriptable>();

    public static Action<CardScriptable> OnCardAddedToHandDeck;
    public static Action<List<CardScriptable>> OnGroupCardAddedToHandDeck;
    public static Action<CardScriptable, int> OnCardMoved;
    public static Action<CardScriptable> OnCardRemovedFromHandDeck;

    public static InventoryManager Instance;
    public Action OnZeroCards;

    private CardScriptable[] allCardScriptables;

    protected override void Awake()
    {
        base.Awake();
        if (!Instance) Instance = this;
        else Destroy(this);
    }

    protected override void Beginning()
    {
        //RuntimeMsg.Info(Directory.Exists("D:\\Github\\card-game\\Assets\\Scriptables\\Cards\\Character").ToString());
        allCardScriptables = Resources.LoadAll<CardScriptable>("Scriptables");

        Card.OnCardZeroUsages += OnCardZeroUsages;
    }

    public void AddCard(CardScriptable card, bool signal = true)
    {
        handDeck.Add(card);
        if (signal) OnCardAddedToHandDeck?.Invoke(card);
    }

    public void AddCardAt(CardScriptable card, int index, bool signal = true)
    {
        handDeck.Insert(index, card);
        if(signal) OnCardMoved?.Invoke(card, index);
    }

    protected override void ReadyUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            LoadAllCardsInInventory();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            RemoveAllCards();
        }
    }

    private void RemoveAllCards()
    {
        for (int i = 0; i < handDeck.Count; i++)
        {
            RemoveCard(handDeck[i]);
        }
    }
    
    
    private void LoadAllCardsInInventory()
    {
        for (int i = 0; i < allCardScriptables.Length; i++)
        {
            AddCard(allCardScriptables[i], false);
        }

        OnGroupCardAddedToHandDeck.Invoke(allCardScriptables.ToList());

    }

    /// <summary>
    /// Removes the specified card if found from the specified deck (Either hand or side deck)
    /// </summary>
    /// <param name="card">The card to remove</param>
    /// <param name="removeFromHandDeck">If true, the spefified card will be removed from the hand deck otherwhise from the card deck</param>
    public void RemoveCard(CardScriptable card, bool removeFromHandDeck = true, bool alert = true)
    {

        if (removeFromHandDeck)
        {
            handDeck.Remove(card);
            if(alert) OnCardRemovedFromHandDeck?.Invoke(card);
            if (handDeck.Count == 0) OnZeroCards?.Invoke();
            return;
        }

        sideDeck.Remove(card);
    }

    public void RemoveCard(Card card, bool removeFromHandDeck = true, bool alert = true)
    {
        RemoveCard(card.cardData, removeFromHandDeck, alert);
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

    private void OnCardZeroUsages(Card card)
    {
        RemoveCard(card.cardData);
    }

    private void OnDestroy()
    {
        GameManager.OnDoneGenerating -= Beginning;
    }
}
