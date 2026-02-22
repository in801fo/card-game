using System;


public class TrapCard : Card
{
    public bool isFaceDown { get; private set; }

    public static Action<Card> OnTrapCardActivated;

    private void Awake()
    {
        TurnManager.OnLocalTurnStart += CheckAndSetUsability;
    }

    private void CheckAndSetUsability(){

        for (int i = 0; i < cardData.affectedTags.Length; i++) {
            if (EnumMaskHandler<playerTagsEnum>.HasEnumValueInMask(GameManager.localPlayerInfo.playerTagsMask, cardData.affectedTags[i])){
                
                canUseCard = true;
                break;
            }
        }
    }

    private void FixedUpdate()
    {
        if (!TurnManager.IsMyTurn) return;

        //if the card's conditions have been met and the card is face down then automatically activate it
        if (canUseCard && isFaceDown) UseCard();
    }

    public override void HandleCardLogic()
    {
        if (!canUseCard) {
            RuntimeMsg.Warning("Tried executing but the required conditions were not met!");
            return;
        }

        HandleEffectsAndSFX();
    }
    
    public void CoverCard()
    {
        //code for when the card is covered and is placed on the table
        isFaceDown = true;
    }
}