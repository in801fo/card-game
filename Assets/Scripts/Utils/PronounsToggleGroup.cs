using UnityEngine.UI;

public class PronounsToggleGroup : ToggleGroup
{
    public pronouns GetSelectedPronoun()
    {
        return GetFirstActiveToggle().GetComponent<PronounToggle>().representingPronoun;
    }


}