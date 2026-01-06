using UnityEngine;

public class HpManager : MonoBehaviour
{
    public static HpManager Instance;

    public float _hp { get; private set; }

    public const float maxHp = 100;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);
    }

    public void LowerHp(int damage)
    {
        if (_hp - damage <= 0) _hp = 0;
        else _hp -= damage;
    }

    public void IncrementHp(int amount)
    {
        if (_hp + amount >= maxHp) _hp = maxHp;
        else _hp += amount;
    }


}
