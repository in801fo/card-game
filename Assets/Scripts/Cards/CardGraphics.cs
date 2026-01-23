using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardGraphics : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameBox;
    [SerializeField] private Image cardImage;
    [SerializeField] private TextMeshProUGUI descBox;
    [SerializeField] private TextMeshProUGUI damageBox;
    [SerializeField] private GameObject effectsScrollViewContentObject;
    [SerializeField] private AnimationClip cardSelectedAnimation;
    [SerializeField] private bool isInfoCard;

    public Card card { get; private set; }

    private Animator animator;

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

    public void SetUpGameCard(float cardFrontOffset, Card card)
    {
        this.card = card;

        nameBox.SetText(card.cardData.Name);

        MeshFilter meshFilter = this.gameObject.GetComponent<MeshFilter>();

        SetCardUVs(meshFilter.mesh, cardFrontOffset);
    }
    
    private void HandleAnimationState(Card card)
    {
        
        if (card != null && card == this)
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

    public void SetUpInfoCard(Card card)
    {
        SetUpGameCard(CardGenerationManager.Instance.GetCardFrontIndex(card.cardData.Type), card);

        descBox.SetText(card.cardData.Description);

        //TODO: improve formatting in strings
        if (card.cardData.Type != cardTypeEnum.CHARACTER)
            damageBox.SetText("Damage: " + card.cardData.damageAmount + "\nMax Usages: " + card.cardData.maxCardUsages + "\nLeft Usages: " + card.currentCardWear);
        else damageBox.SetText("No Damage\nNo Max Usages");
        isInfoCard = true;
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
}
