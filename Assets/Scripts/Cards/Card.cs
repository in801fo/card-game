using System;
using UnityEngine;

public abstract class Card : MonoBehaviour
{
    protected bool Initialized;
    public CardScriptable cardData { get; private set; }
    public static Action<Card> OnCardUseReady;

    public virtual void UseCard()
    {
        if (!Initialized)
        {
            RuntimeMsg.Error("Tried using card before it was initialized!", "Tried using card before it was initialized!");
            return;
        }

        if (cardData.Type != cardTypeEnum.CHARACTER)
            if (cardData.hasSoundEffect) AudioManager.PlayCardSFX(cardData.onUseSoundEffect);
    }

    public void InitializeCard(CardScriptable cardData)
    {
        if (Initialized) return;
        Initialized = true;
        this.cardData = cardData;
    }

}