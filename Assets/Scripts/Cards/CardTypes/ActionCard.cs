using UnityEngine;

public class ActionCard : Card
{
    [field: SerializeField] public int damangeAmount { get; private set; }

    public override void UseCard()
    {
        base.UseCard();
        HpManager.Instance.LowerHp(GameManager.localPlayerHashCode, damangeAmount);
        //if (cardData.cardEffects != null && cardData.cardEffects.Length > 0)
            //EffectManager.ApplyEffects(EffectsEnumToEffectConverter.GetEffectFromEnum(cardData.cardEffects));
    }
}