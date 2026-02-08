using UnityEngine.UI;

public class PronounsToggleGroup : ToggleGroup
{
    public pronouns GetSelectedPronoun()
    {
        PronounToggle t = GetFirstActiveToggle().GetComponent<PronounToggle>();

        return t.representingPronoun;
    }

    public void SetPronounActive(pronouns pr)
    {
        m_Toggles.Find((Toggle t) => t.gameObject.GetComponent<PronounToggle>().representingPronoun == pr).isOn = true;
    }
}