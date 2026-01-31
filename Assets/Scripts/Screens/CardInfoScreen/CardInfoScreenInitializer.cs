using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class CardInfoScreenInitializer : ScreenInitializer<CardInfoScreenData>
{
    [SerializeField] private GameObject effectEntryPrefab;

    private GameObject contentObjectEffectsScrollView;

    private const string effectsLabel = "effectsList";

    private List<EffectEntryUI> effectEntries = new List<EffectEntryUI>();

    private Card card;

    public override void Initialize(CardInfoScreenData data)
    {
        contentObjectEffectsScrollView = GameObject.FindWithTag(effectsLabel);
        
        GetComponentInChildren<CardGraphics>().SetUpInfoCard(data.card, data.isMasked);

        this.card = data.card;
        EffectsScrollView();

        base.Initialize(data);
    }

    private void EffectsScrollView()
    {
        if (card.cardData.Type == cardTypeEnum.CHARACTER)
            return;
        //To go around a bug which, for the love of God, I cannot figure out the origin,
        //I pool existing effects and will only show the ones which the current card has
        if (!hasInitialized) InitializeEffectsScrollView();
        else ToggleEffects();
    }

    private void ToggleEffects()
    {
        for (int i = 0; i < effectEntries.Count; i++)
        {
            //I know I could simplify this but for readability I'll leave it like this
            if (card.cardData.cardEffects.Contains(effectEntries[i].representingEffect))
                effectEntries[i].gameObject.SetActive(true);
            else
                effectEntries[i].gameObject.SetActive(false);
        }
    }

    private void InitializeEffectsScrollView()
    {

        int numberOfEffects = card.cardData.cardEffects.Length;

        for (int i = 0; i < numberOfEffects; i++)
        {
            InitializeEffectEntry(
                card.cardData.cardEffects[i]
                /*//does the card have effects? No? return false. Yes? Does the list of effects contain the current effect (value)
                (card.cardData == null || card.cardData.cardEffects == null) ?
                    false : 
                    card.cardData.cardEffects.Where((effectData effectData) => effectData.effect == value) != null*/
            );
        }

    }

    private void InitializeEffectEntry(effectData effectData)
    {
        GameObject currentEntryPrefab = Instantiate(effectEntryPrefab, Vector3.zero, Quaternion.identity);

        EffectEntryUI effectEntryUI = currentEntryPrefab.AddComponent<EffectEntryUI>();
        effectEntryUI.representingEffect = effectData;

        TextMeshProUGUI textArea = currentEntryPrefab.GetComponentInChildren<TextMeshProUGUI>();
        textArea.SetText(effectData.effect.ToString());

        currentEntryPrefab.transform.SetParent(contentObjectEffectsScrollView.transform);
        effectEntries.Add(effectEntryUI);
        //currentEntryPrefab.SetActive(state);
    }

}

public struct CardInfoScreenData
{
    public Card card;
    public bool isMasked;
}