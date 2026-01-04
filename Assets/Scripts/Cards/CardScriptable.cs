using UnityEngine;

[CreateAssetMenu(fileName = "NewCard", menuName = "Create New Card")]

public class CardScriptable : ScriptableObject
{


    [field: SerializeField] public string Name { get; private set; }
    [field: SerializeField] public Sprite Sprite { get; private set; }
    [field: SerializeField] public string Description { get; private set; }
    [field: SerializeField] public CardTypeEnum type { get; private set; }
    [field: SerializeField][Range(1, HpManager.maxHp)] public float damageAmount { get; private set; }
    [field: SerializeField] public effectsEnum[] cardEffects { get; private set; }
    [field: SerializeField][Range(1, 10)] public int maxCardUsages { get; private set; }

}
