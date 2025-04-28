using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Text;
using System.IO;
using System;
using UnityEngine.Events;

namespace _Scripts.SERVER
{
    public class WebServer : MonoBehaviour
    {
        [TextArea(20, 50)] public string jsonResponseReceived;
        public ServerResponse serverResponseObject;
        public WhisperResponse whisperResponseObject;
        public float timeTakenToReceiveResponseFromServer;

        private float GetCurrentTimeInSeconds()
        {
            return Time.realtimeSinceStartup;
        }

        #region TEXT QUERY HANDLER

        public event UnityAction OnTextQueryResponseReceived;


        public void SendTextQuery(string text)
        {
            StartCoroutine(PostTextQuery(text));
        }

        IEnumerator PostTextQuery(string text)
        {
            string url = "http://10.7.0.28:5505/ask";

            // Escape special characters in the text to avoid JSON errors
            string escapedText = EscapeJsonString(text);

            // Create JSON payload with key "query"
            string jsonData = "{\"query\": \"" + escapedText + "\"}";
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

            UnityWebRequest request = new UnityWebRequest(url, "POST");
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            float startTime = GetCurrentTimeInSeconds();
            yield return request.SendWebRequest();

            // Calculate elapsed time
            timeTakenToReceiveResponseFromServer = GetCurrentTimeInSeconds() - startTime;


            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;
                jsonResponseReceived = jsonResponse;
                Debug.Log("Time taken to receive response: " + timeTakenToReceiveResponseFromServer + " seconds");
                serverResponseObject = JsonUtility.FromJson<ServerResponse>(request.downloadHandler.text);
                OnTextQueryResponseReceived?.Invoke();
            }
            else
            {
                Debug.LogError("Error sending request: " + request.error);
                jsonResponseReceived = request.error;
                OnTextQueryResponseReceived?.Invoke();
            }
        }

        private string EscapeJsonString(string str) // Helper method to escape special characters in JSON string
        {
            return str.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");
        }

        #endregion


    #region AUDIO QUERY HANDLER


        public string serverUrl = "http://10.7.0.28:5505/whisper";
        private string selectedLanguage = "en";
        public event UnityAction OnAudioQueryResponseReceived;

        public void SendAudioClip(AudioClip audioInput)
        {
            StartCoroutine(CallServerWhisper(audioInput));
        }

        IEnumerator CallServerWhisper(AudioClip audioInput)
        {
            byte[] wavData = ConvertAudioClipToWav(audioInput);


            // Create a form to send the audio file
            WWWForm form = new WWWForm();
            form.AddBinaryData("file", wavData, "temp_audio.wav", "audio/wav");

            // Append the language as a query parameter in the URL
            string urlWithLanguage = $"{serverUrl}?language={selectedLanguage}";

            using (UnityWebRequest www = UnityWebRequest.Post(urlWithLanguage, form))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Error: " + www.error + "\nResponse: " + www.downloadHandler.text);
                }
                else
                {
                    whisperResponseObject = JsonUtility.FromJson<WhisperResponse>(www.downloadHandler.text);
                    Debug.Log(whisperResponseObject.transcription);
                    OnAudioQueryResponseReceived?.Invoke();
                }
            }
        }
        
        byte[] ConvertAudioClipToWav(AudioClip clip)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                float[] samples = new float[clip.samples * clip.channels];
                clip.GetData(samples, 0);
                byte[] bytes = new byte[samples.Length * 2];
                for (int i = 0; i < samples.Length; i++)
                {
                    short sample = (short)(samples[i] * short.MaxValue);
                    bytes[i * 2] = (byte)(sample & 0xFF);
                    bytes[i * 2 + 1] = (byte)((sample >> 8) & 0xFF);
                }
 
                int hz = clip.frequency;
                int channels = clip.channels;
                int subChunk2Size = bytes.Length;
                int chunkSize = 36 + subChunk2Size;
 
                // RIFF header
                stream.Write(Encoding.UTF8.GetBytes("RIFF"), 0, 4);
                stream.Write(BitConverter.GetBytes(chunkSize), 0, 4);
                stream.Write(Encoding.UTF8.GetBytes("WAVE"), 0, 4);
 
                // fmt sub-chunk
                stream.Write(Encoding.UTF8.GetBytes("fmt "), 0, 4);
                stream.Write(BitConverter.GetBytes(16), 0, 4); // Sub-chunk size
                stream.Write(BitConverter.GetBytes((short)1), 0, 2); // Audio format (1 = PCM)
                stream.Write(BitConverter.GetBytes((short)channels), 0, 2); // Number of channels
                stream.Write(BitConverter.GetBytes(hz), 0, 4); // Sample rate
                stream.Write(BitConverter.GetBytes(hz * channels * 2), 0, 4); // Byte rate
                stream.Write(BitConverter.GetBytes((short)(channels * 2)), 0, 2); // Block align
                stream.Write(BitConverter.GetBytes((short)16), 0, 2); // Bits per sample
 
                // data sub-chunk
                stream.Write(Encoding.UTF8.GetBytes("data"), 0, 4);
                stream.Write(BitConverter.GetBytes(subChunk2Size), 0, 4);
                stream.Write(bytes, 0, bytes.Length);
 
                return stream.ToArray();
            }
        }
    }

    #endregion



    [System.Serializable]
    public class ServerResponse
    {
        public string response;
        public string[] suggestions;
        public string audio_url;
    }
    [Serializable]
    public class WhisperResponse
    {
        public string transcription; // Field for the transcription text
    }
}
