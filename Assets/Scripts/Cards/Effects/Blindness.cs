using System;

public class Blindness : Effect
{
    public Blindness(effectTarget target, int turnDuration, float secondsDuration) : base(target, turnDuration, secondsDuration)
    {   
    }

    public override void Apply()
    {
        OnEffectApplied?.Invoke(target, turnDuration, secondsDuration, effectsEnum.BLINDNESS);
    }
}
