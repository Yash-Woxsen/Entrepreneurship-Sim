using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using _Scripts.SERVER;

public class HandleAudioVoiceOver : MonoBehaviour
{
    public AudioSource audioSource;
    public WebServer webServer;
    public string audioURL;
    public bool isAudioPlaying = false;
    private void OnEnable()
    {
        webServer.OnTextQueryResponseReceived += PlayReceivedAudio;
    }

    private void Update()
    {
        if (audioSource.isPlaying)
        {
            isAudioPlaying = true;
        }
        else
        {
            isAudioPlaying = false;
        }
    }

    private void OnDisable()
    {
        webServer.OnTextQueryResponseReceived -= PlayReceivedAudio;
    }

    void PlayReceivedAudio()
    {
        audioURL = webServer.serverResponseObject.audio_url;
        StartCoroutine(PlayAudioFromURL(webServer.serverResponseObject.audio_url));
    }

    IEnumerator PlayAudioFromURL(string url)
    {
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.WAV))
        {
            Debug.Log("Starting audio download...");
            yield return www.SendWebRequest();

            
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Audio download error: " + www.error);
            }
            else
            {
                Debug.Log("Audio downloaded successfully!");
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                if (clip != null)
                {
                    audioSource.clip = clip;
                    audioSource.Play();
                    Debug.Log("Audio playing...");
                }
                else
                {
                    Debug.LogError("Failed to load audio clip.");
                }
            }
        }
    }
}