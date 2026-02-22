using UnityEngine;

public class PerTurnDamage : Effect
{
    public PerTurnDamage(effectData parameters) : base(parameters)
    {
        RuntimeMsg.Info("Subscribed!");
        TurnManager.OnTurnBegin += HandleLogic;
    }

    public override effectsEnum GetEffectAsEnum()
    {
        return effectsEnum.PERTURNDAMAGE;
    }

    public override void TerminateEffect()
    {
        TurnManager.OnTurnBegin -= HandleLogic;
    }

    protected override void HandleLogic()
    {
        HpManager.Instance.LowerHpServer_Rpc(effectParameters.damage, GameManager.localPlayerInfo.playerId);
    }
}