using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    private AudioSource audioSource;
    private bool soundEnabled = true;

    void Awake()
    {
        Instance = this;
        audioSource = GetComponent<AudioSource>();
        audioSource.spatialBlend = 0f;

        soundEnabled = PlayerPrefs.GetInt("SoundOn", 1) == 1;
        audioSource.mute = !soundEnabled;
    }

    public void PlaySound(AudioClip clip, float volume = 1f)
    {
        if (!soundEnabled || clip == null) return;
        audioSource.PlayOneShot(clip, volume);
    }

    public void SetSoundEnabled(bool enabled)
    {
        soundEnabled = enabled;
        audioSource.mute = !enabled;
    }
}