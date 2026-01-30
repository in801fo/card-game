using System;

public class Blindness : IEffect
{
    public static float secondsDuration { get; set; }
    public static int turnDuration { get; set; }
    public static Action<consequenceTarget, int, float, effectsEnum> OnEffectApplied { get; set; }
    public static consequenceTarget target { get; set; }
    public static effectsEnum effectAsEnum = effectsEnum.BLINDNESS;

    public static void Apply(ulong[] _)
    {
        foreach(CardGraphics card in InventoryUIManager.Instance.cards)
        {
            card.MaskCard();
        }
    }

}
