using System;
using UnityEngine;
using UnityEngine.UI;
using _Scripts.SERVER;

public class ButtonDisableUntilResponseReceived : MonoBehaviour
{
    public WebServer webServer;
    Button buttonToDisable;

    private void Start()
    {
        buttonToDisable = GetComponent<Button>();
        webServer.OnTextQueryResponseReceived += EnableButton;
    }

    public void DisableButton()
    {
        // Assign in Inspector OnClick event to this method
        buttonToDisable.interactable = false;
    }
    public void EnableButton()
    {
        buttonToDisable.interactable = true;
    }
}
