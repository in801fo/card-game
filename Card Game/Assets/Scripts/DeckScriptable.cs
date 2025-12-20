using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewDeck", menuName = "Create New Deck")]
public class DeckScriptable : ScriptableObject
{
    [field: SerializeField] public string deckName { get; private set; }
    [TextArea(50, 100)]
    [field: SerializeField] public string shortDeckDescription { get; private set; }
    public List<CardScriptable> Cards = new List<CardScriptable>();
}
