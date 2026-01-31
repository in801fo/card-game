using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardGraphics : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameBox;
    [SerializeField] private Image cardImageSpace;
    [SerializeField] private TextMeshProUGUI descBox;
    [SerializeField] private TextMeshProUGUI damageBox;
    [SerializeField] private bool isInfoCard;

    private const string maskedCardTitle = "?????";
    private const string maskedCardDescription = "?????";
    private const string maskedCardData = "??????";


    public Card card { get; private set; }

    private Animator animator;

    private Sprite cardImage;

    private bool isMasked;

    private float cardFrontOffset;

    private List<Vector2> defaultUVs = new List<Vector2>()
    {
        new Vector2(0.80f, 1.00f),
        new Vector2(1.00f, 1.00f),
        new Vector2(1.00f, 0.71f),
        new Vector2(0.80f, 0.71f),
        new Vector2(0f, 1.00f),
        new Vector2(0.20f, 1.00f),
        new Vector2(0.20f, 0.71f),
        new Vector2(0.00f, 0.71f)
    };

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        if (!isInfoCard)
            CardInteractionManager.OnCardCursorHover += HandleAnimationState;
    }

    public void SetUpGameCard(float cardFrontOffset, Card card, bool isMasked = false)
    {
        this.card = card;

        MeshFilter meshFilter = this.gameObject.GetComponent<MeshFilter>();

        this.cardFrontOffset = cardFrontOffset;

        SetCardUVs(meshFilter.mesh, cardFrontOffset);

        if (!isMasked)
        {
            nameBox.SetText(card.cardData.Name);
            cardImage = card.cardData.Sprite;
        }
        else
            MaskCard();
    }

    private void HandleAnimationState(Card card)
    {

        if (card != null && card == this.card)
        {
            animator.SetBool("up", true);
            animator.SetBool("down", false);
        }
        else
        {
            animator.SetBool("up", false);
            animator.SetBool("down", true);
        }

    }

    public void MaskCard()
    {
        nameBox.SetText(maskedCardTitle);
        cardImageSpace.sprite = InventoryUIManager.Instance.maskedCardSprite;
        isMasked = true;
    }

    private void SetCardUVs(Mesh mesh, float cardFrontOffset)
    {
        List<Vector2> uvs = new List<Vector2>();

        mesh.SetUVs(0, defaultUVs);

        mesh.GetUVs(0, uvs);

        for (int i = 0; i < 4; i++)
        {
            //print("Before: " + uvs[i+4]);
            uvs[i + 4] -= new Vector2(0, cardFrontOffset);
            //print("After: " + uvs[i+4]);
        }

        mesh.SetUVs(0, uvs);
    }
    
    private void FixedUpdate()
    {
        if(!isInfoCard) 
            this.transform.parent.rotation = Quaternion.LookRotation(this.transform.parent.position - Camera.main.transform.position) * Quaternion.Euler(Vector3.forward * 90);
    }

    public void SetUpInfoCard(Card card, bool mask = false)
    {
        SetUpGameCard(CardGenerationManager.Instance.GetCardFrontIndex(card.cardData.Type), card, mask);

        if (!isMasked) descBox.SetText(card.cardData.Description);
        else descBox.SetText(maskedCardDescription);

        HandleDamageText();

        isInfoCard = true;
    }

    private void HandleDamageText()
    {
        //TODO: improve formatting in strings (i.e. I don't like how they look in-game)
        string damageText = $"Damage: " + (isMasked ? maskedCardData : card.cardData.damageAmount) +
                                "\nMax Usages: " + (isMasked ? maskedCardData : card.cardData.maxCardUsages) +
                                "\nLeft Usages: " + (isMasked ? maskedCardData : card.currentCardWear);
        
        if (card.cardData.Type != cardTypeEnum.CHARACTER || isMasked)
            damageBox.SetText(damageText);
        else damageBox.SetText("No Damage\nNo Max Usages");
    }

    public override bool Equals(object obj)
    {
        if (obj == null) return false;
        CardGraphics card;
        try
        {
            card = (CardGraphics)obj;
        }
        catch (InvalidCastException) { return false; }

        return this.card.cardData == card.card.cardData;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override string ToString()
    {
        return $"Card {card.cardData.Name}. Description {card.cardData.Description}";
    }

    public IEnumerator AnimateCard(cardReshuffleAnimationParams parameters)
    {
        //Also checking all over the place if the current instance is null (only happens when the object is destroyed)
        if (this == null) yield break;
        
        float elapsedTime = 0;

        while (elapsedTime < parameters.duration && this != null)
        {
            float curveValue = parameters.curve.Evaluate(elapsedTime / parameters.duration);

            transform.parent.position = Vector3.Lerp(this.transform.parent.position, parameters.targetPosition, curveValue);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (this == null) yield break;

        transform.parent.position = parameters.targetPosition;
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
        CardInteractionManager.OnCardCursorHover -= HandleAnimationState;
    }

    public void UnMaskCard()
    {
        SetUpGameCard(this.cardFrontOffset, this.card, false);
        cardImageSpace.sprite = cardImage;
        isMasked = false;
    }
}
