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
    }

    private void OnDisable()
    {
        inputTextFieldForQuery.onSubmit.RemoveListener(SendQuery);
        server.OnTextQueryResponseReceived -= OnServerResponseReceived;
    }

    public void SendQuery(string textInInputField)
    {
        string text = inputTextFieldForQuery.text;
        if (!string.IsNullOrEmpty(text))
        {
            server.SendTextQuery(text);
            textAreaForChatHistory.text += "<color=#FF0000>PLAYER</color>: " + text + "\n";
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
        textAreaForChatHistory.text += "<color=#00FF00>SERVER</color>: " + textResponse + "\n";
    }
}
