using _Scripts.SERVER;
using UnityEngine;
public class HandleSuggestions : MonoBehaviour
{
    public WebServer server;
    private void OnEnable()
    {
        server.OnTextQueryResponseReceived += SetSuggestionButtons;
    }
    private void OnDisable()
    {
        server.OnTextQueryResponseReceived -= SetSuggestionButtons;
    }

    public void SetSuggestionButtons()
    {
        if (server.serverResponseObject.suggestions != null && server.serverResponseObject.suggestions.Length > 0)
        {
            for (int i = 0; i < server.serverResponseObject.suggestions.Length; i++)
            {
                Debug.Log($"  {i + 1}. {server.serverResponseObject.suggestions[i]}");
            }
        }
        else
        {
            Debug.Log("No suggestions received.");
        }
        Debug.Log("Audio URL: " + server.serverResponseObject.audio_url);
    }
    
    public void TogglePanel(GameObject panelToToggle)
    {
        panelToToggle.SetActive(!panelToToggle.activeInHierarchy);
    }
    
}
