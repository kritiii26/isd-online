using UnityEngine;

/// <summary>
/// Provides zero-dependency procedural audio chimes for UI clicks, planet gaze hover, and selection.
/// Creates audio clips at runtime using pure mathematical waveforms so no external audio assets are needed.
/// Aligned with NPTEL Week 1 (AudioSource and user feedback).
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class SolarSystemAudio : MonoBehaviour
{
    private AudioSource audioSource;
    private AudioClip hoverToneClip;
    private AudioClip selectToneClip;
    private AudioClip clickToneClip;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D clean stereo audio

        // Generate procedural audio clips
        hoverToneClip = GenerateTone(440f, 0.08f, 0.12f); // Gentle A4 note
        selectToneClip = GenerateTone(880f, 0.25f, 0.25f); // Crisp A5 chime
        clickToneClip = GenerateTone(587f, 0.05f, 0.15f); // D5 tap
    }

    public void PlayHoverTone()
    {
        if (audioSource != null && hoverToneClip != null)
        {
            audioSource.PlayOneShot(hoverToneClip, 0.4f);
        }
    }

    public void PlaySelectTone()
    {
        if (audioSource != null && selectToneClip != null)
        {
            audioSource.PlayOneShot(selectToneClip, 0.7f);
        }
    }

    public void PlayClickTone()
    {
        if (audioSource != null && clickToneClip != null)
        {
            audioSource.PlayOneShot(clickToneClip, 0.5f);
        }
    }

    /// <summary>
    /// Generates a simple sine wave tone with linear decay envelope.
    /// </summary>
    private AudioClip GenerateTone(float frequency, float duration, float gain)
    {
        int sampleRate = 44100;
        int sampleCount = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = 1f - ((float)i / sampleCount); // Decay to 0
            samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * envelope * gain;
        }

        AudioClip clip = AudioClip.Create($"Tone_{frequency}Hz", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
