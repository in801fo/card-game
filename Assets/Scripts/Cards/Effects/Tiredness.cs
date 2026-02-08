using Unity.Netcode;

public class Tiredness : Effect
{
    public Tiredness(effectData parameters) : base(parameters)
    {
    }

    public override effectsEnum GetEffectAsEnum()
    {
        return effectsEnum.TIREDNESS;
    }

    public override void TerminateEffect()
    {
        Card.OnCardUseReady -= OnActionCardUse;
    }

    protected override void HandleLogic()
    {
        Card.OnCardUseReady += OnActionCardUse;
    }

    private void OnActionCardUse(Card c)
    {
        if (c.cardData.Type != cardTypeEnum.ACTION) return;
        HpManager.Instance.LowerHpServer_Rpc(1, NetworkManager.Singleton.LocalClientId);
    }
    
}