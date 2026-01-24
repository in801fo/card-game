using System;
using UnityEngine;
using UnityEngine.UI;

public class PronounToggle : Toggle
{
    [field: SerializeField] public pronouns representingPronoun { get; private set; }

}