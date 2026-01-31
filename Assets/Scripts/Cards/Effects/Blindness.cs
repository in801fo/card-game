
public class Blindness : Effect
{
    public Blindness(effectData parameters) : base(parameters)
    {
    }

    protected override void HandleLogic()
    {
        foreach(CardGraphics card in InventoryUIManager.Instance.cards)
        {
            card.MaskCard();
        }
    }

    public override effectsEnum GetEffectAsEnum()
    {
        return effectsEnum.BLINDNESS;
    }

    public override void TerminateEffect()
    {
        foreach(CardGraphics card in InventoryUIManager.Instance.cards)
        {
            card.UnMaskCard();
        }
    }
}
