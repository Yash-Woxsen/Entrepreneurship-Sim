using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using System;
using System.Text;

namespace _Scripts
{
    public class AudioSender : MonoBehaviour
    {
        public MicRecorder micRecorder;
        public Button sendButton;
        public TextMeshProUGUI statusText;

        private const string ServerUrl = "http://localhost:5000/audio"; // Change if different host

        void Start()
        {
            sendButton.onClick.AddListener(SendAudioToServer);
        }

        public void SendAudioToServer()
        {
            AudioClip clip = micRecorder.GetRecordedClip();

            if (clip == null)
            {
                Debug.LogWarning("No recorded audio to send.");
                statusText.text = "No recorded audio to send.";
                return;
            }

            StartCoroutine(SendAudioCoroutine(clip));
        }

        private IEnumerator SendAudioCoroutine(AudioClip clip)
        {
            // Convert AudioClip to WAV byte[]
            byte[] wavData = ConvertClipToWav(clip);

            // Prepare form for upload
            WWWForm form = new WWWForm();
            form.AddBinaryData("audio", wavData, "unity_recording.wav", "audio/wav");

            using (UnityWebRequest www = UnityWebRequest.Post(ServerUrl, form))
            {
                yield return www.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
                if (www.result != UnityWebRequest.Result.Success)
#else
            if (www.isNetworkError || www.isHttpError)
#endif
                {
                    Debug.LogError("Audio upload failed: " + www.error);
                    statusText.text = "Upload failed: " + www.error;
                }
                else
                {
                    Debug.Log("Audio sent successfully! Server response length: " + www.downloadHandler.data.Length);
                    statusText.text = "Audio sent successfully! Receiving response...";

                    // Once audio is successfully uploaded, handle the received audio
                    byte[] receivedAudio = www.downloadHandler.data;
                    PlayReceivedAudio(receivedAudio);
                }
            }
        }

        private void PlayReceivedAudio(byte[] audioData)
        {
            // Create a memory stream to load the received audio data
            MemoryStream memoryStream = new MemoryStream(audioData);
            byte[] audioBytes = memoryStream.ToArray();

            // Load the received byte data into an AudioClip
            AudioClip receivedClip = WavUtility.ToAudioClip(audioBytes);

            // Assign and play the received audio clip using AudioSource
            AudioSource audioSource = GetComponent<AudioSource>();
            audioSource.clip = receivedClip;
            audioSource.Play();

            Debug.Log("Playing received audio...");
        }

        private byte[] ConvertClipToWav(AudioClip clip)
        {
            int samples = clip.samples * clip.channels;
            float[] sampleData = new float[samples];
            clip.GetData(sampleData, 0);

            // WAV format writing
            const int headerSize = 44;
            int sampleRate = clip.frequency;
            int channels = clip.channels;

            byte[] wavBytes = new byte[samples * 2 + headerSize];

            // RIFF header
            Encoding.ASCII.GetBytes("RIFF").CopyTo(wavBytes, 0);
            BitConverter.GetBytes(wavBytes.Length - 8).CopyTo(wavBytes, 4);
            Encoding.ASCII.GetBytes("WAVE").CopyTo(wavBytes, 8);

            // fmt chunk
            Encoding.ASCII.GetBytes("fmt ").CopyTo(wavBytes, 12);
            BitConverter.GetBytes(16).CopyTo(wavBytes, 16); // Subchunk1Size
            BitConverter.GetBytes((ushort)1).CopyTo(wavBytes, 20); // Audio format (1 = PCM)
            BitConverter.GetBytes((ushort)channels).CopyTo(wavBytes, 22);
            BitConverter.GetBytes(sampleRate).CopyTo(wavBytes, 24);
            BitConverter.GetBytes(sampleRate * channels * 2).CopyTo(wavBytes, 28);
            BitConverter.GetBytes((ushort)(channels * 2)).CopyTo(wavBytes, 32);
            BitConverter.GetBytes((ushort)16).CopyTo(wavBytes, 34);

            // data chunk
            Encoding.ASCII.GetBytes("data").CopyTo(wavBytes, 36);
            BitConverter.GetBytes(samples * 2).CopyTo(wavBytes, 40);

            // PCM data
            int offset = headerSize;
            for (int i = 0; i < sampleData.Length; i++)
            {
                short val = (short)(sampleData[i] * short.MaxValue);
                BitConverter.GetBytes(val).CopyTo(wavBytes, offset);
                offset += 2;
            }

            return wavBytes;
        }
    }
}