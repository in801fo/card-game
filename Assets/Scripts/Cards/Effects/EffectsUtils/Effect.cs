using System;

public abstract class Effect
{
    /// <summary>
    /// Amount of seconds which the effects last
    /// </summary>
    public static float secondsDuration { get; protected set; }

    /// <summary>
    /// Amount of turns which the effects last
    /// </summary>
    public static int turnDuration { get; protected set; }

    public static Action<effectData> OnEffectApplied { get; set; }
    public static effectsEnum effectAsEnum;
    private effectData effectParameters;

    protected bool hasInitialized = false;

    public Effect(effectData parameters)
    {
        Initialize(parameters);
    }

    public void Initialize(effectData parameters)
    {
        if (!parameters.doesTurns)
        {
            turnDuration = -1;
            secondsDuration = parameters.timeLeft;
        }
        else
        {
            secondsDuration = -1;
            turnDuration = parameters.turnsLeft;
        }

        hasInitialized = true;
    }

    public void Apply()
    {
        if (!hasInitialized)
        {
            RuntimeMsg.Warning("Tried to apply effect without initializing first",
                                $"The {GetEffectAsEnum()} effect has been tried to be applied without initializing first!");
            return;
        }
        
        HandleLogic();
        OnEffectApplied?.Invoke(effectParameters);
    }
    
    /// <summary>
    /// The method of which to create an implementation for the logic of the effect
    /// </summary>
    protected abstract void HandleLogic();

    public abstract void TerminateEffect();

    public abstract effectsEnum GetEffectAsEnum();

}
