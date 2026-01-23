using UnityEngine;

[CreateAssetMenu(fileName = "NewCard", menuName = "Create New Card")]
public class CardScriptable : ScriptableObject
{

    public const int maxCardUsagesConst = 10; 

    [field: SerializeField] public string Name { get; private set; }
    [field: SerializeField] public Sprite Sprite { get; private set; }
    [field: SerializeField] public string Description { get; private set; }
    [field: SerializeField] public cardTypeEnum Type { get; private set; }
    [field: SerializeField] public consequenceTarget consequenceTarget { get; private set; }
    //this makes it so that a minimum number of players must play
    [Tooltip("If set to -1, one can choose a tag with which to discriminate players and decide who will receive the damage")]
    [field: SerializeField] public int numberOfAffectedPlayers { get; private set; }

    [Tooltip("Does this card heal instead of damaging other players?")]
    [field: SerializeField] public bool Heals { get; private set; }
    [field: SerializeField][Range(0, HpManager.maxHp)] public float damageAmount { get; private set; }
    [field: SerializeField] public effectsEnum[] cardEffects { get; private set; }
    [field: SerializeField] public int maxCardUsages { get; private set; }
    //I KNOW I KNOW IT'S BAD BUT UNITY'S WORSE WITH ITS SHITTY ASS SERIALIZATION, FORGIVE ME FATHER T_T
    [SerializeField] public playerTagsEnum[] affectedTags;
    [field: SerializeField] public bool hasSoundEffect { get; private set; }
    [field: SerializeField] public AudioClip onUseSoundEffect { get; private set; }

}
