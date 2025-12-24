using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameBox;
    [SerializeField] private Image cardImage;
    [SerializeField] private TextMeshProUGUI descBox;
    [SerializeField] private TextMeshProUGUI damageBox;
    [SerializeField] private GameObject effectsScrollViewContentObject;
    [SerializeField] private AnimationClip cardSelectedAnimation;
    [SerializeField] private bool isInfoCard;

    private Animator animator;

    public CardScriptable card { get; private set; }

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

    private void Start()
    {
        animator = GetComponent<Animator>();
        if (!isInfoCard)
        {
            CardInteractionManager.OnCardCursorHover +=
                (Card card) =>
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
                    };
        }
    }

    public void SetUpGameCard(float cardFrontOffset, CardScriptable card)
    {
        nameBox.SetText(card.name);

        MeshFilter meshFilter = this.gameObject.GetComponent<MeshFilter>();

        SetCardUVs(meshFilter.mesh, cardFrontOffset);
        
        this.card = card;
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
        if(!isInfoCard) this.transform.parent.rotation = Quaternion.LookRotation(this.transform.parent.position - Camera.main.transform.position) * Quaternion.Euler(Vector3.forward * 90);
    }

    public void SetUpInfoCard(CardScriptable card)
    {
        SetUpGameCard(CardGenerationManager.Instance.GetCardFrontIndex(card.type), card);
        descBox.SetText(card.Description);
        damageBox.SetText("Damage: " + card.damageAmount);
        isInfoCard = true;
    }

    public override bool Equals(object obj)
    {
        if (obj == null) return false;
        Card card;
        try
        {
            card = (Card)obj;
        }
        catch (InvalidCastException) { return false; }

        return this.card == card.card;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public IEnumerator AnimateCard(cardReshuffleAnimationParams parameters)
    {
        float elapsedTime = 0;

        while (elapsedTime < parameters.duration)
        {
            float curveValue = parameters.curve.Evaluate(elapsedTime / parameters.duration);

            transform.parent.position = Vector3.Lerp(this.transform.parent.position, parameters.targetPosition, curveValue);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.parent.position = parameters.targetPosition;
    }
}
