using System;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems; // Required for EventTrigger

namespace _Scripts
{
    [RequireComponent(typeof(AudioSource))]
    public class MicRecorder : MonoBehaviour
    {
        const int HeaderSize = 44;
        private AudioSource _audioSource;
        private AudioClip _recordedClip;
        private string _microphoneDevice;
        private bool _isRecording = false;
        private bool _isPlaying = false;

        public Button recordButton;
        public Button playButton;
        public TextMeshProUGUI recordButtonText;
        public TextMeshProUGUI playButtonText;
        public TextMeshProUGUI statusText;

        public string fileName = "MyRecording";

        void Start()
        {
            _audioSource = GetComponent<AudioSource>();
            _microphoneDevice = Microphone.devices.Length > 0 ? Microphone.devices[0] : null;

            if (_microphoneDevice == null)
            {
                Debug.LogError("No microphone found.");
                statusText.text = "Microphone not available!";
                return;
            }

            
            playButton.onClick.AddListener(TogglePlayback);

            // Set up the EventTrigger for the record button
            EventTrigger trigger = recordButton.gameObject.AddComponent<EventTrigger>();
            EventTrigger.Entry pointerDownEntry = new EventTrigger.Entry();
            pointerDownEntry.eventID = EventTriggerType.PointerDown;
            pointerDownEntry.callback.AddListener((eventData) => StartRecording());
            trigger.triggers.Add(pointerDownEntry);

            EventTrigger.Entry pointerUpEntry = new EventTrigger.Entry();
            pointerUpEntry.eventID = EventTriggerType.PointerUp;
            pointerUpEntry.callback.AddListener((eventData) => StopRecording());
            trigger.triggers.Add(pointerUpEntry);

            playButton.interactable = false;
            recordButtonText.text = "Hold to Record";
            playButtonText.text = "Play";
            statusText.text = "Ready to record.";
        }

        
        void StartRecording()
        {
            if (!_isRecording)
            {
                // Start recording
                _recordedClip = Microphone.Start(_microphoneDevice, false, 10, 44100);
                _isRecording = true;
                recordButtonText.text = "Release to Stop";
                playButton.interactable = false;
                statusText.text = "Recording...";
            }
        }

        
        void StopRecording()
        {
            if (_isRecording)
            {
                // Stop recording
                int position = Microphone.GetPosition(_microphoneDevice);
                Microphone.End(_microphoneDevice);
                _isRecording = false;
                recordButtonText.text = "Hold to Record";

                if (position > 0)
                {
                    float[] samples = new float[position * _recordedClip.channels];
                    _recordedClip.GetData(samples, 0);

                    AudioClip trimmedClip = AudioClip.Create("TrimmedClip", position, _recordedClip.channels,
                        _recordedClip.frequency, false);
                    trimmedClip.SetData(samples, 0);
                    _recordedClip = trimmedClip;

                    SaveToFile(_recordedClip);
                    playButton.interactable = true;
                    statusText.text = "Recording saved. Ready to play.";
                }
                else
                {
                    _recordedClip = null;
                    statusText.text = "Recording failed or too short.";
                }
            }
        }

        void TogglePlayback()
        {
            if (_recordedClip == null)
            {
                statusText.text = "No recording to play!";
                return;
            }

            if (!_isPlaying)
            {
                _audioSource.clip = _recordedClip;
                _audioSource.Play();
                _isPlaying = true;
                playButtonText.text = "Stop Playing";
                statusText.text = "Playing recording...";
            }
            else
            {
                _audioSource.Stop();
                _isPlaying = false;
                playButtonText.text = "Play";
                statusText.text = "Playback stopped.";
            }
        }

        void SaveToFile(AudioClip clip)
        {
            float[] samples = new float[clip.samples * clip.channels];
            clip.GetData(samples, 0);

            string filePath = Path.Combine(Application.streamingAssetsPath, fileName + ".wav");

            if (!Directory.Exists(Application.streamingAssetsPath))
                Directory.CreateDirectory(Application.streamingAssetsPath);

            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                int frequency = clip.frequency;
                int channels = clip.channels;
                int samplesCount = samples.Length;

                // WAV Header
                fs.Write(Encoding.ASCII.GetBytes("RIFF"), 0, 4);
                fs.Write(BitConverter.GetBytes(HeaderSize + samplesCount * 2 - 8), 0, 4);
                fs.Write(Encoding.ASCII.GetBytes("WAVE"), 0, 4);
                fs.Write(Encoding.ASCII.GetBytes("fmt "), 0, 4);
                fs.Write(BitConverter.GetBytes(16), 0, 4);
                fs.Write(BitConverter.GetBytes((ushort)1), 0, 2);
                fs.Write(BitConverter.GetBytes((ushort)channels), 0, 2);
                fs.Write(BitConverter.GetBytes(frequency), 0, 4);
                fs.Write(BitConverter.GetBytes(frequency * channels * 2), 0, 4);
                fs.Write(BitConverter.GetBytes((ushort)(channels * 2)), 0, 2);
                fs.Write(BitConverter.GetBytes((ushort)16), 0, 2);
                fs.Write(Encoding.ASCII.GetBytes("data"), 0, 4);
                fs.Write(BitConverter.GetBytes(samplesCount * 2), 0, 4);

                // Data
                short[] intData = new short[samples.Length];
                byte[] byteData = new byte[samples.Length * 2];

                for (int i = 0; i < samples.Length; i++)
                {
                    intData[i] = (short)(samples[i] * 32767);
                    BitConverter.GetBytes(intData[i]).CopyTo(byteData, i * 2);
                }

                fs.Write(byteData, 0, byteData.Length);
            }

            Debug.Log("Saved WAV to: " + filePath);
        }

        public AudioClip GetRecordedClip()
        {
            return _recordedClip;
        }
    }
}