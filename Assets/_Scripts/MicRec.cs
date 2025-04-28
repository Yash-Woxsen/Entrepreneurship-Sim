using UnityEngine;
using System.IO;
using System.Text;
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
        // Get default microphone
        if (Microphone.devices.Length > 0)
            micDevice = Microphone.devices[0];
        else
            Debug.LogWarning("No microphone detected on this device.");
    }

    void Update()
    {
        // Start recording on T key down
        if (!isRecording && Input.GetKeyDown(KeyCode.Alpha0))
        {
            StartRecording();
        }

        // Stop recording and save on T key up
        if (isRecording && Input.GetKeyUp(KeyCode.Alpha0))
        {
            StopRecordingAndSave();
        }
    }

    void StartRecording()
    {
        if (micDevice == null)
            return;

        recordedClip = Microphone.Start(micDevice, false, maxRecordTime, sampleRate);
        isRecording = true;
        Debug.Log("Recording started...");
    }

    void StopRecordingAndSave()
    {
        if (!isRecording)
            return;

        int position = Microphone.GetPosition(micDevice);
        Microphone.End(micDevice);
        isRecording = false;

        if (position > 0)
        {
            // Trim the clip to the actual recorded length
            float[] samples = new float[position * recordedClip.channels];
            recordedClip.GetData(samples, 0);

            AudioClip trimmedClip = AudioClip.Create("TrimmedClip", position, recordedClip.channels, recordedClip.frequency, false);
            trimmedClip.SetData(samples, 0);

            SaveClipToWav(trimmedClip);
            Debug.Log("Recording stopped and saved.");
        }
        else
        {
            Debug.LogWarning("Recording was too short or failed.");
        }
    }

    void SaveClipToWav(AudioClip clip)
    {
        string folderPath = Application.streamingAssetsPath;
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        string filePath = Path.Combine(folderPath, "MicRecording_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".wav");

        // WAV file writing
        using (FileStream fs = new FileStream(filePath, FileMode.Create))
        {
            int samples = clip.samples * clip.channels;
            int frequency = clip.frequency;
            int channels = clip.channels;
            byte[] wavHeader = GetWavHeader(samples, frequency, channels);
            fs.Write(wavHeader, 0, wavHeader.Length);

            float[] data = new float[samples];
            clip.GetData(data, 0);

            short[] intData = new short[samples];
            byte[] bytesData = new byte[samples * 2];

            for (int i = 0; i < samples; i++)
            {
                intData[i] = (short)(data[i] * 32767);
                byte[] byteArr = System.BitConverter.GetBytes(intData[i]);
                byteArr.CopyTo(bytesData, i * 2);
            }
            fs.Write(bytesData, 0, bytesData.Length);
        }
        Debug.Log("Saved WAV to: " + filePath);
        webServer.SendAudioQuery(filePath);
    }

    byte[] GetWavHeader(int samples, int frequency, int channels)
    {
        int byteRate = frequency * channels * 2;
        int fileSize = 44 + samples * 2;

        using (MemoryStream ms = new MemoryStream(44))
        using (BinaryWriter bw = new BinaryWriter(ms, Encoding.ASCII))
        {
            bw.Write(Encoding.ASCII.GetBytes("RIFF"));
            bw.Write(fileSize - 8);
            bw.Write(Encoding.ASCII.GetBytes("WAVE"));
            bw.Write(Encoding.ASCII.GetBytes("fmt "));
            bw.Write(16);
            bw.Write((short)1);
            bw.Write((short)channels);
            bw.Write(frequency);
            bw.Write(byteRate);
            bw.Write((short)(channels * 2));
            bw.Write((short)16);
            bw.Write(Encoding.ASCII.GetBytes("data"));
            bw.Write(samples * 2);
            return ms.ToArray();
        }
    }
}
