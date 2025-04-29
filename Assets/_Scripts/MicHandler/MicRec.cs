using UnityEngine;
using _Scripts.SERVER;

public class MicRec : MonoBehaviour
{
    private AudioClip recordedClip;
    private string micDevice;
    private bool isRecording = false;
    private int sampleRate = 44100;
    private int maxRecordTime = 20; // seconds
    
    public WebServer webServer;

    void Start()
    {
        if (Microphone.devices.Length > 0)
            micDevice = Microphone.devices[0];
        else
            Debug.LogWarning("No microphone detected on this device.");
    }

    void Update()
    {
        // Start recording on 0 key down
        if (!isRecording && Input.GetKeyDown(KeyCode.Alpha0))
        {
            StartRecording();
        }

        // Stop recording and save on 0 key up
        if (isRecording && Input.GetKeyUp(KeyCode.Alpha0))
        {
            StopRecording();
        }
    }

    public void StartRecording()
    {
        if (micDevice == null)
            return;

        recordedClip = Microphone.Start(micDevice, false, maxRecordTime, sampleRate);
        isRecording = true;
        Debug.Log("Recording started...");
    }

    public void StopRecording()
    {
        if (!isRecording)
            return;
        
        Microphone.End(micDevice);
        isRecording = false;
        Debug.Log("Recording stopped.");
        if(recordedClip!=null)
            webServer.SendAudioClip(recordedClip);
        else
            Debug.Log("No audio recorded.");
        
    }
}
