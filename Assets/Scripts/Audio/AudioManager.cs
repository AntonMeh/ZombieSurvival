using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Settings State")]
    public bool isMusicOn = true;
    public bool isSoundOn = true;

    [Header("Audio Sources")]
    public AudioSource musicSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 

            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.loop = true; 
            }

            LoadSettings(); 

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject); 
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {

        ApplySettings();
    }

    public void ToggleMusic(bool value)
    {
        isMusicOn = value;
        PlayerPrefs.SetInt("MusicOn", isMusicOn ? 1 : 0); 
        ApplySettings();
    }

    public void ToggleSounds(bool value)
    {
        isSoundOn = value;
        PlayerPrefs.SetInt("SoundOn", isSoundOn ? 1 : 0);
        ApplySettings();
    }

    private void LoadSettings()
    {
        isMusicOn = PlayerPrefs.GetInt("MusicOn", 1) == 1;
        isSoundOn = PlayerPrefs.GetInt("SoundOn", 1) == 1;
        ApplySettings();
    }

    public void ApplySettings()
    {

        if (musicSource != null)
            musicSource.mute = !isMusicOn;

        MusicPlayer sceneMusicPlayer = Object.FindFirstObjectByType<MusicPlayer>();
        if (sceneMusicPlayer != null)
        {
            AudioSource sceneAudio = sceneMusicPlayer.GetComponent<AudioSource>();
            if (sceneAudio != null)
                sceneAudio.mute = !isMusicOn;
        }

        if (SoundManager.Instance != null)
            SoundManager.Instance.SetSoundEnabled(isSoundOn);

        Debug.Log($"Settings applied: Music={isMusicOn}, Sound={isSoundOn}");
    }

    public void PlayMusic(AudioClip clip)
    {
        if (musicSource == null || clip == null) return;

        if (musicSource.clip == clip && musicSource.isPlaying) return;

        musicSource.clip = clip;
        musicSource.Play();
    }
}