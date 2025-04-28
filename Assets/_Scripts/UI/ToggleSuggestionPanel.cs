using UnityEngine;

public class ToggleSuggestionPanel : MonoBehaviour
{
    public void TogglePanel(GameObject panelToToggle)
    {
        panelToToggle.SetActive(!panelToToggle.activeInHierarchy);
    }
}
