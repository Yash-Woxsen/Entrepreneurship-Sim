using System;
using _Scripts.SERVER;
using UnityEngine;
using UnityEngine.UI;

public class MicButtonEffect : MonoBehaviour
{
    public Image micButtonImage;
    public Color normalColor = Color.white;
    public Color recordingColor = Color.red;
    public WebServer webServer;
    public GameObject loadingIcon;

    private void OnEnable()
    {
        webServer.OnAudioQueryResponseReceived += DisableLoadingIcon;
    }
    private void OnDisable()
    {
        webServer.OnAudioQueryResponseReceived -= DisableLoadingIcon;
    }

    public void ChangeImageColor()
    {
        micButtonImage.color = recordingColor;
    }
    public void ResetImageColor()
    {
        micButtonImage.color = normalColor;
    }

    public void DisableLoadingIcon()
    {
        loadingIcon.SetActive(false);
    }
}
