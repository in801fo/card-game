using System;
using UnityEngine;

public class CardInteractionManager : MonoBehaviour
{
    private RaycastHit hit = new RaycastHit();
    [SerializeField] private LayerMask cardsLayer;
    [Range(0, 1)]
    [Tooltip("The amount of time required for a card to be grabbed and moved around by the player")]
    [SerializeField] private float minTimeHold; 
    public static Action<Card> OnCardCursorHover;
    public static Action<Card> OnCardRelease;
    /// <summary>
    /// Only triggered on the first frame in which a card has started being held
    /// </summary>
    public static Action<Card> OnCardHold;
    private Card currentCardHover = null;
    private Card currentSelected;
    /// <summary>
    /// <para><i>In the previous check, was the player holding the card to change its position in the deck?</i></para>
    /// Need this to decide if the player has pressed the card with the intention of using it or if to just change its position in the hand deck
    /// </summary>
    private bool isHolding;

    private float timePressedMouse0Down;

    /// <summary>
    /// Need this to prevent a loop with the hover animation
    /// </summary>
    private float hoverCoolDown;


    private void Update()
    {
        //for when releasing
        if (Input.GetKeyUp(KeyCode.Mouse0))
        {

            if (!isHolding)
                UseCurrentlySelectedCard();
            else
                HandleCardRelease();


            timePressedMouse0Down = 0;

            currentSelected = null;
            isHolding = false;
        }


        if (Input.GetKey(KeyCode.Mouse0))
        {
            //TODO: clean this shit
            if (!isHolding)
            {
                if (RaycastForCard())
                {
                    currentSelected = currentCardHover;
                    timePressedMouse0Down += Time.deltaTime;
                }
                else
                {
                    currentSelected = null;
                    timePressedMouse0Down = 0;
                }

                //if holding the card
                if (timePressedMouse0Down > minTimeHold)
                {
                    OnCardHold?.Invoke(currentSelected);
                    isHolding = true;
                }
                else isHolding = false;
            }
        }


        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            if (RaycastForCard() && currentCardHover != currentSelected)
            {
                RuntimeMsg.Info("Get info for card: " + currentCardHover);
                GetCardInfoScreen(currentCardHover);
            }
        }
    }

    private void FixedUpdate()
    {
        if (hoverCoolDown > 0)
            hoverCoolDown -= Time.deltaTime;
        if (hoverCoolDown < 0) hoverCoolDown = 0;

        if (!isHolding)
        {
            //1) save the previous value of currentCardHover
            Card beforeCardHover = currentCardHover;
            //2) update currentCardHover only if cool down is zero
            if (hoverCoolDown == 0) RaycastForCard();

            if (currentCardHover == null && beforeCardHover != null) hoverCoolDown = 0.3f; 
            
            if (beforeCardHover != currentCardHover && currentCardHover != null) 
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
                RuntimeMsg.Warning(hit.rigidbody.name + " is on the Cards Layer!");
                return false;
            }

            //if the currentCardHover is not equal to the current card then
            //execute the shit
            if (currentCardHover == null || !currentCardHover.Equals(card))
                currentCardHover = card;

        }
        else currentCardHover = null; //otherwise set it to null

        return res;
    }

    private void GetCardInfoScreen(Card card)
    {
        GameObject screen = ScreensManager.Instance.SpawnScreen<CardInfoScreenData>("cardInfo");
        CardInfoScreenInitializer screenInitializer = screen.GetComponent<CardInfoScreenInitializer>();
        if(screenInitializer != null)
            screenInitializer.Initialize(new CardInfoScreenData(){
                card = card,
                //check if the player is under the effect of blindness (which masks the info of cards)
                isMasked = EnumMaskHandler<effectsEnum>.HasEnumValueInMask(
                    GameManager.localPlayerInfo.playerEffectsMask,
                    effectsEnum.BLINDNESS
                )
            });
    }

    private void UseCurrentlySelectedCard()
    {
        if (!currentSelected) return;
        currentSelected.UseCard();
    }

}