using UnityEngine;

public static class WavUtility
{
    public static AudioClip ToAudioClip(byte[] data)
    {
        int sampleCount = data.Length / 2; // 2 bytes per sample (16-bit WAV)
        short[] shortData = new short[sampleCount];

        // Convert byte[] to short[] (16-bit signed PCM)
        for (int i = 0; i < sampleCount; i++)
        {
            shortData[i] = System.BitConverter.ToInt16(data, i * 2);
        }

        // Create AudioClip
        AudioClip clip = AudioClip.Create("ReceivedAudio", sampleCount, 1, 44100, false); // Assuming mono 44100Hz
        float[] floatData = new float[sampleCount];
        for (int i = 0; i < sampleCount; i++)
        {
            floatData[i] = shortData[i] / 32768f; // Convert short to float (-1 to 1)
        }

        clip.SetData(floatData, 0);
        return clip;
    }
}