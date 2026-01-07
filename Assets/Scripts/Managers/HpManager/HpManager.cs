using System;
using UnityEngine;

public class HpManager : MonoBehaviour
{
    public static HpManager Instance;

    public float _hp { get; private set; } = maxHp;

    public const float maxHp = 100;

    /// <summary>
    /// First int is the Hashcode of the player which has lost/gained the amount specified by the float 
    /// </summary>
    public static Action<int, float> OnHealthChange;
    public static Action<int> OnHealthZero;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow)) LowerHp(GameManager.localPlayerHashCode, 10);
        if (Input.GetKeyDown(KeyCode.UpArrow)) IncrementHp(GameManager.localPlayerHashCode, 10);
    }

    //[ClientRpc]
    public void LowerHp/*ClientRpc*/(int hashCode, float damage)
    {
        if (_hp - damage <= 0)
        {
            _hp = 0;
            OnHealthZero?.Invoke(hashCode);
        }
        else
        {
            _hp -= damage;
            OnHealthChange?.Invoke(hashCode, -damage);
        }
    }

    /*
        clients shall call this one
        [ServerRpc]
        public void LowerHpServerRpc(int hashCode, float damage){
            basically just broadcast it
            LowerHpClientRpc(int hashCode, float damage)
        }
    
    */


    public void IncrementHp(int hashCode, float amount)
    {
        float reportAmount = amount;
        if (_hp + amount >= maxHp) {
            _hp = maxHp;
            //useful only in the case in which _hp + amount > maxHp
            reportAmount = maxHp - _hp;
        }
        else _hp += amount;

        OnHealthChange?.Invoke(hashCode, reportAmount);
    }


}
