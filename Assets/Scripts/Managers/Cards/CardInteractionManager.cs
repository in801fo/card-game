using System;
using UnityEngine;

public class CardInteractionManager : MonoBehaviour
{
    private RaycastHit hit = new RaycastHit();
    [SerializeField] private LayerMask cardsLayer;
    public static Action<Card> OnCardCursorHover;
    public static Action<Card> OnCardUse;
    public static Action<Card> OnCardRelease;
    public static Action<Card> OnCardHold;
    private Card currentCardHover = null;
    private Card currentSelected;
    /// <summary>
    /// <para><i>In the previous check, was the player holding the card to chenge its position in the deck?</i></para>
    /// Need this to decide if the player has pressed the card with the intention of using it or if to just change its position in the hand deck
    /// </summary>
    private bool wasHolding;

    /// <summary>
    /// <para><i>Has the player pressed on the card to use it in the game?</i></para>
    /// Need this to decide if the player has pressed the card with the intention of using it or if to just change its position in the hand deck
    /// </summary>
    private bool hasPressedDown;


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (RaycastForCard() && currentCardHover != currentSelected)
            {
                currentSelected = currentCardHover;
                hasPressedDown = true;
                //RuntimeError.Info("New Card!", "Selected New Card: " + currentSelected.card.Name);
            }
        }


        if (Input.GetKey(KeyCode.Mouse0))
        {
            if (RaycastForCard())
            {
                if (!wasHolding) OnCardHold?.Invoke(currentSelected);
                wasHolding = true;
            }
        }

        //for when releasing
        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            if (hasPressedDown)
            {
                if (!wasHolding)
                    UseCurrentlySelectedCard();
                else
                    HandleCardRelease();
            }

            hasPressedDown = false;
            wasHolding = false;
        }



        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            if (RaycastForCard() && currentCardHover != currentSelected)
            {
                RuntimeError.Info("Get info for card: " + currentCardHover);
                InventoryUIManager.GetCardInfoScreen(currentCardHover.card);
            }
        }
    }

    private void FixedUpdate()
    {
        if (!wasHolding)
        {
            if(RaycastForCard())
                OnCardCursorHover?.Invoke(currentCardHover);
        }
    }

    private void HandleCardRelease()
    {
        OnCardRelease?.Invoke(currentSelected);
    }

    private bool RaycastForCard()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        bool res = Physics.Raycast(ray, out hit, cardsLayer);
        if (res)
        {
            Card card;
            if (!hit.rigidbody.gameObject.TryGetComponent<Card>(out card))
            {
                RuntimeError.Warning(hit.rigidbody.name + " is on the Cards Layer!");
                return false;
            }

            //if the currentCardHover is not equal to the current card then
            //execute the shit
            if (currentCardHover == null || !currentCardHover.Equals(card))
                currentCardHover = card; //otherwhise set it to null

        }
        else currentCardHover = null;
        
        return res;
    }

    private void UseCurrentlySelectedCard()
    {
        OnCardUse?.Invoke(currentSelected);
    }

}