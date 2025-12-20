using System;
using UnityEngine;

public class CardInteractionManager : MonoBehaviour
{
    private RaycastHit hit = new RaycastHit();
    [SerializeField] private LayerMask cardsLayer;
    public static Action<Card> OnCardCursorHover;
    public static Action<Card> OnCardUse; 
    private Card currentCardHover = null;
    private Card currentSelected;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (RaycastForCard() && currentCardHover != currentSelected)
            {
                currentSelected = currentCardHover;
                UseCurrentlySelectedCard();
                RuntimeError.Info("New Card!", "Selected New Card: " + currentSelected.card.Name);
            }
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
        
        OnCardCursorHover?.Invoke(currentCardHover);
        return res;
    }

    private void UseCurrentlySelectedCard()
    {
        OnCardUse?.Invoke(currentSelected);
    }

    private void FixedUpdate()
    {
        RaycastForCard();
    }
}