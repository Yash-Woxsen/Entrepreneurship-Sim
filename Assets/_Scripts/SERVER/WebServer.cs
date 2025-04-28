using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Text;
using System.IO;
using UnityEngine.Events;

namespace _Scripts.SERVER
{
    public class WebServer : MonoBehaviour
    {
        [TextArea(20,50)]public string jsonResponseReceived;
        public ServerResponse serverResponseObject;
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
    
    
    
        // Call this function to send an audio file
        public void SendAudioQuery(string filePath)
        {
            StartCoroutine(PostAudioQuery(filePath));
        }
        
        IEnumerator PostAudioQuery(string filePath)
        {
            string url = "http://10.7.0.28:5505/whisper";
            byte[] audioData = File.ReadAllBytes(filePath);
        
            // Create form and add audio file
            WWWForm form = new WWWForm();
            form.AddBinaryData("file", audioData, Path.GetFileName(filePath), "audio/wav");
        
            UnityWebRequest request = UnityWebRequest.Post(url, form);
        
            yield return request.SendWebRequest();
        
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Response: " + request.downloadHandler.text);
            }
            else
            {
                Debug.LogError("Error: " + request.error);
            }
        }
    
    }
    
    
    
    
    
    
    
    [System.Serializable]
    public class ServerResponse
    {
        public string response;
        public string[] suggestions;
        public string audio_url;
    }
}
