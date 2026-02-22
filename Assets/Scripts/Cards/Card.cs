using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Card : MonoBehaviour
{
    protected bool Initialized;
    public CardScriptable cardData { get; protected set; }
    public static Action<Card> OnCardUseReady;

    public int currentCardWear { get; private set; }

    public static Action<Card> OnCardZeroUsages;

    /// <summary>
    /// Field to know if the conditions for the current card (if of type Trap) have been met
    /// </summary>
    protected bool canUseCard;

    /// <summary>
    /// Array containing all the ids of the attacked players 
    /// </summary>
    protected ulong[] attackedPlayers;

    public void UseCard()
    {
        if (!Initialized)
        {
            RuntimeMsg.Error("Tried using card before it was initialized!", "Tried using card before it was initialized!");
            return;
        }

        //TODO: make this independent from the 
        if (!TurnManager.IsMyTurn)
        {
            RuntimeMsg.Info("You may not use cards whilst it's not your turn!");
            return;
        }

        HandleCardLogic();
    }

    /// <summary>
    /// Triggers the listed effects and also plays a random effect contained in the card scriptable list
    /// </summary>
    /// <param name="affectedPlayers"></param>
    protected void HandleEffectsAndSFX()
    {
        //if the card has effects
        if (cardData.cardEffects != null && cardData.cardEffects.Length != 0)
            //if the passed list of affected players is null that means that the affected are all players in the game
            //TODO: differentiate between ALL_INC and ALL_EX, currently only considering ALL_INC

            //request the server for them to be applied
            EffectsManager.Instance.RequestApplyEffectsServer_Rpc(
                    cardData.consequenceTarget,
                    cardData.cardEffects,
                    attackedPlayers
                );

        if (cardData.hasSoundEffect)
            AudioManager.Instance.RequestPlayCardSFXServer_Rpc(UnityEngine.Random.Range(0, cardData.onUseSoundEffect.Length), cardData.name);

        OnCardUseReady?.Invoke(this);
    }
    /// <summary>
    /// Implement the card's behaviour here. Invoked after some initial checks in the <c>UseCard</c> method
    /// </summary>
    public abstract void HandleCardLogic();

    public void InitializeCard(CardScriptable cardData)
    {
        if (Initialized) return;
        Initialized = true;
        this.cardData = cardData;
        currentCardWear = this.cardData.maxCardUsages;
    }

    protected void ReduceCardUse()
    {
        if (currentCardWear - 1 <= 0)
        {
            OnCardZeroUsages?.Invoke(this);
            return;
        }
        currentCardWear -= 1;
    }

}