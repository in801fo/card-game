using System.Linq;
using UnityEngine;

public abstract class Card : MonoBehaviour
{
    protected bool Initialized;
    public CardScriptable cardData { get; private set; }
    public virtual void UseCard()
    {
        if (!Initialized)
        {
            RuntimeMsg.Error("Tried using card before it was initialized!", "Tried using card before it was initialized!");
            return;
        }
        if (cardData.cardEffects == null || cardData.cardEffects.Length == 0) return;

        EffectManager.ApplyEffects(cardData.cardEffects.ToList());
    }

    public void InitializeCard(CardScriptable cardData)
    {
        if (Initialized) return;
        Initialized = true;
        this.cardData = cardData;
    }

}