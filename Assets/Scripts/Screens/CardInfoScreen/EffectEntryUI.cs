using UnityEngine;

/// <summary>
/// Need this class to remember the type of effect represented by the entry in case I will ever create a menu describing all the effects
/// </summary>
public class EffectEntryUI : MonoBehaviour
{
    public effectData representingEffect
    {
        get
        {
            return _representingEffect.Value;
        }
        set
        {
            if (!_representingEffect.HasValue) _representingEffect = value;
        }

    }
    private effectData? _representingEffect;
}