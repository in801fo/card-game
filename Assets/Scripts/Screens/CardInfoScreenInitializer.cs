using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class CardInfoScreenInitializer : ScreenInitializer<Card>
{
    [SerializeField] private GameObject effectEntryPrefab;

    private GameObject contentObjectEffectsScrollView;

    private const string effectsLabel = "effectsList";

    private List<EffectEntryUI> effectEntries = new List<EffectEntryUI>();

    private Card card;

    private void Awake()
    {
        contentObjectEffectsScrollView = GameObject.FindWithTag(effectsLabel);
    }

    public override void Initialize(Card card)
    {
        GetComponentInChildren<CardGraphics>().SetUpInfoCard(card);
        this.card = card;
        EffectsScrollView();

        base.Initialize(card);
    }
    
    private void EffectsScrollView()
    {
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
            else effectEntries[i].gameObject.SetActive(false);
        }
    }

    private void InitializeEffectsScrollView()
    {

        Array effects = Enum.GetValues(typeof(effectsEnum));
        int numberOfEffects = effects.Length;


        for (int i = 0; i < numberOfEffects; i++)
        {
            effectsEnum value = (effectsEnum)effects.GetValue(i);
            InitializeEffectEntry(
                    value,
                    card.cardData.cardEffects.Contains(value)
            );
        }

    }
    
    private void InitializeEffectEntry(effectsEnum effect, bool state)
    {
        GameObject currentEntryPrefab = Instantiate(effectEntryPrefab, Vector3.zero, Quaternion.identity);

        EffectEntryUI effectEntryUI = currentEntryPrefab.AddComponent<EffectEntryUI>();
        effectEntryUI.representingEffect = effect;

        TextMeshProUGUI textArea = currentEntryPrefab.GetComponentInChildren<TextMeshProUGUI>();
        textArea.SetText(effect.ToString());

        currentEntryPrefab.transform.SetParent(contentObjectEffectsScrollView.transform);
        effectEntries.Add(effectEntryUI);
        currentEntryPrefab.SetActive(state);
    }


}