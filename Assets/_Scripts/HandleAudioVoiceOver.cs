using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using _Scripts.SERVER;

public class HandleAudioVoiceOver : MonoBehaviour
{
    public AudioSource audioSource;
    public WebServer webServer;

    void OnEnable()
    {
        webServer.OnTextQueryResponseReceived += PlayAudio;
    }
    void OnDisable()
    {
        webServer.OnTextQueryResponseReceived -= PlayAudio;
    }

    void PlayAudio()
    {
        StartCoroutine(PlayAudioFromURL(webServer.serverResponseObject.audio_url));
    }

    IEnumerator PlayAudioFromURL(string url)
    {
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Audio download error: " + www.error);
            }
            else
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                audioSource.clip = clip;
                audioSource.Play();
            }
        }
    }
}