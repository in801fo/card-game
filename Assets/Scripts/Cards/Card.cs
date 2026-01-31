using System;
using UnityEngine;

public abstract class Card : MonoBehaviour
{
    protected bool Initialized;
    public CardScriptable cardData { get; private set; }
    public static Action<Card> OnCardUseReady;

    public int currentCardWear { get; private set; }

    public static Action<Card> OnCardZeroUsages;

    public virtual void UseCard()
    {
        if (!Initialized)
        {
            RuntimeMsg.Error("Tried using card before it was initialized!", "Tried using card before it was initialized!");
            return;
        }
    }

    public void InitializeCard(CardScriptable cardData)
    {
        if (Initialized) return;
        Initialized = true;
        this.cardData = cardData;
        currentCardWear = this.cardData.maxCardUsages;
    }

    public void ReduceCardUse()
    {
        if (currentCardWear - 1 <= 0)
        {
            OnCardZeroUsages?.Invoke(this);
            return;
        }
        currentCardWear -= 1;
    }

}