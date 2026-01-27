using System;
using TMPro;
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

    private float initialPreferredSize;

    private void Awake()
    {
        if (healthBarLayoutElement == null) RuntimeMsg.Error("LayoutElement component not present!", $"LayoutElement for player {_myPlayer.Value.Name} was not found...");
        else initialPreferredSize = healthBarLayoutElement.preferredWidth;

        HpManager.OnHealthChange += HandleCheckHealthChange;
        HpManager.OnHealthZero += HandleCheckLifeZero;
        HpUIManager.OnHandleHpPlayerDisconnect += HandlePlayerDisconnect;
    }

    private void HandleCheckLifeZero(ulong playerId)
    {
        if (playerId.Equals(_myPlayer.Value.playerId)) HandlePlayerDeath();
    }

    public void Initialize(playerInfo? info)
    {
        _myPlayer = info;
        /*if (!_myPlayer.HasValue)
        {
            RuntimeMsg.Warning("_myPlayer value not defined", "Value for player not specified therefore I generated a random string of number as identifier for player...");
            _myPlayer = new playerInfo
            {
                Name = Random.Range(10000, 5000).ToString(),
                Pronouns = pronouns.THEYTHEM
            };
        }

        //rare occasions in which the packet with the player's name is lost...
        if (_myPlayer.Value.Name.IsEmpty)
        {
            _myPlayer = new playerInfo {
                Name = Random.Range(10000, 5000).ToString(),
                Pronouns = _myPlayer.Value.Pronouns,
                playerId = _myPlayer.Value.playerId
            };
        }*/


        playerNameSpace.SetText(_myPlayer.Value.Name.ToString());
    }

    /// <summary>
    /// Changes the bar length only if the player which received the damage matches the one
    /// which this instance represents
    /// </summary>
    /// <param name="playerId"></param>
    /// <param name="amount"></param>
    private void HandleCheckHealthChange(ulong playerId, float amount)
    {
        if (playerId.Equals(_myPlayer.Value.playerId)) ChangeLife(amount);
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
    
    private void HandlePlayerDisconnect(ulong playerId)
    {
        if (playerId != _myPlayer.Value.playerId) return;

        //TODO: make it so that if the player manages to reconnect after disconnection their health is saved
        Destroy(healthBarLayoutElement);
        playerNameSpace.SetText(playerNameSpace.text + ": Disconnected (Bummer)");

        HandleUnsubscribeHpEvents();
    }

    private void HandleUnsubscribeHpEvents()
    {
        HpManager.OnHealthChange -= HandleCheckHealthChange;
        HpManager.OnHealthZero -= HandleCheckLifeZero;
    }

    private void HandlePlayerDeath()
    {
        if (healthBarLayoutElement != null) Destroy(healthBarLayoutElement.gameObject);

        playerNameSpace.SetText($"<s>{_myPlayer.Value.Name}</s>");
    }
}