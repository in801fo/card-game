using TMPro;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUIHandler : MonoBehaviour
{
    [SerializeField] private LayoutElement healthBarLayoutElement;
    [SerializeField] private TextMeshProUGUI playerNameSpace;

    public playerInfo? myPlayer
    {
        get
        {
            return _myPlayer;
        }

        set
        {
            if (!_myPlayer.HasValue) _myPlayer = value;
        }
    }
    public playerInfo? _myPlayer;

    private float currentLife = HpManager.maxHp;

    private float initialPreferredSize;

    private void Awake()
    {
        if (healthBarLayoutElement == null) RuntimeMsg.Error("LayoutElement component not present!", $"LayoutElement for player {_myPlayer.Value.Name} was not found...");
        else initialPreferredSize = healthBarLayoutElement.preferredWidth;

        HpManager.OnHealthChange += HandleCheckHealthChange;
        HpManager.OnHealthZero += HandleCheckLifeZero;
    }

    private void HandleCheckLifeZero(int hashCode)
    {
        if (hashCode.Equals(_myPlayer.Value.playerHashCode)) HandlePlayerDeath();
    }

    public void Initialize(playerInfo? info)
    {
        _myPlayer = info;
        if (!_myPlayer.HasValue)
        {
            RuntimeMsg.Warning("_myPlayer value not defined", "Value for player not specified therefore I generated a random string of number as identifier for player...");
            _myPlayer = new playerInfo
            {
                Name = Random.Range(10000, 5000).ToString(),
                Pronouns = pronouns.THEYTHEM
            };

        }
        playerNameSpace.SetText(_myPlayer.Value.Name);
    }

    private void HandleCheckHealthChange(int hashCode, float amount)
    {
        if (hashCode.Equals(_myPlayer.Value.playerHashCode)) ChangeLife(amount);
    }

    private void ChangeLife(float amount)
    {
        if (healthBarLayoutElement == null)
        {
            RuntimeMsg.Warning("Trying to change life points after player death.");
            return;
        }
        //proportion
        float toApply = initialPreferredSize / HpManager.maxHp * amount;
        
        if (healthBarLayoutElement.preferredWidth + toApply <= 0) Destroy(healthBarLayoutElement.gameObject);
        if (healthBarLayoutElement.preferredWidth + toApply >= initialPreferredSize) toApply = initialPreferredSize - healthBarLayoutElement.preferredWidth;
        healthBarLayoutElement.preferredWidth += toApply;
    }

    private void HandlePlayerDeath()
    {
        if (healthBarLayoutElement != null) Destroy(healthBarLayoutElement.gameObject);

        playerNameSpace.SetText($"<s>{_myPlayer.Value.Name}</s>");
    }
}