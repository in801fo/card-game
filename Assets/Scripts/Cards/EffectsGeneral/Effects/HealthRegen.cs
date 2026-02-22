using UnityEngine;

public class HealthRegen : Effect
{
    public HealthRegen(effectData parameters) : base(parameters)
    {
    }

    public override effectsEnum GetEffectAsEnum()
    {
        return effectsEnum.REGENHEALTH;
    }

    public override void TerminateEffect()
    {
        RuntimeMsg.Info("Effect terminated: HealthRegen");
    }

    protected override void HandleLogic()
    {
        if (EnumMaskHandler<playerTagsEnum>.HasEnumValueInMask(GameManager.localPlayerInfo.playerTagsMask,
                playerTagsEnum.HAS_JUST_RECEIVED_DAMAGE))
                
                    HpManager.Instance.IncrementHpServer_Rpc(
                        Mathf.Abs(HpManager.justReceivedDamage),
                        GameManager.localPlayerInfo.playerId
                    );
    }
}