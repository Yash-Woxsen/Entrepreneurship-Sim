using UnityEngine;
using TMPro;
using UnityEngine.Events;
using _Scripts.SERVER;

public class HandleTextQuery : MonoBehaviour
{
    public TMP_InputField inputTextFieldForQuery;
    public WebServer server;
    public TMP_Text textAreaForChatHistory;

    private void OnEnable()
    {
        inputTextFieldForQuery.onSubmit.AddListener(SendQuery);
        server.OnTextQueryResponseReceived += OnServerResponseReceived;
        server.OnAudioQueryResponseReceived += OnWhisperServerAudioResponseReceived;
    }

    private void OnDisable()
    {
        inputTextFieldForQuery.onSubmit.RemoveListener(SendQuery);
        server.OnTextQueryResponseReceived -= OnServerResponseReceived;
        server.OnAudioQueryResponseReceived -= OnWhisperServerAudioResponseReceived;
    }

    public void SendQuery(string textInInputField)
    {
        string text = inputTextFieldForQuery.text;
        if (!string.IsNullOrEmpty(text))
        {
            server.SendTextQuery(text);
            textAreaForChatHistory.text += "<color=#F87297>USER</color>: " + text + "\n";
            inputTextFieldForQuery.text = "";
        }
        else
        {
            Debug.LogWarning("Input field is empty.");
        }
    }
    public void OnServerResponseReceived()
    {
        // Store values in variables
        string textResponse = server.serverResponseObject.response;
        // Log the parsed data
        textAreaForChatHistory.text += "<color=#33FFB6>MENTOR</color>: " + textResponse + "\n";
    }
    
    public void OnWhisperServerAudioResponseReceived()
    {
        string whisperResponse = server.whisperResponseObject.transcription;
        textAreaForChatHistory.text += "<color=#F87297>USER</color>: " + whisperResponse + "\n";
        server.SendTextQuery(whisperResponse);
    }
}
