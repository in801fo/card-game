using UnityEngine;

public abstract class Card : MonoBehaviour
{
    private bool Initialized;
    public CardScriptable cardData { get; private set; }

    public abstract void UseCard();

    public void InitializeCard(CardScriptable cardData)
    {
        if (Initialized) return;
        Initialized = true;
        this.cardData = cardData;
    }

}